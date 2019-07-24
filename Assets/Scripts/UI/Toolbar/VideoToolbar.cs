using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;//Requiered for Event data.
using UnityEngine.UI;

//Uncomment when deleting optionHub.cs
//
//public delegate void offsetVideoChangedEventHandler(float newVal);
//public delegate void toggleAudioTraceEventHandler(bool isTraceOn);
//public delegate void gainAudioChangedEventHandler(float newGain);
//public delegate void idAudioSmChangedEventHandler(int newIdSm);
namespace BTV.UI.Module3D
{
    public class VideoToolbar : Toolbar
    {
        public event offsetVideoChangedEventHandler offsetVideoHasChanged;
        public event toggleAudioTraceEventHandler audioToggled;
        public event gainAudioChangedEventHandler gainAudioHasChanged;
        public event idAudioSmChangedEventHandler smAudioHasChanged;

        Button removeVideoOffset = null;
        Scrollbar offsetScrollBar = null;
        Button addVideoOffset = null;
        Text videoOffsetLabel = null;
        EventTrigger trigger = null;
        float offsetMilliSec = 0;
        //====
        Toggle showAudioTrace = null;
        Button filterAudio = null;
        Button loadAudio = null;
        //====
        Text gainLabel = null;
        Button gainAddButton = null;
        Button gainRemoveButton = null;
        float gain = 1;
        //====
        Button[] smButton = null;
        bool sm_ChoicePending = false;
        //====
        VideoPlayer m_videoPlayer = null;
        CoroutineManager m_coroutineManager = null;
        //====
        Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);
        Color softBlue = new Color(0.6117f, 0.7058f, 0.7960f, 0.627450f);

        public VideoToolbar(GameObject videoOptionsPanel)
        {
            gainLabel = videoOptionsPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>();
            gainAddButton = videoOptionsPanel.transform.GetChild(0).GetChild(1).GetComponent<Button>();
            gainRemoveButton = videoOptionsPanel.transform.GetChild(0).GetChild(2).GetComponent<Button>();
            //==
            videoOffsetLabel = videoOptionsPanel.transform.GetChild(1).GetChild(0).GetComponent<Text>();
            removeVideoOffset = videoOptionsPanel.transform.GetChild(1).GetChild(1).GetComponent<Button>();
            offsetScrollBar = videoOptionsPanel.transform.GetChild(1).GetChild(2).GetComponent<Scrollbar>();
            addVideoOffset = videoOptionsPanel.transform.GetChild(1).GetChild(3).GetComponent<Button>();
            //==
            showAudioTrace = videoOptionsPanel.transform.GetChild(2).GetChild(0).GetComponent<Toggle>();
            filterAudio = videoOptionsPanel.transform.GetChild(2).GetChild(1).GetComponent<Button>();
            loadAudio = videoOptionsPanel.transform.GetChild(2).GetChild(2).GetComponent<Button>();
            //==

            smButton = new Button[6];
            smButton[0] = videoOptionsPanel.transform.GetChild(3).GetChild(0).GetComponent<Button>();
            smButton[1] = videoOptionsPanel.transform.GetChild(3).GetChild(1).GetComponent<Button>();
            smButton[2] = videoOptionsPanel.transform.GetChild(3).GetChild(2).GetComponent<Button>();
            smButton[3] = videoOptionsPanel.transform.GetChild(3).GetChild(3).GetComponent<Button>();
            smButton[4] = videoOptionsPanel.transform.GetChild(3).GetChild(4).GetComponent<Button>();
            smButton[5] = videoOptionsPanel.transform.GetChild(3).GetChild(5).GetComponent<Button>();
            //==
            m_videoPlayer = GameObject.Find("Workable Part").transform.GetChild(0).GetChild(1).GetComponent<VideoPlayer>();
            m_coroutineManager = GameObject.Find("ringSelect").GetComponent<CoroutineManager>();

            //[===]
            removeVideoOffset.onClick.AddListener(() =>
            {
                if (offsetMilliSec - 10 >= -60000)
                {
                    offsetMilliSec -= 10;
                    offsetScrollBar.value = ((offsetMilliSec / 1000) / 120) + 0.5f;
                    setOffsetText(offsetMilliSec);
                    offsetVideoHasChanged(offsetMilliSec);
                }
            });
            addVideoOffset.onClick.AddListener(() =>
            {
                if (offsetMilliSec + 10 <= 60000)
                {
                    offsetMilliSec += 10;
                    offsetScrollBar.value = ((offsetMilliSec / 1000) / 120) + 0.5f;
                    setOffsetText(offsetMilliSec);
                    offsetVideoHasChanged(offsetMilliSec);
                }
            });
            //==
            showAudioTrace.onValueChanged.AddListener((bool isChecked) =>
            {
                audioToggled(isChecked);
            });
            filterAudio.onClick.AddListener(() =>
            {
                m_coroutineManager.StartCoroutine(m_videoPlayer.c_filterAudio());
                filterAudio.interactable = false;
                loadAudio.interactable = false;
                showAudioTrace.isOn = true;
            });
            loadAudio.onClick.AddListener(() =>
            {
                m_coroutineManager.StartCoroutine(m_videoPlayer.c_loadAudio());
                filterAudio.interactable = false;
                loadAudio.interactable = false;
                showAudioTrace.isOn = true;
            });
            //==
            gainLabel.text = "Gain : " + gain;
            gainAddButton.onClick.AddListener(addGain);
            gainRemoveButton.onClick.AddListener(removeGain);
            //==
            for (int i = 0; i < 6; i++)
                connectButtonSM(i);
            changeButtonSMColor(0);
            //==
            trigger = offsetScrollBar.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerUp;
            entry2.callback.AddListener((eventData) => { setOffsetScrollBar(); });
            trigger.triggers.Add(entry2);

            videoOffsetLabel.text = "Offset : 00 m: 00 s: 00ms";
        }

