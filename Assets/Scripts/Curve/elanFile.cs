using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class elanFile
{
    public elanFile(string p_eegFilePath)
    {
        eegFilePath = p_eegFilePath;
        eegEntFilePath = eegFilePath + ".ent";

        //readHeader(eegEntFilePath);
        //readEEGData(eegFilePath);
    }

    ~elanFile()
    {

    }

    public void readHeader(string p_eegEntFilePath)
    {
        try
        {
            using (StreamReader sr = new StreamReader(p_eegEntFilePath))
            {
                string r;
                nameElectrode = new List<string>();
                physicalMinimum = new List<int>();
                physicalMaximum = new List<int>();
                logicMinimum = new List<int>();
                logicMaximum = new List<int>();

                for (int i = 0; i < 8; i++)
                {
                    r = sr.ReadLine();
                }

                r = sr.ReadLine();
                samplingFrequency = Convert.ToInt32(1 / Convert.ToDouble(r));
                r = sr.ReadLine();
                numberBipoles = Convert.ToInt32(r);

                for (int i = 0; i < numberBipoles; i++)
                {
                    r = sr.ReadLine();
                    string[] result = r.Split(new char[] { '.' });
                    nameElectrode.Add(result[0]);
                }

                //2 * numberbip
                for (int i = 0; i < 2 * numberBipoles; i++)
                {
                    r = sr.ReadLine();
                }

                //Physic Min
                for(int i = 0; i< numberBipoles;i++)
                {
                    r = sr.ReadLine();
                    physicalMinimum.Add(Convert.ToInt32(r));
                }

                for (int i = 0; i < numberBipoles; i++)
                {
                    r = sr.ReadLine();
                    physicalMaximum.Add(Convert.ToInt32(r));
                }

                for (int i = 0; i < numberBipoles; i++)
                {
                    r = sr.ReadLine();
                    logicMinimum.Add(Convert.ToInt32(r));
                }

                for (int i = 0; i < numberBipoles; i++)
                {
                    r = sr.ReadLine();
                    logicMaximum.Add(Convert.ToInt32(r));
                }
                sr.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The ent file could not be read:");
            Console.WriteLine(e.Message);
        }
    }

    public void readEEGData(string p_eegFilePath)
    {
        byte[] fileBytes = File.ReadAllBytes(p_eegFilePath);

        dataSize = fileBytes.Length;
        numberSample = dataSize / (numberBipoles * 2);
        numberSec = dataSize / (numberBipoles * samplingFrequency * 2);

        eegData = new double[numberBipoles][];
        for (int i = 0; i < numberBipoles; i++)
        {
            eegData[i] = new double[numberSample];
            for (int j = 0; j < numberSample; j++)
            {
                byte[] a = new byte[2];
                a[0] = fileBytes[0 + (i * 2) + (j * (numberBipoles * 2))];
                a[1] = fileBytes[1 + (i * 2) + (j * (numberBipoles * 2))];
                Array.Reverse(a); //little endian

                eegData[i][j] = BitConverter.ToInt16(a, 0);
                //eegData[i][j] = (eegData[i][j] / (logicMaximum[i] - logicMinimum[i] + 1)) * (physicalMaximum[i] - physicalMinimum[i]);
            }

            centerSignalZero(eegData[i], (int)numberSample);
        }

        //writeCSVOutput(eegData[0], (int)numberSample, @"D:\Users\Florian\Desktop\test"+count+".csv");
        //count++;
        //Debug.Log("Data Extracted");
    }

    void centerSignalZero(double[] data, int numberElement)
    {
        double min = data.Min();
        double max = data.Max();
        double mean = data.Average();
        double MAX = Mathf.Max((float)max, (float)Math.Abs(min));

        for (int i = 0; i < numberElement; i++)
        {
            data[i] = (data[i] - mean) / MAX;
        }
    }

    void writeCSVOutput(double[] data, int numberElement, string outputFilePath)
    {
        StreamWriter sw = new StreamWriter(File.Create(outputFilePath));
        for (int i = 0; i < numberElement; i++)
        {
            sw.Write(data[i] + "\n");
        }
        sw.Close();
    }

    void writeCSVOutput(double[][] data, int numberElement1, int numberElement2, string outputFilePath)
    {
        StreamWriter sw = new StreamWriter(File.Create(outputFilePath));
        for (int i = 0; i < numberElement2; i++)
        {
            for (int j = 0; j< numberElement1; j++)
            {
                sw.Write(data[j][i] + ";");
            }
            sw.Write("\n");
        }
        sw.Close();
    }


    public string eegFilePath;
    public string eegEntFilePath;
    public int samplingFrequency;
    public int numberBipoles;
    public long dataSize;
    public long numberSec;
    public long numberSample;
    public List<string> nameElectrode;
    public List<int> physicalMinimum, physicalMaximum, logicMinimum, logicMaximum;
    int count = 0;
    public double[][] eegData;

}

