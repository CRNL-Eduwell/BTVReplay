using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using hyperVLC;

using System.Threading;

public class VLCSharp : MonoBehaviour
{
    void Start ()
    {
        texPause = Resources.Load("Pictures/playIcone", typeof(Texture2D)) as Texture2D;
        texPlay = Resources.Load("Pictures/pauseIcone", typeof(Texture2D)) as Texture2D;
        texLogo = Resources.Load("Pictures/BTVLogo", typeof(Texture2D)) as Texture2D;

        TextureToDraw.texture = Instantiate(texLogo);
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
    public RawImage playButton = null;

    Texture2D texPlay = null, texPause = null, texLogo = null;
    Bitmap picCopy = null;
    VlcInstance instance = null;
    VlcMediaPlayer player = null;
    public bool newPicLoaded = false, picRendered = true, videoPaused = true, initialized = false;
    int totalTimeSeconde = 0;
    public long totalTimeMSec = 0;
    public long totalTimeSec = 0;
    float mem = 0;
    #endregion

    #region vlcFunctions

    public void Play()
    {
        if (videoPaused == true)
        {
            player.Play();
            videoPaused = false;
            playButton.texture = texPlay;
        }
        else 
        {
            player.Pause();
            videoPaused = true;
            playButton.texture = texPause;
        }
    }

    public void Stop()
    {
        if (player.IsStopped == false)
        {
            player.Stop();
            playButton.texture = texPause;
            //prepare for reinstancement if ever

            TextureToDraw.texture = Instantiate(texLogo); ;

            player = null;
            instance = new VlcInstance(new string[] { "" });
        }
    }

    public void moveTime(int timeSec)
    {
        totalTimeSec += timeSec;
        scrollBar.value = totalTimeSec * (float)1 / totalTimeSeconde;
    }

    public void getTime(bool totalTime, Text toDisplay)
    {
        if (totalTime == false)
        {
            totalTimeMSec = player.getTime();
            totalTimeSec  = totalTimeMSec / 1000;

            mem = totalTimeSec * (float)1 / totalTimeSeconde;
            scrollBar.value = totalTimeSec * (float)1 / totalTimeSeconde;

            long h = totalTimeSec / 3600;
            long m = ((totalTimeSec / 60) % 60);
            long s = (totalTimeSec % 60);

            if (h > 0)
            {
                toDisplay.text = returnTimeString(h) + ":" + returnTimeString(m) + ":" + returnTimeString(s);
            }
            else
            {
                toDisplay.text = returnTimeString(m) + ":" + returnTimeString(s);
            }
        }
        else
        {
            totalTimeSec = player.getTotalTime() / 1000;
            if (totalTimeSec != 0)
            {
                totalTimeSeconde = (int)totalTimeSec;
            }

            long h = totalTimeSec / 3600;
            long m = ((totalTimeSec / 60) % 60);
            long s = (totalTimeSec % 60);
            
            if (h > 0)
            {
                toDisplay.text = returnTimeString(h) + ":" + returnTimeString(m) + ":" + returnTimeString(s);
            }
            else
            {
                toDisplay.text = returnTimeString(m) + ":" + returnTimeString(s);
            }
        }
    }

    public void setTime()
    {
        if (mem != scrollBar.value)
        {
            long timeMS = (long)(scrollBar.value * totalTimeSeconde * 1000);
            player.setTime(timeMS);
            mem = scrollBar.value;
        }
    }

    public void setVolume()
    {
        player.SetVolume((int)(volumeBar.value * 200)); //until 200% volume 
    }

    public void loadVideoInit()
    {        
        using (VlcMedia media = new VlcMedia(instance, "file:///" + btvMedia.video.transform.GetChild(1).GetComponent<InputField>().text))  // @"D:\\Users\\Florian\\Desktop\\MM_15SEP09G\\MM_15SEP09G_BTV_V15.AVI"))  
        {
            if (player == null)
            {
                player = new VlcMediaPlayer(media);

                //string a = "sout=#std{access=file,dst=D:\\vid.mp4}";
                //string a = ":sout=#stream_out_duplicate{dst=display,dst=std{access=file,mux=mp4,dst=D:\\vid.mp4}}";
                //string a = "sout=#stream_out_duplicate{dst=nodisplay,dst=std{access=file,mux=mp4,dst=D:\\vid.mp4}}";
                //string a = "sout=#std{access=screen,mux=mp4,dst=D:\\vid.mp4}";
                //LibVlc.libvlc_media_add_option(media.Handle, a);

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

    string returnTimeString(long time)
    {
        if (time < 10)
        {
            return "0" + time;
        }
        else
        {
            return time.ToString();
        }
    }
    #endregion

    #region Functions

    void UpdateTexture()
    {
        Image2Texture(picCopy, (Texture2D)TextureToDraw.texture);
        newPicLoaded = false;
        picRendered = true;
    }

    static void Image2Texture(System.Drawing.Image im, Texture2D myTex)
    {
        //Memory stream to store the bitmap data.
        MemoryStream ms = new MemoryStream();


        //Save to that memory stream.
        im.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        //Go to the beginning of the memory stream.
        ms.Seek(0, SeekOrigin.Begin);
        //make a new Texture2D

        myTex.LoadImage(ms.ToArray());

        //Close the stream.
        ms.Close();
        ms = null;
    }

    #endregion

    #region DLLImport

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetDllDirectory(string lpPathName);

    #endregion
}
