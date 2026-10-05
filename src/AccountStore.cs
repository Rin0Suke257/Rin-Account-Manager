using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Security.Cryptography;

namespace RinAccountManager
{
    [Serializable]
    public class Account
    {
        public string Alias { get; set; }
        public string Cookie { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public string BrowserTrackerId { get; set; }
        public string Note { get; set; }
        public string Live { get; set; }

        public Account()
        {
            Alias = "";
            Cookie = "";
            UserId = 0;
            Username = "";
            BrowserTrackerId = new Random().Next(100000, 175000).ToString() + new Random().Next(100000, 900000).ToString();
            Note = "";
            Live = "";
        }
    }

    public static class AccountStore
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
            return Path.Combine(dir, "accounts.dat");
        }

        public static List<Account> Load()
        {
            try
            {
                string path = DataPath();
                if (!File.Exists(path))
                {
                    return new List<Account>();
                }
                byte[] protectedBytes = File.ReadAllBytes(path);
                byte[] jsonBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                string json = Encoding.UTF8.GetString(jsonBytes);
                JavaScriptSerializer ser = new JavaScriptSerializer();
                List<Account> list = ser.Deserialize<List<Account>>(json);
                if (list == null)
                {
                    return new List<Account>();
                }
                return list;
            }
            catch
            {
                return new List<Account>();
            }
        }

        public static bool Save(List<Account> accounts, out string error)
        {
            error = null;
            try
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                string json = ser.Serialize(accounts);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                byte[] protectedBytes = ProtectedData.Protect(jsonBytes, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(DataPath(), protectedBytes);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static string DataFilePath()
        {
            return DataPath();
        }
    }
}
