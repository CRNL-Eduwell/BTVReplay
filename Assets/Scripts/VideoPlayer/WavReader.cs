using System;
using System.IO;
using System.Diagnostics;
using System.Collections; //IEnumerator
using System.Runtime.InteropServices;
using CielaSpike;

public class WavReader : CppDLLImportBase
{
    public bool filterFileExist
    {
        get; set;
    }
    public int originalSampFreq
    {
        get
        {
            return originalSamplingFreq(_handle);
        }
    }
    public int originalNumSample
    {
        get
        {
            return originalNbSample(_handle);
        }
    }
    public int filteredNumSample(int downsampFreq)
    {
        return filteredNbSample(_handle, downsampFreq);
    }
    public float[] currentAudio
    {
        get
        {
            return filtData2D[idAudioHandle];
        }
    }
    public int idAudioHandle
    {
        get;set;
    }

    private float[][] filtData2D = null;
    private string m_wavFilePath = null;
    private int[] winMs = new int[6] { 0, 250, 500, 1000, 2500, 5000 };

    public static IEnumerator c_loadAudioFile(string p_wavFilePath, Action<WavReader> resultReader)
    {
        if (p_wavFilePath != "")
        {
            UnityEngine.Debug.Log("a");
            string[] videoPathSplit = p_wavFilePath.Split('.');
            string filteredDataPath = p_wavFilePath.Replace("." + videoPathSplit[videoPathSplit.Length - 1], "_audio.csv");
            UnityEngine.Debug.Log("a");

            WavReader reader = new WavReader(p_wavFilePath);
            if (new FileInfo(filteredDataPath).Exists)
            {
                UnityEngine.Debug.Log("aaa");
                reader.loadAudioFreq(filteredDataPath);
                reader.filterFileExist = true;
                UnityEngine.Debug.Log("aaa");
            }
            resultReader(reader);
            yield return null;
        }
        else
        {
            UnityEngine.Debug.Log("nop");
            resultReader(null);
        }
    }

    //public static IEnumerator c_extractAudio(string audioPath, string videoPath)
    //{
    //    Process process = new Process();
    //    ProcessStartInfo startInfo = new ProcessStartInfo();
    //    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
    //    startInfo.FileName = "cmd.exe";

    //    string cmd = "c:\\Program^ Files^ ^(x86^)\\VideoLAN\\VLC\\vlc.exe";
    //    string cmd2 = " -I dummy-quiet --sout ^\"#transcode{acodec=s16l,channels=2,ab=128,samplerate=11025}:std{access=file,mux=wav,dst=" + audioPath.Replace("/", "\\") + "}\" " + "\"" + videoPath.Replace("/", "\\") + "\" vlc://quit";

    //    startInfo.Arguments = "/c " + "^\"" + cmd + "^\"" + cmd2;
    //    process.StartInfo = startInfo;
    //    process.Start();

    //    process.WaitForExit();
    //    yield return null;
    //}

    //cmdline working for mac : /Applications/VLC.app/Contents/MacOS/VLC -I dummy --sout "#transcode{acodec=s16l,channels=2}:std{access=file,mux=wav,dst=/Users/fsipp/Documents/Arbeit/Data/BTV/LYONNEURO_2017_DAMA/LYONNEURO_2017_DAMa_BTV.wav}" /Users/fsipp/Documents/Arbeit/Data/BTV/LYONNEURO_2017_DAMA/LYONNEURO_2017_DAMa_BTV.AVI vlc://quit
    public static IEnumerator c_extractAudio(string audioPath, string videoPath)
    {
        Process process = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = "/Applications/VLC.app/Contents/MacOS/VLC";

        string cmd = "/Applications/VLC.app/Contents/MacOS/VLC";
        string cmd2 = "-I dummy --sout \"#transcode{acodec=s16l,channels=2,samplerate=11025}:std{access=file,mux=wav,dst=" + audioPath + "}\" " + "\"" + videoPath + "\" vlc://quit";

        //startInfo.Arguments = "-c " + "\"" + cmd + "\"" + cmd2;
        startInfo.Arguments = cmd2;
        UnityEngine.Debug.Log(startInfo.Arguments);
        process.StartInfo = startInfo;
        process.Start();

        process.WaitForExit();
        yield return null;
    }

