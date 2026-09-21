using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.CSharp.EEG;
using UnityEngine;

namespace BTV.Services.TaskPerformanceService
{
    public static class TaskPerformanceService
    {
        public static List<EegTrigger> ProcessedTriggers
        {
            get { return Session.Current.ProcessedTriggers; }
            private set { Session.Current.ProcessedTriggers = value; }
        }
        public static List<Color> Colors
        {
            get { return Session.Current.TaskPerformanceColors; }
            private set { Session.Current.TaskPerformanceColors = value; }
        }

        public static void Reset()
        {
            ProcessedTriggers = null;
            Colors = null;
        }

        //calculateReactionTime in old pos.cs
        public static void ProcessEventsForExperiment(Protocol protocol, int flagCode = 99, int DownsamplingFactor = 1)
        {
            ProcessEventsForExperiment(Session.Current, protocol, flagCode, DownsamplingFactor);
        }

        public static void ProcessEventsForExperiment(Session session, Protocol protocol, int flagCode = 99, int DownsamplingFactor = 1)
        {
            List<EegTrigger> Triggers = GetTriggerList(session, flagCode, DownsamplingFactor);
            if (protocol.ChangeCodeFilePath != "")
            {
                RenameTriggersForExperiment(protocol, ref Triggers);
            }
            PairStimulationWithResponses(protocol, ref Triggers);
            DeleteTriggerNotInExperiment(protocol, ref Triggers);

            //Define color in a hardcoded way, later will be done via conf file
            session.TaskPerformanceColors = DefineColorForTriggers(protocol, Triggers);
            session.ProcessedTriggers = Triggers;
        }

        //==== Private
        private static List<EegTrigger> GetTriggerList(Session session, int flagCode = 99, int DownsamplingFactor = 1)
        {
            int beginvalue = 0;
            if (flagCode != -1)
                beginvalue = FindFirstIndexAfter(session, flagCode);

            int TriggerCount = session.Events.Count;
            List<EegTrigger> triggers = new List<EegTrigger>();
            if (TriggerCount > 0)
            {
                for (int i = beginvalue; i < TriggerCount; i++)
                {
                    int code = session.Events[i].Code;
                    float time = session.Events[i].TimeInMilliSeconds;
                    triggers.Add(new EegTrigger(new EegEvent(code, time)));
                }
            }

            return triggers;

        }

        private static int FindFirstIndexAfter(Session session, int flagCode)
        {
            int beginValue = 0;
            List<int> indexBegin = session.Events
                .Select((btvEvent, index) => new { btvEvent, index })
                .Where(item => item.btvEvent.Code == flagCode)
                .Select(item => item.index)
                .ToList();
            if (indexBegin.Count > 0)
            { 
                for (int i = 1; i < indexBegin.Count; i++)
                {
                    if (indexBegin[i - 1] + 1 != indexBegin[i])
                    {
                        return indexBegin[i - 1] + 1;
                    }
                }
            }
            return beginValue;
        }

