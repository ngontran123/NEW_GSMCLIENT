using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using NLog;

namespace C3TekClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 

        [STAThread]
        static void Main()
        {
            File.WriteAllText("log.log", "START\r\n");

            ServicePointManager.ServerCertificateValidationCallback
              += (sender, certificate, chain, sslPolicyErrors) => { return true; };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            File.AppendAllText("log.log", "Chilkat\r\n");

            Chilkat.Global g = new Chilkat.Global();
            bool unlocked = g.UnlockBundle("C3TEKc.CBX0126_GHEo0Shsnflm");

            File.AppendAllText("log.log", "CheckForUpdate\r\n");

            //var portal = new C3TekPortal();
            //var loginInfo = portal.Login("0349982338", "fXLaE4tY");
            //var loginInfo = portal.Login("0846889911", "tranminhduc123");

            //Client.AccessToken = loginInfo.token;
            //VersionManager.InitUpdater();
            //VersionManager.CheckForUpdate();
            File.AppendAllText("log.log", "Enable visual style\r\n");


            //portal.GetLastestVersion();
            //new MyMobifone.MMFAccount().Register();
            //var account = new C3TekClient.MyViettel.MVTAccount().Register();

            string language = ConfigurationManager.AppSettings["language"] ?? "vi-VN";

            if (string.IsNullOrEmpty(language))
            {
                language = "vi-VN";
            }
            AppModeSetting.Locale = language;

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
            AppModeSetting.CurrentCultureInfo = new System.Globalization.CultureInfo(language);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //log config

            var config = new NLog.Config.LoggingConfiguration();
            
            // Targets where to log to: File and Console
            var logGeneralFile = new NLog.Targets.FileTarget("logfile") { FileName = "log_general.log" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logGeneralFile);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            

            // Apply config           
            NLog.LogManager.Configuration = config;


            //Application.Run(new AddProxy());

            //BonusSkins.Register();

            Application.Run(new MainUI());
        }
    }
}
