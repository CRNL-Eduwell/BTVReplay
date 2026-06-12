using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class UiToBrainMessage
{
    // ChangeReferential cycles mni -> pat -> just electrodes.
    public enum Task
    {
        ChangeReferential = 0,
        ChangeMeshDisplay = 1,
        UpdateGain = 2,
    }

    public Task TaskToExecute
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
