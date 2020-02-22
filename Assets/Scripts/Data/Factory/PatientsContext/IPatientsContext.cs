using System.Collections.Generic;

namespace Assets.Scripts.Data.Factory
{
    public interface IPatientsContext
    {
        List<Patient> Patients
        {
            get;
        }
        string FilePath
        {
            get;
        }
    }
}
