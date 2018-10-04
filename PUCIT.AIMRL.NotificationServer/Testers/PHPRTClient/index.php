<!DOCTYPE html>
<html>
<head>
    <title></title>
    <style>
        * {
            margin:0px;
            padding:0px;
        }
    </style>
    <meta charset="utf-8" />
    <link href="https://fonts.googleapis.com/icon?family=Material+Icons"
          rel="stylesheet">
    <script src="Scripts/jquery-3.2.1.min.js"></script>
    
    <script src="http://aim.pucit.edu.pk:8182/NotificationServer/scripts/NotificationLoader.js" id="scriptNotificationLoader"></script>
    <script>
        $(function () {
            var notification_args =
            {
                appID: '5ac078f7-618c-4a3c-8ffb-eb9d4e967a12',
                uniqueKey: '5ac078f7-618c-4a3c-8ffb-eb9d4e967a12',
                EID: '1', //This is User ID of current user
                notificationRecievedCallback: function (notObj) {
                    //This function is called on "Target" side to notify that a notification has come
                    alert(notObj.msg);
                },
                itemClickedCallback: function (notObj) {
                    //This function is called when user clicks on a notificaiton in panel
                    console.log(notObj);
                    if (notObj && notObj.ED && notObj.ED.ID) {
                        alert('Data Came with notification: ID=' + notObj.ED.ID + ", Name:" + notObj.ED.Name);
                    }
                    return false;
                }
            };

            //This line loads relevant JS & CSS files
            InitializeNotificationAPI.Initialize(notification_args);

            $("#btnTestNot").click(function () {
                //Here 2 is User ID of user you want to send notification.
                PUCIT.AIMRL.NotificationServerHandler.sendMessage(1, $("#txtMessage").val(), false, { ID: 100, Name: 'Test' });
                return false;
            });
        });
    </script>
</head>
<body>
    <h1>Welcome First User!</h1>

    Message:<input type="text" id="txtMessage" placeholder="Type your text here" />
    <button id="btnTestNot" value="Test Notification">Send Notification</button>
    <div style="position:absolute;left:500px;border:1px red solid;width:50px;">
        <div class="ib-st-notify" id="notifcation-dd">
        </div>
    </div>

</body>
</html>
