using System;
using LauncherFiles.Source;

namespace LauncherFiles{
    class Program{
        static void Main(){
            Info LauncherInfo = new Info();
            Console.WriteLine("Infinity Launcher FileList Creator");
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Creating file list....");
            Console.WriteLine("----------------------------------");
            LauncherInfo.FileListCreate();
            Console.WriteLine("----------------------------------");
            Console.WriteLine("File list created!");
            Console.WriteLine("Writing File List to file...");
            LauncherInfo.FileListWrite();
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Dome! Press any key to exit...");
            Console.ReadKey();
        }
    }
}
