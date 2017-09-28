using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class eventEeg
{
    public eventEeg(int code, int sample = -1, int samplingFreq = -1,  int duration = 0, string elecOfInterest = "", string secondElecOfInterest = "", string comment = "")
    {
        this.code = code;
        this.sample = sample;
        this.samplingFreq = samplingFreq;
        this.duration = duration;
        this.elecOfInterest = elecOfInterest;
        this.secondElecOfInterest = secondElecOfInterest;
        this.comment = comment;
    }

    public eventEeg(eventEeg currentEvent)
    {
        this.code = currentEvent.code;
        this.sample = currentEvent.sample;
        this.samplingFreq = currentEvent.samplingFreq;
        this.timeMs = currentEvent.timeMs;
        this.duration = currentEvent.duration;
        this.elecOfInterest = currentEvent.elecOfInterest;
        this.secondElecOfInterest = currentEvent.secondElecOfInterest;
        this.comment = currentEvent.comment;
    }

    public float getTimeSec()
    {
        return (float)sample / samplingFreq;
    }

    public float getTimeMSec()
    {
        return getTimeSec() * 1000;
    }

    public int code = -1;
    public int sample = -1;
    public int samplingFreq = -1;
    public int timeMs = -1;
    public int duration = 0;
    public string elecOfInterest = "";
    public string secondElecOfInterest = "";
    public string comment = "";
}

public class trigg
{
    public trigg(trigg trigger)
    {
        this.trigger = new eventEeg(trigger.trigger);
        this.response = new eventEeg(trigger.response);
    }

    public trigg(eventEeg trigger)
    {
        this.trigger = new eventEeg(trigger);
    }

    public trigg(eventEeg trigger, eventEeg response)
    {
        this.trigger = new eventEeg(trigger);
        this.response = new eventEeg(response);
    }

    ~trigg()
    {

    }

    public static bool operator !=(trigg c1, trigg c2)
    {
        return !(c1 == c2);
    }

    public static bool operator ==(trigg c1, trigg c2)
    {
        if (c1.trigger.code == c2.trigger.code)
            return true;
        else
            return false;
    }

    public eventEeg trigger;
    public eventEeg response;
    public int rtSample;
    public int rtMs;
}

public class POS
{
    public POS(string posFilePath, int samplingFreq)
    {
        this.posFilePath = posFilePath;
        this.samplingFreq = samplingFreq;
    }

    ~POS()
    {

    }

    public List<trigg> FileTriggers
    {
        get { return triggers; }
    }

    public List<trigg> Triggers
    {
        get { return triggersTrimmed; }
    }

    public int rtMsMax
    {
        get
        {
            int Max = 0;
            for (int i = 0; i < Triggers.Count; i++)
            {
                if (Triggers[i].rtMs > Max)
                    Max = Triggers[i].rtMs;
            }
            return Max;
        }
    }

