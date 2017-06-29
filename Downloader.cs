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

        public void DownloadAssignmentFromLab(string labId, string groupId, string labname = "")
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
                                    var fileName = Settings.Instance.Directory + userName + "_" + labname + "_" + tfile;
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

                                var fileName = Settings.Instance.Directory + userName + "_" + labname + "_" + "_merged_" + count.ToString() + "." + extension;

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

                    string[] titles = Html.Substrings(cont, "h2 class=\"title\"><a title=\"", "\"");
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
                    // \"instancename\">
                    string[] titles = Html.Substrings(cont, "class=\"iconlarge activityicon\" alt=\"Завдання\" /><span class=", "<span");
                    string[] ids = Html.Substrings(cont, "mod/assign/view.php?id=", "\">");

                    for (int i = 0; i < titles.Length; i++)
                    {
                        if (!titles[i].Contains("accesshide"))
                            d.Add(titles[i].Replace("\"instancename\">", ""), ids[i]);
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
