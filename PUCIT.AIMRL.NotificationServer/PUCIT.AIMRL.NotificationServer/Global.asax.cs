using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace PUCIT.AIMRL.NotificationServer
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            PUCIT.AIMRL.Common.Logger.LogHandler.ConfigureLogger(Server.MapPath("~/logging.config"));
            PUCIT.AIMRL.Common.Logger.LogHandler.WriteLog("App", "Application Starting", PUCIT.AIMRL.Common.Logger.LogType.ErrorMsg);
            PUCIT.AIMRL.Common.EncryptDecryptUtility.SetParameters("pUcITAIMRLARRRPRojecT", "PUCITAImRLARRRPRojECT", "MD5", 50, "aImrLpucitorerpj", 256);

            //var encString = PUCIT.AIMRL.Common.EncryptDecryptUtility.Encrypt(@"Data Source=.\SQLEXPRESS2012;Initial Catalog=AimNotificationServer;Integrated Security=True;");
            //NotificationServer.DAL.NotificationEngineDataService.GetNotifcations("1", 1, 0);

            GlobalDataManager.NotificationApplicationsList =  NotificationServer.DAL.NotificationEngineDataService.GetNotifcationApplications();

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}