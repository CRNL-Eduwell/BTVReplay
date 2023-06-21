using BTV.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI
{
    public class MontageWindow : Window
    {
        [SerializeField] InputField m_MontageName;
        [SerializeField] Dropdown m_FileDropdown;
        [SerializeField] ChannelCorrespondanceList m_ChannelCorrespondanceList;
        private BtvMontage m_EditedMontage = null;

        [SerializeField] Button m_PresetsButton;
        [SerializeField] GameObject m_PresetsPanel;
        [SerializeField] Button m_MonopolarButton;
        [SerializeField] Button m_BipolarButton;

        protected override void SetFields()
        {
            base.SetFields();
            foreach (var label in Services.EegFileService.EegFileService.DefaultMontage.MontageDescription.Select(c => c.BaseLabel))
            {
                m_ChannelCorrespondanceList.Add(new ChannelCorrespondance(label, label));
            }
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            options.Add(new Dropdown.OptionData("All files"));
            options.AddRange(Services.EegFileService.EegFileService.DefaultMontage.EegFiles.Where(e => e != null).Select(e => new Dropdown.OptionData(e.Description)));
            m_FileDropdown.options = options;
            m_FileDropdown.value = 0;
            m_PresetsButton.onClick.AddListener(() => m_PresetsPanel.SetActive(!m_PresetsPanel.activeSelf));
            m_MonopolarButton.onClick.AddListener(Monopolar);
            m_BipolarButton.onClick.AddListener(Bipolar);
        }

        public void SetMontage(BtvMontage montage)
        {
            m_MontageName.text = montage.Name;
            m_ChannelCorrespondanceList.Set(montage.MontageDescription.Select(cc => new ChannelCorrespondance(cc.BaseLabel, cc.CorrespondingLabel)));
            m_EditedMontage = montage;
        }

        public void OK()
        {
            if (m_EditedMontage != null)
            {
                Services.EegFileService.EegFileService.EditMontage(m_EditedMontage, m_MontageName.text, m_ChannelCorrespondanceList.Objects.ToList(), m_FileDropdown.value == 0 ? "" : m_FileDropdown.options[m_FileDropdown.value].text);
            }
            else
            {
                Services.EegFileService.EegFileService.AddMontage(m_MontageName.text, m_ChannelCorrespondanceList.Objects.ToList(), m_FileDropdown.value == 0 ? "" : m_FileDropdown.options[m_FileDropdown.value].text);
            }
            Close();
        }

        public void Monopolar()
        {
            m_PresetsPanel.SetActive(false);
            foreach (var channelCorrespondance in m_ChannelCorrespondanceList.Objects.ToList())
                m_ChannelCorrespondanceList.Replace(new ChannelCorrespondance(channelCorrespondance.BaseLabel, channelCorrespondance.BaseLabel), channelCorrespondance);
            m_ChannelCorrespondanceList.Refresh();
        }

        public void Bipolar()
        {
            m_PresetsPanel.SetActive(false);
            Regex regex = new Regex("^([a-zA-Z']+)([0-9]+)$");
            Dictionary<string, string> channelCorrespondances = m_ChannelCorrespondanceList.Objects.ToDictionary(cc => cc.BaseLabel, cc => cc.BaseLabel);
            List<string> channels = channelCorrespondances.Keys.ToList();
            foreach (var channel in channels)
            {
                string correspondance = "";
                Match match = regex.Match(channel);
                if (match.Success)
                {
                    string electrode = match.Groups[1].Value;
                    if (int.TryParse(match.Groups[2].Value, out int number))
                    {
                        if (channelCorrespondances.ContainsKey(electrode + (number - 1)))
                        {
                            correspondance = string.Format("{0} - {1}", electrode + number, electrode + (number - 1));
                        }
                    }
                }
                channelCorrespondances[channel] = correspondance;
            }
            foreach (var channelCorrespondance in m_ChannelCorrespondanceList.Objects.ToList())
            {
                var newCorrespondance = channelCorrespondances[channelCorrespondance.BaseLabel];
                m_ChannelCorrespondanceList.Replace(new ChannelCorrespondance(channelCorrespondance.BaseLabel, newCorrespondance), channelCorrespondance);
            }
            m_ChannelCorrespondanceList.Refresh();
        }
    }
}