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
    UiToLayouts,
    EventsToTraceMessage,
    EventsToTaskPerformanceMessage,
    EventsModificationMessage,
    BrainWardenToElectrodePointerMessage,
    ReactionTimePointerMessage,
    BrainWardenToTraceMessage,
    ModulesToVideoMessage,
    VideoToModulesMessage,
    FileMenuMessage,
    EditMenuMessage,
    LoadSubjectMessage,
    LoaderMessage,
    ShowWindowMessage,
    ForceUpdateTraceMessage,
    ShortcutMessage,
    ForceToggleToolbar
}
