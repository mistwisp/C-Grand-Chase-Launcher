using System;
using Crc32C;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LauncherFiles.Source{
    public class Info{
        string FileDir = AppDomain.CurrentDomain.BaseDirectory;
        public List<Arquivos> FileInfoz = new List<Arquivos>();
        public static byte[] ConverteStreamToByteArray(Stream stream){
            byte[] byteArray = new byte[16 * 1024];
            using(MemoryStream mStream = new MemoryStream()){
                int bit;
                while((bit = stream.Read(byteArray, 0, byteArray.Length)) > 0){
                    mStream.Write(byteArray, 0, bit);
                }
                return mStream.ToArray();
            }
        }
        public string GetChecksumBuffered(string filePath){
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            MemoryStream ms = new MemoryStream((int)stream.Length);
            stream.CopyTo(ms);
            uint crc = Crc32CAlgorithm.Compute(ms.ToArray());
            return crc.ToString();
        }
        public void FileListCreate(){
            int FileID = 1;
            string[] allfiles = Directory.GetFiles(FileDir, "*.*", SearchOption.AllDirectories);
            foreach(var file in allfiles){
                FileInfo info = new FileInfo(file);
                if(info.Name != "LauncherFiles.exe" && info.Name != "grandchase.dll" && info.Name != "grandchase.exe"){
                    string hashFinal = null;
                    hashFinal = GetChecksumBuffered(@info.DirectoryName + "\\" + info.Name);
                    Uri TempPath = new Uri(info.Directory + "\\" + info.Name);
                    Uri TempFolder = new Uri(FileDir);
                    string RelativePath = Uri.UnescapeDataString(TempFolder.MakeRelativeUri(TempPath).ToString().Replace('/', Path.DirectorySeparatorChar));
                    FileInfoz.Add(new Arquivos(){
                        FileID = FileID,
                        FilePath = RelativePath,
                        FileHash = hashFinal
                    });
                    Console.WriteLine("FileID: " + FileID + " - FilePath: " + RelativePath + " - Hash: " + hashFinal);
                    FileID++;
                }
            }
        }
        public void FileListWrite() {
            var json = JsonConvert.SerializeObject(FileInfoz, Formatting.Indented);
            File.Create("files.json").Dispose();
            using(var tw = new StreamWriter("files.json")){
                tw.Write(json);
            }
            Console.WriteLine("File List was written on file!");
        }
    }
}