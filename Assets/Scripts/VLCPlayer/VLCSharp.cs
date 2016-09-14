using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using hyperVLC;

using System.Globalization;
using System.Text.RegularExpressions;

public class VLCSharp : MonoBehaviour
{
    void Start ()
    {
        //SetDllDirectory(@"D:\Users\Florian\Desktop\BTVReplay\BTVReplay_Data\Plugins");
        string[] args = new string[] { "" };

        instance = new VlcInstance(args);
        player = null;

    }

    void Update ()
    {
        if (player != null)
        {
            if (newPicLoaded)
            {
                UpdateTexture();
            }

            if (player.IsPlaying)
            {
                getTime(true, totalTime);
                getTime(false, actualTime);
            }
        }
        else
        {
            if (btvMedia.loaded == true && initialized == false)
            {
                loadVideoInit();
                setVolume();
                initialized = true;
            }
        }
    }

    #region Members
    public BTVMedia btvMedia;
    public RawImage TextureToDraw;
    public Text actualTime, totalTime;
    public Scrollbar scrollBar;
    public Scrollbar volumeBar;
    Bitmap picCopy = null;
    VlcInstance instance = null;
    VlcMediaPlayer player = null;
    public bool newPicLoaded = false, picRendered = true, videoPaused = true, initialized = false;
    int totalTimeSeconde = 0;
    public long totalTimeMSec = 0;
    public long totalTimeSec = 0;
    float memorySc = 0;
    #endregion

    #region vlcFunctions

    public void Play()
    {
        if (videoPaused == true)
        {
            player.Play();
            videoPaused = false;
        }
        else 
        {
            player.Pause();
            videoPaused = true;
        }
    }

    public void Stop()
    {
        if (player.IsStopped == false)
        {
            player.Stop();
            //prepare for reinstancement if ever
            player = null;
            instance = new VlcInstance(new string[] { "" });
        }
    }

    public void getTime(bool totalTime, Text toDisplay)
    {
        if (totalTime == false)
        {
            totalTimeMSec = player.getTime();
            totalTimeSec  = totalTimeMSec / 1000;

            long h = totalTimeSec / 3600;
            long m = ((totalTimeSec / 60) % 60);
            long s = (totalTimeSec % 60);

            if (h > 0)
            {
                toDisplay.text = h + ":" + m + ":" + s;
            }
            else
            {
                toDisplay.text = m + ":" + s;
            }
        }
        else
        {
            totalTimeSec = player.getTotalTime() / 1000;
            if (totalTimeSec != 0)
            {
                totalTimeSeconde = (int)totalTimeSec;
                //scrollBar.size = (float)1 / totalTimeSeconde;
            }

            long h = totalTimeSec / 3600;
            long m = ((totalTimeSec / 60) % 60);
            long s = (totalTimeSec % 60);
            
            if (h > 0)
            {
                toDisplay.text = h + ":" + m + ":" + s;
            }
            else
            {
                toDisplay.text = m + ":" + s;
            }
        }
    }

    public void setTime()
    {
        if (memorySc != scrollBar.value)
        {
            long timeMS = (long)(scrollBar.value * totalTimeSeconde * 1000);
            player.setTime(timeMS);
        }
        scrollBar.value = totalTimeSec * (float)1 / totalTimeSeconde;
        memorySc = scrollBar.value;
    }

    public void setVolume()
    {
        player.SetVolume((int)(volumeBar.value * 100));
    }

    public void loadVideoInit()
    {
        using (VlcMedia media = new VlcMedia(instance, "file:///" + btvMedia.video.transform.GetChild(1).GetComponent<InputField>().text))  // @"D:\\Users\\Florian\\Desktop\\MM_15SEP09G\\MM_15SEP09G_BTV_V15.AVI")) 
        {
            if (player == null)
            {
                player = new VlcMediaPlayer(media);

                IMemoryRenderer memRender = player.CustomRenderer;
                memRender.SetCallback(delegate (Bitmap frame)
                {
                    if (picRendered == true)
                    {
                        Bitmap b = frame.Clone(new RectangleF(0, 0, frame.Width, frame.Height), PixelFormat.Format32bppArgb);
                        picCopy = b;
                        newPicLoaded = true;
                        picRendered = false;
                    }
                });
                memRender.SetFormat(new BitmapFormat(640, 480, ChromaType.RV32));
            }
            else
            {
                player.Media = media;
            }
        }


        //player.Play();
    }

    #endregion

    #region Functions

    void UpdateTexture()
    {
        Texture2D videoTexture = Image2Texture(picCopy);
        newPicLoaded = false;
        picRendered = true;
        TextureToDraw.texture = videoTexture;
    }

    static Texture2D Image2Texture(System.Drawing.Image im)
    {
        if (im == null)
        {
            return new Texture2D(4, 4);
        }


        //Memory stream to store the bitmap data.
        MemoryStream ms = new MemoryStream();


        //Save to that memory stream.
        im.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        //Go to the beginning of the memory stream.
        ms.Seek(0, SeekOrigin.Begin);
        //make a new Texture2D
        Texture2D tex = new Texture2D(im.Width, im.Height);

        tex.LoadImage(ms.ToArray());

        //Close the stream.
        ms.Close();
        ms = null;

        //
        return tex;
    }

    #endregion

    #region DLLImport

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetDllDirectory(string lpPathName);

    #endregion
}
