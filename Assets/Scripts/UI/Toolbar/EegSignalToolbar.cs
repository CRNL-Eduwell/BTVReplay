using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//Uncomment when deleting optionHub.cs
//
//public delegate void gainChangedEventHandler(float newVal);
//public delegate void offsetChangedEventHandler(float newVal);
//public delegate void idFileChangedEventHandler(int newIdHandle);
//public delegate void idElecChangedEventHandler(int newIDElec);
//public delegate void timePeriodChangedEventHandler(int newPeriod);
//public delegate void toggleGridDisplay(bool isGridOn);
//public delegate void toggleSonification(bool isSonifOn);
//public delegate void newSoundSonif(int newIDSound);

namespace BTV.UI.Module3D
{
    public class EegSignalToolbar : Toolbar
    {
        public event gainChangedEventHandler gainHasChanged;
        public event offsetChangedEventHandler offsetHasChanged;
        public event idFileChangedEventHandler idFileHasChanged;
        public event idElecChangedEventHandler idElecHasChanged;
        public event timePeriodChangedEventHandler timeHasChanged;
        public event toggleGridDisplay gridToggled;
        public event toggleSonification sonifToggled;
        public event newSoundSonif soundChanged;

        [SerializeField]
        private Tools.EegSignalGain m_Gain = null;

        [SerializeField]
        private Tools.EegSignalOffset m_Offset = null;

        [SerializeField]
        private Tools.EegSignalWindow m_Window = null;

        [SerializeField]
        private Tools.EegSignalSonification m_Sonifier = null;

        [SerializeField]
        private Tools.EegSignalColorPicker m_ColorPicker = null;

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
            m_Tools.Add(m_FileSwitcher);
        }

        protected override void AddListeners()
        {

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