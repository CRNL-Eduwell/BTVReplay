using BTV.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI
{
    public class ChannelCorrespondanceItem : Tools.Unity.Lists.Item<ChannelCorrespondance>
    {
        [SerializeField] Text m_BaseLabelText;
        [SerializeField] InputField m_CorrespondingLabelInputField;

        public override ChannelCorrespondance Object
        {
            get
            {
                return base.Object;
            }
            set
            {
                base.Object = value;
                SetFields();
            }
        }

        private void SetFields()
        {
            m_BaseLabelText.text = Object.BaseLabel;
            m_CorrespondingLabelInputField.text = Object.CorrespondingLabel;
            m_CorrespondingLabelInputField.onEndEdit.AddListener((value) =>
            {
                Object.CorrespondingLabel = value;
            });
        }
    }
}