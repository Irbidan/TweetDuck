using System;

namespace TweetDck.Core.Notification.Sound{
    sealed class PlaybackErrorEventArgs : EventArgs{
        public string Message { get; }
        public bool Ignore { get; set; }

        public PlaybackErrorEventArgs(string message){
            this.Message = message;
            this.Ignore = false;
        }
    }
}
