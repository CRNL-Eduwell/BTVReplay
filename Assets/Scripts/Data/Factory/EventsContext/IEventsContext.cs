using BTV.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data.Factory
{
    public interface IEventsContext
    {
        List<BtvEvent> Events
        {
            get;
        }
        string FilePath
        {
            get;
        }
    }
}
