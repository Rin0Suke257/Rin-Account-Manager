using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace RinAccountManager
{
    [Serializable]
    public class SavedGame
    {
        public long PlaceId { get; set; }
        public string Name { get; set; }
        public string ThumbUrl { get; set; }
        public bool IsVip { get; set; }
        public string VipLink { get; set; }
    }

    [Serializable]
    public class RecentGame
    {
        public long PlaceId { get; set; }
        public string Name { get; set; }
    }

    [Serializable]
    public class GameData
    {
        public List<SavedGame> Saved { get; set; }
        public List<RecentGame> Recent { get; set; }
    }

    public static class GamesStore
    {
        static GamesStore()
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

        private static string DataPath()
        {
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RinAccountManager");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.Combine(dir, "games.json");
        }

        public static string ThumbDir()
        {
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RinAccountManager", "thumbs");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        public static GameData Load()
        {
            GameData d = new GameData();
            d.Saved = new List<SavedGame>();
            d.Recent = new List<RecentGame>();
            try
            {
                string path = DataPath();
                if (!File.Exists(path))
                {
                    return d;
                }
                string json = File.ReadAllText(path, Encoding.UTF8);
                JavaScriptSerializer ser = new JavaScriptSerializer();
                GameData loaded = ser.Deserialize<GameData>(json);
                if (loaded != null)
                {
                    if (loaded.Saved != null)
                    {
                        d.Saved = loaded.Saved;
                    }
                    if (loaded.Recent != null)
                    {
                        d.Recent = loaded.Recent;
                    }
                }
            }
            catch { }
            return d;
        }

        public static void Save(GameData d)
        {
            try
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                File.WriteAllText(DataPath(), ser.Serialize(d), Encoding.UTF8);
            }
            catch { }
        }

        private static string Get(string url, out string error)
        {
            error = null;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.Accept = "application/json";
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) RinAccountManager/1.0";
                req.Timeout = 15000;
                req.ReadWriteTimeout = 15000;
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse r = wex.Response as HttpWebResponse;
                if (r != null)
                {
                    error = "HTTP " + ((int)r.StatusCode).ToString();
                    try { r.Close(); } catch { }
                }
                else
                {
                    error = wex.Message;
                }
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        private static Dictionary<string, object> ParseObj(string json)
        {
            try
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                return ser.Deserialize<Dictionary<string, object>>(json);
            }
            catch
            {
                return null;
            }
        }

        // Lay ten + thumbnail tu PlaceId. Tra false + error neu ID sai.
        public static bool FetchInfo(long placeId, out string name, out string thumbUrl, out string error)
        {
            name = null;
            thumbUrl = null;
            error = null;
            string e1;
            string u = Get("https://apis.roblox.com/universes/v1/places/" + placeId.ToString() + "/universe", out e1);
            if (u == null)
            {
                error = "PlaceId khong ton tai (" + e1 + ").";
                return false;
            }
            Dictionary<string, object> ud = ParseObj(u);
            object uid;
            if (ud == null || !ud.TryGetValue("universeId", out uid))
            {
                error = "Khong doc duoc universe.";
                return false;
            }
            long universeId = Convert.ToInt64(uid);
            string e2;
            string g = Get("https://games.roblox.com/v1/games?universeIds=" + universeId.ToString(), out e2);
            if (g == null)
            {
                error = "Khong lay duoc ten game (" + e2 + ").";
                return false;
            }
            try
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                Dictionary<string, object> gd = ser.Deserialize<Dictionary<string, object>>(g);
                System.Collections.ArrayList data = gd["data"] as System.Collections.ArrayList;
                if (data == null || data.Count == 0)
                {
                    error = "Game khong ton tai.";
                    return false;
                }
                Dictionary<string, object> g0 = data[0] as Dictionary<string, object>;
                name = Convert.ToString(g0["name"]);
            }
            catch
            {
                error = "Khong doc duoc ten game.";
                return false;
            }
            string e3;
            string t = Get("https://thumbnails.roblox.com/v1/games/icons?universeIds=" + universeId.ToString() + "&size=128x128&format=Png&isCircular=false", out e3);
            if (t != null)
            {
                try
                {
                    JavaScriptSerializer ser2 = new JavaScriptSerializer();
                    Dictionary<string, object> td = ser2.Deserialize<Dictionary<string, object>>(t);
                    System.Collections.ArrayList tdata = td["data"] as System.Collections.ArrayList;
                    if (tdata != null && tdata.Count > 0)
                    {
                        Dictionary<string, object> t0 = tdata[0] as Dictionary<string, object>;
                        thumbUrl = Convert.ToString(t0["imageUrl"]);
                    }
                }
                catch { }
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "Game " + placeId.ToString();
            }
            return true;
        }

        public static string ThumbPath(long placeId)
        {
            return Path.Combine(ThumbDir(), placeId.ToString() + ".png");
        }

        public static bool DownloadThumb(string url, long placeId)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.UserAgent = "Mozilla/5.0";
                req.Timeout = 15000;
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        resp.GetResponseStream().CopyTo(ms);
                        File.WriteAllBytes(ThumbPath(placeId), ms.ToArray());
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static Image LoadThumb(long placeId)
        {
            try
            {
                string p = ThumbPath(placeId);
                if (!File.Exists(p))
                {
                    return null;
                }
                using (Image src = Image.FromFile(p))
                {
                    return new Bitmap(src);
                }
            }
            catch
            {
                return null;
            }
        }

        public static void AddRecent(GameData d, long placeId, string name)
        {
            for (int i = d.Recent.Count - 1; i >= 0; i--)
            {
                if (d.Recent[i].PlaceId == placeId)
                {
                    d.Recent.RemoveAt(i);
                }
            }
            RecentGame r = new RecentGame();
            r.PlaceId = placeId;
            r.Name = string.IsNullOrEmpty(name) ? ("Game " + placeId.ToString()) : name;
            d.Recent.Insert(0, r);
            while (d.Recent.Count > 10)
            {
                d.Recent.RemoveAt(d.Recent.Count - 1);
            }
            Save(d);
        }
    }
}
