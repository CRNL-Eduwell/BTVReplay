using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class LoaderToBrainMessage
{
    public bool HasAnatomy
    {
        get;
        set;
    }

    public brain_anat Anatomy
    {
        get;
        set;
    }

    public eeg_Technology Techno
    {
        get;
        set;
    }
}
