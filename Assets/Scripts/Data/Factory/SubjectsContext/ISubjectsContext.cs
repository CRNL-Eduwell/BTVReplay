using System.Collections.Generic;

namespace Assets.Scripts.Data.Factory
{
    public interface ISubjectsContext
    {
        List<Subject> Subjects
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