var InitializeNotificationAPI = {};
InitializeNotificationAPI.Initialize = function (notification_args, callbackfunction) {
    
    var version = 1;
    //debugger;
    var notificationServerBasePath = '';
    var p = document.getElementById('scriptNotificationLoader');
    if (p) {
        notificationServerBasePath = p.getAttribute('src');
        notificationServerBasePath = notificationServerBasePath.replace('/scripts/NotificationLoader.js', '');
    }
    if (notificationServerBasePath == '') {
        console.log('There is some problem with notificaiton loader script configuration');
        return;
    }
    var cssPath = notificationServerBasePath + "/content/PUCIT.AIMRL.NotificationServerHandler.css?v=" + version;
    var css = document.createElement('link');
    css.setAttribute('rel', "stylesheet");
    css.setAttribute('href', cssPath);
    document.head.appendChild(css);

    var jqueryPath = notificationServerBasePath + '/scripts/jquery.signalR-2.3.0.min.js';
    var s = document.createElement('script');
    s.setAttribute('src', jqueryPath);
    s.onload = function () {
        console.log('jquery.signalr loaded');
        //Load Hub file
        var hubfilePath = notificationServerBasePath + '/aimrlsignalr/hubs';
        var s1 = document.createElement('script');
        s1.setAttribute('src', hubfilePath);

        s1.onload = function () {
            console.log('hub file loaded');

            //Load Notification API
            var notificationhandlePath = notificationServerBasePath + '/scripts/PUCIT.AIMRL.NotificationServerHandler.js?v=' + version;
            var s2 = document.createElement('script');
            s2.setAttribute('src', notificationhandlePath);
            s2.onload = function () {
                console.log('NotificationServerHandler.js loaded');
                
                if (notification_args)
                {
                }
                else
                {
                    notification_args = {};
                    
                }
                notification_args.hubURL = hubfilePath;
                notification_args.appBasePath = notificationServerBasePath;

                PUCIT.AIMRL.NotificationServerHandler.initialise(notification_args);

                if (callbackfunction)
                    callbackfunction();
            };
            document.head.appendChild(s2);
        };
        document.head.appendChild(s1);


    }
    document.head.appendChild(s);
}