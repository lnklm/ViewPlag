using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MoodleDownloader
{
    public partial class frmMain : Form
    {

        Downloader down = new Downloader();
        Dictionary<string, string> courseList;
        Dictionary<string, string> labList;
        Dictionary<string, string> groupList;

        public frmMain()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!bw.IsBusy)
            {
                bw.RunWorkerAsync();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            

            down.Auth(txtLogin.Text, txtPassword.Text);

            if (down.isAuth)
            {
                pbAccess.Image = MoodleDownloader.Properties.Resources.ok;

                courseList = down.DownloadCourseList();
                cmbCourses.Items.Clear();
                foreach (var itm in courseList)
                {
                    cmbCourses.Items.Add(itm.Key);
                }
                cmbCourses.SelectedIndex = 0;
            }
            else
            {
                pbAccess.Image = MoodleDownloader.Properties.Resources.bad;
            }
        }

        private void download_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                string root = RegistryHelper.GetSetting("FolderSettings", "rootFolder", "");
                string mask = RegistryHelper.GetSetting("FolderSettings", "mask", "{course}/{lab}/{group}");


                foreach (var itm in lbLabs.CheckedItems)
                {
                    
                    foreach (var grp in lbGroups.CheckedItems)
                    {
                        if (root.Length < 2)
                            Settings.Instance.Directory = Application.StartupPath + "/";
                        else
                            Settings.Instance.Directory = root + "/";
                            
                        Settings.Instance.Directory += mask.Replace("{group}",Transliteration.Encode(grp.ToString())).
                            Replace("{lab}",Transliteration.Encode(itm.ToString())).Replace("{course}",Transliteration.Encode(cmbCourses.Text)) + "/";
                            

                        if (!Directory.Exists(Settings.Instance.Directory))
                             Directory.CreateDirectory(Settings.Instance.Directory);

                        down.DownloadAssignmentFromLab(labList[itm.ToString()], groupList[grp.ToString()], Transliteration.Encode(itm.ToString()));
                    }

                }

                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Transliteration.Encode("Сторчевий Владислав"));
        }

        private void cmbCourses_SelectedIndexChanged(object sender, EventArgs e)
        {
            groupList = null;
            labList = down.DownloadLabList(courseList[cmbCourses.Text]);
            lbLabs.Items.Clear();
            string key = String.Empty;
            foreach (var itm in labList)
            {
                if (groupList == null || groupList.Count == 0)
                groupList = down.DownloadGroupsList(itm.Value);
                lbLabs.Items.Add(itm.Key);
            }
            lbGroups.Items.Clear();
            foreach (var itm in groupList)
            {
                lbGroups.Items.Add(itm.Key);
            }
           
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (!download.IsBusy)
            {
                download.RunWorkerAsync();
            }
            else
                MessageBox.Show("Почекайте, вже відбувається завантаження");
        }

        private void download_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Завантаження завершено!");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var f = new FolderSettings();
            f.Show();
        }

    }
}
