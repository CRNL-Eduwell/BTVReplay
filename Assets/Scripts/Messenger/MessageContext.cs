using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

enum MessageContext
{
    LoaderToBrain,
    UiToBrain,
    UiToTrace,
    UiToTaskPerformanceMessage,
    UiToVideo,
    UiToEvents,
    EventsToTraceMessage,
    EventsToTaskPerformanceMessage,
    EventsModificationMessage,
    BrainWardenToElectrodePointerMessage,
    BrainWardenToTraceMessage,
}
