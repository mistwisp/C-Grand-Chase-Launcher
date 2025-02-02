using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Security.Principal;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace GCLauncher.Source {
    public class Info {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        const int SW_RESTORE = 9;
        public int currentLang {  get; set; }
        public Language launcherLang { get; set; }
        public string launcherExec = Process.GetCurrentProcess().ProcessName;
        public string launcherURL = "http://127.0.0.1/";
        public string filesURL = "http://127.0.0.1/patch/"; // Complete URL path to the files foler, inside the "launcher" folder
        public Info() {
            launcherLang = new Language();
            SetCurrentLanguage();
        }
        public List<Version> VersionInfo { get; set; }
        public string GetFileString(int type) {
            string getString = null;
            switch(type) {
                case 0: getString = launcherURL + "/files.xml"; break;
                case 1: getString = launcherURL + "/maintenance.txt"; break;
                case 2: getString = launcherURL + "/launcher.txt"; break;
                case 3: getString = launcherURL + "/version.bin"; break;
            }
            try {
                using(var client = new WebClient()) {
                    return client.DownloadString(getString);
                }
            }
            catch {
                return null;
            }
        }
        public void StartMain() {
            string startLang = "KOR";
            switch(currentLang)
            {
                case 0:
                case 1:
                    startLang = "KOR";
                    break;
                case 2:
                    startLang = "ENG";
                    break;
                default:
                    startLang = "KOR";
                    break;
            }
            string mainParam = "__My_Main_Param__";
            string param = mainParam + " -ml " + startLang;
            string verMain = "Main.exe";
            if (File.Exists("_d3d9.dll")) {
                File.Move("_d3d9.dll", "d3d9.dll");
            }
            Process p = new Process();
            p.StartInfo.FileName = verMain;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.Arguments = param;
            p.Start();
            if (p != null)
            {
                p.WaitForInputIdle();
                IntPtr mainWindowHandle = FindWindow(null, p.MainWindowTitle);
                if (mainWindowHandle != IntPtr.Zero) {
                    SetForegroundWindow(mainWindowHandle);
                    ShowWindow(mainWindowHandle, SW_RESTORE);
                }
            }
        }
        public string FileData(string path, int opt) {
            if(opt == 0) {
                return Path.GetFileName(path);
            }
            if(opt == 1) {
                return Path.GetDirectoryName(path);
            }
            path.Replace('\'', '/');
            return path;
        }
        public static byte[] Combine(params byte[][] arrays) {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays) {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }
        public string createMD5FromFile(string input) {
            var FileBuffer = File.ReadAllBytes(input);
            MD5 md5 = MD5.Create();
            byte[] checksum = md5.ComputeHash(FileBuffer);
            return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
        }
        public void VersionPopulate() {
            string xmlTemp = GetFileString(0);
            using (StringReader stringReader = new StringReader(xmlTemp)) {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Version>));
                VersionInfo = (List<Version>)serializer.Deserialize(stringReader);
            }
        }
        public bool IsAdministrator() {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }
        public int GetCurrentVersion() {
            if (!File.Exists("version.bin")) {
                return 1;
            }
            using (StreamReader sr = new StreamReader("version.bin")) {
                int number = 1;
                if (int.TryParse(sr.ReadLine(), out number)) {
                    if (number < 1 || number > int.Parse(GetFileString(3))) {
                        return 1;
                    }
                    return number;
                }
                return 1;
            }
        }
        public void SetCurrentLanguage() {
            currentLang = GetCurrentLanguage();
        }
        public int GetCurrentLanguage() {
            if (!File.Exists("language.bin")) {
                return 0;
            }
            using (StreamReader sr = new StreamReader("language.bin")) {
                int number;
                if (int.TryParse(sr.ReadLine(), out number)) {
                    if (number < 0 || number > 2) {
                        return 0;
                    }
                    return number;
                }
                return 0;
            }
        }
        public void WriteVersion() {
            using (StreamWriter writer = new StreamWriter("version.bin")) {
                writer.WriteLine(GetFileString(3));
            }
        }
        public int GetTotalCount() {
            int totalCount = 0;
            int curVersion = GetCurrentVersion();
            int lastVersion = int.Parse(GetFileString(3));
            while (curVersion <= lastVersion) {
                totalCount += VersionInfo[curVersion - 1].FileList.Count;
                curVersion++;
            }
            return totalCount;
        }
    }
}