using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Net.Http;
using System.Threading;
using GCLauncher.Source;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

namespace GCLauncher
{
    public partial class Launcher : Window {
        public int selected { get; set; } = 0;
        public int updateCount { get; set; } = 0;
        public int progressRegulator { get; set; } = 0;
        public int checkCount { get; set; } = 0;
        public Config ConfigWindow { get; set; }
        public bool launcherCache;
        public static Info launcherInfo = new Info();
        private DateTime startTime;
        public Launcher(List<Source.Version> Version, int countUpdate) {
            launcherInfo.VersionInfo = Version;
            updateCount = countUpdate;
            InitializeComponent();
            UpdateLauncherText();
            DownloaderInt();
        }
        private void WorkerComplete() {
            unitText.Text = launcherInfo.launcherLang.GetString(launcherInfo.currentLang, "launcher_worker_complete");
            totalText.Text = "";
            speedText.Text = "";
            DownloadBarUnit.Value = 100;
            DownloadBarFiles.Value = 100;
            game.IsEnabled = true;
            btn_image_game.Source = new BitmapImage(new Uri(@"/Images/buttons/start/start_default.png", UriKind.Relative));
        }
        private void WorkerMaintenance() {
            unitText.Text = launcherInfo.launcherLang.GetString(launcherInfo.currentLang, "launcher_worker_maintenance");
            totalText.Text = "";
            speedText.Text = "";
            DownloadBarUnit.Value = 100;
            DownloadBarFiles.Value = 100;
            game.IsEnabled = false;
            btn_image_game.Source = new BitmapImage(new Uri(@"/Images/buttons/start/start_disabled.png", UriKind.Relative));
        }
        private void StartMain() {
            launcherInfo.StartMain();
            Hide();
            Thread.Sleep(100);
            Close();
        }
        public void UpdateLauncherText() {
            launcherInfo.SetCurrentLanguage();
            if(DownloadBarUnit.Value == 100 && DownloadBarFiles.Value == 100) { 
                if(launcherInfo.GetFileString(1) == "true") {
                    unitText.Text = launcherInfo.launcherLang.GetString(launcherInfo.currentLang, "launcher_worker_maintenance");
                }
                else {
                    unitText.Text = launcherInfo.launcherLang.GetString(launcherInfo.currentLang, "launcher_worker_complete");
                }
            }
            playText.Text = launcherInfo.launcherLang.GetString(launcherInfo.currentLang, "launcher_game_start");
            titlebar.Text = launcherInfo.launcherLang.GetString(launcherInfo.currentLang, "launcher_title_bar");
        }
        public string GetPassLangString(string key) { 
            return launcherInfo.launcherLang.GetString(launcherInfo.currentLang, key);
        }
        private void DownloadProgressChanged(string fileName, string fileCurrent, int fileProgress, long bytesReceived) {
            if (fileProgress == 0) {
                startTime = DateTime.Now;
            }
            string filesTotal = updateCount.ToString();
            int currentFile = int.Parse(fileCurrent);
            int barPercentage = GetBarPercentage(currentFile, updateCount);
            if (progressRegulator != barPercentage) {
                progressRegulator = barPercentage;
            }
            double bytesPerSecond = bytesReceived / (DateTime.Now - startTime).TotalSeconds;
            string speed = FormatBytes(bytesPerSecond) + "/s";
            unitText.Text = launcherInfo.launcherLang.GetString(launcherInfo.currentLang, "launcher_worker_downloading") + ": " + fileName;
            totalText.Text = launcherInfo.launcherLang.GetString(launcherInfo.currentLang, "launcher_worker_total") + ": " + fileCurrent + "/" + filesTotal;
            speedText.Text = speed;
            DownloadBarUnit.SetPercent(fileProgress);
            DownloadBarFiles.SetPercent(progressRegulator);
        }
        private int GetBarPercentage(int current, int total) {
            return (int)((double)current / total * 100);
        }
        private string FormatBytes(double bytes) {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            while (bytes >= 1024 && suffixIndex < suffixes.Length - 1) {
                bytes /= 1024;
                suffixIndex++;
            }
            return $"{bytes:0.##} {suffixes[suffixIndex]}";
        }
        public async Task WorkerUpdating() {
            int ammountUpdated = 1;
            int curVersion = launcherInfo.GetCurrentVersion();
            int lastVersion = int.Parse(launcherInfo.GetFileString(3));
            while (curVersion <= lastVersion)
            {
                if(launcherInfo.VersionInfo[curVersion - 1].ToUpdate == false) {
                    curVersion++;
                    continue;
                }
                foreach (Arquivo fileUnit in launcherInfo.VersionInfo[curVersion - 1].FileList) {
                    if (fileUnit.ToUpdate) {
                        string filePath = fileUnit.FilePath;
                        string fileName = launcherInfo.FileData(filePath, 0).ToString();
                        string path = launcherInfo.FileData(filePath, 1);
                        string inverted = launcherInfo.FileData(filePath, 2);
                        if (!string.IsNullOrEmpty(path)) {
                            Directory.CreateDirectory(path);
                        }
                        if (File.Exists(fileUnit.FilePath)) {
                            File.Delete(fileUnit.FilePath);
                        }
                        using (var httpClient = new HttpClient()) {
                            string downloadUrl = launcherInfo.filesURL + inverted;
                            HttpResponseMessage response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                            if (response.IsSuccessStatusCode) {
                                long? totalBytes = response.Content.Headers.ContentLength;
                                using (var fileStream = new FileStream(fileUnit.FilePath, FileMode.Create, FileAccess.Write)) {
                                    using (var downloadStream = await response.Content.ReadAsStreamAsync()) {
                                        var buffer = new byte[8192];
                                        int bytesRead;
                                        long bytesDownloaded = 0;
                                        while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                                            bytesDownloaded += bytesRead;
                                            int progressPercentage = (int)(bytesDownloaded * 100 / totalBytes);
                                            DownloadProgressChanged(fileName, ammountUpdated.ToString(), progressPercentage, bytesDownloaded);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    ammountUpdated++;
                }
                curVersion++;
            }
            updateCount = 0;
            launcherInfo.WriteVersion();
            if(launcherInfo.GetFileString(1) == "true") {
                WorkerMaintenance();
            }
            else {
                WorkerComplete();
            }
        }
        private void ConfigPopup(int option, Launcher parent) {
            ConfigWindow = new Config(option, parent) {
                Owner = this
            };
            ConfigWindow.Show();
            ConfigWindow.Left = Left;
            ConfigWindow.Top = Top;
        }
        private void DownloaderInt() {
            game.IsEnabled = false;
            btn_image_game.Source = new BitmapImage(new Uri(@"/Images/buttons/start/start_disabled.png", UriKind.Relative));
            download_worker.Opacity = 1;
            DownloadBarUnit.Value = 0;
            DownloadBarUnit.Value = 0;
            _ = WorkerUpdating();
        }
        private void ClickLauncher(object sender, RoutedEventArgs e) {
            string content = (sender as Button).Name.ToString();
            switch(content){
                case "game":          StartMain(); break;
                case  "clb":              Close(); break;
                case  "fps": ConfigPopup(2, this); break;
                case  "dll": ConfigPopup(1, this); break;
            }
        }
        public static bool IsWindowOpen<T>(string name = "") where T : Window {
            return string.IsNullOrEmpty(name) ? Application.Current.Windows.OfType<T>().Any() : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
        void GameLeave(object sender, EventArgs e) {
            if(game.IsEnabled) {
                btn_image_game.Source = new BitmapImage(new Uri(@"/images/buttons/start/start_default.png", UriKind.Relative));
            }
        }
        void GameEnter(object sender, EventArgs e) {
            if(game.IsEnabled){
                btn_image_game.Source = new BitmapImage(new Uri(@"/images/buttons/start/start_hover.png", UriKind.Relative));
            }
        }
        void ConfigLeave(object sender, EventArgs e) {
            if(dll.IsEnabled) {
                btn_image_dll.Source = new BitmapImage(new Uri(@"/images/buttons/config/config_default.png", UriKind.Relative));
            }
        }
        void ConfigEnter(object sender, EventArgs e) {
            if(dll.IsEnabled){
                btn_image_dll.Source = new BitmapImage(new Uri(@"/images/buttons/config/config_hover.png", UriKind.Relative));
            }
        }
        void LangLeave(object sender, EventArgs e) {
            if(lang.IsEnabled) {
                btn_image_lang.Source = new BitmapImage(new Uri(@"/images/buttons/lang/lang_default.png", UriKind.Relative));
            }
        }
        void LangEnter(object sender, EventArgs e) {
            if(lang.IsEnabled){
                btn_image_lang.Source = new BitmapImage(new Uri(@"/images/buttons/lang/lang_hover.png", UriKind.Relative));
            }
        }
    }
}