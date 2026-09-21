using System;
using Tools.CSharp.EEG;
using BTV.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleExpressionEngine;
using UnityEngine;
using System.IO;

//If need destructor https://stackoverflow.com/questions/4364665/static-destructor

namespace BTV.Services.EegFileService
{
    public static class EegFileService
    {
        public static List<BtvMontage> Montages { get { return Session.Current.Montages; } }
        public static int SelectedMontageID
        {
            get
            {
                return Session.Current.SelectedMontageID;
            }
            set
            {
                Session.Current.SelectedMontageID = value;
                MontageMessage message = new MontageMessage
                {
                    TaskToExecute = MontageMessage.Task.SelectMontage,
                    SelectedMontageID = value
                };
                Messenger.Default.Send(message, MessageContext.MontageMessage);
            }
        }
        public static BtvMontage CurrentMontage { get { return GetCurrentMontage(Session.Current); } }
        public static BtvMontage DefaultMontage { get { return Montages[0]; } }

        public static BtvMontage GetCurrentMontage(Session session)
        {
            return session.Montages[session.SelectedMontageID];
        }

        public static void Reset()
        {
            Session.Current.Montages = Session.CreateDefaultMontages();
            Session.Current.SelectedMontageID = 0;
            MontageMessage message = new MontageMessage
            {
                TaskToExecute = MontageMessage.Task.UpdateMontageList,
                SelectedMontageID = 0
            };
            Messenger.Default.Send(message, MessageContext.MontageMessage);
        }

        public static async Task LoadAsync(IEegFileInfo fileInfo, int FileID, string description)
        {
            if (FileID >= 6)
                throw new ArgumentException("There is only 6 possible file to load, fileID argument is wrong => " + FileID);

            // The native EEG read + managed copy runs on a worker; the montage slot is assigned
            // after the await, back on the main thread (the old version mutated the static
            // montage from the worker thread).
            Session session = Session.Current;
            BtvProgram eegFile = await Task.Run(() =>
            {
                if (fileInfo.Files.Length > 0 && System.IO.File.Exists(fileInfo.Files[0]))
                {
                    IEegDataContainer container = new IEegDataContainer(fileInfo);
                    return new BtvProgram(container, description);
                }
                return null;
            });

            TryPublishEegFile(session, eegFile, FileID);
        }

        private static bool TryPublishEegFile(Session session, BtvProgram eegFile, int fileID)
        {
            if (!Session.IsCurrent(session))
            {
                BtvLog.Log("Discarded an EEG file loaded for a previous patient session.");
                return false;
            }

            session.Montages[0].SetEEGFile(eegFile, fileID);
            return true;
        }

        public static int GetContainerSuffix(BtvProgram currentFile)
        {
            return GetContainerSuffix(Session.Current, currentFile);
        }

        public static int GetContainerSuffix(Session session, BtvProgram currentFile)
        {
            BtvProgram[] eegFiles = GetCurrentMontage(session).EegFiles;
            for (int i = 0; i < eegFiles.Length; i++)
            {
                if (eegFiles[i] == currentFile)
                    return i;
            }
            return -1;
        }

        public static BtvProgram ChangeContainerHandle(BtvProgram currentFile, int newID)
        {
            return ChangeContainerHandle(Session.Current, currentFile, newID);
        }

        public static BtvProgram ChangeContainerHandle(Session session, BtvProgram currentFile, int newID)
        {
            BtvProgram candidate = GetCurrentMontage(session).EegFiles[newID];
            return candidate != null ? candidate : currentFile;
        }

        public static BtvProgram ReturnFirstValidContainer()
        {
            return ReturnFirstValidContainer(Session.Current);
        }

        public static BtvProgram ReturnFirstValidContainer(Session session)
        {
            BtvProgram[] eegFiles = GetCurrentMontage(session).EegFiles;
            for (int i = 0; i < eegFiles.Length; i++)
            {
                if (eegFiles[i] != null)
                    return eegFiles[i];
            }

            return null;
        }

        public static bool IsFileIdValid(int FileID)
        {
            return IsFileIdValid(Session.Current, FileID);
        }

