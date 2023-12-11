using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C3TekClientAutoUpdate
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args == null)
            {
                Application.Exit();
                return;
            }

            if (!args.Any())
            {
                Application.Exit();
                return;
            }

            string json = args[0];
            VersionInfo versionInfo = JsonConvert.DeserializeObject<VersionInfo>(json);
            if (versionInfo == null)
            {
                Application.Exit();
                return;
            }

            if (string.IsNullOrEmpty(versionInfo.url_download))
            {
                Application.Exit();
                return;
            }

            Application.Run(new MainUI(versionInfo));
        }
    }

    public class VersionInfo
    {
        public string version_code { get; set; }
        public string version_name { get; set; }
        public string version_date { get; set; }
        public string description { get; set; }
        public string product_code { get; set; }
        public string product_name { get; set; }
        public string url_download { get; set; }
        public bool is_latest { get; set; }
        public string version_type { get; set; }
        public bool force_upgrade { get; set; }
    }

}
