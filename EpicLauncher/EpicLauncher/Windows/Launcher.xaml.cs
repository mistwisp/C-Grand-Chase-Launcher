using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Threading;
using GCLauncher.Source;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace GCLauncher{
    public partial class Launcher : Window{
        public int selected { get; set; } = 0;
        public Repair RepairWindow { get; set; }
        public int updateCount { get; set; } = 0;
        public int progressRegulator { get; set; } = 0;
        public static Info launcherInfo = new Info();
        private static string CleanifyBrowserPath(string p){
            string[] url = p.Split('"');
            string clean = url[1];
            return clean;
        }
        public static string GetDefaultBrowserPath(){
	        string urlAssociation = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http";
	        string browserPathKey = @"$BROWSER$\shell\open\command";
	        RegistryKey userChoiceKey;
            try
            {
		        userChoiceKey = Registry.CurrentUser.OpenSubKey(urlAssociation + @"\UserChoice", false);
		        if(userChoiceKey == null){
			        var browserKey = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);
			        if(browserKey == null){
				        browserKey = Registry.CurrentUser.OpenSubKey(urlAssociation, false);
			        }
			        var path = CleanifyBrowserPath(browserKey.GetValue(null) as string);
			        browserKey.Close();
			        return path;
		        }
		        else{
			        string progId = (userChoiceKey.GetValue("ProgId").ToString());
			        userChoiceKey.Close();
			        string concreteBrowserKey = browserPathKey.Replace("$BROWSER$", progId);
			        var kp = Registry.ClassesRoot.OpenSubKey(concreteBrowserKey, false);
                    string browserPath = CleanifyBrowserPath(kp.GetValue(null) as string);
                    kp.Close();
			        return browserPath;
		        }
	        }
	        catch(Exception){
		        return "";
	        }
        }
        public Launcher(List<Arquivo> Files, int countUpdate){
            launcherInfo.FileInfo = Files;
            updateCount = countUpdate;
            InitializeComponent();
            LauncherInt();
            WorkerUpdating(0);
        }
        private void WorkerComplete(){
            totalText.Text = "Atualização completa!";
            game.IsEnabled = true;
            btnrpr.IsEnabled = true;
            percentText.Text = "100%";
            DownloadBarFiles.Value = 100;
            DownloadProgress.Margin = new Thickness { Left = 300 };
            btn_image_game.Source = new BitmapImage(new Uri(@"/Images/play/active.png", UriKind.Relative));
        }
        private void WorkerMaintenance(){
            totalText.Text = "Servidor em manutenção!";
            btnrpr.IsEnabled = true;
            percentText.Text = "100%";
            DownloadBarFiles.Value = 100;
            DownloadProgress.Margin = new Thickness { Left = 300 };
        }
        private int GetBarPercentage(int min, int max){
            return ((100 * min) / max);
        }
        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e){
            string myFileNameID = ((WebClient)(sender)).QueryString["fileName"];
            string myFilePosition = ((WebClient)(sender)).QueryString["fileCurrent"];
            string filesTotal = ((WebClient)(sender)).QueryString["fileTotal"];
            int barPercentage = GetBarPercentage(int.Parse(myFilePosition), int.Parse(filesTotal));
            if(progressRegulator != barPercentage){
                progressRegulator = barPercentage;
            }
            totalText.Text = "Baixando: " + myFileNameID + " - " + e.ProgressPercentage + "%";
            percentText.Text = progressRegulator + "%";
            DownloadBarFiles.Value = progressRegulator;
            DownloadProgress.Margin = new Thickness { Left = progressRegulator * 3 };
        }
        public List<Arquivo> LauncherRepair(List<Arquivo> FileList, ref int FileUpdatecount, int option){
            switch(option){
                case 1:
                    foreach(Arquivo FileUnit in FileList){
                        if(FileUnit.FilePath.Contains("Stage\\") || FileUnit.FilePath.Contains("Model\\") || FileUnit.FilePath.Contains("sbta") || FileUnit.FilePath.Contains("tex_abta\\")){
                            FileUnit.ToUpdate = true;
                        }
                    }
                    FileUpdatecount = (from temp in FileList where temp.ToUpdate.Equals(true) select temp).Count();
                break;
                case 2:
                    FileList.All(c => { c.ToUpdate = true; return true; });
                    FileUpdatecount = (from temp in FileList where temp.ToUpdate.Equals(true) select temp).Count();
                break;
            }
            return FileList;
        }
        public async void WorkerUpdating(int option){
            string launcherMaintenance = launcherInfo.GetFileString(1);
            int updateRemaining = updateCount;
            int ammountUpdated = 1;
            if(option > 0){
                launcherInfo.FileInfo.Clear();
                launcherInfo.FilePopulate(3);
                launcherInfo.FileInfo = LauncherRepair(launcherInfo.FileInfo, ref updateRemaining, option);
            }
            foreach(Arquivo fileUnit in launcherInfo.FileInfo){
                if(fileUnit.ToUpdate){
                    string path = launcherInfo.FileData(fileUnit.FilePath, 1);
                    string inverted = launcherInfo.FileData(fileUnit.FilePath, 2);
                    if (!Directory.Exists(path) && path != ""){
                        Directory.CreateDirectory(path);
                    }
                    if(File.Exists(fileUnit.FilePath)){
                        File.Delete(fileUnit.FilePath);
                    }
                    string output = null;
                    using(var client = new WebClient()){
                        client.DownloadFileCompleted += (sender, e) =>{
                            output = e.ToString();
                        };
                        client.QueryString.Add("fileName", launcherInfo.FileData(fileUnit.FilePath, 0).ToString());
                        client.QueryString.Add("fileCurrent", ammountUpdated.ToString());
                        client.QueryString.Add("fileTotal", updateRemaining.ToString());
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                        client.DownloadFileAsync(new Uri(launcherInfo.launcherURL + "/Update/" + inverted), fileUnit.FilePath);
                        var n = DateTime.Now;
                        while(output == null){
                            await Task.Delay(1);
                        }
                    }
                    ammountUpdated++;
                }
            }
            updateCount = 0;
            if(launcherMaintenance == "true"){
                WorkerMaintenance();
            }
            else{
                WorkerComplete();
            }
        }
        private void GameStart(){
            launcherInfo.StartMain();
            Hide();
            Thread.Sleep(10000);
            Close();
        }
        private void SecondaryWindow(){
            RepairWindow = new Repair {
                Owner = this
            };
            RepairWindow.Show();
            RepairWindow.Left = Left;
            RepairWindow.Top = Top;
        }
        private void SecondaryRender(){
            SecondaryWindow();
        }
        private void LauncherInt(){
            string launcherMaintenance = launcherInfo.GetFileString(1);
            if(launcherMaintenance == "true"){
                serverText.Text = "Servidor: EM MANUTENÇÃO";
                serverText.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else{
                serverText.Text = "Servidor: ONLINE";
                serverText.Foreground = new SolidColorBrush(Colors.Green);
            }
            totalText.Text = "Criando lista de atualização";
            DownloadBarFiles.Value = 0;
            game.IsEnabled = false;
            btnrpr.IsEnabled = false;
            btn_image_game.Source = new BitmapImage(new Uri(@"/Images/play/inactive.png", UriKind.Relative));
        }
        private void ClickLauncher(object sender, RoutedEventArgs e){
            string content = (sender as Button).Name.ToString();
            switch(content){
                case   "game":                                                                     GameStart(); break;
                case "btnrpr":                                                               SecondaryRender(); break;
                case    "clb":                                                                         Close(); break;
                case    "ldb": Process.Start(GetDefaultBrowserPath(), "https://discordapp.com/invite/A3DpVzh"); break;
                case    "lfb":   Process.Start(GetDefaultBrowserPath(), "https://www.facebook.com/GrandChaseLiveBR"); break;
                case    "lsb":          Process.Start(GetDefaultBrowserPath(), "https://gclive.com.br/"); break;
            }
        }
        public static bool IsWindowOpen<T>(string name = "") where T : Window{
            return string.IsNullOrEmpty(name)?Application.Current.Windows.OfType<T>().Any():Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e){
            DragMove();
        }
        void GameLeave(object sender, EventArgs e){
            if(game.IsEnabled){
                btn_image_game.Source = new BitmapImage(new Uri(@"/Images/play/active.png", UriKind.Relative));
            }
        }
        void GameEnter(object sender, EventArgs e){
            if(game.IsEnabled){
                btn_image_game.Source = new BitmapImage(new Uri(@"/Images/play/hover.png", UriKind.Relative));
            }
        }
    }
}