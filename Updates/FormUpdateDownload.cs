﻿using System;
using System.Windows.Forms;
using TweetDuck.Core.Utils;

namespace TweetDuck.Updates{
    sealed partial class FormUpdateDownload : Form{
        private readonly UpdateInfo updateInfo;

        public FormUpdateDownload(UpdateInfo info){
            InitializeComponent();

            this.updateInfo = info;

            Text = "Updating "+Program.BrandName;
            labelDescription.Text = "Downloading version "+info.VersionTag+"...";
            timerDownloadCheck.Start();
        }

        private void btnCancel_Click(object sender, EventArgs e){
            Close();
        }

        private void timerDownloadCheck_Tick(object sender, EventArgs e){
            if (updateInfo.DownloadStatus == UpdateDownloadStatus.Done){
                timerDownloadCheck.Stop();
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (updateInfo.DownloadStatus == UpdateDownloadStatus.Failed){
                timerDownloadCheck.Stop();

                if (MessageBox.Show("Could not download the update: "+(updateInfo.DownloadError?.Message ?? "unknown error")+"\r\n\r\nDo you want to open the website and try downloading the update manually?", "Update Has Failed", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) == DialogResult.Yes){
                    BrowserUtils.OpenExternalBrowserUnsafe(Program.Website);
                    DialogResult = DialogResult.OK;
                }
                
                Close();
            }
        }
    }
}
