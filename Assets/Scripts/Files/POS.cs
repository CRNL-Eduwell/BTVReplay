using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class eventEeg
{
    public eventEeg(int code, int sample = -1, int duration = 0, string elecOfInterest = "", string comment = "")
    {
        this.code = code;
        this.sample = sample;
        this.duration = duration;
        this.elecOfInterest = elecOfInterest;
        this.comment = comment;
    }

    public eventEeg(eventEeg currentEvent)
    {
        this.code = currentEvent.code;
        this.sample = currentEvent.sample;
        this.timeMs = currentEvent.timeMs;
        this.duration = currentEvent.duration;
        this.elecOfInterest = currentEvent.elecOfInterest;
        this.comment = currentEvent.comment;
    }

    public int code = -1;
    public int sample = -1;
    public int timeMs = -1;
    public int duration = 0;
    public string elecOfInterest = "";
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
    public POS(string posFilePath)
    {
        this.posFilePath = posFilePath;
    }

    ~POS()
    {

    }

    public List<trigg> Triggers
    {
        get
        {
            return triggersTrimmed;
        }
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

        for (int i = 0; i < oldTrigg.Count; i++)
        {
            int[] winSam = getCurrentWindow(p_prov, newTrigg[i].trigger.code);
            if (winSam[0] != -1 && winSam[1] != -1)
            {
                int idRespEvent = 0;
                for (int j = 0; j < triggers.Count - 1; j++)
                {
                    if (triggers[j].trigger.code == oldTrigg[i].trigger.code)
                    {
                        triggers[j].trigger.code = newTrigg[i].trigger.code;
                        if (j + 1 < triggers.Count - 1)
                        {
                            idRespEvent = j + 1;
                            //==
                            int winMin = triggers[j].trigger.sample - Math.Abs(winSam[0]);
                            int winMax = triggers[j].trigger.sample + winSam[1];

                            while (triggers[idRespEvent].trigger.sample < winMax &&
                                   triggers[idRespEvent].trigger.sample > winMin &&
                                   idRespEvent + 1 < triggers.Count - 1)
                            {
                                if (triggers[idRespEvent].trigger.code == oldTrigg[i].response.code)
                                {
                                    triggers[idRespEvent].trigger.code = newTrigg[i].response.code;
                                }
                                idRespEvent++;
                            }
                        }
                    }
                }
            }
        }
        removeDuplicateEventCode();
    }

    public void calculateReactionTime(PROV p_prov, float samplingFreq)
    {
        pairStimWithResp(p_prov);
        removeNonStimEvents(p_prov);

        for (int i = 0; i < triggersTrimmed.Count; i++)
        {
            triggersTrimmed[i].trigger.timeMs = (int)(1000 * (triggersTrimmed[i].trigger.sample / samplingFreq));
            triggersTrimmed[i].response.timeMs = (int)(1000 * (triggersTrimmed[i].response.sample / samplingFreq));
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
            winSam[0] = (int)Math.Round((64 * Convert.ToDouble(p_prov.blocs[indexVisuBloc].dispBloc.epochWindow[0])) / 1000);
            winSam[1] = (int)Math.Round((64 * Convert.ToDouble(p_prov.blocs[indexVisuBloc].dispBloc.epochWindow[1])) / 1000);
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
        for (int i = 0; i < oldTrigg.Count; i++)
        {
            int[] winSam = getCurrentWindow(p_prov, newTrigg[i].trigger.code);
            if (winSam[0] != -1 && winSam[1] != -1)
            {
                int idRespEvent = 0;
                for (int j = 0; j < triggers.Count - 1; j++)
                {
                    if (j + 1 < triggers.Count - 1)
                    {
                        idRespEvent = j + 1;
                        //==
                        int winMin = triggers[j].trigger.sample - Math.Abs(winSam[0]);
                        int winMax = triggers[j].trigger.sample + winSam[1];

                        while (triggers[idRespEvent].trigger.sample < winMax &&
                               triggers[idRespEvent].trigger.sample > winMin &&
                               idRespEvent + 1 < triggers.Count - 1)
                        {
                            if ((triggers[idRespEvent].trigger.code == oldTrigg[i].response.code) ||
                                (triggers[idRespEvent].trigger.code == newTrigg[i].response.code))
                            {
                                triggers[j].response = new eventEeg(triggers[idRespEvent].trigger);
                                triggers[j].response.code = newTrigg[i].response.code;
                            }
                            idRespEvent++;
                        }
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

    string posFilePath;
    List<trigg> triggers;
    List<trigg> triggersTrimmed;
    List<trigg> oldTrigg, newTrigg;
}

