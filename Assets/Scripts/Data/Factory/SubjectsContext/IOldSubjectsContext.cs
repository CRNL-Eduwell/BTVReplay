using System.Collections.Generic;

namespace Assets.Scripts.Data.Factory
{
    public interface IOldSubjectsContext
    {
        List<OldSubject> Subjects
        {
            get;
        }
        string FilePath
        {
            get;
            set;
        }
    }
}