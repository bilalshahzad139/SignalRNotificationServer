using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUCIT.AIMRL.NotificationServer.Entities
{
    public class RealTimeNotifications
    {
        public long NotificationID { get; set; }
        public int senderAppID { get; set; }
        public string receiverAppID { get; set; }
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string NotificationDetail { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadOn { get; set; }

        public String extraDataAsJson { get; set; }
    }

    public class ConnectionMapping<T>
    {
        private readonly Dictionary<T, HashSet<string>> _connections =
            new Dictionary<T, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public void Add(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }

        public Dictionary<T, HashSet<string>> GetAllConnections()
        {
            return _connections;
        }
    }
}