    public IEnumerator c_ToHilbert(string freqBand, int downFreq)
    {
        UnityEngine.Debug.Log("hilbert " + freqBand + " et " + downFreq);
        yield return Ninja.JumpBack;
        filtData2D = new float[6][];
        for (int i = 0; i < 6; i++)
            filtData2D[i] = new float[filteredNumSample(downFreq)];

        UnityEngine.Debug.Log(filteredNumSample(downFreq));
        ToHilbert(_handle, freqBand, downFreq, filtData2D[0]);
        for (int i = 1; i < 6; i++)
            convolution(filtData2D[0], filtData2D[i].Length, filtData2D[i], (downFreq * winMs[i]) / 1000); 

        saveAudioFreq();
        releaseCppHandle();
        filterFileExist = true;
        yield return Ninja.JumpToUnity;
    }

    void saveAudioFreq()
    {
        string[] videoPathSplit = m_wavFilePath.Split('.');
        string filePath = m_wavFilePath.Replace("." + videoPathSplit[videoPathSplit.Length - 1], "_audio.csv");

        using (var w = new StreamWriter(filePath))
        {
            for (int i = 0; i < filtData2D.Length; i++)
            {
                for (int j = 0; j < filtData2D[i].Length; j++)
                {
                    w.Write(filtData2D[i][j] + ";");
                }
                w.Write("\n");
            }
            w.Close();
        }
    }

    void loadAudioFreq(string filePath)
    {
        using (StreamReader sr = new StreamReader(filePath))
        {
            string[] resultSplit = sr.ReadToEnd().Split(new char[] {'\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            filtData2D = new float[resultSplit.Length][];
            for (int i = 0; i < filtData2D.Length; i++)
            {
                string[] resultSplit2 = resultSplit[i].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                filtData2D[i] = new float[resultSplit2.Length];
                for (int j = 0; j < filtData2D[i].Length; j++)
                {
                    filtData2D[i][j] = float.Parse(resultSplit2[j]);
                }
            }
            sr.Close();
        }
    }

    public float[] getAudioHandle(int idHandle)
    {
        return filtData2D[idHandle];
    }

    #region memory_management
    public WavReader(string pathAudioFile) : base(pathAudioFile)
    {
        m_wavFilePath = pathAudioFile;
    }

    void releaseCppHandle()
    {
        delete_WAV(_handle);
    }

    protected override void createDLLClass() { }

    protected override void createDLLClass(string str)
    {
        _handle = new HandleRef(this, create_WAV(str));
    }

    protected override void deleteDLLClass()
    {
        filtData2D = null;
    }
    #endregion

    #region DLLImport
    [DllImport("BTVReplayLibraryC++", EntryPoint = "create_WAV", CallingConvention = CallingConvention.Cdecl)]
    static private extern IntPtr create_WAV(string pathAudioFile);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "delete_WAV", CallingConvention = CallingConvention.Cdecl)]
    static private extern void delete_WAV(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "ToHilbert", CallingConvention = CallingConvention.Cdecl)]
    static private extern int ToHilbert(HandleRef handle, string freqBands, int downFrequency, float[] filteredData);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "convolution", CallingConvention = CallingConvention.Cdecl)]
    static private extern void convolution(float[] iinputData, int length, float[] outputData, int coeff);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "originalSamplingFreq", CallingConvention = CallingConvention.Cdecl)]
    static private extern int originalSamplingFreq(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "originalNbSample", CallingConvention = CallingConvention.Cdecl)]
    static private extern int originalNbSample(HandleRef handle);

    [DllImport("BTVReplayLibraryC++", EntryPoint = "filteredNbSample", CallingConvention = CallingConvention.Cdecl)]
    static private extern int filteredNbSample(HandleRef handle, int downFrequency);
    #endregion
}
