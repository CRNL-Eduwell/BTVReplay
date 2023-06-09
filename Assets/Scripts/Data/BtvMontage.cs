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
        private Dictionary<string, string> m_MontageDescription = new Dictionary<string, string>();
        #endregion

        #region Public Methods
        public BtvMontage(string name, BtvProgram[] eegFiles)
        {
            Name = name;
            EegFiles = eegFiles;
            m_MontageDescription = new Dictionary<string, string>();
            IsCustom = false;
        }
        public BtvMontage(string name, BtvMontage baseMontage, Dictionary<string, string> montageDescription)
        {
            Name = name;
            m_MontageDescription = montageDescription;

            for (int i = 0; i < 6; ++i)
            {
                BtvProgram baseEEGFile = baseMontage.EegFiles[i];

                if (baseEEGFile == null)
                    continue;

                EegFiles[i] = new BtvProgram(baseEEGFile);

                ChannelContext context = new ChannelContext(baseEEGFile.Channels);

                foreach (var channel in EegFiles[i].Channels)
                {
                    if (!m_MontageDescription.TryGetValue(channel.Label, out string description))
                        description = channel.Label;

                    Node descriptionNode = Parser.Parse(description);
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
                if (!m_MontageDescription.ContainsKey(channel.Label))
                    m_MontageDescription.Add(channel.Label, channel.Label);
        }
        public void Save(string path)
        {

        }
        #endregion
    }
}