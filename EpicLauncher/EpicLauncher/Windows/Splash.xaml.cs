using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Threading;
using GCLauncher.Source;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GCLauncher{
    public partial class Splash : Window {
        public static Info status = new Info();
        public int progressRegulator { get; set; } = 0;
        public bool launcherHaveCache;
        public int totalCount { get; set; } = 0;
        public int updateCount { get; set; } = 0;
        public int count { get; set; } = 0;
        public int versionCount { get; set; } = 0;
        public Splash(){
            InitializeComponent();
            splashText.Text = status.launcherLang.GetString(status.currentLang, "splash_check");
            LauncherBarTotal.Value = 0;
            LauncherBarTotal.IsEnabled = true;
            Show();
            Thread.Sleep(1000);
            status.VersionPopulate();
            string getData = status.GetFileString(2);
            string getLauncherSha1 = GetLauncherSHA1();
            if (getLauncherSha1 != getData){
                splashText.Text = status.launcherLang.GetString(status.currentLang, "splash_update_launcher") + " - 0%";
                LauncherUpdating();
            }
            else{
                splashText.Text = status.launcherLang.GetString(status.currentLang, "splash_check") + " - 0%";
                LauncherFileVerify();
            }
        }
        private void StartLancher(List<Source.Version> Version, int countUpdate) {
            var Launcher = new Launcher(Version, countUpdate);
            Launcher.Show();
            Close();
        }
        private void LauncherProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            if (progressRegulator != e.ProgressPercentage)
            {
                progressRegulator = e.ProgressPercentage;
            }
            splashText.Text = status.launcherLang.GetString(status.currentLang, "splash_update_launcher") + " - " + progressRegulator + "%";
            LauncherBarTotal.SetPercent(progressRegulator);
        }
        private async void LauncherUpdating(){
            string output = null;
            using (var client = new WebClient()){
                client.DownloadFileCompleted += (sender, e) =>{
                    output = e.ToString();
                };
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(LauncherProgressChanged);
                client.DownloadFileAsync(new Uri(status.launcherURL + "/Launcher.exe"), "Launcher_Updated.exe");
                var n = DateTime.Now;
                while(output == null){
                    await Task.Delay(1);
                }
            }
            RunCMD();
            Close();
        }
        private int GetBarPercentage(int current, int total) {
            return (int)((double)current / total * 100);
        }
        private async void LauncherFileVerify(){
            totalCount = status.GetTotalCount();
            int curVersion = status.GetCurrentVersion();
            int lastVersion = int.Parse(status.GetFileString(3));
            while (curVersion <= lastVersion)
            {
                await FileCheckUpdateAsync(curVersion);
                curVersion++;
            }
            StartLancher(status.VersionInfo, updateCount);
            Close();
        }
        public async Task FileCheckUpdateAsync(int currentVersion)
        {
            while (count < totalCount)
            {
                while (versionCount < status.VersionInfo[currentVersion - 1].FileList.Count)
                {
                    int launcherPercent = GetBarPercentage(count, totalCount);
                    if (launcherPercent > 100)
                    {
                        LauncherBarTotal.Value = 100;
                        splashText.Text = status.launcherLang.GetString(status.currentLang, "splash_check") + " - 100%";
                    }
                    else
                    {
                        LauncherBarTotal.SetPercent(launcherPercent);
                        splashText.Text = status.launcherLang.GetString(status.currentLang, "splash_check") + " - " + launcherPercent + "%";
                    }
                    await Task.Delay(100);
                    bool doesFileExists = await Task.Run(() => File.Exists(status.VersionInfo[currentVersion - 1].FileList[versionCount].FilePath));
                    if (!doesFileExists)
                    {
                        status.VersionInfo[currentVersion - 1].FileList[versionCount].ToUpdate = true;
                        status.VersionInfo[currentVersion - 1].ToUpdate = true;
                        updateCount += 1;
                    }
                    else
                    {
                        string fileHash = await Task.Run(() => status.createMD5FromFile(status.VersionInfo[currentVersion - 1].FileList[versionCount].FilePath));
                        if (fileHash != status.VersionInfo[currentVersion - 1].FileList[versionCount].FileHash)
                        {
                            status.VersionInfo[currentVersion - 1].FileList[versionCount].ToUpdate = true;
                            status.VersionInfo[currentVersion - 1].ToUpdate = true;
                            updateCount += 1;
                        }
                    }
                    versionCount++;
                    count++;
                }
                currentVersion++;
                versionCount = 0;
            }
        }

        private void RunCMD()
        {
            File.Create(@"update.bat").Dispose();
            using (var cmdr = new StreamWriter(@"update.bat"))
            {
                cmdr.WriteLine("@ECHO OFF");
                cmdr.WriteLine(">NUL TIMEOUT /T 2");
                cmdr.WriteLine("2>NUL DEL /f launcher.exe");
                cmdr.WriteLine("@MOVE launcher_updated.exe launcher.exe");
                cmdr.WriteLine("ping 127.0.0.1 -n 2 > nul");
                cmdr.WriteLine("start launcher.exe");
                cmdr.WriteLine("2>NUL DEL /f update.bat");
            }
            Process myProcess = new Process();
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.FileName = "cmd.exe";
            myProcess.StartInfo.Arguments = "/C update.bat";
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
        }
        private string GetLauncherSHA1(){
            using(var cryptoProvider = new SHA1CryptoServiceProvider()){
                FileStream fs = File.OpenRead(status.launcherExec + ".exe");
                string hash = BitConverter.ToString(cryptoProvider.ComputeHash(fs)).ToLower().Replace("-", string.Empty);
                fs.Close();
                return hash;
            }
        }
    }
}