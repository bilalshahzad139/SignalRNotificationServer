using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

using System.Threading.Tasks;
using PUCIT.AIMRL.NotificationServer.DAL;
using PUCIT.AIMRL.NotificationServer.Entities;

namespace PUCIT.AIMRL.NotificationServer
{
    public class MainNotificationHub : Hub
    {

        public void AgentConnect(Int64 employeeid, String appid)
        {
            try
            {
                String empConn = string.Format("{0}{1}{2}", employeeid, '_' , appid);

                GlobalDataManager._userIdentity.TryAdd(Context.ConnectionId, empConn);

                // add conection with reference to employee(key : 'employeeID_appID')
                IEnumerable<string> employeeConnections = GlobalDataManager._connections.GetConnections(empConn);

                if (!employeeConnections.Contains(Context.ConnectionId))
                    GlobalDataManager._connections.Add(empConn, Context.ConnectionId);

                Clients.Caller.loginResult(true);
            }
            catch (Exception ex)
            {
                PUCIT.AIMRL.Common.Logger.LogHandler.WriteLog("App", ex.ToString(), PUCIT.AIMRL.Common.Logger.LogType.ErrorMsg);
                Clients.Caller.loginResult(false);
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            String empConn;
            if (GlobalDataManager._userIdentity.TryGetValue(Context.ConnectionId, out empConn))
            {
                GlobalDataManager._connections.Remove(empConn, Context.ConnectionId);

                String removedConnectionEmployeeid;
                GlobalDataManager._userIdentity.TryRemove(Context.ConnectionId, out removedConnectionEmployeeid);
            }
            
            return base.OnDisconnected(stopCalled);
        }

        public void sendNotification(String appTo, String empTo, String message, Boolean showDesktopNotif, String extraDataAsJson)
        {
            String empConn = string.Format("{0}{1}{2}", empTo, '_', appTo);

            String pToEmployeeID = empConn;
          
            DateTime messageTime = DateTime.Now;
            string messageTimeString = messageTime.ToString("MM/dd/yyyy hh:mm tt");

            Int64 notificationId = SaveNotification(appTo,int.Parse(empTo), message, DateTime.UtcNow, extraDataAsJson);

            foreach (var connectionId in GlobalDataManager._connections.GetConnections(pToEmployeeID))
                Clients.Client(connectionId).addMessage(notificationId, empTo, message, messageTimeString, showDesktopNotif, extraDataAsJson);
            
        }

        public void markNotification(Int64 pNotificationID, string receiverAppID, int empID, Boolean isRead, Boolean markAll)
        {
            NotificationEngineDataService.UpdateNotificationReadStatus(pNotificationID, receiverAppID, empID, isRead, markAll, DateTime.UtcNow);
        }



        public Object getNotifcations(string appID, int empID, int max_notification_id)
        {
            var result = NotificationEngineDataService.GetNotifcations(appID,empID, max_notification_id);

            long maxId = 0;
            int unReadCount = 0;
            if (result.Count > 0)
            {
                maxId = result.Max(p => p.NotificationID);
                unReadCount = result.Count(p => p.IsRead == false);
            }

            return new
            {
                Notifications = result.Select(p => new
                {
                    NotificationID = p.NotificationID,
                    NotificationDetail = p.NotificationDetail,
                    IsRead = p.IsRead,
                    CreatedOn = p.CreatedOn.ToString("MM/dd/yyyy hh:mm tt"),
                    extraDataAsJson = p.extraDataAsJson
                }).ToList(),
                MaxID = maxId,
                UnReadCount = unReadCount
            };
        }


        #region HelperMethods
        private Int64 SaveNotification(string ReceiverAppId, int to, string message, DateTime messageTime, String extraDataAsJson)
        {
            RealTimeNotifications obj = new RealTimeNotifications();
            obj.NotificationID = 0;
            obj.receiverAppID = ReceiverAppId;
            obj.SenderID = 0;
            obj.ReceiverID = to;
            obj.NotificationDetail = message;
            obj.IsRead = false;
            obj.CreatedOn = messageTime;
            obj.extraDataAsJson = extraDataAsJson;

            var result = NotificationEngineDataService.SaveNotification(obj);
            return result;

        }
        #endregion

    }
}