using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MoodleDownloader
{
    public partial class FolderSettings : Form
    {
        public FolderSettings()
        {
            InitializeComponent();
        }

        private void FolderSettings_Load(object sender, EventArgs e)
        {
           txtFolder.Text = RegistryHelper.GetSetting("FolderSettings", "rootFolder", "");
           txtMask.Text = RegistryHelper.GetSetting("FolderSettings", "mask", "{course}/{lab}/{group}");
        }

        private void FolderSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            RegistryHelper.SaveSetting("FolderSettings", "rootFolder", txtFolder.Text);
            RegistryHelper.SaveSetting("FolderSettings", "mask", txtMask.Text);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            DialogResult result = fbd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                txtFolder.Text = fbd.SelectedPath;
            }
        }
    }
}
