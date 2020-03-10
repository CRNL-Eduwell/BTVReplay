using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data.Factory
{
    public class ElectrodesFactory
    {
        public static IElectrodesContext GetElectrodeContext(EegTechnology eeg)
        {
            switch (eeg)
            {
                case EegTechnology.Intra:
                    return new IntraContext();
                case EegTechnology.Scalp:
                    return new ScalpContext();
                default:
                    throw new ArgumentException("ElectrodesFactory.GetElectrodeContext : eeg_Technology value unknown => " + eeg);
            }
        }
    }
}
