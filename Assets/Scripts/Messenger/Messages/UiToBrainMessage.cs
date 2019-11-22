using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class UiToBrainMessage
{
    // 0 : CHange Brain Referential => mni to pat to just electrodes
    // 1 : Change what part of the brain is shown
    // 2 : Update Electrodes Gain
    public int TaskToExecute
    {
        get;
        set;
    }

    public int ModelId
    {
        get;
        set;
    }

    //-1 : Only left
    // 0 : Both 
    // 1 : Only Right
    public int MeshesToDisplay
    {
        get;
        set;
    }

    public float Gain
    {
        get;
        set;
    }

}
