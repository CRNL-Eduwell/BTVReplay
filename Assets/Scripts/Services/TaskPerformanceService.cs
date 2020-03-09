using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.CSharp.EEG;

namespace BTV.Services.TaskPerformanceService
{
    public static class TaskPerformanceService
    {
        public static List<EegTrigger> ProcessedTriggers { get; private set; } = null;

        public static void Reset()
        {
            ProcessedTriggers = null;
        }

        //calculateReactionTime in old pos.cs
        public static void ProcessEventsForExperiment(ProvFile protocol, int flagCode = 99, int DownsamplingFactor = 1)
        {
            List<EegTrigger> Triggers = GetTriggerList(flagCode, DownsamplingFactor);
            if (protocol.changeCodeFilePath != "")
            {
                RenameTriggersForExperiment(protocol, ref Triggers);
            }
            PairStimulationWithResponses(protocol, ref Triggers);
            DeleteTriggerNotInExperiment(protocol, ref Triggers);

            //if (myprovFile->getSecondaryCodes()[0][0] != 0) //At this point , if there is secondary code, we need to check if all have been paired correctly 
            //{
            //	DeleteTriggerNotPaired(m_processedTriggers);
            //}
            //m_subGroupStimTrials = SortTrialsForExperiment(m_processedTriggers, myprovFile);
            //if (myprovFile->invertmapsinfo != "")
            //{
            //	SwapStimulationsAndResponses(myprovFile);
            //}

            ProcessedTriggers = Triggers;
        }

        //==== Private
        private static List<EegTrigger> GetTriggerList(int flagCode = 99, int DownsamplingFactor = 1)
        {
            int beginvalue = 0;
            if (flagCode != -1)
                beginvalue = FindFirstIndexAfter(flagCode);

            int TriggerCount = EventsService.EventsService.Events.Count;
            List<EegTrigger> triggers = new List<EegTrigger>();
            if (TriggerCount > 0)
            {
                for (int i = beginvalue; i < TriggerCount; i++)
                {
                    int code = EventsService.EventsService.Events[i].Code;
                    float time = EventsService.EventsService.Events[i].TimeInMilliSeconds;
                    triggers.Add(new EegTrigger(new EegEvent(code, time)));
                }
            }

            return triggers;

        }

        private static int FindFirstIndexAfter(int flagCode)
        {
            int beginValue = 0;
            List<int> indexBegin = EventsService.EventsService.FindIndexes(flagCode);
            if (indexBegin.Count > 0)
                beginValue = indexBegin[indexBegin.Count - 1] + 1;
            return beginValue;
        }

        private static void RenameTriggersForExperiment(ProvFile protocol, ref List<EegTrigger> processedTriggers)
        {
            ChangeCodeFile chgCode = new ChangeCodeFile(protocol.changeCodeFilePath);

            int TriggerCount = processedTriggers.Count;
            for (int k = 0; k < TriggerCount; k++)
            {
                int idVisuBloc = -1;
                int idMain = -1;
                int idSec = -1;

                for (int l = 0; l < chgCode.OldCodes.Count; l++)
                {
                    for (int m = 0; m < chgCode.OldCodes[l].Count; m++)
                    {
                        if (processedTriggers[k].MainEnventCode == chgCode.OldCodes[l][m].Key)
                        {
                            idMain = k;
                            int BlocCount = protocol.blocs.Count;
                            for (int n = 0; n < BlocCount; n++)
                            {
                                if (chgCode.NewCodes[l].Key == protocol.blocs[n].mainEvent.code)
                                    idVisuBloc = n;
                            }
                        }
                    }
                }

                int memId = 0;
                if (idMain != -1)
                {
                    int[] blocEpochWindow = protocol.blocs[idVisuBloc].dispBloc.epochWindow;
                    int winMsMin = blocEpochWindow[0];
                    int winMsMax = blocEpochWindow[1];

                    int CurrentTriggerId = k + 1;
                    while (idSec == -1 && CurrentTriggerId < TriggerCount - 1)
                    {
                        for (int l = 0; l < chgCode.OldCodes.Count; l++)
                        {
                            for (int m = 0; m < chgCode.OldCodes[l].Count; m++)
                            {
                                if (processedTriggers[idMain].MainEnventCode == chgCode.OldCodes[l][m].Key && processedTriggers[CurrentTriggerId].MainEnventCode == chgCode.OldCodes[l][m].Value)
                                {
                                    idSec = CurrentTriggerId;
                                    memId = l;
                                    break;
                                }
                                else if (processedTriggers[CurrentTriggerId].MainEnventCode == chgCode.OldCodes[l][m].Key && idSec == -1)
                                {
                                    idMain = CurrentTriggerId;
                                    memId = l;
                                }
                            }
                        }
                        CurrentTriggerId++;
                    }

                    if (idMain != -1 && idSec != -1)
                    {
                        int winMax = (int)processedTriggers[idMain].MainEventTimeInMilliSeconds + winMsMax;
                        int winMin = (int)processedTriggers[idMain].MainEventTimeInMilliSeconds - Math.Abs(winMsMin);

                        bool isInWindow = (processedTriggers[idSec].MainEventTimeInMilliSeconds < winMax) && (processedTriggers[idSec].MainEventTimeInMilliSeconds > winMin);
                        if (isInWindow)
                        {
                            processedTriggers[idMain].MainEnventCode = chgCode.NewCodes[memId].Key;
                            processedTriggers[idSec].MainEnventCode = chgCode.NewCodes[memId].Value;
                        }
                    }
                }
            }
            //removeDuplicateEventCode();
        }

