﻿using System;
using System.IO;
using System.Media;

namespace TweetDuck.Core.Notification.Sound{
    sealed class SoundPlayerImplFallback : ISoundNotificationPlayer{
        string ISoundNotificationPlayer.SupportedFormats => "*.wav";

        public event EventHandler<PlaybackErrorEventArgs> PlaybackError;

        private readonly SoundPlayer player;
        private bool ignorePlaybackError;

        public SoundPlayerImplFallback(){
            player = new SoundPlayer{
                LoadTimeout = 5000
            };
        }

        void ISoundNotificationPlayer.Play(string file){
            if (player.SoundLocation != file){
                player.SoundLocation = file;
                ignorePlaybackError = false;
            }

            try{
                player.Play();
            }catch(FileNotFoundException e){
                OnNotificationSoundError("File not found: "+e.FileName);
            }catch(InvalidOperationException){
                OnNotificationSoundError("File format was not recognized.");
            }catch(TimeoutException){
                OnNotificationSoundError("File took too long to load.");
            }
        }

        void ISoundNotificationPlayer.Stop(){
            player.Stop();
        }

        void IDisposable.Dispose(){
            player.Dispose();
        }

        private void OnNotificationSoundError(string message){
            if (!ignorePlaybackError && PlaybackError != null){
                PlaybackErrorEventArgs args = new PlaybackErrorEventArgs(message);
                PlaybackError(this, args);
                ignorePlaybackError = args.Ignore;
            }
        }
    }
}
