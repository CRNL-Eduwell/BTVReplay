using System;
using System.IO;
using System.Diagnostics;
using System.Collections; //IEnumerator
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

struct WawHeader
{
    public int chunkID;
    public int fileSize;
    public int riffType;
    public int fmtID;
    public int fmtSize;
    public int fmtCode;
    public int channels;
    public int sampleRate;
    public int fmtAvgBPS;
    public int fmtBlockAlign;
    public int bitDepth;
    public int dataID;
    public int dataSize;
};

public class WawReader
{
    public static IEnumerator c_loadAudioFile(string p_wavFilePath, float sampFreq, Action<WawReader> resultReader)
    {
        if (p_wavFilePath != "")
        {
            WawReader reader = new WawReader(p_wavFilePath);
            reader.calculateRMS((int) sampFreq);
            reader.normalizeValues();
            resultReader(reader);
            yield return null;
        }
        else
        {
            resultReader(null);
        }
    }

    public static IEnumerator c_extractAudio(string audioPath, string videoPath)
    {
        Process process = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";

        string cmd = "c:\\Program^ Files\\VideoLAN\\VLC\\vlc.exe";
        string cmd2 = " -I dummy-quiet --sout ^\"#transcode{acodec=s16l,channels=2}:std{access=file,mux=wav,dst=" + audioPath.Replace("/", "\\") + "}\" " + "\"" + videoPath.Replace("/", "\\") + "\" vlc://quit";

        startInfo.Arguments = "/c " + "^\"" + cmd + "^\"" + cmd2;
        process.StartInfo = startInfo;
        process.Start();

        process.WaitForExit();
        yield return null;
    }

    public WawReader(string p_wavFilePath)
    {
        try
        {
            using (StreamReader sr = new StreamReader(p_wavFilePath))
            {
                BinaryReader reader = new BinaryReader(sr.BaseStream);
                readHeader(reader);
                readData(reader);

                reader.Close();
                sr.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
    }

    void readHeader(BinaryReader reader)
    {
        header.chunkID = reader.ReadInt32();
        header.fileSize = reader.ReadInt32();
        header.riffType = reader.ReadInt32();
        header.fmtID = reader.ReadInt32();
        header.fmtSize = reader.ReadInt32();
        header.fmtCode = reader.ReadInt16();
        header.channels = reader.ReadInt16();
        header.sampleRate = reader.ReadInt32();
        header.fmtAvgBPS = reader.ReadInt32();
        header.fmtBlockAlign = reader.ReadInt16();
        header.bitDepth = reader.ReadInt16();

        if (header.fmtSize == 18)
        {
            //Read any extra values
            int fmtExtraSize = reader.ReadInt16();
            reader.ReadBytes(fmtExtraSize);
        }

        header.dataID = reader.ReadInt32();
        header.dataSize = reader.ReadInt32();
    }

    void readData(BinaryReader reader)
    {
        byteArray = reader.ReadBytes(header.dataSize);
        LeftvalueArray = new int[byteArray.Length / 4];
        RightValueArray = new int[byteArray.Length / 4];
        MonoValueArray = new int[byteArray.Length / 4];

        for (int i = 0; i < byteArray.Length; i += 4)
        {
            int leftVal = (byteArray[0 + i] << 8) | (byteArray[1 + i]);
            int rightVal = (byteArray[2 + i] << 8) | (byteArray[3 + i]);
            LeftvalueArray[i / 4] = checkValueOverFlow(leftVal);
            RightValueArray[i / 4] = checkValueOverFlow(rightVal);
        }

        stereoToMono(ref LeftvalueArray, ref RightValueArray, ref MonoValueArray);
    }

    int checkValueOverFlow(int audioValue)
    {
        if (audioValue >= Int16.MinValue && audioValue <= Int16.MaxValue)
            return audioValue;
        else
            return audioValue - (Int16.MaxValue - Int16.MinValue);
    }

    void stereoToMono(ref int[] leftChannel, ref int[] rightChannel, ref int[] monoChannel)
    {
        for (int i = 0; i < leftChannel.Count(); i++)
            monoChannel[i] = (leftChannel[i] + rightChannel[i]) / 2;
    }

    public void calculateRMS(int signalFrequency)
    {
        int Fs = header.sampleRate;
        int newFs = signalFrequency;

        if (Fs % newFs != 0)
        {
            int nearFs = nearestFrequency(Fs, newFs);
            rootMeanSquare(Fs, nearFs);
            double factor = ((double)nearFs / newFs);

            List<double> newrms = new List<double>();
            newrms.Add(rms[0]);
            for (int i = 0; i < rms.Count() - 1; i++)
                newrms.Add(((rms[i + 1] - rms[i]) * factor) + rms[i]);
            newrms.Add(rms[rms.Count - 1]);

            rms = new List<double>(newrms);
        }
        else
        {
            rootMeanSquare(Fs, newFs);
        }
    }

    int nearestFrequency(int Fs, int newFs)
    {
        int wantedValue = newFs;
        int denI = wantedValue, denD = wantedValue;

        while (Fs % wantedValue != 0)
        {
            denI++; denD--;
            if (Fs % denD == 0)
                return denD;
            else if (Fs % denI == 0)
                return denI;
        }

        return 0;
    }

    void rootMeanSquare(int Fs, int newFs)
    {
        rms = new List<double>();
        for (int i = 0; i < MonoValueArray.Length / Fs; i++)
        {
            for (int j = 0; j < newFs; j++)
            {
                int windowSize = (Fs / newFs);
                double sum = 0.0;
                for (int k = 0; k < windowSize; k++)
                    sum += Math.Pow(MonoValueArray[(i * Fs) + (j * windowSize) + k], 2);

                rms.Add(Math.Sqrt(sum / windowSize));
            }
        }
    }

    public void normalizeValues()
    {
        double maxval = rms.Max<double>();
        double minval = rms.Min<double>();

        for (int i = 0; i < rms.Count; i++)
            rms[i] = (((rms[i] - minval) / (maxval - minval)) - 0.5) * 200;
    }

    WawHeader header;
    byte[] byteArray = null;
    int[] LeftvalueArray = null, RightValueArray = null, MonoValueArray = null;
    public List<double> rms = null;
}