        public static bool IsFileIdValid(Session session, int FileID)
        {
            if (FileID < 0) return false;
            if (FileID >= 6) return false;

            return GetCurrentMontage(session).EegFiles[FileID] != null;
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
            BtvProgram[] baseFiles = DefaultMontage.EegFiles; // static state: snapshot the reference on the main thread
            Session session = Session.Current;
            LoadingManager.Load(async progress =>
            {
                BtvProgram[] eegFiles = await Task.Run(() => GenerateMontage(baseFiles, montageDescription, fileName, progress));

                TryPublishNewMontage(session, name, eegFiles, montageDescription);
            });
        }
        public static void RemoveSelectedMontage()
        {
            if (CurrentMontage.IsCustom)
            {
                Montages.Remove(CurrentMontage);
                MontageMessage message = new MontageMessage
                {
                    TaskToExecute = MontageMessage.Task.UpdateMontageList,
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
            BtvProgram[] baseFiles = DefaultMontage.EegFiles; // static state: snapshot the reference on the main thread
            Session session = Session.Current;
            LoadingManager.Load(async progress =>
            {
                BtvProgram[] eegFiles = await Task.Run(() => GenerateMontage(baseFiles, montageDescription, fileName, progress));

                TryPublishEditedMontage(session, montage, name, eegFiles, montageDescription);
            });
        }

        private static bool TryPublishNewMontage(Session session, string name, BtvProgram[] eegFiles, List<ChannelCorrespondance> montageDescription)
        {
            if (!Session.IsCurrent(session))
            {
                BtvLog.Log("Discarded a montage built for a previous patient session.");
                return false;
            }

            session.Montages.Add(new BtvMontage(name, eegFiles, montageDescription));
            SendMontageListUpdate(session.Montages.Count - 1);
            return true;
        }

        private static bool TryPublishEditedMontage(Session session, BtvMontage montage, string name, BtvProgram[] eegFiles, List<ChannelCorrespondance> montageDescription)
        {
            if (!Session.IsCurrent(session))
            {
                BtvLog.Log("Discarded a montage edit built for a previous patient session.");
                return false;
            }

            if (!session.Montages.Contains(montage))
            {
                Debug.LogWarning("Discarded a completed montage edit because its target montage no longer exists.");
                return false;
            }

            montage.Load(name, eegFiles, montageDescription);
            SendMontageListUpdate(session.Montages.IndexOf(montage));
            return true;
        }

        private static void SendMontageListUpdate(int selectedMontageID)
        {
            MontageMessage message = new MontageMessage
            {
                TaskToExecute = MontageMessage.Task.UpdateMontageList,
                SelectedMontageID = selectedMontageID
            };
            Messenger.Default.Send(message, MessageContext.MontageMessage);
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
        /// <summary>
        /// Builds the montage files by evaluating each channel's correspondance expression.
        /// Runs on a worker thread (CPU-bound, only touches the snapshot it was given); progress
        /// reports are marshalled back to the main thread by the caller's Progress instance.
        /// </summary>
        private static BtvProgram[] GenerateMontage(BtvProgram[] baseFiles, List<ChannelCorrespondance> montageDescription, string fileName, IProgress<(float progress, string message)> onChangeProgress)
        {
            int globalProgress = 0;
            int totalNumberOfValidFiles = string.IsNullOrEmpty(fileName) ? baseFiles.Count(f => f != null) : 1;
            BtvProgram[] eegFiles = new BtvProgram[6];
            string errorList = "";
            for (int i = 0; i < 6; ++i)
            {
                BtvProgram baseEEGFile = baseFiles[i];

                if (baseEEGFile == null)
                    continue;

                onChangeProgress.Report(((float)globalProgress / totalNumberOfValidFiles, string.Format("Preparing file {0}", baseEEGFile.Description)));
                eegFiles[i] = new BtvProgram(baseEEGFile);

                if (!string.IsNullOrEmpty(fileName) && baseEEGFile.Description != fileName)
                    continue;

                ChannelContext context = new ChannelContext(baseEEGFile.Channels);

                float localProgress = 0;
                float localProgressStep = 1f / eegFiles[i].Channels.Count;
                foreach (var channel in eegFiles[i].Channels)
                {
                    onChangeProgress.Report(((float)(globalProgress + localProgress) / totalNumberOfValidFiles, string.Format("File {0} (channel {1})", baseEEGFile.Description, channel.Label)));

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
            if (!string.IsNullOrEmpty(errorList)) // TODO : make this visible for user maybe
                BtvLog.Log(errorList);
            return eegFiles;
        }
    }
}