        ~VideoToolbar()
        {
            removeVideoOffset.onClick.RemoveAllListeners();
            addVideoOffset.onClick.RemoveAllListeners();
            showAudioTrace.onValueChanged.RemoveAllListeners();
            filterAudio.onClick.RemoveAllListeners();
            loadAudio.onClick.RemoveAllListeners();
            gainAddButton.onClick.RemoveAllListeners();
            gainRemoveButton.onClick.RemoveAllListeners();

            for (int i = 0; i < 6; i++)
                smButton[i].onClick.RemoveAllListeners();

            for (int i = 0; i < trigger.triggers.Count; i++)
                trigger.triggers[i].callback.RemoveAllListeners();
        }

        void setOffsetScrollBar()
        {
            float offsetBar = offsetScrollBar.value - 0.5f;
            offsetMilliSec = (int)(offsetBar * 120) * 1000;
            setOffsetText(offsetMilliSec);
            offsetVideoHasChanged(offsetMilliSec);
        }

        void setOffsetText(float milliSec)
        {
            int m = ((int)milliSec / 1000) / 60;
            int s = ((int)milliSec / 1000) % 60;
            int ms = (int)milliSec - (((int)milliSec / 1000) * 1000);
            videoOffsetLabel.text = "Offset : " + m + "m: " + s + "s:" + ms + "ms";
        }

        public void setButtonsInteractable(bool value)
        {
            if (value)
            {
                filterAudio.interactable = true;
                loadAudio.interactable = false;
            }
            else
            {
                filterAudio.interactable = false;
                loadAudio.interactable = true;
            }
        }

        void addGain()
        {
            if (gain < 1 && gain >= -1)
                gain += 0.25f;
            else
                gain += 1;
            gainLabel.text = "Gain : " + gain;
            gainAudioHasChanged(gain);
        }

        void removeGain()
        {
            if (gain <= 1 && gain > -1)
                gain -= 0.25f;
            else
                gain -= 1;
            gainLabel.text = "Gain : " + gain;
            gainAudioHasChanged(gain);
        }

        void connectButtonSM(int id)
        {
            smButton[id].onClick.AddListener(() =>
            {
                if (!sm_ChoicePending)
                {
                    setButtonsVisible(smButton, true, -1);
                    sm_ChoicePending = true;
                }
                else
                {
                    smAudioHasChanged(id);
                    changeButtonSMColor(id);
                    setButtonsVisible(smButton, true, id);
                    sm_ChoicePending = false;
                }
            });
        }

        public void changeButtonSMColor(int id = -1)
        {
            for (int i = 0; i < 6; i++)
            {
                if (i == id)
                    smButton[i].gameObject.GetComponent<Image>().color = hardBlue;
                else
                    smButton[i].gameObject.GetComponent<Image>().color = softBlue;
            }
        }

        void setButtonsVisible(Button[] buttons, bool isVisible, int Id)
        {
            if (Id != -1)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (i == Id)
                        buttons[i].gameObject.SetActive(isVisible);
                    else
                        buttons[i].gameObject.SetActive(!isVisible);
                }
            }
            else
            {
                for (int i = 0; i < buttons.Length; i++)
                    buttons[i].gameObject.SetActive(isVisible);
            }
        }

        protected override void AddTools()
        {
            throw new System.NotImplementedException();
        }
    }
}