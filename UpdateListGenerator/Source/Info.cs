using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace UpdateListGenerator
{
    public class Info {
        public List<Version> VersionInfo = new List<Version>();
        static T DeserializeFromXml<T>(string filePath) {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open)) {
                return (T)serializer.Deserialize(fileStream);
            }
        }
        public void VersionPopulate() {
            if (File.Exists("files.xml")) {
                VersionInfo = DeserializeFromXml<List<Version>>("files.xml");
            }
        }
        public bool IsNewFile(string filePath, string brutePath) {
            foreach (Version v in VersionInfo) {
                if (v.FileList.Any(obj => obj.FilePath == filePath && obj.FileHash == createMD5FromFile(brutePath))) {
                    return false;
                }
            }
            foreach (Version v in VersionInfo) {
                v.FileList.RemoveAll(obj => obj.FilePath == filePath);
                UpdateListID();
            }
            return true;
        }
        public void UpdateListID() {
            int nextNumber = 1;
            foreach (Version v in VersionInfo) {
                foreach(Arquivo a in v.FileList ) {
                    a.FileID = nextNumber;
                    nextNumber++;
                }
                nextNumber = 1;
            }
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
        int GetHighestVersion(int sum) {
            int versionSum = VersionInfo.Count + sum;
            if (versionSum < 1)
                return 1;
            return VersionInfo.Count + sum;
        }
        private List<string> DirSearch() {
            string sDir = "patch";
            string [] files = Directory.GetFiles(sDir, "*", SearchOption.AllDirectories);
            List<string> list = new List<string>(files);
            return list;
        }
        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
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
        public void CreateFileList(List<string> files) {
            string sDir = "patch";
            int fileID = 0;
            Version verTemp = new Version();
            verTemp.VersionID = GetHighestVersion(1);
            verTemp.FileList = new List<Arquivo>();
            for (int i = 0; i < files.Count; i++){
                string filePathTemp = files[i].Remove(0, sDir.Length + 1);
                if (IsNewFile(filePathTemp, files[i])) {
                    Arquivo Temp = new Arquivo();
                    Temp.FileID = fileID + 1;
                    Temp.FileHash = createMD5FromFile(files[i]);
                    Temp.FilePath = filePathTemp;
                    verTemp.FileList.Add(Temp);
                    Console.WriteLine("Add file: " + verTemp.FileList[fileID].FilePath + " - MD5: " + verTemp.FileList[fileID].FileHash);
                    fileID++;
                }
            }
            if (verTemp.FileList.Count > 0) {
                VersionInfo.Add(verTemp);
            }
        }
        private void SetLauncherSHA1(){
            using (var cryptoProvider = new SHA1CryptoServiceProvider()){
                FileStream fs = File.OpenRead("Launcher.exe");
                string hash = BitConverter.ToString(cryptoProvider.ComputeHash(fs)).ToLower().Replace("-", string.Empty);
                fs.Close();
                File.Create("launcher.txt").Dispose();
                using (var tw = new StreamWriter("launcher.txt")){
                    tw.Write(hash);
                }
            }
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine(" Writting launcher SHA1 to file...");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }
        public void WriteXML() {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine(" Writting XML to file...");
            File.Create("files.xml").Dispose();
            using (var tw = new StreamWriter("files.xml")){
                var serializer = new XmlSerializer(typeof(List<Version>));
                serializer.Serialize(tw, VersionInfo);
            }
        }
        static private void HeaderText() {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ___ _ _   ___   _   _  __ _____   __ __  __  _  _____ ___    ");
            Console.WriteLine(" | __| | | | __| | | | /' _|_   _| |  V  |/  \\| |/ | __| _ \\ ");
            Console.WriteLine(" | _|| | |_| _|  | |_| `._`. | |   | \\_/ | /\\ |   <| _|| v / ");
            Console.WriteLine(" |_| |_|___|___| |___|_|___/ |_|   |_| |_|_||_|_|\\_|___|_|_\\ ");
            Console.ResetColor();
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }
        public void WriteVersion() {
            using (StreamWriter writer = new StreamWriter("version.bin")) {
                writer.WriteLine(GetHighestVersion(0));
            }
        }
        public void MakeFileListXML() {
            VersionPopulate();
            HeaderText();
            List<string> FileList = DirSearch();
            CreateFileList(FileList);
            WriteXML();
            SetLauncherSHA1();
            WriteVersion();
            Console.WriteLine("Done!");
        }
    }
}