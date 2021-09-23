using Crc32C;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Controls;
using System.Security.Principal;
using System.Collections.Generic;
using System;

namespace GCLauncher.Source{
    public class Info{
        public string launcherExec = Process.GetCurrentProcess().ProcessName;
        public string launcherURL = "https://gclive.com.br/launcher";
        private string launcherParam = "_HarbleyImpy_GcLive369956543";
        public List<Arquivo> FileInfo { get; set; }
        public string GetFileString(int type){
            string getString = null;
            switch(type){
                case 0: getString = launcherURL + "/files.json"; break;
                case 1: getString = launcherURL + "/maintenance.txt"; break;
                case 2: getString = launcherURL + "/launcher.txt"; break;
                case 3: getString = launcherURL + "/client.json"; break;
            }
            try{
                using(var client = new WebClient()){
                    return client.DownloadString(getString);
                }
            }
            catch{
                return null;
            }
        }
        public void StartMain(){
            ProcessStartInfo startInfo = new ProcessStartInfo("main.exe");
            startInfo.Arguments = launcherParam;
            Process.Start(startInfo);
        }
        public string FileData(string path, int opt){
            if(opt == 0){
                return Path.GetFileName(path);
            }
            if(opt == 1){
                return Path.GetDirectoryName(path);
            }
            path.Replace('\'', '/');
            return path;
        }
        public string GetChecksumBuffered(string filePath){
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[stream.Length];
            int read = 0;
            int chunk;
            uint crc;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0){
                read += chunk;
                if(read == buffer.Length){
                    int nextByte = stream.ReadByte();
                    if(nextByte == -1){
                        crc = Crc32CAlgorithm.Compute(buffer);
                        return crc.ToString();
                    }
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            crc = Crc32CAlgorithm.Compute(ret);
            return crc.ToString();
        }
        public Arquivo FileCheckUpdate(Arquivo unit, int total, ref ProgressBar LauncherBarTotal, ref TextBlock splashText){
            int launcherPercent = (100 * unit.FileID) / total;
            LauncherBarTotal.Value = launcherPercent;
            splashText.Text = "Verificando arquivos " + "- " + launcherPercent + "%";
            if(!File.Exists(@unit.FilePath)){
                unit.ToUpdate = true;
            }
            else{
                if(GetChecksumBuffered(@unit.FilePath) != unit.FileHash){
                    unit.ToUpdate = true;
                }
            }
            return unit;
        }
        public void FilePopulate(int option){
            if(option > 0){
                FileInfo = JsonConvert.DeserializeObject<List<Arquivo>>(GetFileString(3));
            }
            else{ 
                FileInfo = JsonConvert.DeserializeObject<List<Arquivo>>(GetFileString(0));
            }
        }
        public bool IsAdministrator(){
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}