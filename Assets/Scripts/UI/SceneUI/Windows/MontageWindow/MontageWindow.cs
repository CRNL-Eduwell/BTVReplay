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
        private BtvMontage m_EditedMontage = null;

        protected override void SetFields()
        {
            base.SetFields();
            foreach (var label in Services.EegFileService.EegFileService.DefaultMontage.MontageDescription.Select(c => c.BaseLabel))
            {
                m_ChannelCorrespondanceList.Add(new ChannelCorrespondance(label, label));
            }
        }

        public void SetMontage(BtvMontage montage)
        {
            m_ChannelCorrespondanceList.Set(montage.MontageDescription.Select(cc => new ChannelCorrespondance(cc.BaseLabel, cc.CorrespondingLabel)));
            m_ChannelCorrespondanceList.Refresh();
        }

        public void OK()
        {
            if (m_EditedMontage != null)
            {
                Services.EegFileService.EegFileService.EditMontage(m_EditedMontage, m_MontageName.text, m_ChannelCorrespondanceList.Objects.ToList());
            }
            else
            {
                Services.EegFileService.EegFileService.AddMontage(m_MontageName.text, m_ChannelCorrespondanceList.Objects.ToList());
            }
            Close();
        }
    }
}