        private static void PairStimulationWithResponses(ProvFile protocol, ref List<EegTrigger> processedTriggers)
        {
            List<KeyValuePair<int, int>> NewCodes = new List<KeyValuePair<int, int>>();

            if (System.IO.File.Exists(protocol.changeCodeFilePath))
            {
                ChangeCodeFile chgCode = new ChangeCodeFile(protocol.changeCodeFilePath);
                for (int i = 0; i < chgCode.NewCodes.Count; i++)
                {
                    NewCodes.Add(chgCode.NewCodes[i]);
                }
            }
            else // in that case, no change code, no new codes it's only the normal codes
            {
                for (int i = 0; i < protocol.blocs.Count; i++)
                {
                    for (int j = 0; j < protocol.blocs[i].secondaryEvents.code.Count(); j++)
                    {
                        for (int k = 0; k < protocol.blocs[i].secondaryEvents.code[j].Count(); k++)
                        {
                            KeyValuePair<int, int> kvp = new KeyValuePair<int, int>(protocol.blocs[i].mainEvent.code, protocol.blocs[i].secondaryEvents.code[j][k]);
                            NewCodes.Add(kvp);
                        }
                    }
                }
            }

            int TriggerCount = processedTriggers.Count;
            for (int k = 0; k < TriggerCount; k++)
            {
                int idVisuBloc = -1;
                int idMain = -1;
                int idSec = -1;
                int dd = -1;
                int idcode = -1;

                for (int l = 0; l < NewCodes.Count; l++)
                {
                    if (processedTriggers[k].MainEnventCode == NewCodes[l].Key)
                    {
                        idMain = k;
                        for (int m = 0; m < protocol.blocs.Count; m++)
                        {
                            if (NewCodes[l].Key == protocol.blocs[m].mainEvent.code)
                                idVisuBloc = m;
                        }
                    }
                }

                if (idMain != -1)
                {
                    int[] blocEpochWindow = protocol.blocs[idVisuBloc].dispBloc.epochWindow;
                    int winSamMin = blocEpochWindow[0];
                    int winSamMax = blocEpochWindow[1];

                    dd = k + 1;

                    while (idSec == -1 && dd < processedTriggers.Count - 1)
                    {
                        for (int l = 0; l < NewCodes.Count; l++)
                        {
                            if (processedTriggers[dd].MainEnventCode == NewCodes[l].Value &&
                                processedTriggers[idMain].MainEnventCode == NewCodes[l].Key)
                            {
                                idSec = dd;
                                idcode = l;
                            }
                            else if (processedTriggers[dd].MainEnventCode == NewCodes[l].Key && idSec == -1)
                            {
                                idMain = dd;
                                idcode = l;
                            }
                        }
                        dd++;
                    }

                    if (idMain != -1 && idSec != -1)
                    {
                        int winMax = (int)processedTriggers[idMain].MainEventTimeInMilliSeconds + winSamMax;
                        int winMin = (int)processedTriggers[idMain].MainEventTimeInMilliSeconds - Math.Abs(winSamMin);

                        bool isInWindow = (processedTriggers[idSec].MainEventTimeInMilliSeconds < winMax) && (processedTriggers[idSec].MainEventTimeInMilliSeconds > winMin);
                        if (isInWindow)
                        {
                            processedTriggers[idMain].ResponseCode = processedTriggers[idSec].MainEnventCode;
                            processedTriggers[idMain].ResponsTimeInMilliSeconds = processedTriggers[idSec].MainEventTimeInMilliSeconds;
                        }
                    }
                }
            }
        }

        private static void DeleteTriggerNotInExperiment(ProvFile protocol, ref List<EegTrigger> processedTriggers)
        {
            List<int> IDsToDelete = new List<int>();
            for (int i = 0; i < processedTriggers.Count; i++)
            {
                bool isTriggerIn = IsTriggerInExperiment(protocol, processedTriggers[i]);
                if (!isTriggerIn)
                    IDsToDelete.Add(i);
            }

            for (int i = IDsToDelete.Count - 1; i >= 0; i--)
                processedTriggers.RemoveAt(IDsToDelete[i]);
        }

        private static bool IsTriggerInExperiment(ProvFile protocol, EegTrigger trigger)
        {
            if (trigger.ResponseCode == -666 && trigger.ResponsTimeInMilliSeconds == -666)
                return false;

            for (int i = 0; i < protocol.blocs.Count; i++)
            {
                if (trigger.MainEnventCode == protocol.blocs[i].mainEvent.code)
                    return true;
            }

            return false;
        }
    }
}
