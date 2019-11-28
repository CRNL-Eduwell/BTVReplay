using BTV.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IEventsService
{
    List<BtvEvent> GetAllEvents();
    List<BtvEvent> GetEventsBeetween(); //beetween two times , sample or ms ? 
    //GetEvent(TraceEvent Event) => some functions to find in list ? 
    //UpdateEvent(TraceEvent Event) => update evnt ? 


}