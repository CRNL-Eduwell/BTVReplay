using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BTV.Data
{
    public class ChannelCorrespondance
    {
        public string BaseLabel { get; set; }
        public string CorrespondingLabel { get; set; }
        public ChannelCorrespondance(string baseLabel, string correspondingLabel)
        {
            BaseLabel = baseLabel;
            CorrespondingLabel = correspondingLabel;
        }
    }
}