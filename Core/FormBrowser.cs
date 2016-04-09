﻿using System.Windows.Forms;
using CefSharp.WinForms;

namespace TweetDick.Core{
    public partial class FormBrowser : Form{
        private readonly ChromiumWebBrowser browser;

        public FormBrowser(){
            InitializeComponent();

            browser = new ChromiumWebBrowser("https://tweetdeck.twitter.com/");
            Controls.Add(browser);
        }
    }
}