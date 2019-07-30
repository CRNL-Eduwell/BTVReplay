using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data.Factory
{
    public class ElectrodesFactory
    {
        public static IElectrodesContext GetElectrodeContext(eeg_Technology eeg)
        {
            switch (eeg)
            {
                case eeg_Technology.intra:
                    return new IntraContext();
                case eeg_Technology.scalp:
                    return new ScalpContext();
                default:
                    throw new ArgumentException("ElectrodesFactory.GetElectrodeContext : eeg_Technology value unknown => " + eeg);
            }
        }
    }
}
