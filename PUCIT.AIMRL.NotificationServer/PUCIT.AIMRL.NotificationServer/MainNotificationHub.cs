using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

using System.Threading.Tasks;
using PUCIT.AIMRL.NotificationServer.DAL;
using PUCIT.AIMRL.NotificationServer.Entities;
using PUCIT.AIMRL.Common;

namespace PUCIT.AIMRL.NotificationServer
{
    public class MainNotificationHub : Hub
    {
        public override Task OnConnected()
        {
            try
            {
                var employeeid = Convert.ToInt64(Context.QueryString["employeeid"]);
                var appid = Context.QueryString["appid"];
                var requesterAppUrl = Context.Headers["Origin"];

                /*
                 Perform Employee ID, App ID validation, URL validation 
                */

                var obj = GlobalDataManager.NotificationApplicationsList.Where(a => a.AppId == appid && a.AppBaseUrl == requesterAppUrl).FirstOrDefault();
                if(obj == null)
                {
                    Clients.Caller.stopConnection("Invalid Request or Unregistered App!");
                    return null;
                }

                

                var empConn = new CustomConnectionData() { UserID = employeeid, AppID = appid };

                GlobalDataManager._userIdentity.TryAdd(Context.ConnectionId, empConn);

                // add conection with reference to employee(key : 'employeeID_appID')
                IEnumerable<string> employeeConnections = GlobalDataManager._connections.GetConnections(empConn.ToString());

                if (!employeeConnections.Contains(Context.ConnectionId))
                    GlobalDataManager._connections.Add(empConn.ToString(), Context.ConnectionId);
            }
            catch (Exception ex)
            {
                PUCIT.AIMRL.Common.Logger.LogHandler.WriteLog("App", ex.ToString(), PUCIT.AIMRL.Common.Logger.LogType.ErrorMsg);
                Clients.Caller.stopConnection("Invalid Request");
            }

            return base.OnConnected();
        }
        public void AgentConnect(Int64 employeeid, String appid)
        {
            //try
            //{

            //    var requesterAppUrl = Context.Headers["Origin"];

            //    /*
            //     Perform Employee ID, App ID validation, URL validation 
            //    */

            //    var empConn = new CustomConnectionData() { UserID = employeeid,AppID=appid };

            //    GlobalDataManager._userIdentity.TryAdd(Context.ConnectionId, empConn);

            //    // add conection with reference to employee(key : 'employeeID_appID')
            //    IEnumerable<string> employeeConnections = GlobalDataManager._connections.GetConnections(empConn.ToString());

            //    if (!employeeConnections.Contains(Context.ConnectionId))
            //        GlobalDataManager._connections.Add(empConn.ToString(), Context.ConnectionId);

            //    /*
            //     * Return connection ID to client so this connection ID should come in further requests
            //     */
            //    Clients.Caller.loginResult(true);
            //}
            //catch (Exception ex)
            //{
            //    PUCIT.AIMRL.Common.Logger.LogHandler.WriteLog("App", ex.ToString(), PUCIT.AIMRL.Common.Logger.LogType.ErrorMsg);
            //    Clients.Caller.loginResult(false);
            //}
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var empConn = new CustomConnectionData();
            if (GlobalDataManager._userIdentity.TryGetValue(Context.ConnectionId, out empConn))
            {
                GlobalDataManager._connections.Remove(empConn.ToString(), Context.ConnectionId);

                GlobalDataManager._userIdentity.TryRemove(Context.ConnectionId, out empConn);
            }
            
            return base.OnDisconnected(stopCalled);
        }

        public void sendNotification(long empTo, String message, Boolean showDesktopNotif, String extraDataAsJson)
        {
            //Sender Connection

            var srcConnData = new CustomConnectionData();
            if (!GlobalDataManager._userIdentity.TryGetValue(Context.ConnectionId, out srcConnData))
            {
                return;
            }

            var targetConnData = new CustomConnectionData() { UserID = empTo, AppID = srcConnData.AppID };

            DateTime messageTime = DateTime.UtcNow;
            string messageTimeString = messageTime.ToTimeZoneTime(NotificationEngineDataService.tzi).ToString("MM/dd/yyyy hh:mm tt");
            String uniqueNotificationID = "";
            long newID = SaveNotification(srcConnData, targetConnData, message, messageTime, extraDataAsJson, out uniqueNotificationID);

            if(newID > 0) { 
                foreach (var connectionId in GlobalDataManager._connections.GetConnections(targetConnData.ToString()))
                    Clients.Client(connectionId).addMessage(uniqueNotificationID, newID, message, messageTimeString, showDesktopNotif, extraDataAsJson);
            }
            else
            {
                return;
            }
        }

        public void markNotification(String pNotificationID, Boolean isRead, Boolean markAll)
        {
            var myConnData = new CustomConnectionData();
            if (!GlobalDataManager._userIdentity.TryGetValue(Context.ConnectionId, out myConnData))
            {
                return;
            }
            NotificationEngineDataService.UpdateNotificationReadStatus(pNotificationID, myConnData.AppID, myConnData.UserID, isRead, markAll, DateTime.UtcNow);
        }

        public Object getNotifcations(long max_notification_id)
        {
            var myConnData = new CustomConnectionData();
            if (!GlobalDataManager._userIdentity.TryGetValue(Context.ConnectionId, out myConnData))
            {
                return null;
            }

            var result = NotificationEngineDataService.GetNotifcations(myConnData.AppID, myConnData.UserID, max_notification_id);

            long maxId = 0;
            int unReadCount = 0;
            if (result.Count > 0)
            {
                maxId = result.Max(p => p.ID);
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
        private long SaveNotification(CustomConnectionData src,CustomConnectionData target, string message, DateTime messageTime, String extraDataAsJson, out String uniqueNotificationID)
        {
            uniqueNotificationID = Guid.NewGuid().ToString();
            RealTimeNotifications obj = new RealTimeNotifications();
            obj.NotificationID = uniqueNotificationID;
            obj.SenderAppID = src.AppID;
            obj.SenderID = src.UserID;

            obj.ReceiverAppID = target.AppID;
            obj.ReceiverID = target.UserID;

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