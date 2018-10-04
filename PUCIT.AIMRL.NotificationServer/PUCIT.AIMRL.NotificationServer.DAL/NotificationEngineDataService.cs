using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data.Entity.Validation;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Common;
using System.Reflection;
using PUCIT.AIMRL.NotificationServer.Entities;
using PUCIT.AIMRL.Common.Logger;
using PUCIT.AIMRL.Common;

namespace PUCIT.AIMRL.NotificationServer.DAL
{
    public static class NotificationEngineDataService
    {
        public static TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

        static NotificationEngineDataService()
        {
            Database.SetInitializer<NotificationEngineDataContext>(null);
        }

        public static long SaveNotification(RealTimeNotifications obj)
        {
            try
            {
                using (var ctx = new NotificationEngineDataContext())
                {
                    var query = "execute [dbo].[usp_CreateAndUpdateNotifications] @pNotificationID, @senderAppID, @SenderID, @ReceiverAppID,  @ReceiverID, @NotificationDetail,@IsRead,@datetime,@extraDataAsJson";

                    var args = new DbParameter[] {
                    new SqlParameter { ParameterName = "@pNotificationID", Value = obj.NotificationID}, 
                                       
                    
                    new SqlParameter { ParameterName = "@senderAppID", Value = obj.SenderAppID},
                    new SqlParameter { ParameterName = "@SenderID", Value = obj.SenderID},

                        new SqlParameter { ParameterName = "@ReceiverAppID", Value = obj.ReceiverAppID},
                        new SqlParameter { ParameterName = "@ReceiverID", Value = obj.ReceiverID},

                    new SqlParameter { ParameterName = "@NotificationDetail", Value = obj.NotificationDetail},
                    new SqlParameter { ParameterName = "@isRead", Value = obj.IsRead},
                    new SqlParameter { ParameterName = "@datetime", Value = obj.CreatedOn},
                    new SqlParameter { ParameterName = "@extraDataAsJson", Value = obj.extraDataAsJson},
                };

                    var notificationID = ctx.Database.SqlQuery<long>(query, args).FirstOrDefault();
                    return notificationID;
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(MethodBase.GetCurrentMethod().Name, ex.Message, PUCIT.AIMRL.Common.Logger.LogType.ErrorMsg, ex);
                return 0;
            }

        }

        public static List<RealTimeNotifications> GetNotifcations(string appID,long empID, long max_notification_id)
        {
            try
            {
                using (var ctx = new NotificationEngineDataContext())
                {
                    var query = "execute [dbo].[usp_GetRealTimeNotification] @AppID , @EmployeeID , @maxNotificationId";

                    var args = new DbParameter[] {
                        new SqlParameter { ParameterName = "@AppID", Value = appID},
                    new SqlParameter { ParameterName = "@EmployeeID", Value = empID},
                    new SqlParameter { ParameterName = "@maxNotificationId", Value = max_notification_id},
                };

                    var data = ctx.Database.SqlQuery<RealTimeNotifications>(query, args).ToList();

                    foreach (var d in data)
                    {
                        d.CreatedOn = d.CreatedOn.ToTimeZoneTime(tzi);
                    }

                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(MethodBase.GetCurrentMethod().Name, ex.Message, PUCIT.AIMRL.Common.Logger.LogType.ErrorMsg, ex);
                return new List<RealTimeNotifications>();
            }

        }

        public static String UpdateNotificationReadStatus(String pNotificationID,string receiverAppID, long empID, Boolean isRead, Boolean markAll, DateTime dateTime)
        {
            try
            {
                using (var ctx = new NotificationEngineDataContext())
                {
                    var query = "execute dbo.UpdateNotificationReadStatus @pNotificationID,@ReceiverAppID,@empID, @isRead,@markAll, @dateTime";

                    var args = new DbParameter[] {
                    new SqlParameter { ParameterName = "@pNotificationID", Value = pNotificationID},
                    new SqlParameter { ParameterName = "@ReceiverAppID", Value = receiverAppID},
                    new SqlParameter { ParameterName = "@empID", Value = empID},
                    new SqlParameter { ParameterName = "@isRead", Value = isRead},
                    new SqlParameter { ParameterName = "@markAll", Value = markAll},
                    new SqlParameter { ParameterName = "@dateTime", Value = dateTime},
                };

                    var data = ctx.Database.SqlQuery<String>(query, args).FirstOrDefault();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(MethodBase.GetCurrentMethod().Name, ex.Message, PUCIT.AIMRL.Common.Logger.LogType.ErrorMsg, ex);
                return "";
            }
        }
        public static List<NotificationApplications> GetNotifcationApplications()
        {
            try
            {
                using (var ctx = new NotificationEngineDataContext())
                {
                    var query = "execute dbo.usp_GetNotificationApps";

                    var args = new DbParameter[] {
                    };

                    var data = ctx.Database.SqlQuery<NotificationApplications>(query, args).ToList();

                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteLog(MethodBase.GetCurrentMethod().Name, ex.Message, PUCIT.AIMRL.Common.Logger.LogType.ErrorMsg, ex);
                return new List<NotificationApplications>();
            }

        }


    }
}