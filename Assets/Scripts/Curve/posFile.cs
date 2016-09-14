using System;
using System.IO;
using System.Collections.Generic;

public class posFile
{
    public posFile(string p_posFilePath)
    {
        posFilePath = p_posFilePath;
    }

    ~posFile()
    {

    }

    public void readPosData()
    {
        try
        {
            using (StreamReader sr = new StreamReader(posFilePath))
            {
                string r;
                sampleEvent = new List<int>();
                codeEvent = new List<int>();

                while ((r = sr.ReadLine()) != null)
                {
                    string[] resultSplit = r.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    sampleEvent.Add(Convert.ToInt32(resultSplit[0]));
                    codeEvent.Add(Convert.ToInt32(resultSplit[1]));
                }

                sr.Close();

                int beginValue = codeEvent.FindLastIndex(x => x == 99);

                for (int i = beginValue; i >= 0; i--)
                {
                    sampleEvent.RemoveAt(i);
                    codeEvent.RemoveAt(i);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The pos file could not be read:");
            Console.WriteLine(e.Message);
        }
    }

    public void renameTrigger(provFile p_prov)
    {
        int indexVisuBloc = 0, winMax = -1, winMin = -1;

        extractChangeCodeData(p_prov.changeCodeFilePath);

        for (int i = 0; i < oldMainCode.Count; i++)
        {
            for (int j = 0; j < p_prov.blocs.Count; j++)
            {
                if (p_prov.blocs[j].mainEvent.code == newMainCode[i])
                {
                    indexVisuBloc = j;
                }
            }

            int winSamMin = (int)Math.Round((64 * Convert.ToDouble(p_prov.blocs[indexVisuBloc].dispBloc.epochWindow[0])) / 1000);
            int winSamMax = (int)Math.Round((64 * Convert.ToDouble(p_prov.blocs[indexVisuBloc].dispBloc.epochWindow[1])) / 1000);

            int count = 0;
            for (int j = 0; j < sampleEvent.Count; j++)
            {
                if (codeEvent[j] == oldMainCode[i])
                {
                    count = j + 1;
                    winMax = sampleEvent[j] + winSamMax;
                    winMin = sampleEvent[j] - Math.Abs(winSamMin);
                    while (sampleEvent[count] < winMax && sampleEvent[count] > winMin)
                    {
                        if (codeEvent[count] == oldSecondaryCode[i])
                        {
                            codeEvent[j] = newMainCode[i];
                            codeEvent[count] = newSecondaryCode[i];

                            count++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    public void calculateReactionTime(provFile p_prov)
    {
        bool toKeep = false;
        int nextLatence = -1;
        List<int> index2keep = new List<int>();
        List<int> index2delete = new List<int>();
        reactionTimeMs = new List<int>();
        reactionTimeCode = new List<int>();

        sampleEventTrimmed = new List<int>(sampleEvent);
        codeEventTrimmed = new List<int>(codeEvent);

        for (int i = 0; i < sampleEventTrimmed.Count; i++)
        {
            toKeep = false;
            for (int j = 0; j < p_prov.blocs.Count; j++)
            {
                if(codeEventTrimmed[i] == p_prov.blocs[j].mainEvent.code)
                {
                    toKeep = true;
                }
            }

            if (toKeep == false)
            {
                index2delete.Add(i);
            }
            else
            {
                index2keep.Add(i);
            }
        }

        for (int i = index2delete.Count - 1; i >= 0; i--)
        {
            sampleEventTrimmed.RemoveAt(index2delete[i]);
            codeEventTrimmed.RemoveAt(index2delete[i]);
        }

        int numberTotalEvent = sampleEventTrimmed.Count;

        List<int> possibleSecondaryEvents = new List<int>();
        for(int i = 0; i< p_prov.blocs.Count;i++)
        {
            for (int j = 0; j < p_prov.blocs[i].secondaryEvents.code.Length; j++)
            {
                bool isInList = possibleSecondaryEvents.IndexOf(p_prov.blocs[i].secondaryEvents.code[j][0]) != -1;
                if (isInList == false)
                {
                    possibleSecondaryEvents.Add(p_prov.blocs[i].secondaryEvents.code[j][0]);
                }
            }
        }

        for (int i = 0; i < numberTotalEvent; i++)
        {
            if (i < numberTotalEvent - 1)
            {
                nextLatence = sampleEventTrimmed[i + 1];
            }
            else
            {
                nextLatence = sampleEvent[sampleEvent.Count - 1];
            }

            List<int> m_tpos = new List<int>();
            List<int> m_sam = new List<int>();
            for (int j = index2keep[i] + 1; j < sampleEvent.Count - 1; j++)
            {
                m_tpos.Add(codeEvent[j]);
                m_sam.Add(sampleEvent[j]);
            }

            int s_rtms = 10000000;
            int s_rtcode = -1;

            for (int j = 0; j < possibleSecondaryEvents.Count; j++)
            {
                int s_resp = possibleSecondaryEvents[j];

                int re = m_tpos.IndexOf(s_resp);

                if (re != -1 && re != m_tpos.Count)
                {
                    int s_resplat = m_sam[re];
                    if (s_resplat < nextLatence)
                    {
                        int compareMe = (int)(1000 * (Convert.ToDouble((s_resplat - sampleEvent[index2keep[i]])) / 64));
                        s_rtms = Math.Min(s_rtms, compareMe);
                        s_rtcode = s_resp;
                    }
                }
            }
            reactionTimeMs.Add(s_rtms - 750);
            reactionTimeCode.Add(s_rtcode);
        }
    }

    void extractChangeCodeData(string p_pathFile)
    {
        try
        {
            using (StreamReader sr = new StreamReader(p_pathFile))
            {
                string[] changeFileSplit = sr.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                oldMainCode = new List<int>();
                oldSecondaryCode = new List<int>();
                newMainCode = new List<int>();
                newSecondaryCode = new List<int>();

                for (int i = 0; i < changeFileSplit.Length; i++)
                {
                    string[] lineSplit = changeFileSplit[i].Split(new char[] { '+', '=' });
                    oldMainCode.Add(Convert.ToInt32(lineSplit[0]));
                    oldSecondaryCode.Add(Convert.ToInt32(lineSplit[1]));
                    newMainCode.Add(Convert.ToInt32(lineSplit[2]));
                    newSecondaryCode.Add(Convert.ToInt32(lineSplit[3]));
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

    public string posFilePath;
    public List<int> reactionTimeMs, reactionTimeCode;
    public List<int> sampleEvent, codeEvent;
    public List<int> sampleEventTrimmed, codeEventTrimmed;
    public List<int> oldMainCode, oldSecondaryCode;
    public List<int> newMainCode, newSecondaryCode;

}