        private static void RenameTriggersForExperiment(Protocol protocol, ref List<EegTrigger> processedTriggers)
        {
            ChangeCodeFile chgCode = new ChangeCodeFile(protocol.ChangeCodeFilePath);

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
                            int BlocCount = protocol.Blocs.Count;
                            for (int n = 0; n < BlocCount; n++)
                            {
                                if (chgCode.NewCodes[l].Key == protocol.Blocs[n].mainEvent.code)
                                    idVisuBloc = n;
                            }
                        }
                    }
                }

                int memId = 0;
                if (idMain != -1)
                {
                    int[] blocEpochWindow = protocol.Blocs[idVisuBloc].dispBloc.epochWindow;
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

        private static void PairStimulationWithResponses(Protocol protocol, ref List<EegTrigger> processedTriggers)
        {
            List<KeyValuePair<int, int>> NewCodes = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < protocol.Blocs.Count; i++)
            {
                for (int j = 0; j < protocol.Blocs[i].secondaryEvents.code.Count(); j++)
                {
                    for (int k = 0; k < protocol.Blocs[i].secondaryEvents.code[j].Count(); k++)
                    {
                        KeyValuePair<int, int> kvp = new KeyValuePair<int, int>(protocol.Blocs[i].mainEvent.code, protocol.Blocs[i].secondaryEvents.code[j][k]);
                        NewCodes.Add(kvp);
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
                int newCodeIndex = -1;

                for (int l = 0; l < NewCodes.Count; l++)
                {
                    if (processedTriggers[k].MainEnventCode == NewCodes[l].Key)
                    {
                        idMain = k;
                        for (int m = 0; m < protocol.Blocs.Count; m++)
                        {
                            if (NewCodes[l].Key == protocol.Blocs[m].mainEvent.code)
                            {
                                idVisuBloc = m;
                                newCodeIndex = l;
                            }
                        }
                    }
                }

                if (idMain != -1)
                {
                    if (NewCodes[newCodeIndex].Value == -1)
                    {
                        processedTriggers[idMain].ResponseCode = -1;
                        processedTriggers[idMain].ResponsTimeInMilliSeconds = processedTriggers[idMain].MainEventTimeInMilliSeconds;
                    }
                    else
                    {
                        int[] blocEpochWindow = protocol.Blocs[idVisuBloc].dispBloc.epochWindow;
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
                                }
                                else if (processedTriggers[dd].MainEnventCode == NewCodes[l].Key && idSec == -1)
                                {
                                    idMain = dd;
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
        }

        private static void DeleteTriggerNotInExperiment(Protocol protocol, ref List<EegTrigger> processedTriggers)
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

        private static bool IsTriggerInExperiment(Protocol protocol, EegTrigger trigger)
        {
            if (trigger.ResponseCode == -666 && trigger.ResponsTimeInMilliSeconds == -666)
                return false;

            for (int i = 0; i < protocol.Blocs.Count; i++)
            {
                if (trigger.MainEnventCode == protocol.Blocs[i].mainEvent.code)
                    return true;
            }

            return false;
        }

        private static List<Color> DefineColorForTriggers(Protocol protocol, List<EegTrigger> processedTriggers)
        {
            List<Color> ProcessedColors = new List<Color>();

            List<KeyValuePair<int, int>> NewCodes = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < protocol.Blocs.Count; i++)
            {
                for (int j = 0; j < protocol.Blocs[i].secondaryEvents.code.Count(); j++)
                {
                    for (int k = 0; k < protocol.Blocs[i].secondaryEvents.code[j].Count(); k++)
                    {
                        KeyValuePair<int, int> kvp = new KeyValuePair<int, int>(protocol.Blocs[i].mainEvent.code, protocol.Blocs[i].secondaryEvents.code[j][k]);
                        NewCodes.Add(kvp);
                    }
                }
            }

            foreach (EegTrigger trigger in processedTriggers)
            {
                int count = 0;
                foreach (KeyValuePair<int, int> codes in NewCodes)
                {
                    if (codes.Key == trigger.MainEnventCode)
                    {
                        switch (count)
                        {
                            case 0:
                                {
                                    ProcessedColors.Add(Color.red);
                                }
                                break;
                            case 1:
                                {
                                    ProcessedColors.Add(Color.blue);
                                }
                                break;
                            case 2:
                                {
                                    ProcessedColors.Add(Color.gray);
                                }
                                break;
                            case 3:
                                {
                                    ProcessedColors.Add(Color.green);
                                }
                                break;
                            case 4:
                                {
                                    ProcessedColors.Add(Color.yellow);
                                }
                                break;
                            case 5:
                                {
                                    ProcessedColors.Add(Color.cyan);
                                }
                                break;
                        }
                        continue;
                    }
                    count++;
                }
            }
            return ProcessedColors;
        }
    }
}
