using System;
using System.Collections.Generic;

namespace UpdateListGenerator
{
    [Serializable]
    public class Version
    {
        public int VersionID { get; set; }
        public List<Arquivo> FileList { get; set; }
        public bool ToUpdate { get; set; } = false;
    }
}
