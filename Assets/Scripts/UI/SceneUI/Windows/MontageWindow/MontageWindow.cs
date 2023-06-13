using BTV.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI
{
    public class MontageWindow : Window
    {
        [SerializeField] InputField m_MontageName;
        [SerializeField] ChannelCorrespondanceList m_ChannelCorrespondanceList;

        protected override void SetFields()
        {
            base.SetFields();
            BtvMontage defaultMontage = Services.EegFileService.EegFileService.DefaultMontage;
            foreach (var label in defaultMontage.MontageDescription.Select(c => c.BaseLabel))
            {
                m_ChannelCorrespondanceList.Add(new ChannelCorrespondance(label, label));
            }
        }

        public void OK()
        {
            Services.EegFileService.EegFileService.AddMontage(m_MontageName.text, m_ChannelCorrespondanceList.Objects.ToList());
            Close();
        }
    }
}