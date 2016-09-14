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
            }

            centerSignalZero(eegData[i], (int)numberSample);
        }

        //writeCSVOutput(eegData[0], (int)numberSample, @"D:\Users\Florian\Desktop\test.csv");
        //Debug.Log("Data Extracted");
    }

    void centerSignalZero(double[] data, int numberElement)
    {
        //double average = data.Average();

        //for (int i = 0; i < numberElement; i++)
        //{
        //    data[i] = (data[i] - average) / 1000;
        //}

        double average = data.Average();

        for (int i = 0; i < numberElement; i++)
        {
            data[i] = data[i] - average;
        }

        double min = data.Min();
        double max = data.Max();

        max = Math.Max(Math.Abs(min), max);

        for (int i = 0; i < numberElement; i++)
        {
            data[i] = data[i] / max;
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

    public string eegFilePath;
    public string eegEntFilePath;
    public int samplingFrequency;
    public int numberBipoles;
    public long dataSize;
    public long numberSec;
    public long numberSample;
    public List<string> nameElectrode;

    public double[][] eegData;

}

