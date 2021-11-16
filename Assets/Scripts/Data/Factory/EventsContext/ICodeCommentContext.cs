using BTV.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Factory
{
    public interface ICodeCommentContext
    {
        List<CodeCommentPair> Pairs
        {
            get;
        }
        string FilePath
        {
            get;
        }
    }
}
