using PUCIT.AIMRL.NotificationServer.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace PUCIT.AIMRL.NotificationServer
{
    public static class GlobalDataManager
    {
        // Connections
        public readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();
        public readonly static ConcurrentDictionary<string, CustomConnectionData> _userIdentity = new ConcurrentDictionary<string, CustomConnectionData>();
        
        public static List<NotificationApplications> NotificationApplicationsList { get; set; }
    }
}