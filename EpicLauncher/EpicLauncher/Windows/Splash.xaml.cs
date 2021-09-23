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
    public partial class Splash : Window{
        public static Info status = new Info();
        public Splash(){
            InitializeComponent();
            splashText.Text = "Verificando arquivos ";
            LauncherBarTotal.Value = 0;
            LauncherBarTotal.IsEnabled = true;
            Show();
            Thread.Sleep(1000);
            string getData = status.GetFileString(2);
            status.FilePopulate(0);
            if(GetLauncherMD5() != getData){
                splashText.Text = "Atualizando Launcher - 0%";
                LauncherUpdating();
            }
            else{
                splashText.Text = "Verificando arquivos - 0%";
                LauncherFileVerify();
            }
        }
        private void StartLancher(List<Arquivo> Files, int countUpdate) {
            var Launcher = new Launcher(Files, countUpdate);
            Launcher.Show();
            Close();
        }
        private void LauncherProgressChanged(object sender, DownloadProgressChangedEventArgs e){
            splashText.Text = "Atualizando Launcher - " + e.ProgressPercentage + "%";
            LauncherBarTotal.Value = e.ProgressPercentage;
        }
        private async void LauncherUpdating(){
            string output = null;
            using (var client = new WebClient()){
                client.DownloadFileCompleted += (sender, e) =>{
                    output = e.ToString();
                };
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(LauncherProgressChanged);
                client.DownloadFileAsync(new Uri(status.launcherURL + "/Update/grandchase.dll"), "grandchase.dll");
                var n = DateTime.Now;
                while(output == null){
                    await Task.Delay(1);
                }
            }
            RunCMD();
            Close();
        }
        private async void LauncherFileVerify(){
            int count = 0;
            int totalCount = status.FileInfo.Count;
            int updateCount = 0;
            while(count < totalCount){
                status.FileInfo[count] = status.FileCheckUpdate(status.FileInfo[count], totalCount, ref LauncherBarTotal, ref splashText);
                if (status.FileInfo[count].ToUpdate == true) { updateCount += 1; }
                count++;
                await Task.Delay(1);
            }
            StartLancher(status.FileInfo, updateCount);
            Close();
        }
        private void RunCMD(){
            File.Create(@"grandchase.bat").Dispose();
            using(var cmdr = new StreamWriter(@"grandchase.bat")){
                cmdr.WriteLine("@ECHO OFF");
                cmdr.WriteLine(">NUL TIMEOUT /T 2");
                cmdr.WriteLine("2>NUL DEL /f grandchase.exe");
                cmdr.WriteLine("@MOVE grandchase.dll grandchase.exe");
                cmdr.WriteLine("ping 127.0.0.1 -n 2 > nul");
                cmdr.WriteLine("start grandchase.exe");
                cmdr.WriteLine("2>NUL DEL /f grandchase.bat");
            }
            Process myProcess = new Process();
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.FileName = "cmd.exe";
            myProcess.StartInfo.Arguments = "/C grandchase.bat";
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
        }
        private string GetLauncherMD5(){
            byte[] buff = File.ReadAllBytes(status.launcherExec + ".exe");
            var md5 = MD5.Create().ComputeHash(buff);
            string hash = BitConverter.ToString(md5).Replace("-", "").ToLowerInvariant();
            return hash;
        }
    }
}