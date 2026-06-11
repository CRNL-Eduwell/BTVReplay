using System;
using System.Collections;
using Tools.CSharp.EEG;
using BTV.Data;
using CielaSpike;
using System.Collections.Generic;
using System.Linq;
using SimpleExpressionEngine;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

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

        public static void AddMontage(string name, List<ChannelCorrespondance> montageDescription, string fileName = "")
        {
            // Generate unique name
            if (Montages.Any(m => m.Name == name))
            {
                int count = 1;
                string newName = string.Format("{0}({1})", name, count);
                while (Montages.Any(m => m.Name == newName))
                {
                    count++;
                    newName = string.Format("{0}({1})", name, count);
                }
                name = newName;
            }
            GenericEvent<float, string> onChangeProgress = new GenericEvent<float, string>();
            Action<BtvProgram[]> addGeneratedMontage = (eegFiles) =>
            {
                Montages.Add(new BtvMontage(name, eegFiles, montageDescription));
                MontageMessage message = new MontageMessage
                {
                    TaskToExecute = 0,
                    SelectedMontageID = Montages.Count - 1
                };
                Messenger.Default.Send(message, MessageContext.MontageMessage);
            };
            LoadingManager.Load(c_GenerateMontage(montageDescription, fileName, onChangeProgress, addGeneratedMontage), onChangeProgress);
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
        public static void EditMontage(BtvMontage montage, string name, List<ChannelCorrespondance> montageDescription, string fileName = "")
        {
            // Generate unique name
            if (Montages.Any(m => m.Name == name && m != montage))
            {
                int count = 1;
                string newName = string.Format("{0}({1})", name, count);
                while (Montages.Any(m => m.Name == newName && m != montage))
                {
                    count++;
                    newName = string.Format("{0}({1})", name, count);
                }
                name = newName;
            }
            GenericEvent<float, string> onChangeProgress = new GenericEvent<float, string>();
            Action<BtvProgram[]> editSelectedMontage = (eegFiles) =>
            {
                montage.Load(name, eegFiles, montageDescription);
                MontageMessage message = new MontageMessage
                {
                    TaskToExecute = 0,
                    SelectedMontageID = Montages.IndexOf(montage)
                };
                Messenger.Default.Send(message, MessageContext.MontageMessage);
            };
            LoadingManager.Load(c_GenerateMontage(montageDescription, fileName, onChangeProgress, editSelectedMontage), onChangeProgress);
        }
        public static void LoadMontage(string path)
        {
            string name = "";
            List<ChannelCorrespondance> montageDescription = new List<ChannelCorrespondance>();
            string line = "";
            using (StreamReader sr = new StreamReader(path))
            {
                name = sr.ReadLine();
                while ((line = sr.ReadLine()) != null)
                {
                    string[] splits = line.Split(',');
                    if (splits.Length == 2)
                    {
                        montageDescription.Add(new ChannelCorrespondance(splits[0], splits[1]));
                    }
                }
            }
            AddMontage(name, montageDescription);
        }
        private static IEnumerator c_GenerateMontage(List<ChannelCorrespondance> montageDescription, string fileName, GenericEvent<float, string> onChangeProgress, Action<BtvProgram[]> onEnd)
        {
            int globalProgress = 0;
            int totalNumberOfValidFiles = string.IsNullOrEmpty(fileName) ? DefaultMontage.EegFiles.Count(f => f != null) : 1;
            BtvProgram[] eegFiles = new BtvProgram[6];
            string errorList = "";
            for (int i = 0; i < 6; ++i)
            {
                BtvProgram baseEEGFile = DefaultMontage.EegFiles[i];

                if (baseEEGFile == null)
                    continue;

                yield return Ninja.JumpToUnity;
                onChangeProgress.Invoke((float)globalProgress / totalNumberOfValidFiles, string.Format("Preparing file {0}", baseEEGFile.Description));
                yield return Ninja.JumpBack;
                eegFiles[i] = new BtvProgram(baseEEGFile);

                if (!string.IsNullOrEmpty(fileName) && baseEEGFile.Description != fileName)
                    continue;

                ChannelContext context = new ChannelContext(baseEEGFile.Channels);

                float localProgress = 0;
                float localProgressStep = 1f / eegFiles[i].Channels.Count;
                foreach (var channel in eegFiles[i].Channels)
                {
                    yield return Ninja.JumpToUnity;
                    onChangeProgress.Invoke((float)(globalProgress + localProgress) / totalNumberOfValidFiles, string.Format("File {0} (channel {1})", baseEEGFile.Description, channel.Label));
                    yield return Ninja.JumpBack;

                    ChannelCorrespondance correspondance = montageDescription.FirstOrDefault(c => c.BaseLabel == channel.Label);
                    if (correspondance == null) correspondance = new ChannelCorrespondance(channel.Label, channel.Label);

                    try
                    {
                        Node descriptionNode = Parser.Parse(correspondance.CorrespondingLabel);
                        for (int j = 0; j < channel.Data.Length; ++j)
                        {
                            context.Index = j;
                            channel.Data[j] = (float)descriptionNode.Eval(context);
                        }
                        context.Reset();
                    }
                    catch (Exception e)
                    {
                        errorList += string.Format("Could not parse correspondance {0} of channel {1} in file {2}. Keeping base values. Reason: {3}\n", correspondance.CorrespondingLabel, correspondance.BaseLabel, baseEEGFile.Description, e.Message);
                        Node descriptionNode = Parser.Parse(correspondance.BaseLabel);
                        for (int j = 0; j < channel.Data.Length; ++j)
                        {
                            context.Index = j;
                            channel.Data[j] = (float)descriptionNode.Eval(context);
                        }
                        context.Reset();
                    }
                    localProgress += localProgressStep;
                }
                globalProgress++;
            }
            yield return Ninja.JumpToUnity;
            if (!string.IsNullOrEmpty(errorList)) // TODO : make this visible for user maybe
                BtvLog.Log(errorList);
            onEnd(eegFiles);
        }
    }
}
