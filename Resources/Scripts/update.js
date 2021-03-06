(function($, $TDU){
  //
  // Variable: Current timeout ID for update checking.
  //
  var updateCheckTimeoutID;
  
  //
  // Constant: Update exe file name.
  //
  const updateFileName = "TweetDuck.Update.exe";
  
  //
  // Constant: Url that returns JSON data about latest version.
  //
  const updateCheckUrlLatest = "https://api.github.com/repos/chylex/TweetDuck/releases/latest";
  
  //
  // Constant: Url that returns JSON data about all versions, including prereleases.
  //
  const updateCheckUrlAll = "https://api.github.com/repos/chylex/TweetDuck/releases";
  
  //
  // Constant: Fallback url in case the update installer file is missing.
  //
  const updateDownloadFallback = "https://tweetduck.chylex.com/#download";
  
  //
  // Function: Creates the update notification element. Removes the old one if already exists.
  //
  var displayNotification = function(version, download){
    var outdated = version === "unsupported";
    
    var ele = $("#tweetduck-update");
    var existed = ele.length > 0;
    
    if (existed){
      ele.remove();
    }
    
    var html = outdated ? [
      "<div id='tweetduck-update'>",
      "<p class='tdu-title'>Unsupported System</p>",
      "<p class='tdu-info'>You will not receive updates.</p>",
      "<div class='tdu-buttons'>",
      "<button class='btn btn-positive tdu-btn-unsupported'><span class='label'>Read More</span></button>",
      "<button class='btn btn-negative tdu-btn-dismiss'><span class='label'>Dismiss</span></button>",
      "</div>",
      "</div>"
    ] : [
      "<div id='tweetduck-update'>",
      "<p class='tdu-title'>TweetDuck Update</p>",
      "<p class='tdu-info'>Version "+version+" is now available.</p>",
      "<div class='tdu-buttons'>",
      "<button class='btn btn-positive tdu-btn-download'><span class='label'>Download</span></button>",
      "<button class='btn btn-negative tdu-btn-dismiss'><span class='label'>Dismiss</span></button>",
      "</div>",
      "</div>"
    ];

    $(document.body).append(html.join(""));

    ele = $("#tweetduck-update");

    var buttonDiv = ele.children("div.tdu-buttons").first();

    ele.css({
      color: "#fff",
      backgroundColor: "rgb(32,94,138)",
      position: "absolute",
      left: "4px",
      bottom: "4px",
      width: "192px",
      height: "86px",
      display: existed ? "block" : "none",
      borderRadius: "2px",
      zIndex: 9999
    });

    ele.children("p.tdu-title").first().css({
      fontSize: "17px",
      fontWeight: "bold",
      textAlign: "center",
      letterSpacing: "0.2px",
      margin: "5px auto 2px"
    });

    ele.children("p.tdu-info").first().css({
      fontSize: "12px",
      textAlign: "center",
      margin: "2px auto 6px"
    });

    buttonDiv.css({
      textAlign: "center"
    });

    buttonDiv.children().css({
      margin: "0 4px",
      minHeight: "25px",
      boxShadow: "1px 1px 1px rgba(17,17,17,0.5)"
    });

    buttonDiv.find("span").css({
      verticalAlign: "baseline"
    });

    ele.find("span.tdu-data-tag").first().css({
      cursor: "pointer",
      textDecoration: "underline"
    });

    buttonDiv.children(".tdu-btn-download").click(function(){
      ele.remove();
      
      if (download){
        $TDU.onUpdateAccepted();
      }
      else{
        $TDU.openBrowser(updateDownloadFallback);
      }
    });

    buttonDiv.children(".tdu-btn-unsupported").click(function(){
      $TDU.openBrowser("https://github.com/chylex/TweetDuck/wiki/Supported-Systems");
    });

    buttonDiv.children(".tdu-btn-dismiss,.tdu-btn-unsupported").click(function(){
      $TDU.onUpdateDismissed();
      ele.slideUp(function(){ ele.remove(); });
    });
    
    if (!existed){
      ele.slideDown();
    }
    
    return ele;
  };
  
  //
  // Function: Returns milliseconds until the start of the next hour, with an extra offset in seconds that can skip an hour if the clock would roll over too soon.
  //
  var getTimeUntilNextHour = function(extra){
    var now = new Date();
    var offset = new Date(+now+extra*1000);
    return new Date(offset.getFullYear(), offset.getMonth(), offset.getDate(), offset.getHours()+1, 0, 0)-now;
  };
  
  //
  // Function: Runs an update check and updates all DOM elements appropriately.
  //
  var runUpdateCheck = function(eventID, versionTag, dismissedVersionTag, allowPre){
    clearTimeout(updateCheckTimeoutID);
    updateCheckTimeoutID = setTimeout($TDU.triggerUpdateCheck, getTimeUntilNextHour(60*30)); // 30 minute offset
    
    $.getJSON(allowPre ? updateCheckUrlAll : updateCheckUrlLatest, function(response){
      var release = allowPre ? response[0] : response;
      
      var tagName = release.tag_name;
      var hasUpdate = tagName !== versionTag && tagName !== dismissedVersionTag && release.assets.length > 0;
      
      if (hasUpdate){
        var obj = release.assets.find(asset => asset.name === updateFileName) || { browser_download_url: "" };
        displayNotification(tagName, obj.browser_download_url);
        
        if (eventID){ // ignore undefined and 0
          $TDU.onUpdateCheckFinished(eventID, tagName, obj.browser_download_url);
        }
      }
      else if (eventID){ // ignore undefined and 0
        $TDU.onUpdateCheckFinished(eventID, null, null);
      }
    });
  };
  
  //
  // Block: Setup global functions.
  //
  window.TDUF_displayNotification = displayNotification;
  window.TDUF_runUpdateCheck = runUpdateCheck;
})($, $TDU);
