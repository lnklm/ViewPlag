using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xNet;
using System.Diagnostics;
using System.IO;

namespace MoodleDownloader
{
    public class Downloader
    {
        private string mUrl = "http://moodle.asu.kpi.ua/";
        private CookieDictionary cookie;
        public bool isAuth { get; private set; }

        public Downloader()
        {
            cookie = new CookieDictionary();
        }

        public void Auth(string username, string password)
        {
            using (HttpRequest req = new HttpRequest())
            {
                try
                {
                    req.UserAgent = Http.ChromeUserAgent();
                    req.Cookies = cookie;
                    req.Get(mUrl + "login/index.php");
                    string content = req.Post(mUrl + "login/index.php", String.Format("username={0}&password={1}", username, password), "application/x-www-form-urlencoded").ToString();
                    isAuth = content.Contains("http://moodle.asu.kpi.ua/login/logout.php?sesskey");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        public void DownloadFromCourse(string courseId)
        {
            if (!isAuth)
                throw new Exception("Ви повинні авторизуватися!");

            using (HttpRequest req = new HttpRequest())
            {
                try
                {
                    req.UserAgent = Http.ChromeUserAgent();
                    req.Cookies = cookie;

                    string cont = req.Get(mUrl + "course/view.php?id=" + courseId).ToString();

                    string[] ids = Html.Substrings(cont, "http://moodle.asu.kpi.ua/mod/resource/view.php?id=", "\">");

                    foreach (string id in ids)
                    {
                        req.Get(mUrl + "mod/resource/view.php?id=" + id).ToFile(Path.GetFileName(req.Address.ToString()));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        public void DownloadAssignmentFromLab(string labId, string groupId)
        {
            if (!isAuth)
                throw new Exception("Ви повинні авторизуватися!");

            using (HttpRequest req = new HttpRequest())
            {
                try
                {
                    req.UserAgent = Http.ChromeUserAgent();
                    req.Cookies = cookie;

                    string cont = req.Get(mUrl + String.Format("/mod/assign/view.php?id={0}&action=grading&group={1}", labId, groupId)).ToString();

                    string[] names = Html.Substrings(cont, "<td class=\"cell c2\">", "</a></td>").Select(x => x.Substring(">")).ToArray();

                    string[] files = Html.Substrings(cont, "<td class=\"cell c8\">", "</td><td class=\"cell c9\">");

                    for (int i = 0; i < names.Length; i++)
                    {
                        try
                        {
                            string userName = Transliteration.Encode(names[i]);
                            string[] filePath = Html.Substrings(files[i], "<a href=\"", "\">");
                            string[] fileTitle = Html.Substrings(files[i], "title=\"", "\"");

                            if (filePath.Length == 1)
                            {
                                if (filePath[0].Contains("http"))
                                {
                                    var tfile = Transliteration.Encode(fileTitle[0].Split('.')[0]) + "." + fileTitle[0].Split('.')[1];
                                    var fileName = Settings.Instance.Directory + userName + "_" + tfile;
                                    req.Get(filePath[0]).ToFile(fileName);
                                }
                            }
                            else if (filePath.Length > 1)
                            {
                                string fullFile = String.Empty;
                                string extension = String.Empty;
                                int count = 0;
                                for (int j = 0; j < filePath.Length; j++)
                                {
                                    try
                                    {
                                        string[] table = MoodleDownloader.Properties.Resources.AllowFiles.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                                        if (table.Contains(fileTitle[j].Split('.')[1]))
                                        {
                                            extension = fileTitle[j].Split('.')[1];
                                            if (filePath[j].Contains("http"))
                                            {
                                                fullFile += Environment.NewLine + req.Get(filePath[j]).ToString();
                                                count++;
                                            }
                                        }
                                    }
                                    catch { }
                                }

                                var fileName = Settings.Instance.Directory + userName + "_merged_" + count.ToString() + "." + extension;

                                File.WriteAllText(fileName, fullFile);
                            }

                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        public Dictionary<string, string> DownloadCourseList()
        {
            var d = new Dictionary<string, string>();

            if (!isAuth)
                throw new Exception("Ви повинні авторизуватися!");

            using (HttpRequest req = new HttpRequest())
            {
                try
                {
                    req.UserAgent = Http.ChromeUserAgent();
                    req.Cookies = cookie;

                    string cont = req.Get("http://moodle.asu.kpi.ua/my/").ToString();

                    string[] titles = Html.Substrings(cont,"h2 class=\"title\"><a title=\"","\"");
                    string[] ids = Html.Substrings(cont, "course/view.php?id=", "\"").Distinct().ToArray();

                    for (int i = 0; i < titles.Length; i++)
                    {
                        d.Add(titles[i], ids[i]);
                    }
                }
                catch { }

            }

            return d;
        }

        public Dictionary<string, string> DownloadLabList(string courseId)
        {

            var d = new Dictionary<string, string>();

            if (!isAuth)
                throw new Exception("Ви повинні авторизуватися!");

            using (HttpRequest req = new HttpRequest())
            {
                try
                {
                    req.UserAgent = Http.ChromeUserAgent();
                    req.Cookies = cookie;

                    string cont = req.Get("http://moodle.asu.kpi.ua/course/view.php?id=" + courseId).ToString();

                    string[] titles = Html.Substrings(cont, "class=\"iconlarge activityicon\" alt=\"Завдання\" /><span class=\"instancename\">", "<span");
                    string[] ids = Html.Substrings(cont, "mod/assign/view.php?id=", "\">");

                    for (int i = 0; i < titles.Length; i++)
                    {
                        d.Add(titles[i], ids[i]);
                    }
                }
                catch { }

            }

            return d;

        }

        public Dictionary<string, string> DownloadGroupsList(string assignId)
        {

            var d = new Dictionary<string, string>();

            if (!isAuth)
                throw new Exception("Ви повинні авторизуватися!");

            using (HttpRequest req = new HttpRequest())
            {
                try
                {
                    req.UserAgent = Http.ChromeUserAgent();
                    req.Cookies = cookie;

                    string cont = req.Get("http://moodle.asu.kpi.ua/mod/assign/view.php?id=" + assignId + "&action=grading").ToString();

                    string tmp = Html.Substring(cont, "Усі учасники</option>", "</select><noscript");

                    string[] titles = Html.Substrings(tmp, "\">", "</option>");
                    string[] ids = Html.Substrings(tmp, "option value=\"", "\"");

                    for (int i = 0; i < titles.Length; i++)
                    {
                        d.Add(titles[i], ids[i]);
                    }
                }
                catch { }

            }

            return d;

        }

    }
}
﻿using System;
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
﻿namespace MoodleDownloader
{
    partial class FolderSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FolderSettings));
            this.label1 = new System.Windows.Forms.Label();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMask = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Коренева папка:";
            // 
            // txtFolder
            // 
            this.txtFolder.Location = new System.Drawing.Point(15, 26);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(153, 20);
            this.txtFolder.TabIndex = 1;
            // 
            // btnOpen
            // 
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.Location = new System.Drawing.Point(174, 23);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(32, 26);
            this.btnOpen.TabIndex = 2;
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Маска:";
            // 
            // txtMask
            // 
            this.txtMask.Location = new System.Drawing.Point(15, 75);
            this.txtMask.Name = "txtMask";
            this.txtMask.Size = new System.Drawing.Size(191, 20);
            this.txtMask.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Примітка";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "{course} - ім\'я курсу";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 152);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(127, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "{lab} - ім\'я лабораторної";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 175);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(100, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "{group} - ім\'я групи";
            // 
            // FolderSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(223, 201);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtMask);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FolderSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FolderSettings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FolderSettings_FormClosing);
            this.Load += new System.EventHandler(this.FolderSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMask;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}﻿using System;
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

                        down.DownloadAssignmentFromLab(labList[itm.ToString()], groupList[grp.ToString()]);
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
﻿namespace MoodleDownloader
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.txtLogin = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnLogin = new System.Windows.Forms.Button();
            this.pbAccess = new System.Windows.Forms.PictureBox();
            this.txtPassword = new System.Windows.Forms.MaskedTextBox();
            this.bw = new System.ComponentModel.BackgroundWorker();
            this.download = new System.ComponentModel.BackgroundWorker();
            this.cmbCourses = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lbGroups = new System.Windows.Forms.CheckedListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lbLabs = new System.Windows.Forms.CheckedListBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbAccess)).BeginInit();
            this.SuspendLayout();
            // 
            // txtLogin
            // 
            this.txtLogin.Location = new System.Drawing.Point(65, 12);
            this.txtLogin.Name = "txtLogin";
            this.txtLogin.Size = new System.Drawing.Size(100, 20);
            this.txtLogin.TabIndex = 0;
            this.txtLogin.Text = "teacher";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Логін:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Пароль:";
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(188, 10);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(65, 23);
            this.btnLogin.TabIndex = 4;
            this.btnLogin.Text = "Ввійти";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // pbAccess
            // 
            this.pbAccess.Location = new System.Drawing.Point(188, 45);
            this.pbAccess.Name = "pbAccess";
            this.pbAccess.Size = new System.Drawing.Size(30, 27);
            this.pbAccess.TabIndex = 5;
            this.pbAccess.TabStop = false;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(65, 46);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(100, 20);
            this.txtPassword.TabIndex = 6;
            this.txtPassword.Text = "111112";
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // bw
            // 
            this.bw.WorkerSupportsCancellation = true;
            this.bw.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bw_DoWork);
            // 
            // download
            // 
            this.download.WorkerSupportsCancellation = true;
            this.download.DoWork += new System.ComponentModel.DoWorkEventHandler(this.download_DoWork);
            this.download.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.download_RunWorkerCompleted);
            // 
            // cmbCourses
            // 
            this.cmbCourses.FormattingEnabled = true;
            this.cmbCourses.Location = new System.Drawing.Point(44, 79);
            this.cmbCourses.Name = "cmbCourses";
            this.cmbCourses.Size = new System.Drawing.Size(209, 21);
            this.cmbCourses.TabIndex = 7;
            this.cmbCourses.SelectedIndexChanged += new System.EventHandler(this.cmbCourses_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Курс:";
            // 
            // lbGroups
            // 
            this.lbGroups.FormattingEnabled = true;
            this.lbGroups.Location = new System.Drawing.Point(11, 224);
            this.lbGroups.Name = "lbGroups";
            this.lbGroups.Size = new System.Drawing.Size(242, 79);
            this.lbGroups.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 204);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Групи:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 108);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(73, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Лабораторні:";
            // 
            // lbLabs
            // 
            this.lbLabs.FormattingEnabled = true;
            this.lbLabs.Location = new System.Drawing.Point(11, 124);
            this.lbLabs.Name = "lbLabs";
            this.lbLabs.Size = new System.Drawing.Size(242, 79);
            this.lbLabs.TabIndex = 13;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(166, 310);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(88, 26);
            this.btnLoad.TabIndex = 14;
            this.btnLoad.Text = "Завантажити";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // button1
            // 
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.Location = new System.Drawing.Point(224, 45);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(33, 27);
            this.button1.TabIndex = 15;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(269, 348);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.lbLabs);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbGroups);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbCourses);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.pbAccess);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtLogin);
            this.Name = "frmMain";
            this.Text = "Moodle Downloader 1.0";
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbAccess)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLogin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.PictureBox pbAccess;
        private System.Windows.Forms.MaskedTextBox txtPassword;
        private System.ComponentModel.BackgroundWorker bw;
        private System.ComponentModel.BackgroundWorker download;
        private System.Windows.Forms.ComboBox cmbCourses;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox lbGroups;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckedListBox lbLabs;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button button1;
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoodleDownloader
{
    class Manager
    {
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MoodleDownloader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
﻿using System;
using Microsoft.Win32;
using System.Windows.Forms;
namespace MoodleDownloader
{
    public class RegistryHelper
    {
        private static string FormRegKey(string sSect)
        {
            return sSect;
        }
        public static void SaveSetting(string Section, string Key, string Setting)
        {

            string text1 = FormRegKey(Section);
            RegistryKey key1 =

            Application.UserAppDataRegistry.CreateSubKey(text1);
            if (key1 == null)
            {
                return;
            }
            try
            {
                key1.SetValue(Key, Setting);
            }
            catch
            {
                return;
            }
            finally
            {
                key1.Close();
            }

        }
        public static string GetSetting(string Section, string Key, string Default)
        {
            if (Default == null)
            {
                Default = "";
            }
            string text2 = FormRegKey(Section);
            RegistryKey key1 = Application.UserAppDataRegistry.OpenSubKey(text2);
            if (key1 != null)
            {
                object obj1 = key1.GetValue(Key, Default);
                key1.Close();
                if (obj1 != null)
                {
                    if (!(obj1 is string))
                    {
                        return null;
                    }
                    return (string)obj1;
                }
                return null;
            }
            return Default;
        }
    }
}﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoodleDownloader
{
    public class Settings
    {
        private static Settings instance;
        public static Settings Instance
        {
            get { return instance ?? (instance = new Settings()); }
        }
        protected Settings() { }

        public string Directory;

    }
}
﻿    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Text;
    using System.Text.RegularExpressions;
    namespace MoodleDownloader
    {
        // ISO 9-95
        public static class Transliteration
        {
            private static Dictionary<string, string> iso = new Dictionary<string, string>();

            public static string Encode(string text)
            {
                string output = text;

                output = Regex.Replace(output, @"\s|\.|\(", " ");
                output = Regex.Replace(output, @"\s+", " ");
                output = Regex.Replace(output, @"[^\s\w\d-]", "");
                output = output.Trim();

                foreach (KeyValuePair<string, string> key in iso)
                {
                    output = output.Replace(key.Key, key.Value);
                }

                return output;
            }
          
            public static string Decode(string text)
            {
                string output = text;

                foreach (KeyValuePair<string, string> key in iso)
                {
                    output = output.Replace(key.Value, key.Key);
                }
                return output;
            }

            static Transliteration()
            {
                string[] table = MoodleDownloader.Properties.Resources.TransliterationTable.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (string itm in table)
                    iso.Add(itm.Split(' ')[0], itm.Split(' ')[1]);
                iso.Add(" ", "-");
            }
        }
    }