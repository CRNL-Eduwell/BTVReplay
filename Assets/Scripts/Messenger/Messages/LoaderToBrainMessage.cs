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

    public BrainDataContainer Anatomy
    {
        get;
        set;
    }

    public EegTechnology Techno
    {
        get;
        set;
    }
}
