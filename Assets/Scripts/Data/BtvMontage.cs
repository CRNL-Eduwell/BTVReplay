using SimpleExpressionEngine;
using System.Collections;
using System.Collections.Generic;
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
        public BtvMontage(string name, BtvProgram[] eegFiles)
        {
            Name = name;
            EegFiles = eegFiles;
            MontageDescription = new List<ChannelCorrespondance>();
            IsCustom = false;
        }
        public BtvMontage(string name, BtvMontage baseMontage, List<ChannelCorrespondance> montageDescription)
        {
            Name = name;
            MontageDescription = montageDescription;

            for (int i = 0; i < 6; ++i)
            {
                BtvProgram baseEEGFile = baseMontage.EegFiles[i];

                if (baseEEGFile == null)
                    continue;

                EegFiles[i] = new BtvProgram(baseEEGFile);

                ChannelContext context = new ChannelContext(baseEEGFile.Channels);

                foreach (var channel in EegFiles[i].Channels)
                {
                    ChannelCorrespondance correspondance = MontageDescription.FirstOrDefault(c => c.BaseLabel == channel.Label);
                    if (correspondance == null) correspondance = new ChannelCorrespondance(channel.Label, channel.Label);

                    Node descriptionNode = Parser.Parse(correspondance.CorrespondingLabel);
                    for (int j = 0; j < channel.Data.Length; ++j)
                    {
                        context.Index = j;
                        channel.Data[j] = (float)descriptionNode.Eval(context);
                    }
                }
            }

            IsCustom = true;
        }
        public void SetEEGFile(BtvProgram eegFile, int id)
        {
            EegFiles[id] = eegFile;
            foreach (var channel in eegFile.Channels)
                if (MontageDescription.FirstOrDefault(c => c.BaseLabel == channel.Label) == null)
                    MontageDescription.Add(new ChannelCorrespondance(channel.Label, channel.Label));
        }
        public void Save(string path)
        {

        }
        #endregion
    }
}