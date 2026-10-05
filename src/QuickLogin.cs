using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace RinAccountManager
{
    public class QuickLoginTicket
    {
        public string Code;
        public string PrivateKey;
        public DateTime ExpiresUtc;
        public string QrUrl;
    }

    // Quick Login theo tai lieu cong dong Roblox (devforum 3147931):
    // create -> user nhap ma tren thiet bi da dang nhap -> poll status ->
    // validated -> POST v2/login lay .ROBLOSECURITY tu Set-Cookie.
    public static class QuickLogin
    {
        static QuickLogin()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol =
                    System.Net.SecurityProtocolType.Tls12 |
                    System.Net.SecurityProtocolType.Tls11 |
                    System.Net.SecurityProtocolType.Tls;
            }
            catch { }
        }

        private const string Base = "https://apis.roblox.com";

        private class PostResult
        {
            public bool TransportOk;
            public HttpStatusCode Status;
            public string Body;
            public WebHeaderCollection Headers;
            public string TransportError;
        }

        private static string Escape(string s)
        {
            if (s == null)
            {
                return "";
            }
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static PostResult PostJson(string url, string json, string csrfToken)
        {
            PostResult r = new PostResult();
            r.TransportOk = false;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Accept = "application/json";
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) RinAccountManager/1.0";
                req.Timeout = 15000;
                req.ReadWriteTimeout = 15000;
                if (!string.IsNullOrEmpty(csrfToken))
                {
                    req.Headers["X-CSRF-TOKEN"] = csrfToken;
                }
                byte[] bytes = Encoding.UTF8.GetBytes(json == null ? "" : json);
                req.ContentLength = bytes.Length;
                if (bytes.Length > 0)
                {
                    using (Stream ws = req.GetRequestStream())
                    {
                        ws.Write(bytes, 0, bytes.Length);
                    }
                }
                else
                {
                    // van phai mo stream de gui POST rong
                    using (Stream ws = req.GetRequestStream())
                    {
                    }
                }
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    r.Status = resp.StatusCode;
                    r.Headers = resp.Headers;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        r.Body = sr.ReadToEnd();
                    }
                    r.TransportOk = true;
                    return r;
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse resp = wex.Response as HttpWebResponse;
                if (resp != null)
                {
                    try
                    {
                        r.Status = resp.StatusCode;
                        r.Headers = resp.Headers;
                        try
                        {
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                            {
                                r.Body = sr.ReadToEnd();
                            }
                        }
                        catch { r.Body = ""; }
                        r.TransportOk = true;
                    }
                    finally
                    {
                        try { resp.Close(); } catch { }
                    }
                    return r;
                }
                r.TransportError = wex.Message;
                return r;
            }
            catch (Exception ex)
            {
                r.TransportError = ex.Message;
                return r;
            }
        }

        // POST, neu gap 403 kem x-csrf-token thi tu retry 1 lan.
        private static PostResult PostWithCsrf(string url, string json)
        {
            PostResult r = PostJson(url, json, null);
            if (r.TransportOk && r.Status == HttpStatusCode.Forbidden && r.Headers != null)
            {
                string tok = r.Headers["x-csrf-token"];
                if (!string.IsNullOrEmpty(tok))
                {
                    return PostJson(url, json, tok);
                }
            }
            return r;
        }

        private static Dictionary<string, object> ParseJson(string body)
        {
            try
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                return ser.Deserialize<Dictionary<string, object>>(body);
            }
            catch
            {
                return null;
            }
        }

        private static string Str(Dictionary<string, object> d, string key)
        {
            if (d == null)
            {
                return null;
            }
            object v;
            if (!d.TryGetValue(key, out v) || v == null)
            {
                return null;
            }
            return Convert.ToString(v);
        }

        public static bool TryCreate(out QuickLoginTicket ticket, out string error)
        {
            ticket = null;
            error = null;
            PostResult r = PostJson(Base + "/auth-token-service/v1/login/create", "", null);
            if (!r.TransportOk)
            {
                error = "Khong ket noi duoc Roblox: " + r.TransportError;
                return false;
            }
            if (r.Status != HttpStatusCode.OK)
            {
                error = "Tao ma that bai (HTTP " + ((int)r.Status).ToString() + ").";
                return false;
            }
            Dictionary<string, object> d = ParseJson(r.Body);
            string code = Str(d, "code");
            string key = Str(d, "privateKey");
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(key))
            {
                error = "Roblox tra ve thieu code.";
                return false;
            }
            QuickLoginTicket t = new QuickLoginTicket();
            t.Code = code;
            t.PrivateKey = key;
            try
            {
                // server tra gio UTC nhung khong kem mui gio -> ep hieu la UTC
                t.ExpiresUtc = DateTime.Parse(Str(d, "expirationTime"), null,
                    System.Globalization.DateTimeStyles.AssumeUniversal |
                    System.Globalization.DateTimeStyles.AdjustToUniversal);
            }
            catch
            {
                t.ExpiresUtc = DateTime.UtcNow.AddMinutes(10);
            }
            string img = Str(d, "imagePath");
            // imagePath dang "/v1/login/qr-code-image?..." -> ghep duoi service root
            t.QrUrl = string.IsNullOrEmpty(img) ? null : (Base + "/auth-token-service" + img);
            ticket = t;
            return true;
        }

        // Tra ve status: Created / UserLinked / Validated / Cancelled / Expired
        public static string GetStatus(QuickLoginTicket t, out string accountName, out string error)
        {
            accountName = null;
            error = null;
            string json = "{\"code\":\"" + Escape(t.Code) + "\",\"privateKey\":\"" + Escape(t.PrivateKey) + "\"}";
            PostResult r = PostWithCsrf(Base + "/auth-token-service/v1/login/status", json);
            if (!r.TransportOk)
            {
                error = r.TransportError;
                return null;
            }
            if (r.Status == HttpStatusCode.BadRequest && r.Body != null && r.Body.Contains("CodeInvalid"))
            {
                return "Expired";
            }
            if (r.Status != HttpStatusCode.OK)
            {
                error = "Status HTTP " + ((int)r.Status).ToString();
                return null;
            }
            Dictionary<string, object> d = ParseJson(r.Body);
            accountName = Str(d, "accountName");
            string st = Str(d, "status");
            return string.IsNullOrEmpty(st) ? "Created" : st;
        }

        public static bool TryExchange(QuickLoginTicket t, out string cookie, out string error)
        {
            cookie = null;
            error = null;
            string json = "{\"ctype\":\"AuthToken\",\"cvalue\":\"" + Escape(t.Code) + "\",\"password\":\"" + Escape(t.PrivateKey) + "\"}";
            PostResult r = PostWithCsrf("https://auth.roblox.com/v2/login", json);
            if (!r.TransportOk)
            {
                error = "Login that bai: " + r.TransportError;
                return false;
            }
            if (r.Status != HttpStatusCode.OK)
            {
                error = "Login that bai (HTTP " + ((int)r.Status).ToString() + "). Ma co the het han.";
                return false;
            }
            try
            {
                string[] setCookies = r.Headers.GetValues("Set-Cookie");
                if (setCookies != null)
                {
                    for (int i = 0; i < setCookies.Length; i++)
                    {
                        string sc = setCookies[i];
                        if (sc != null && sc.StartsWith(".ROBLOSECURITY="))
                        {
                            string v = sc.Substring(".ROBLOSECURITY=".Length);
                            int semi = v.IndexOf(';');
                            if (semi >= 0)
                            {
                                v = v.Substring(0, semi);
                            }
                            cookie = v.Trim();
                            return true;
                        }
                    }
                }
            }
            catch { }
            error = "Khong thay cookie trong response.";
            return false;
        }

        public static void Cancel(QuickLoginTicket t)
        {
            if (t == null || string.IsNullOrEmpty(t.Code))
            {
                return;
            }
            try
            {
                PostJson(Base + "/auth-token-service/v1/login/cancel",
                    "{\"code\":\"" + Escape(t.Code) + "\"}", null);
            }
            catch { }
        }

        public static System.Drawing.Image LoadQr(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) RinAccountManager/1.0";
                req.Timeout = 15000;
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        resp.GetResponseStream().CopyTo(ms);
                        ms.Position = 0;
                        return System.Drawing.Image.FromStream(ms);
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
