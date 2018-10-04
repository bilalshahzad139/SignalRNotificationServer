using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Cors;
using Microsoft.AspNet.SignalR;
using System.Web.Cors;

[assembly: OwinStartup(typeof(PUCIT.AIMRL.NotificationServer.Startup))]

namespace PUCIT.AIMRL.NotificationServer
{
    public class Startup
    {

        //To allow specific origins, we've to use this code
        //https://github.com/DamianEdwards/SignalR-2.x-demo/blob/master/SignalR2x/SelfHost/Startup.cs
        private static Lazy<CorsOptions> _corsOptions = new Lazy<CorsOptions>(() =>
        {
            return new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = context =>
                    {
                        var policy = new CorsPolicy();
                        // Only allow CORS requests from the non-SSL version of my site

                        foreach(var a in GlobalDataManager.NotificationApplicationsList)
                        {
                            policy.Origins.Add(a.AppBaseUrl);
                        }

                        policy.AllowAnyMethod = true;
                        policy.AllowAnyHeader = true;
                        policy.SupportsCredentials = true;
                        return Task.FromResult(policy);
                    }
                }
            };
        });

        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            
            app.Map("/aimrlsignalr", map =>
            {
                // Setup the CORS middleware to run before SignalR.
                // By default this will allow all origins. You can 
                // configure the set of origins and/or http verbs by
                // providing a cors options with a different policy.
                
                //map.UseCors(CorsOptions.AllowAll);
                map.UseCors(_corsOptions.Value);

                var hubConfiguration = new HubConfiguration
                {
                    // You can enable JSONP by uncommenting line below.
                    // JSONP requests are insecure but some older browsers (and some
                    // versions of IE) require JSONP to work cross domain
                    EnableJSONP = true
                };
                // Run the SignalR pipeline. We're not using MapSignalR
                // since this branch already runs under the "/signalr"
                // path.
                map.RunSignalR(hubConfiguration);
            });
            
        }
    }
}
