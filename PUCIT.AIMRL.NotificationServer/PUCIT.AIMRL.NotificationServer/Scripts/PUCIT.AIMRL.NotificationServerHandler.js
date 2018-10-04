var PUCIT = {};
PUCIT.AIMRL = {};
PUCIT.AIMRL.NotificationServerHandler = (function () {

    var _options;
    var _mainNotificationHub = {};

    var notifciationstrip = $("#notification-panel");

    var _localStorageSubKey = "Notifications";
    var _NotificationLocalStorageData = {};
    var _notificationPanel;
    var _hubUrl = '';
    var $mainContainer = null;
    var _baseKey = '';
    var _desktopNotificationSupport = true;
    var _stoppedFromServerSide = false;

    function config(args) {
        var _defaults = {};
        _defaults.appID = '';
        _defaults.uniqueKey = '';
        _defaults.mainContainerSelector = '#notifcation-dd';
        _defaults.EID = '';
        _defaults.hubURL = _hubUrl;
        _defaults.encEid = 0;
        _defaults.defaultNotificationCount = 5;
        _defaults.appBasePath = '';
        _defaults.useBase64 = true;
        _defaults.desktopNotIconPath = 'Content/images/aim-fav.png';
        _defaults.notificationRecievedCallback = function (notObj) {
            //Coaching.UI.showRoasterMessage(message, Enums.MessageType.Info);
        }
        _defaults.itemClickedCallback = function (notObj) {
            //console.log('item is clicked' + notObj.notificationId);
        }

        _baseKey = _defaults.appID + "_" + _defaults.uniqueKey;

        if (args)
            _options = $.extend(_defaults, args);
        else 
            _options = $.extend(_defaults, {});

        $mainContainer = $(_options.mainContainerSelector);
    }

    function init(args) {
        
        config(args);

        //Generate HTML of Main Container if container is empty
        $mainContainer.addClass('ib-st-notify');

        if ($mainContainer.find('*').length == 0) {
            
            var content = 
                '<div class="ib-notifybtn-container"> ' +
                    '<button class="ib-notify-btn" id="dev_bellicon"> ' +
                        '<i class="material-icons" >&#xE7F4;</i> ' +
                    '</button> ' +
                    '<span class="ib-counter" id="dev_notif_count">0</span> ' +
                '</div> ' +
                '<div class="ib-stdnotification-dropdown" style="display:none"> ' +
                    '<div class="ib-notify-drop-title"> ' +
                        '<div class=""> ' +
                            '<div class="ib-pull-left">Notification</div> ' +
                            '<div class="ib-pull-right"> ' +
                                '<a href="#" id="MarkAllAsRead">Mark all as read</a> ' +
                            '</div> ' +
                        '</div> ' +
                    '</div> ' +
                    '<div class="ib-drop-content dev_notifications"> ' +
                        '<div class="ib-drop-footer dev_showmore"> ' +
                            '<a href="#" id="loadMoreNotiifications"><i class="fa fa-eye"></i> Show more </a> ' +
                        '</div> ' +
                    '</div> ' +
                '</div>';

            $mainContainer.append(content);
        }

        //It will load datat in _NotificationLocalStorageData object
        LoadDataFromLocalStorage();

        //If there is no data available, then initialize it with an empty object
        if (!_NotificationLocalStorageData) {
            _NotificationLocalStorageData = {
                unreadCount: 0,
                maxNotificationId: 0,
                history: []
            };
            SaveDataInLocalStorage();
        }

        _notificationPanel = $('div.dev_notifications');

        $mainContainer.find('#dev_bellicon').unbind("click").bind("click", function (e) {
            e.stopPropagation();
            $mainContainer.find(".ib-stdnotification-dropdown").toggle();
            return false;
        });

        $mainContainer.find('div.dev_notifications div.ib-notify-item-group').remove();

        $mainContainer.find('#MarkAllAsRead').unbind("click").bind("click", function (e) {
            e.stopPropagation();
            makeAllUnRead();
            _mainNotificationHub.server.markNotification(_NotificationLocalStorageData.maxNotificationId,true, true);
            return false;
        });

        $mainContainer.find('#loadMoreNotiifications').unbind("click").bind("click", function (e) {
            e.preventDefault();

            loadNextNotitifications();

            return false;
        });

        $("body").click(function () {
            $mainContainer.find(".ib-stdnotification-dropdown").hide();
        });

        $mainContainer.find('.ib-stdnotification-dropdown').on({
            "click": function (e) {
                e.stopPropagation();
            }
        });

        initChatModule();
    }

    function initChatModule() {

        $.connection.hub.logging = true;

        // Reference the auto-generated proxy for the hub.
        $.connection.hub.url = _options.hubURL;
        _mainNotificationHub = $.connection.mainNotificationHub;

        // Create a function that hub can call back to load message at recevier side
        _mainNotificationHub.client.addMessage = function (notifciationId,newID, message, msgTime, showDesktopNotif, extraData) { //, msgTime) {

            try {
                extraData = JSON.parse(extraData);
            } catch (e) {
                extraData = {};
            }

            var msgObj = { notificationId: notifciationId, msg: message, msgTime: msgTime, isRead: false, DN: showDesktopNotif, ED: extraData };

            manageMessageInNotifications(msgObj, newID);

            if (showDesktopNotif) {
                ShowDesktopNotification(notifciationId, message, msgTime);
            }

            if (_options.notificationRecievedCallback) {
                _options.notificationRecievedCallback(msgObj);
            }

            return;
        }

        // Create a function that hub can call back after agent successfully logged into the website
        _mainNotificationHub.client.stopConnection = function (message) {
            _stoppedFromServerSide = true;
            $.connection.hub.stop();
            if (message)
                alert(message);

            //if (success == true) {
            //    loadNotifcationFromServer();
            //}
            //else
            //    alert('Error occurred');
        };

        // Methods for error handling
        $.connection.hub.disconnected(function () {
            if ($.connection.hub.lastError) {
                console.log($.connection.hub.lastError.message);
                showConnectionError('Disconnected ... ');
            }
            if (_stoppedFromServerSide == false) {
                setTimeout(function () {
                    showConnectionError('Reconnecting ...');
                    startConnection();
                }, 4000); // Re-start connection after 4 seconds
            }
        });

        $.connection.hub.connectionSlow(function () {
            showConnectionError('We are currently experiencing difficulties with the connection.');
        });

        $.connection.hub.reconnecting(function () {
            showConnectionError('Reconnecting ...');
        });

        $.connection.hub.reconnected(function () {
            startConnection();
        });

        startConnection();
    }

    function showConnectionError(errorMsg) {
        console.log("Connection Error(s)!", errorMsg, "error");
    }

    function startConnection() {

        $.connection.hub.qs = "employeeid=" + _options.EID + "&appid=" + _options.appID;
        
        $.connection.hub.start()
            .done(function () {
                loadNotifcationFromServer();
                //_mainNotificationHub.server.agentConnect(_options.EID, _options.appID);
            })
            .fail(function () {
                showConnectionError('Could not connect');
            });
    }

    function makeAllUnRead() {
        $(".dev_notifications div.ib-notify-item-group").removeClass('unread');
        //$(".dev_notifications li i").removeClass('checked')

        for (var i = 0; i < _NotificationLocalStorageData.history.length; i++) {

            if (_NotificationLocalStorageData.history[i].isRead == false) {
                _NotificationLocalStorageData.history[i].isRead = true;
            }
        }
        _NotificationLocalStorageData.unreadCount = 0;

        $("#dev_notif_count").text(_NotificationLocalStorageData.unreadCount);
        SaveDataInLocalStorage();
    }

    function manageMessageInNotifications(msgObj,newID) {

        //var msgObj = { notificationId: notifciationId, msg: msg, msgTime: msgTime, isRead: false, DN: showDesktopNotif, ED: extraData };

        _NotificationLocalStorageData.unreadCount++;
        _NotificationLocalStorageData.maxNotificationId = newID,
        _NotificationLocalStorageData.history.unshift(msgObj)

        SaveDataInLocalStorage();
        showMessageInNotificationBox([msgObj], true);

    }

    function loadNotifcationFromServer() {

        var employee_id = _options.EID;

        _mainNotificationHub.server.getNotifcations(_NotificationLocalStorageData.maxNotificationId).then(function (res) {
            /*
            ----What is res---------------
            res = return new
            {
                Notifications = result.Select(p => new
                {
                    NotificationID = p.NotificationID,
                    NotificationDetail = p.NotificationDetail,
                    IsRead = p.IsRead,
                    CreatedOn = p.CreatedOn.ToString("MM/dd/yyyy hh:mm tt")
                }).ToList(),
                MaxID = maxId,
                UnReadCount = unReadCount
            };
            */

            if (res) {

                var newArr = [];

                if (res.Notifications.length > 0) {
                    _NotificationLocalStorageData.unreadCount = _NotificationLocalStorageData.unreadCount + res.UnReadCount;
                    _NotificationLocalStorageData.maxNotificationId = res.MaxID;
                    var extraData;

                    for (var i = 0; i < res.Notifications.length; i++) {

                        try {
                            extraData = JSON.parse(res.Notifications[i].extraDataAsJson);
                        } catch (e) {
                            extraData = {};
                        }

                        var obj = {
                            notificationId: res.Notifications[i].NotificationID,
                            msg: res.Notifications[i].NotificationDetail,
                            msgTime: res.Notifications[i].CreatedOn,
                            isRead: res.Notifications[i].IsRead,
                            ED: extraData,
                        };

                        newArr.push(obj);
                    }
                    if (newArr.length > 0) {
                        for (var i = newArr.length - 1; i >= 0; i--) {
                            _NotificationLocalStorageData.history.unshift(newArr[i]);
                        }
                        SaveDataInLocalStorage();
                    }
                }//end of notifications came
            }//end of if

            FindChunkAndShow(0, _options.defaultNotificationCount, true);

            //var arr = _NotificationLocalStorageData.history.slice(0, _options.defaultNotificationCount);
            //showMessageInNotificationBox(arr, true);
        });
    }//end of loadNotifcationFromServer


    //This is called when 'Show More' is clicked
    function loadNextNotitifications() {
        var notId = "";
        var lastItem = $("div.dev_notifications div.ib-notify-item-group[nid]:last");

        if (lastItem.length == 1) {
            notId = lastItem.attr('nid');
        }

        var startIndex = 0;

        //if there is already notification in panel
        if (notId) {
            for (var i = 0; i < _NotificationLocalStorageData.history.length; i++) {
                var msg = _NotificationLocalStorageData.history[i];
                if (msg.notificationId == notId) {
                    startIndex = i + 1;
                    break;
                }
            }//end of for
        }
        var endIndex = startIndex + _options.defaultNotificationCount;
        FindChunkAndShow(startIndex, endIndex, false);

        //if (endIndex >= _NotificationLocalStorageData.history.length) {
        //    $("#loadMoreNotiifications").hide();
        //}
        //var nextBatch = _NotificationLocalStorageData.history.slice(startIndex, endIndex);
        //showMessageInNotificationBox(nextBatch, false);

    }

    //It will get chunk of notifications from Local Data and will show in notification panel
    function FindChunkAndShow(startIndex, endIndex, isPrepend) {
        //var endIndex = startIndex + _options.defaultNotificationCount;
        if (endIndex >= _NotificationLocalStorageData.history.length) {
            $("#loadMoreNotiifications").hide();
        }
        var nextBatch = _NotificationLocalStorageData.history.slice(startIndex, endIndex);
        showMessageInNotificationBox(nextBatch, isPrepend);
    }

    //Add message in Notification Panel
    function showMessageInNotificationBox(msgsToPrepend, isPrepend) {

        $("#dev_notif_count").text(_NotificationLocalStorageData.unreadCount);


        //------------START: CREATE HTML
        var $dummyContainer = $("<div></div>");

        for (var i = 0; i < msgsToPrepend.length; i++) {
            var itemObj = msgsToPrepend[i];
            var $itemOuterDiv = $("<div class='ib-notify-item-group'>").attr('nid', itemObj.notificationId);

            if (itemObj.extraData)
                $itemOuterDiv.data('extraData', itemObj.ED);

            if (itemObj.isRead == false) {
                $itemOuterDiv.addClass('unread');
                //$itemOuterDiv.addClass('checked');
            }

            var $icon = $('<span class="ib-icon-holder"><i class="ib-noti-icons dev_itemicon"></i><span>')
            $icon.find("i").attr('notificationid', itemObj.notificationId);

            var $msgContainer = $('<div class="ib-notify-item">');

            $itemOuterDiv.append($icon).append($msgContainer);

            var $msg = $('<div>').text(itemObj.msg);
            var $time = $('<div class="ib-time">').text(itemObj.msgTime + ' EST');

            $msgContainer.append($msg).append($time);


            //var $div1 = $("<div class='col-md-2 col-sm-2 col-xs-3'>");
            //var $div2 = $("<div class='notify-img'>");
            //var $i = $('<i class="material-icons">&#xE5CA;</i>').attr('notificationid', itemObj.notificationId);



            //$div2.append($i);
            //$div1.append($div2);
            //$li.append($div1);

            //var $div3 = $('<div class="col-md-10 col-sm-10 col-xs-9 pd-l0"><span>' + itemObj.msg + '</span><p class="time">' + itemObj.msgTime + ' EST</p></div>');

            //$li.append($div3);

            $dummyContainer.append($itemOuterDiv);
        }


        //-------------END: HTML Creation

        //var html = __getHTMLusingHandleBar('#notificationitemTemplate', { Notifications: msgsToPrepend });

        var html = $dummyContainer.html();
        //alert(html);

        if (isPrepend)
            _notificationPanel.prepend(html);
        else
            _notificationPanel.find('div.dev_showmore').before(html);


        //Attach event with each notificaton item
        _notificationPanel.find('div.ib-notify-item-group').unbind("click").bind("click", function (e) {
            e.stopPropagation();
            e.preventDefault();

            try {
                $(this).removeClass('unread');
                //$(this).find('i.material-icons').removeClass('checked');

                var notificationId = $(this).find('i.dev_itemicon').attr('notificationId');

                var obj = __MyFind(_NotificationLocalStorageData.history, 'notificationId', notificationId);

                if (obj && obj.isRead == false) {
                    obj.isRead = true;
                    _NotificationLocalStorageData.unreadCount = _NotificationLocalStorageData.unreadCount - 1;
                    $("#dev_notif_count").text(_NotificationLocalStorageData.unreadCount);
                    SaveDataInLocalStorage();

                    _mainNotificationHub.server.markNotification(notificationId,  true, false);
                }

                if (_options.itemClickedCallback) {
                    _options.itemClickedCallback(obj);
                }
            } catch (e) {

            }

            return false;
        });//End of 
    }

    function SaveDataInLocalStorage() {
        __SetInLocalStorageWrapper(_localStorageSubKey, _NotificationLocalStorageData);
    }

    function LoadDataFromLocalStorage() {
        _NotificationLocalStorageData = __GetDataFromLocalStorageWrapper(_localStorageSubKey);
    }



    /*----------------START: Function duplication to make it independent library from Coaching---------------------*/

    //function __getHTMLusingHandleBar(templateSelector, data) {

    //    var source = $(templateSelector).html();
    //    var template = Handlebars.compile(source);
    //    return template(data);

    //}

    function __MyFind(arr, attrToMatch, valueToMatch) {
        return arr.find(function (element) {
            return element[attrToMatch] == valueToMatch;
        });
    }

    function __SetInLocalStorageWrapper(subkey, data) {

        var key = __custombtoa(_baseKey + "_" + _options.EID + "_" + subkey);
        data = __custombtoa(JSON.stringify(data));
        localStorage[key] = data;
    }
    function __GetDataFromLocalStorageWrapper(subkey) {

        var key = __custombtoa(_baseKey + "_" + _options.EID + "_" + subkey);
        var str = localStorage[key];
        try {
            str = __customatob(str);
            return JSON.parse(str);
        } catch (e) {
            return null;
        }
    }

    function __custombtoa(str) {
        if (_options.useBase64 == false)
            return str;
        else
            return window.btoa(unescape(encodeURIComponent(str)));
    }
    function __customatob(b64) {
        if (_options.useBase64 == false)
            return b64;
        else
            return decodeURIComponent(escape(window.atob(b64)));
    }

    /*----------------END: Function duplication to make it idependent library---------------------*/



    function ShowDesktopNotification(notifciationId, message, msgTime) {
        
        if (!("Notification" in window)) {
            alert("This browser does not support desktop notification");
        }
            // Let's check whether notification permissions have already been granted
        else if (Notification.permission === "granted") {
            // If it's okay let's create a notification
            GenerateNotification(notifciationId, message, msgTime);
        }
            // Otherwise, we need to ask the user for permission
        else if (Notification.permission !== "denied") {
            Notification.requestPermission(function (permission) {
                // If the user accepts, let's create a notification
                if (permission === "granted") {
                    //Notification.permission = permission;
                    GenerateNotification(notifciationId, message, msgTime);
                }
            });
        }
    }
    function GenerateNotification(notifciationId, message, msgTime) {
        let notification = new Notification('Notification', {
            icon: _options.desktopNotIconPath,
            body: message,
            tag: notifciationId
        });

        notification.onclick = function () {
            parent.focus();
            window.focus(); //just in case, older browsers
            this.close();
        };
        setTimeout(notification.close.bind(notification), 5000);

    }
    return {
        initialise: function (args) {
            $("body").hover(function () {
                PUCIT.AIMRL.NotificationServerHandler.EnableDesktopNotification();
            });
            init(args);
        },
        setHubUrl: function (hubPath) {
            _hubUrl = hubPath;
        },
        sendMessage: function (empTo, message, showDesktopNotif, extraData) {
            if (!showDesktopNotif)
                showDesktopNotif = false;
            if (!extraData)
                extraData = {};

            try {
                extraData = JSON.stringify(extraData);
            } catch (e) {
                console.log('error in extraData format');
                extraData = '{}';
            }
            
            _mainNotificationHub.server.sendNotification(empTo, message, showDesktopNotif, extraData);

        },
        EnableDesktopNotification: function () {

            if (_desktopNotificationSupport == false)
                return;

            if (!("Notification" in window)) {
                _desktopNotificationSupport = false;
                alert("This browser does not support desktop notification"); 
            }
                // Let's check whether notification permissions have already been granted
            else if (Notification.permission === "granted") {
                // If it's okay let's create a notification
                //GenerateNotification(notifciationId, message, msgTime);
            }
                // Otherwise, we need to ask the user for permission
            else if (Notification.permission !== "denied") {
                Notification.requestPermission(function (permission) {
                    // If the user accepts, let's create a notification
                    if (permission === "granted") {
                        //alert('permission is granted');
                        //Notification.permission = permission;
                        //GenerateNotification(notifciationId, message, msgTime);
                    }
                });
            }
        }

    };

})();