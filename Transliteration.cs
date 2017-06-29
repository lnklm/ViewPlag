    using System;
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