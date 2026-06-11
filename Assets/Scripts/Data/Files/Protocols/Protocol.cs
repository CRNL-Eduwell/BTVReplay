
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Protocol
{
    public string FilePath { get; private set; } = "";
    public string ShortName { get; private set; } = "";
    public string ChangeCodeFilePath { get; private set; } = "";
    public List<Bloc> Blocs { get; private set; } = new List<Bloc>();

    public Protocol(string file)
    {
        FilePath = file;
        string[] splitPath = FilePath.Split(new char[] { '/', '.' });
        ShortName = splitPath[splitPath.Length - 2];

        if (File.Exists(FilePath))
            Load(FilePath);
        else
            UnityEngine.Debug.LogError("Protocol => Filepath : " + FilePath + " does not exist ");
    }

    private void Load(string FilePath)
    {
        try
        {
            using (StreamReader sr = new StreamReader(FilePath))
            {
                string[] provFileSplit = sr.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                Blocs = new List<Bloc>();
                for (int h = 1; h < provFileSplit.Length - 1; h++)
                {
                    string[] resSplit = provFileSplit[h].Split(new char[] { ';' });
                    Bloc tempBloc = new Bloc();
                    int[] window = new int[2];
                    int[] baseLineWindow = new int[2];
                    int[][] secondaryEvents;
                    string[] secondaryEventsLabel;

                    for (int i = 0; i < resSplit.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                tempBloc.dispBloc.row = Convert.ToInt32(resSplit[i]);
                                break;
                            case 1:
                                tempBloc.dispBloc.col = Convert.ToInt32(resSplit[i]);
                                break;
                            case 2:
                                tempBloc.dispBloc.name = resSplit[i];
                                break;
                            case 3:
                                tempBloc.dispBloc.path = Application.dataPath + resSplit[i];
                                break;
                            case 4:
                                string[] windowSplit = resSplit[i].Split(new char[] { ':' });
                                if (windowSplit.Length == 2)
                                {
                                    window[0] = Convert.ToInt32(windowSplit[0]);
                                    window[1] = Convert.ToInt32(windowSplit[1]);
                                    tempBloc.dispBloc.epochWindow = window;
                                }
                                else
                                {
                                    window[0] = 0;
                                    window[1] = 0;
                                    tempBloc.dispBloc.epochWindow = window;
                                    //BtvLog.Log("Problème avec la fenêtre");
                                }
                                break;
                            case 5:
                                string[] baselineWindowSplit = resSplit[i].Split(new char[] { ':' });
                                if (baselineWindowSplit.Length == 2)
                                {
                                    baseLineWindow[0] = Convert.ToInt32(baselineWindowSplit[0]);
                                    baseLineWindow[1] = Convert.ToInt32(baselineWindowSplit[1]);
                                    tempBloc.dispBloc.baselineWindow = baseLineWindow;
                                }
                                else
                                {
                                    baseLineWindow[0] = 0;
                                    baseLineWindow[1] = 0;
                                    tempBloc.dispBloc.baselineWindow = baseLineWindow;
                                    //BtvLog.Log("Problème avec la baseline");
                                }
                                break;
                            case 6:
                                tempBloc.mainEvent.code = Convert.ToInt32(resSplit[i]);
                                break;
                            case 7:
                                tempBloc.mainEvent.label = resSplit[i];
                                break;
                            case 8:
                                string[] secEventSplit = resSplit[i].Split(new char[] { ':' });
                                secondaryEvents = new int[secEventSplit.Length][];
                                for (int j = 0; j < secEventSplit.Length; j++)
                                {
                                    string[] secEventSplit2 = secEventSplit[j].Split(new char[] { '_' });
                                    secondaryEvents[j] = new int[secEventSplit2.Length];
                                    for (int k = 0; k < secEventSplit2.Length; k++)
                                    {
                                        secondaryEvents[j][k] = Convert.ToInt32(secEventSplit2[k]);
                                    }
                                }
                                tempBloc.secondaryEvents.code = secondaryEvents;
                                break;
                            case 9:
                                string[] secEventLabelSplit = resSplit[i].Split(new char[] { ':' });
                                secondaryEventsLabel = new string[secEventLabelSplit.Length];
                                for (int j = 0; j < secEventLabelSplit.Length; j++)
                                {
                                    secondaryEventsLabel[j] = secEventLabelSplit[j];
                                }
                                tempBloc.secondaryEvents.label = secondaryEventsLabel;
                                break;
                            case 10:
                                tempBloc.dispBloc.sort = resSplit[i];
                                break;
                            default:
                                //BtvLog.Log("Problème avec le .prov");
                                break;
                        }
                    }
                    //CREATE BLOCK OBJECTS
                    Blocs.Add(tempBloc);
                }
                sr.Close();

                if (provFileSplit[provFileSplit.Length - 1] == "NO_CHANGE_CODE" || provFileSplit[provFileSplit.Length - 1].Split(new char[] { ';' }).Length > 1)
                {
                    ChangeCodeFilePath = "";
                }
                else
                {
                    ChangeCodeFilePath = Application.dataPath + provFileSplit[provFileSplit.Length - 1];
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The prov file could not be read:");
            Console.WriteLine(e.Message);
        }
    }
}

