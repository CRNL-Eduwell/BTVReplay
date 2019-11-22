using System;
using System.Collections.Generic;
using System.Linq;
using Tools.CSharp.EEG;

namespace BTV.Data
{
    public class DataContainer
    {
        public Dictionary<string, float[]> ValuesByChannel { get; set; } = new Dictionary<string, float[]>();
        public Dictionary<string, string> UnitByChannel { get; set; } = new Dictionary<string, string>();
        public Frequency Frequency { get; set; } = new Frequency();
        public string FilePath
        {
            get;
            private set;
        }

        public DataContainer(string path)
        {
            FilePath = path;
        }   
    }
}
