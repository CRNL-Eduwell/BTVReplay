using System;
using System.Collections;
using Tools.CSharp.EEG;
using BTV.Data;
using CielaSpike;
using System.Collections.Generic;
using System.Linq;
using SimpleExpressionEngine;
using UnityEngine;

//If need destructor https://stackoverflow.com/questions/4364665/static-destructor

namespace BTV.Services.EegFileService
{
    public static class EegFileService
    {
        public static List<BtvMontage> Montages { get; private set; } = new List<BtvMontage>() { new BtvMontage("Default", new BtvProgram[6] { null, null, null, null, null, null }) };
        private static int m_SelectedMontageID = 0;
        public static int SelectedMontageID
        {
            get
            {
                return m_SelectedMontageID;
            }
            set
            {
                m_SelectedMontageID = value;
                MontageMessage message = new MontageMessage
                {
                    TaskToExecute = 1,
                    SelectedMontageID = value
                };
                Messenger.Default.Send(message, MessageContext.MontageMessage);
            }
        }
        public static BtvMontage CurrentMontage { get { return Montages[SelectedMontageID]; } }
        public static BtvMontage DefaultMontage { get { return Montages[0]; } }

        public static void Reset()
        {
            Montages = new List<BtvMontage>() { new BtvMontage("Default", new BtvProgram[6] { null, null, null, null, null, null }) };
            MontageMessage message = new MontageMessage
            {
                TaskToExecute = 0,
                SelectedMontageID = 0
            };
            Messenger.Default.Send(message, MessageContext.MontageMessage);
        }

        public static IEnumerator c_Load(IEegFileInfo fileInfo, int FileID, string description)
        {
            if (FileID >= 6)
                throw new ArgumentException("There is only 6 possible file to load, fileID argument is wrong => " + FileID);

            BtvProgram eegFile = null;
            if (fileInfo.Files.Length > 0 && System.IO.File.Exists(fileInfo.Files[0]))
            {
                IEegDataContainer container = new IEegDataContainer(fileInfo);
                eegFile = new BtvProgram(container, description);
            }
            Montages[0].SetEEGFile(eegFile, FileID);

            yield return null;
        }

        public static void Load(IEegFileInfo fileInfo, int FileID, string description)
        {
            if (FileID >= 6)
                throw new ArgumentException("There is only 6 possible file to load, fileID argument is wrong => " + FileID);

            BtvProgram eegFile = null;
            if (fileInfo.Files.Length > 0 && System.IO.File.Exists(fileInfo.Files[0]))
            {
                IEegDataContainer container = new IEegDataContainer(fileInfo);
                eegFile = new BtvProgram(container, description);
            }
            Montages[0].SetEEGFile(eegFile, FileID);
        }

        public static int GetContainerSuffix(BtvProgram currentFile)
        {
            for (int i = 0; i < CurrentMontage.EegFiles.Length; i++)
            {
                if (CurrentMontage.EegFiles[i] == currentFile) 
                    return i;
            }
            return -1;
        }

        public static BtvProgram ChangeContainerHandle(BtvProgram currentFile, int newID)
        {
            return CurrentMontage.EegFiles[newID] != null ? CurrentMontage.EegFiles[newID] : currentFile;
        }

        public static BtvProgram ReturnFirstValidContainer()
        {
            for (int i = 0; i < CurrentMontage.EegFiles.Length; i++)
            {
                if (CurrentMontage.EegFiles[i] != null)
                    return CurrentMontage.EegFiles[i];
            }

            return null;
        }

        public static bool IsFileIdValid(int FileID)
        {
            if (FileID < 0) return false;
            if (FileID >= 6) return false;

            return CurrentMontage.EegFiles[FileID] != null;
        }

        public static void AddNewChannel(float[] Data, string Name, int SamplingFrequency, int ProgramID)
        {
            if (CurrentMontage.EegFiles[ProgramID] == null)
                throw new Exception("Error : Attempting to add data to an empty program");

            if (CurrentMontage.EegFiles[ProgramID].Frequency.Value != SamplingFrequency)
                throw new Exception("Error : Sampling Frequency from new data is different from the program");

            CurrentMontage.EegFiles[ProgramID].AddData(Data, Name);
        }

        public static void AddMontage(string name, List<ChannelCorrespondance> montageDescription)
        {
            // Generate unique name
            if (Montages.Any(m => m.Name == name))
            {
                int count = 1;
                string newName = string.Format("{0}({1})", name, count);
                while (Montages.Any(g => g.Name == newName))
                {
                    count++;
                    newName = string.Format("{0}({1})", name, count);
                }
                name = newName;
            }
            Montages.Add(new BtvMontage(name, GenerateMontage(montageDescription), montageDescription));
            MontageMessage message = new MontageMessage
            {
                TaskToExecute = 0,
                SelectedMontageID = Montages.Count - 1
            };
            Messenger.Default.Send(message, MessageContext.MontageMessage);
        }
        public static void RemoveSelectedMontage()
        {
            if (CurrentMontage.IsCustom)
            {
                Montages.Remove(CurrentMontage);
                MontageMessage message = new MontageMessage
                {
                    TaskToExecute = 0,
                    SelectedMontageID = 0
                };
                Messenger.Default.Send(message, MessageContext.MontageMessage);
            }
        }
        public static void EditMontage(BtvMontage montage, string name, List<ChannelCorrespondance> montageDescription)
        {
            montage.Load(name, GenerateMontage(montageDescription), montageDescription);
            MontageMessage message = new MontageMessage
            {
                TaskToExecute = 0,
                SelectedMontageID = Montages.IndexOf(montage)
            };
            Messenger.Default.Send(message, MessageContext.MontageMessage);
        }
        // TODO : make this a coroutine
        private static BtvProgram[] GenerateMontage(List<ChannelCorrespondance> montageDescription)
        {
            BtvProgram[] eegFiles = new BtvProgram[6];
            for (int i = 0; i < 6; ++i)
            {
                BtvProgram baseEEGFile = DefaultMontage.EegFiles[i];

                if (baseEEGFile == null)
                    continue;

                eegFiles[i] = new BtvProgram(baseEEGFile);

                ChannelContext context = new ChannelContext(baseEEGFile.Channels);

                foreach (var channel in eegFiles[i].Channels)
                {
                    ChannelCorrespondance correspondance = montageDescription.FirstOrDefault(c => c.BaseLabel == channel.Label);
                    if (correspondance == null) correspondance = new ChannelCorrespondance(channel.Label, channel.Label);

                    Node descriptionNode = Parser.Parse(correspondance.CorrespondingLabel);
                    for (int j = 0; j < channel.Data.Length; ++j)
                    {
                        context.Index = j;
                        channel.Data[j] = (float)descriptionNode.Eval(context);
                    }
                    context.Reset();
                }
            }
            return eegFiles;
        }
    }
}
