# SignalRNotificationServer
It is generic SignalR based Notification Server which can be easily integrated with your web application. 

Tools: Visual Studio 2015, .NET Framework 4.6.1, SignalR 2.3.0

What is in this repository?
--------------
1) SignalR Based Notification Server
2) A Test website with simple HTML page. You can use same approach in other type of applications (PHP, ASP.NET MVC etc.)

Steps to Setup 
------------
1) Create another database (for example "NotificationServer")
2) Execute "\PUCIT.AIMRL.NotificationServer\DBScripts\DB_Script.txt" on above DB to create objects for notification.
3) Change connection string "PUCIT.AIMRL.NotificationServer\PUCIT.AIMRL.NotificationServer\web.config"

-----------------

4) Right click on solution -> Properties -> Choose Multiple Startup Projects -> Select "Start" against "PUCIT.AIMRL.NotificationServer" & against "SimpleTestWebSite"

5) Rebuild the solution & Run the application.

6) Test Website url is (http://localhost:29647). Open http://localhost:29647/first.html.
-----------------

To send realtime notification, you can use following JS function

Signature of sendMessage: (empTo, message, showDesktopNotif, extraData)
Here empTo: UserID to whom you want to send notification.
-------
message: Message you want to show in notificaiton panel.
-------
showDesktopNotif: Do you want to show message in Desktop notification.
-------
extraData: If you want to send any data & want to use it on other side.

-----------------

You may check in first.html, Following example is available

PUCIT.AIMRL.NotificationServerHandler.sendMessage(1, "hello world", false,{});
