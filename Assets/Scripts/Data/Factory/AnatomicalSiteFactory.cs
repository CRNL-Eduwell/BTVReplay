using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data.Factory
{
    public class AnatomicalSiteFactory
    {
        public static IAnatomicalSiteContext GetAnatomicalSiteContext(string FilePath)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch(fileInfo.Extension)
            {
                case ".pts":
                    return new PtsFile2(FilePath);
                default:
                    throw new ArgumentException("AnatomicalSiteFactory.GetAnatomicalSiteContext : file extension unknown");
            }
        }
    }
}

