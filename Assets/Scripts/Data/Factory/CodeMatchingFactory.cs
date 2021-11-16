using Assets.Scripts.Data.Files;
using BTV.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data.Factory
{
    public class CodeMatchingFactory
    {
        public static ICodeCommentContext GetMatchingContext(string FilePath)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".match":
                    return new MatchFile(FilePath);
                default:
                    throw new ArgumentException("EventsFactory.GetEventsContext : file extension not supported => " + fileInfo.Extension); ;
            }
        }

        public static void SaveMatchingContext(string FilePath)
        {
            throw new ArgumentException("CodeMatchingFactory.SaveMatchingContext : Matching file saving is not supported at the moment");
        }
    }
}
