using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace RinAccountManager
{
    [Serializable]
    public class AppSettings
    {
        public string WebhookUrl { get; set; }
        public bool EvJoin { get; set; }
        public bool EvFail { get; set; }
        public bool EvRelogin { get; set; }
        public bool EvDie { get; set; }
        public bool ShotEnabled { get; set; }
        public int ShotDelaySec { get; set; }
        public int ReloginMax { get; set; }
        public int ReloginGapSec { get; set; }
        public bool UseAlias { get; set; }
        public bool ReloginEnabled { get; set; }
        public bool AfkEnabled { get; set; }
        public int AfkMinutes { get; set; }

        public AppSettings()
        {
            WebhookUrl = "";
            EvJoin = true;
            EvFail = true;
            EvRelogin = true;
            EvDie = true;
            ShotEnabled = true;
            ShotDelaySec = 45;
            ReloginMax = 5;
            ReloginGapSec = 15;
            UseAlias = true;
            ReloginEnabled = false;
            AfkEnabled = false;
            AfkMinutes = 15;
        }
    }

    public static class SettingsStore
    {
        private static string DataPath()
        {
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RinAccountManager");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.Combine(dir, "settings.json");
        }

        public static AppSettings Load()
        {
            AppSettings d = new AppSettings();
            try
            {
                string path = DataPath();
                if (!File.Exists(path))
                {
                    return d;
                }
                string json = File.ReadAllText(path, Encoding.UTF8);
                JavaScriptSerializer ser = new JavaScriptSerializer();
                AppSettings loaded = ser.Deserialize<AppSettings>(json);
                if (loaded != null)
                {
                    d = loaded;
                }
            }
            catch { }
            if (d.ShotDelaySec < 10)
            {
                d.ShotDelaySec = 10;
            }
            if (d.ShotDelaySec > 300)
            {
                d.ShotDelaySec = 300;
            }
            if (d.ReloginMax < 1)
            {
                d.ReloginMax = 1;
            }
            if (d.ReloginMax > 20)
            {
                d.ReloginMax = 20;
            }
            if (d.ReloginGapSec < 5)
            {
                d.ReloginGapSec = 5;
            }
            return d;
        }

        public static void Save(AppSettings d)
        {
            try
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                File.WriteAllText(DataPath(), ser.Serialize(d), Encoding.UTF8);
            }
            catch { }
        }
    }
}