    public void readPosData()
    {
        try
        {
            using (StreamReader sr = new StreamReader(posFilePath))
            {
                string r;
                triggers = new List<trigg>();

                while ((r = sr.ReadLine()) != null)
                {
                    string[] resultSplit = r.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    eventEeg codeEvent = new eventEeg(Convert.ToInt32(resultSplit[1]), Convert.ToInt32(resultSplit[0]));
                    trigg currentTrigg = new trigg(codeEvent);
                    triggers.Add(currentTrigg);
                }
                sr.Close();

                int beginValue = triggers.FindLastIndex(x => x.trigger.code == 99);
                for (int i = beginValue; i >= 0; i--)
                {
                    triggers.RemoveAt(i);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The pos file could not be read:");
            Console.WriteLine(e.Message);
        }
    }

    public void renameTrigger(PROV p_prov)
    {
        extractChangeCodeData(p_prov.changeCodeFilePath);

        for (int k = 0; k < triggers.Count; k++)
        {
            int idVisuBloc = -1;
            int idMain = -1;
            int idSec = -1;
            int dd = -1;
            int idcode = -1;

            for (int l = 0; l < oldTrigg.Count; l++)
            {
                if (triggers[k].trigger.code == oldTrigg[l].trigger.code)
                {
                    idMain = k;
                    for (int m = 0; m < p_prov.blocs.Count; m++)
                    {
                        if (newTrigg[l].trigger.code == p_prov.blocs[m].mainEvent.code)
                            idVisuBloc = m;
                    }
                }
            }


            if (idMain != -1)
            {
                int winSamMin = (int)Math.Round((double)(64 * p_prov.blocs[idVisuBloc].dispBloc.epochWindow[0]) / 1000);
                int winSamMax = (int)Math.Round((double)(64 * p_prov.blocs[idVisuBloc].dispBloc.epochWindow[1]) / 1000);

                dd = k + 1;

                while (idSec == -1 && dd < triggers.Count - 1)
                {
                    for (int l = 0; l < oldTrigg.Count; l++)
                    {
                        if (triggers[dd].trigger.code == oldTrigg[l].response.code &&
                            triggers[idMain].trigger.code == oldTrigg[l].trigger.code)
                        {
                            idSec = dd;
                            idcode = l;
                        }
                        else if (triggers[dd].trigger.code == oldTrigg[l].trigger.code && idSec == -1)
                        {
                            idMain = dd;
                            idcode = l;
                        }
                    }
                    dd++;
                }


                if (idMain != -1 && idSec != -1)
                {
                    int winMax = triggers[idMain].trigger.sample + winSamMax;
                    int winMin = triggers[idMain].trigger.sample - Math.Abs(winSamMin);

                    if ((triggers[idSec].trigger.sample < winMax) &&
                        (triggers[idSec].trigger.sample > winMin))
                    {
                        triggers[idMain].trigger.code = newTrigg[idcode].trigger.code;
                        triggers[idSec].trigger.code = newTrigg[idcode].response.code;
                    }
                }
            }
        }
        removeDuplicateEventCode();
    }

    public void calculateReactionTime(PROV p_prov)
    {
        pairStimWithResp(p_prov);
        removeNonStimEvents(p_prov);

        for (int i = 0; i < triggersTrimmed.Count; i++)
        {
            triggersTrimmed[i].trigger.timeMs = (int)(1000 * ((double)triggersTrimmed[i].trigger.sample / samplingFreq));
            triggersTrimmed[i].response.timeMs = (int)(1000 * ((double)triggersTrimmed[i].response.sample / samplingFreq));
            triggersTrimmed[i].rtMs = triggersTrimmed[i].response.timeMs - triggersTrimmed[i].trigger.timeMs;
            triggersTrimmed[i].rtSample = triggersTrimmed[i].response.sample - triggersTrimmed[i].trigger.sample;
        }
    }

    void extractChangeCodeData(string p_pathFile)
    {
        try
        {
            using (StreamReader sr = new StreamReader(p_pathFile))
            {
                string[] changeFileSplit = sr.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                oldTrigg = new List<trigg>();
                newTrigg = new List<trigg>();
                for (int i = 0; i < changeFileSplit.Length; i++)
                {
                    string[] lineSplit = changeFileSplit[i].Split(new char[] { '+', '=' });
                    oldTrigg.Add(new trigg(new eventEeg(Convert.ToInt32(lineSplit[0])), new eventEeg(Convert.ToInt32(lineSplit[1]))));
                    newTrigg.Add(new trigg(new eventEeg(Convert.ToInt32(lineSplit[2])), new eventEeg(Convert.ToInt32(lineSplit[3]))));
                }

                sr.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The change code file could not be read:");
            Console.WriteLine(e.Message);
        }
    }

    int[] getCurrentWindow(PROV p_prov, int currentNewCode)
    {
        int indexVisuBloc = -1;
        for (int j = 0; j < p_prov.blocs.Count; j++)
        {
            if (p_prov.blocs[j].mainEvent.code == currentNewCode)
            {
                indexVisuBloc = j;
            }
        }

        int[] winSam = new int[2] { -1, -1 };

        if (indexVisuBloc != -1)
        {
            winSam[0] = (int)Math.Round((samplingFreq * Convert.ToDouble(p_prov.blocs[indexVisuBloc].dispBloc.epochWindow[0])) / 1000);
            winSam[1] = (int)Math.Round((samplingFreq * Convert.ToDouble(p_prov.blocs[indexVisuBloc].dispBloc.epochWindow[1])) / 1000);
        }
        return winSam;
    }

    void removeDuplicateEventCode()
    {
        List<int> idToDelete = new List<int>();
        for (int i = 0; i < triggers.Count - 1; i++)
        {
            if (triggers[i].trigger.code == triggers[i + 1].trigger.code)
            {
                idToDelete.Add(i);
            }
        }

        for (int i = idToDelete.Count - 1; i >= 0; i--)
        {
            triggers.RemoveAt(idToDelete[i]);
        }
    }

    void pairStimWithResp(PROV p_prov)
    {
        if (newTrigg == null)
        {
            newTrigg = new List<trigg>();
            for (int i = 0; i < p_prov.blocs.Count; i++)
            {
                for (int j = 0; j < p_prov.blocs[i].secondaryEvents.code.Count(); j++)
                {
                    for (int k = 0; k < p_prov.blocs[i].secondaryEvents.code[j].Count(); k++)
                    {
                        newTrigg.Add(new trigg(new eventEeg(p_prov.blocs[i].mainEvent.code), new eventEeg(p_prov.blocs[i].secondaryEvents.code[j][k])));
                    }
                }
            }
        }

        for (int k = 0; k < triggers.Count; k++)
        {
            int idVisuBloc = -1;
            int idMain = -1;
            int idSec = -1;
            int dd = -1;
            int idcode = -1;

            for (int l = 0; l < newTrigg.Count; l++)
            {
                if (triggers[k].trigger.code == newTrigg[l].trigger.code)
                {
                    idMain = k;
                    for (int m = 0; m < p_prov.blocs.Count; m++)
                    {
                        if (newTrigg[l].trigger.code == p_prov.blocs[m].mainEvent.code)
                            idVisuBloc = m;
                    }
                }
            }

            if (idMain != -1)
            {
                int winSamMin = (int)Math.Round((double)(64 * p_prov.blocs[idVisuBloc].dispBloc.epochWindow[0]) / 1000);
                int winSamMax = (int)Math.Round((double)(64 * p_prov.blocs[idVisuBloc].dispBloc.epochWindow[1]) / 1000);

                dd = k + 1;

                while (idSec == -1 && dd < triggers.Count - 1)
                {
                    for (int l = 0; l < newTrigg.Count; l++)
                    {
                        if (triggers[dd].trigger.code == newTrigg[l].response.code &&
                            triggers[idMain].trigger.code == newTrigg[l].trigger.code)
                        {
                            idSec = dd;
                            idcode = l;
                        }
                        else if (triggers[dd].trigger.code == newTrigg[l].trigger.code && idSec == -1)
                        {
                            idMain = dd;
                            idcode = l;
                        }
                    }
                    dd++;
                }


                if (idMain != -1 && idSec != -1)
                {
                    int winMax = triggers[idMain].trigger.sample + winSamMax;
                    int winMin = triggers[idMain].trigger.sample - Math.Abs(winSamMin);

                    if ((triggers[idSec].trigger.sample < winMax) &&
                        (triggers[idSec].trigger.sample > winMin))
                    {
                        triggers[idMain].response = new eventEeg(triggers[idSec].trigger);
                    }
                }
            }
        }
    }

    void removeNonStimEvents(PROV p_prov)
    {
        List<int> idToDelete = new List<int>();
        triggersTrimmed = new List<trigg>(triggers);
        for (int i = 0; i < triggersTrimmed.Count; i++)
        {
            bool keepMe = false;
            for (int j = 0; j < p_prov.blocs.Count; j++)
            {
                if (triggersTrimmed[i].trigger.code == p_prov.blocs[j].mainEvent.code)
                    keepMe = true;
            }
            //==
            if (triggersTrimmed[i].response == null)
                keepMe = false;

            if (keepMe == false)
                idToDelete.Add(i);
        }

        for (int i = idToDelete.Count - 1; i >= 0; i--)
            triggersTrimmed.RemoveAt(idToDelete[i]);
    }

    int samplingFreq = 0;
    string posFilePath;
    List<trigg> triggers;
    List<trigg> triggersTrimmed;
    List<trigg> oldTrigg, newTrigg;
}

