using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BTV.Data
{
    public class BtvMontage
    {
        #region Properties
        public string Name { get; private set; } = "Default";
        public BtvProgram[] EegFiles { get; private set; } = new BtvProgram[6] { null, null, null, null, null, null };
        public bool IsCustom { get; private set; } = false;
        #endregion

        #region Public Methods
        public BtvMontage(string name, BtvProgram[] eegFiles)
        {
            Name = name;
            EegFiles = eegFiles;
            IsCustom = false;
        }
        public BtvMontage(string name, BtvMontage baseMontage, Dictionary<string, string> changes)
        {
            Name = name;
            // TODO : copy montage and process changes
            IsCustom = true;
        }
        #endregion
    }
}