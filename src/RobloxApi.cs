using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace RinAccountManager
{
    public class RobloxUser
    {
        public long Id;
        public string Name;
        public bool Ok;
        public string Error;
    }

    public static class RobloxApi
    {
        static RobloxApi()
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

        private const string RefererGame = "https://www.roblox.com/games/4924922222/Brookhaven-RP";

        private static HttpWebRequest Create(string url, string cookie)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) RinAccountManager/1.0";
            req.Timeout = 15000;
            req.ReadWriteTimeout = 15000;
            if (!string.IsNullOrEmpty(cookie))
            {
                // Cookie co the bat dau bang "_|WARNING:-DO-NOT-SHARE-THIS..."
                // Chi lay phan sau dau "|" cuoi cung.
                string c = ExtractCookie(cookie);
                req.Headers["Cookie"] = ".ROBLOSECURITY=" + c;
            }
            return req;
        }

        public static string ExtractCookie(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return raw;
            }
            string t = raw.Trim().Trim('"');
            // dinh dang chuan: ...|_COOKIE_THAT
            // lay sau cum "|_" cuoi cung de bo warning + dau _ thua
            int sep = t.LastIndexOf("|_");
            if (sep >= 0 && sep + 2 < t.Length)
            {
                string tail = t.Substring(sep + 2);
                if (tail.Length >= 100)
                {
                    return tail;
                }
            }
            // fallback: lay sau dau | cuoi
            int lastPipe = t.LastIndexOf('|');
            if (lastPipe >= 0 && t.Length - lastPipe > 200)
            {
                return t.Substring(lastPipe + 1);
            }
            // neu nguoi dung paste full "cookie:xxx" thi cat
            if (t.StartsWith("_|"))
            {
                int p = t.LastIndexOf('|');
                if (p >= 0 && p + 1 < t.Length)
                {
                    return t.Substring(p + 1);
                }
            }
            return t;
        }

        public static string ExtractVipCode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            int i = text.IndexOf("privateServerLinkCode=");
            if (i < 0)
            {
                return null;
            }
            string code = text.Substring(i + "privateServerLinkCode=".Length);
            int amp = code.IndexOf('&');
            if (amp >= 0)
            {
                code = code.Substring(0, amp);
            }
            code = code.Trim();
            return code == "" ? null : code;
        }

        // Lay accessCode cho VIP server tu trang game (can cookie cua acc duoc moi).
        public static bool TryGetPrivateAccessCode(string cookie, long placeId, string linkCode, out string accessCode, out string error)
        {
            accessCode = null;
            error = null;
            try
            {
                string url = "https://www.roblox.com/games/" + placeId.ToString() + "?privateServerLinkCode=" + linkCode;
                HttpWebRequest req = Create(url, cookie);
                req.Method = "GET";
                req.Accept = "text/html";
                req.Referer = "https://www.roblox.com/games/" + placeId.ToString();
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string html = sr.ReadToEnd();
                        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(
                            html, "Roblox\\.GameLauncher\\.joinPrivateGame\\(\\d+\\,\\s*'([\\w\\-]+)'");
                        if (m.Success && m.Groups.Count >= 2)
                        {
                            accessCode = m.Groups[1].Value;
                            return true;
                        }
                        error = "Link VIP sai/het han hoac acc khong duoc moi.";
                        return false;
                    }
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse r = wex.Response as HttpWebResponse;
                if (r != null)
                {
                    error = "VIP HTTP " + ((int)r.StatusCode).ToString();
                    try { r.Close(); } catch { }
                }
                else
                {
                    error = "Mang loi (co the bi chan roblox.com): " + wex.Message;
                }
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        // Mang VN chan www.roblox.com: kiem tra truoc de bao bat VPN.
        public static bool IsWwwReachable()
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.roblox.com/");
                req.Method = "HEAD";
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) RinAccountManager/1.0";
                req.Timeout = 7000;
                req.ReadWriteTimeout = 7000;
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    resp.Close();
                    return true;
                }
            }
            catch (WebException wex)
            {
                // Co HTTP response (ke ca 4xx/5xx) nghia la toi duoc, chi mang rot moi null.
                if ((wex.Response as HttpWebResponse) != null)
                {
                    try { wex.Response.Close(); } catch { }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Username -> UserId (cho join theo user).
        public static bool ResolveUserId(string username, out long userId, out string error)
        {
            userId = 0;
            error = null;
            string name = username.Trim().TrimStart('@');
            if (name == "")
            {
                error = "Chua nhap username.";
                return false;
            }
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://users.roblox.com/v1/usernames/users");
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Accept = "application/json";
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) RinAccountManager/1.0";
                req.Timeout = 15000;
                req.ReadWriteTimeout = 15000;
                string json = "{\"usernames\":[\"" + name.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"],\"excludeBannedUsers\":true}";
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                req.ContentLength = bytes.Length;
                using (Stream ws = req.GetRequestStream())
                {
                    ws.Write(bytes, 0, bytes.Length);
                }
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string body = sr.ReadToEnd();
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        Dictionary<string, object> d = ser.Deserialize<Dictionary<string, object>>(body);
                        System.Collections.ArrayList data = d["data"] as System.Collections.ArrayList;
                        if (data == null || data.Count == 0)
                        {
                            error = "Khong tim thay user @" + name + ".";
                            return false;
                        }
                        Dictionary<string, object> u0 = data[0] as Dictionary<string, object>;
                        userId = Convert.ToInt64(u0["id"]);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                error = "Mang loi: " + ex.Message;
                return false;
            }
        }

        public static RobloxUser GetAuthenticatedUser(string cookie)
        {
            RobloxUser u = new RobloxUser();
            u.Ok = false;
            try
            {
                HttpWebRequest req = Create("https://users.roblox.com/v1/users/authenticated", cookie);
                req.Method = "GET";
                req.Accept = "application/json";
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string body = sr.ReadToEnd();
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        try
                        {
                            var dict = ser.Deserialize<System.Collections.Generic.Dictionary<string, object>>(body);
                            object idObj;
                            object nameObj;
                            if (dict.TryGetValue("id", out idObj) && dict.TryGetValue("name", out nameObj))
                            {
                                u.Id = Convert.ToInt64(idObj);
                                u.Name = Convert.ToString(nameObj);
                                u.Ok = true;
                                return u;
                            }
                            u.Error = "Cookie het han hoac khong hop le.";
                            return u;
                        }
                        catch (Exception ex)
                        {
                            u.Error = "Parse user: " + ex.Message;
                            return u;
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse resp = wex.Response as HttpWebResponse;
                int code = 0;
                if (resp != null)
                {
                    try { code = (int)resp.StatusCode; } catch { }
                    try { resp.Close(); } catch { }
                }
                if (code == 401 || code == 403)
                {
                    u.Error = "cookie die (HTTP " + code.ToString() + ")";
                }
                else if (code > 0)
                {
                    u.Error = "loi mang (HTTP " + code.ToString() + ")";
                }
                else
                {
                    u.Error = "loi mang: " + wex.Message;
                }
                return u;
            }
            catch (Exception ex)
            {
                u.Error = ex.Message;
                return u;
            }
        }

        public static bool TryGetCsrfToken(string cookie, out string token, out string error)
        {
            token = null;
            error = null;
            try
            {
                HttpWebRequest req = Create("https://auth.roblox.com/v1/authentication-ticket/", cookie);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.ContentLength = 0;
                req.Referer = RefererGame;
                try
                {
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {
                        resp.Close();
                    }
                    error = "Roblox khong tra 403 nhu ky vong.";
                    return false;
                }
                catch (WebException wex)
                {
                    HttpWebResponse resp = wex.Response as HttpWebResponse;
                    if (resp != null)
                    {
                        try
                        {
                            if (resp.StatusCode == HttpStatusCode.Forbidden)
                            {
                                string t = resp.Headers["x-csrf-token"];
                                if (!string.IsNullOrEmpty(t))
                                {
                                    token = t;
                                    return true;
                                }
                                error = "Khong tim thay x-csrf-token.";
                                return false;
                            }
                            error = "CSRF status: " + ((int)resp.StatusCode).ToString() + " " + resp.StatusCode.ToString();
                            return false;
                        }
                        finally
                        {
                            try { resp.Close(); } catch { }
                        }
                    }
                    error = wex.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static bool TryGetAuthTicket(string cookie, string csrfToken, out string ticket, out string error)
        {
            ticket = null;
            error = null;
            try
            {
                HttpWebRequest req = Create("https://auth.roblox.com/v1/authentication-ticket/", cookie);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.ContentLength = 0;
                req.Headers["X-CSRF-TOKEN"] = csrfToken;
                req.Referer = RefererGame;
                try
                {
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {
                        ticket = resp.Headers["rbx-authentication-ticket"];
                        resp.Close();
                    }
                    if (!string.IsNullOrEmpty(ticket))
                    {
                        return true;
                    }
                    error = "Khong co rbx-authentication-ticket trong response.";
                    return false;
                }
                catch (WebException wex)
                {
                    HttpWebResponse resp = wex.Response as HttpWebResponse;
                    string body = "";
                    if (resp != null)
                    {
                        try
                        {
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                body = sr.ReadToEnd();
                            }
                        }
                        catch { }
                        try { resp.Close(); } catch { }
                    }
                    error = "Ticket failed: " + wex.Message + " " + body;
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
