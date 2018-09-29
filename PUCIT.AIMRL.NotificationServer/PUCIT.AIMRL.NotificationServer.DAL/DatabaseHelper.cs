using System;
using System.Configuration;

namespace PUCIT.AIMRL.NotificationServer.DAL
{    
    internal static class DatabaseHelper
    {
        private static readonly object SyncRoot = new Object();
        private static readonly Boolean IsCSEncrypted = false;
        public static string MainDBConnectionString
        {
            get;
            private set;
        }
       
        static DatabaseHelper()
        {
            String connStr = ConfigurationManager.ConnectionStrings["NotificationDBConnectionString"].ConnectionString;

            bool flag = false;
            Boolean.TryParse(ConfigurationManager.AppSettings["IsCSEncrypted"], out flag);
            IsCSEncrypted = flag;

            if (IsCSEncrypted)
            {
                connStr = Common.EncryptDecryptUtility.Decrypt(connStr);
            }
            MainDBConnectionString = connStr;
        }
                
        
    }
}