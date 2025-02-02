using System;

namespace GCLauncher.Source {
    [Serializable]
    public class Arquivo {
        public int FileID { get; set; }
        public string FileHash { get; set; }
        public string FilePath { get; set; }
        public bool ToUpdate { get; set; } = false;
    }
}