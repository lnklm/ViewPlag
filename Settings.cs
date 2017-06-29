using System;
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
