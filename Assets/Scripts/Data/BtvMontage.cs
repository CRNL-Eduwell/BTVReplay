using SimpleExpressionEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BTV.Data
{
    public class BtvMontage
    {
        #region Properties
        public string Name { get; private set; } = "Default";
        public BtvProgram[] EegFiles { get; private set; } = new BtvProgram[6] { null, null, null, null, null, null };
        public bool IsCustom { get; private set; } = false;
        public List<ChannelCorrespondance> MontageDescription { get; private set; } = new List<ChannelCorrespondance>();
        #endregion

        #region Public Methods
        public BtvMontage(string name, BtvProgram[] eegFiles, List<ChannelCorrespondance> montageDescription = null)
        {
            Load(name, eegFiles, montageDescription);
        }
        public void SetEEGFile(BtvProgram eegFile, int id)
        {
            EegFiles[id] = eegFile;
            foreach (var channel in eegFile.Channels)
                if (MontageDescription.FirstOrDefault(c => c.BaseLabel == channel.Label) == null)
                    MontageDescription.Add(new ChannelCorrespondance(channel.Label, channel.Label));
        }
        public void Load(string name, BtvProgram[] eegFiles, List<ChannelCorrespondance> montageDescription)
        {
            Name = name;
            EegFiles = eegFiles;
            if (montageDescription == null)
            {
                MontageDescription = new List<ChannelCorrespondance>();
                IsCustom = false;
            }
            else
            {
                MontageDescription = montageDescription;
                IsCustom = true;
            }
        }
        public void Save(string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(Name);
                foreach (var channelCorrespondance in MontageDescription)
                {
                    sw.WriteLine(string.Format("{0},{1}", channelCorrespondance.BaseLabel, channelCorrespondance.CorrespondingLabel));
                }
            }
        }
        #endregion
    }
}