using UnityEngine;

namespace BTV.UI.Module3D
{
    public class EegSignalToolbar : Toolbar
    {
        [SerializeField]
        private int m_TraceID = 0;

        [SerializeField]
        private Tools.EegSignalGain m_Gain = null;

        [SerializeField]
        private Tools.EegSignalOffset m_Offset = null;

        [SerializeField]
        private Tools.EegSignalWindow m_Window = null;

        [SerializeField]
        private Tools.EegSignalSonification m_Sonifier = null;

        [SerializeField]
        private Tools.ColorPicker m_ColorPicker = null;

        [SerializeField]
        private Tools.EegSignalFileSwitcher m_FileSwitcher = null;

        GameObject elecPlot = null;
        GameObject electrodeContentPanel = null;
        BTVMedia media = null;

        Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);
        Color softBlue = new Color(0.6117f, 0.7058f, 0.7960f, 0.392156f);
        Color yellow = new Color(0.9058f, 0.8784f, 0.0f);

        #region Private Methods
        protected override void AddTools()
        {
            m_Tools.Add(m_Gain);
            m_Tools.Add(m_Offset);
            m_Tools.Add(m_Window);
            m_Tools.Add(m_Sonifier);
            m_Tools.Add(m_ColorPicker);
            //m_Tools.Add(m_FileSwitcher);
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            m_Gain.gainHasChanged += UpdateTraceGain;
            m_Offset.offsetHasChanged += UpdateTraceOffset;
            m_Window.gridToggled += ToggleTraceGrid;
            m_Window.timeHasChanged += UpdateTracePeriod;
            m_Sonifier.sonifToggled += ToggleSonification;
            m_Sonifier.soundChanged += UpdateSonificationSound;
            m_ColorPicker.UpdateColor += UpdateTraceColor;
        }

        private void UpdateTraceGain(float NewGain)
        {
            UnityEngine.Debug.Log("Update Trace gain");
            UiToTraceMessage message = new UiToTraceMessage
            {
                TaskToExecute = 0,
                TraceID = m_TraceID,
                Gain = NewGain
            };
            Messenger.Default.Send(message, MessageContext.UiToTrace);
        }

        private void UpdateTraceOffset(float NewOffset)
        {
            UnityEngine.Debug.Log("Update Trace offset");
            UiToTraceMessage message = new UiToTraceMessage
            {
                TaskToExecute = 1,
                TraceID = m_TraceID,
                Offset = NewOffset
            };
            Messenger.Default.Send(message, MessageContext.UiToTrace);
        }

        private void ToggleTraceGrid(bool isGridOn)
        {
            UnityEngine.Debug.Log("Update Grid Toggle");
            UiToTraceMessage message = new UiToTraceMessage
            {
                TaskToExecute = 2,
                TraceID = m_TraceID,
                IsGridOn = isGridOn
            };
            Messenger.Default.Send(message, MessageContext.UiToTrace);
        }

        private void UpdateTracePeriod(int NewPeriod)
        {
            UnityEngine.Debug.Log("Update Trace period");
            UiToTraceMessage message = new UiToTraceMessage
            {
                TaskToExecute = 3,
                TraceID = m_TraceID,
                TimeWindow = NewPeriod
            };
            Messenger.Default.Send(message, MessageContext.UiToTrace);
        }

        private void ToggleSonification(bool IsSonificationOn)
        {
            UnityEngine.Debug.Log("Toggle sonification");
            UiToTraceMessage message = new UiToTraceMessage
            {
                TaskToExecute = 4,
                TraceID = m_TraceID,
                IsSonificationOn = IsSonificationOn
            };
            Messenger.Default.Send(message, MessageContext.UiToTrace);
        }

        private void UpdateSonificationSound(int NewIdSound)
        {
            UnityEngine.Debug.Log("Update sonification sound");
            UiToTraceMessage message = new UiToTraceMessage
            {
                TaskToExecute = 5,
                TraceID = m_TraceID,
                NewSonificationId = NewIdSound
            };
            Messenger.Default.Send(message, MessageContext.UiToTrace);
        }

        private void UpdateTraceColor(Color NewColor)
        {
            UnityEngine.Debug.Log("Update Trace Color");
            UiToTraceMessage message = new UiToTraceMessage
            {
                TaskToExecute = 6,
                TraceID = m_TraceID,
                Color = NewColor
            };
            Messenger.Default.Send(message, MessageContext.UiToTrace);
        }
        #endregion

        //public EegSignalToolbar(ButtonUI_show buttonOpt, BTVMedia p_media)
        //{
        //    media = p_media;
        //    elecPlot = Resources.Load("Prefabs/Hub-Elec", typeof(GameObject)) as GameObject;
        //    electrodeContentPanel = buttonOpt.optionsPanel2.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        //}

        //public void loadElectrodeInPanel(elecFile[] electrodeList)
        //{
        //    deleteElectrodeInPanel();
        //    for (int i = 0; i < electrodeList.Length; i++)
        //    {
        //        GameObject currentElectrode = GameObject.Instantiate(elecPlot);
        //        Button currentElecButton = currentElectrode.GetComponent<Button>();
        //        currentElecButton.onClick.AddListener(() =>
        //        {
        //            for (int j = 0; j < electrodeContentPanel.transform.childCount; j++)
        //            {
        //                if (electrodeContentPanel.transform.GetChild(j).name == currentElecButton.name)
        //                {
        //                //if (sphereColor.isValidForChange(currentElecButton.name))
        //                //{
        //                idElecHasChanged(j);
        //                //    changeColorElec();
        //                break;
        //                //}
        //            }
        //            }
        //        });

        //        Text currentElecText = currentElectrode.transform.GetChild(0).GetComponent<Text>();
        //        currentElecText.text = electrodeList[i].name;
        //        currentElectrode.name = electrodeList[i].name;
        //        currentElectrode.transform.SetParent(electrodeContentPanel.transform);
        //        currentElectrode.transform.localScale = new Vector3(1, 1, 1);
        //    }
        //}

        //public void deleteElectrodeInPanel()
        //{
        //    if (electrodeContentPanel.transform.childCount > 0)
        //    {
        //        for (int i = 0; i < electrodeContentPanel.transform.childCount; i++)
        //        {
        //            electrodeContentPanel.transform.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
        //            GameObject.Destroy(electrodeContentPanel.transform.GetChild(i).gameObject);
        //        }
        //    }
        //}
    }
}