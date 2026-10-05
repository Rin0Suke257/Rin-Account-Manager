using System;
using System.IO;
using System.Net;
using System.Text;

namespace RinAccountManager
{
    // Gui thong bao ve Discord webhook (best-effort, khong bao gio throw).
    public static class Webhook
    {
        static Webhook()
        {
            try
            {
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls;
            }
            catch { }
        }

        private static string Escape(string s)
        {
            if (s == null)
            {
                return "";
            }
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        }

        public static bool Send(string url, string content, string title, string desc, byte[] imageBytes)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            try
            {
                string payload = "{\"content\":\"" + Escape(content) + "\"";
                if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(desc))
                {
                    payload += ",\"embeds\":[{\"title\":\"" + Escape(title) + "\",\"description\":\"" + Escape(desc) + "\",\"color\":5790790,\"timestamp\":\"" + DateTime.UtcNow.ToString("o") + "\"";
                    if (imageBytes != null)
                    {
                        payload += ",\"image\":{\"url\":\"attachment://shot.png\"}";
                    }
                    payload += "}]";
                }
                payload += "}";
                if (imageBytes != null)
                {
                    PostMultipart(url, payload, imageBytes);
                }
                else
                {
                    PostJson(url, payload);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void PostJson(string url, string payload)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/json";
            req.UserAgent = "RinAccountManager/1.0";
            req.Timeout = 15000;
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            req.ContentLength = bytes.Length;
            using (Stream ws = req.GetRequestStream())
            {
                ws.Write(bytes, 0, bytes.Length);
            }
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                resp.Close();
            }
        }

        private static void PostMultipart(string url, string payload, byte[] imageBytes)
        {
            string boundary = "----RinBoundary" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "multipart/form-data; boundary=" + boundary;
            req.UserAgent = "RinAccountManager/1.0";
            req.Timeout = 30000;
            byte[] head1 = Encoding.UTF8.GetBytes("--" + boundary + "\r\nContent-Disposition: form-data; name=\"payload_json\"\r\nContent-Type: application/json\r\n\r\n");
            byte[] head2 = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"shot.png\"\r\nContent-Type: image/png\r\n\r\n");
            byte[] tail = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            byte[] payBytes = Encoding.UTF8.GetBytes(payload);
            req.ContentLength = head1.Length + payBytes.Length + head2.Length + imageBytes.Length + tail.Length;
            using (Stream ws = req.GetRequestStream())
            {
                ws.Write(head1, 0, head1.Length);
                ws.Write(payBytes, 0, payBytes.Length);
                ws.Write(head2, 0, head2.Length);
                ws.Write(imageBytes, 0, imageBytes.Length);
                ws.Write(tail, 0, tail.Length);
            }
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                resp.Close();
            }
        }

        public static string AccName(AppSettings s, Account a)
        {
            if (a == null)
            {
                return "?";
            }
            string alias = a.Alias == null ? "" : a.Alias.Trim();
            if (s != null && s.UseAlias && alias != "")
            {
                string note = a.Note == null ? "" : a.Note.Trim();
                if (note != "")
                {
                    return "[" + alias + "] (" + note + ")";
                }
                return "[" + alias + "]";
            }
            return a.Username;
        }
    }
}
