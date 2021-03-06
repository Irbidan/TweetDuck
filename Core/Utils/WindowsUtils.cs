﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace TweetDuck.Core.Utils{
    static class WindowsUtils{
        public static bool ShouldAvoidToolWindow { get; }

        static WindowsUtils(){
            Version ver = Environment.OSVersion.Version;
            ShouldAvoidToolWindow = ver.Major == 6 && ver.Minor == 2; // windows 8/10
        }

        public static bool CheckFolderWritePermission(string path){
            string testFile = Path.Combine(path, ".test");

            try{
                Directory.CreateDirectory(path);

                using(File.Create(testFile)){}
                File.Delete(testFile);
                return true;
            }catch{
                return false;
            }
        }

        public static Process StartProcess(string file, string arguments, bool runElevated){
            ProcessStartInfo processInfo = new ProcessStartInfo{
                FileName = file,
                Arguments = arguments
            };

            if (runElevated){
                processInfo.Verb = "runas";
            }

            return Process.Start(processInfo);
        }

        public static bool TrySleepUntil(Func<bool> test, int timeoutMillis, int timeStepMillis){
            for(int waited = 0; waited < timeoutMillis; waited += timeStepMillis){
                if (test()){
                    return true;
                }

                Thread.Sleep(timeStepMillis);
            }

            return false;
        }

        public static void TryDeleteFolderWhenAble(string path, int timeout){
            new Thread(() => {
                TrySleepUntil(() => {
                    try{
                        Directory.Delete(path, true);
                        return true;
                    }catch(DirectoryNotFoundException){
                        return true;
                    }catch{
                        return false;
                    }
                }, timeout, 500);
            }).Start();
        }

        public static void ClipboardStripHtmlStyles(){
            if (!Clipboard.ContainsText(TextDataFormat.Html)){
                return;
            }

            string originalText = Clipboard.GetText(TextDataFormat.UnicodeText);
            string originalHtml = Clipboard.GetText(TextDataFormat.Html);

            string updatedHtml = ClipboardRegexes.RegexStripHtmlStyles.Replace(originalHtml, string.Empty);

            int removed = originalHtml.Length-updatedHtml.Length;
            updatedHtml = ClipboardRegexes.RegexOffsetClipboardHtml.Replace(updatedHtml, match => (int.Parse(match.Value)-removed).ToString().PadLeft(match.Value.Length, '0'));

            DataObject obj = new DataObject();
            obj.SetText(originalText, TextDataFormat.UnicodeText);
            obj.SetText(updatedHtml, TextDataFormat.Html);
            SetClipboardData(obj);
        }

        public static void SetClipboard(string text, TextDataFormat format){
            if (string.IsNullOrEmpty(text)){
                return;
            }

            DataObject obj = new DataObject();
            obj.SetText(text, format);
            SetClipboardData(obj);
        }

        private static void SetClipboardData(DataObject obj){
            try{
                Clipboard.SetDataObject(obj);
            }catch(ExternalException e){
                Program.Reporter.HandleException("Clipboard Error", Program.BrandName+" could not access the clipboard as it is currently used by another process.", true, e);
            }
        }

        private static class ClipboardRegexes{ // delays construction of regular expressions until needed
            public static readonly Regex RegexStripHtmlStyles = new Regex(@"\s?(?:style|class)="".*?""");
            public static readonly Regex RegexOffsetClipboardHtml = new Regex(@"(?<=EndHTML:|EndFragment:)(\d+)");
        }
    }
}
