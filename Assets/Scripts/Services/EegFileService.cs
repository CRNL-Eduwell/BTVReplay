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
                SetSelectedMontage(Session.Current, value);
            }
        }
        public static BtvMontage CurrentMontage { get { return GetCurrentMontage(Session.Current); } }
        public static BtvMontage DefaultMontage { get { return GetDefaultMontage(Session.Current); } }

        public static BtvMontage GetCurrentMontage(Session session)
        {
            return session.Montages[session.SelectedMontageID];
        }

        public static IReadOnlyList<BtvMontage> GetMontages(Session session)
        {
            return session.Montages;
        }

        public static BtvMontage GetDefaultMontage(Session session)
        {
            return session.Montages[0];
        }

        public static void SetSelectedMontage(Session session, int value)
        {
            session.SelectedMontageID = value;
            MontageMessage message = new MontageMessage
            {
                TaskToExecute = MontageMessage.Task.SelectMontage,
                SelectedMontageID = value
            };
            Messenger.Default.Send(message, MessageContext.MontageMessage);
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
            if (FileID >= EegSlots.Count)
                throw new ArgumentException("There are only " + EegSlots.Count + " EEG file slots, fileID argument is wrong => " + FileID);

            // The native EEG read + managed copy runs on a worker; the montage slot is assigned
            // after the await, back on the main thread (the old version mutated the static
            // montage from the worker thread).
            Session session = Session.Current;
            BtvProgram eegFile = await Task.Run(() =>
            {
                if (fileInfo.Files.Length == 0)
                    return null;
                // A file that vanished after the loadability check used to leave its slot empty
                // with no trace at all; throwing lets the loader report it.
                if (!System.IO.File.Exists(fileInfo.Files[0]))
                    throw new System.IO.FileNotFoundException("EEG file not found.", fileInfo.Files[0]);
                // Header only: the samples stay on disk and are read by range. The statistics
                // pass below reads the file once, block by block, as the whole load used to.
                IEegDataContainer container = new IEegDataContainer(fileInfo);
                try
                {
                    return new BtvProgram(container.Source, container.Labels, container, description);
                }
                catch
                {
                    container.Source.Dispose();
                    throw;
                }
            });

            TryPublishEegFile(session, eegFile, FileID);
        }

        private static bool TryPublishEegFile(Session session, BtvProgram eegFile, int fileID)
        {
            if (!Session.IsCurrent(session))
            {
                BtvLog.Log("Discarded an EEG file loaded for a previous patient session.");
                eegFile?.OwnedSource?.Dispose();
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
            if (FileID >= EegSlots.Count) return false;

            return GetCurrentMontage(session).EegFiles[FileID] != null;
        }

        public static void AddMontage(string name, List<ChannelCorrespondance> montageDescription, string fileName = "")
        {
            AddMontage(Session.Current, name, montageDescription, fileName);
        }

        public static void AddMontage(Session session, string name, List<ChannelCorrespondance> montageDescription, string fileName = "")
        {
            List<BtvMontage> montages = session.Montages;
            // Generate unique name
            if (montages.Any(m => m.Name == name))
            {
                int count = 1;
                string newName = string.Format("{0}({1})", name, count);
                while (montages.Any(m => m.Name == newName))
                {
                    count++;
                    newName = string.Format("{0}({1})", name, count);
                }
                name = newName;
            }
            BtvProgram[] baseFiles = GetDefaultMontage(session).EegFiles;
            LoadingManager.Load(async progress =>
            {
                (BtvProgram[] eegFiles, string errors) = await Task.Run(() => GenerateMontage(baseFiles, montageDescription, fileName, progress));

                if (TryPublishNewMontage(session, name, eegFiles, montageDescription))
                    ReportMontageErrors(name, errors);
            });
        }
        public static void RemoveSelectedMontage()
        {
            RemoveSelectedMontage(Session.Current);
        }

        public static void RemoveSelectedMontage(Session session)
        {
            BtvMontage currentMontage = GetCurrentMontage(session);
            if (currentMontage.IsCustom)
            {
                session.Montages.Remove(currentMontage);
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
            EditMontage(Session.Current, montage, name, montageDescription, fileName);
        }

        public static void EditMontage(Session session, BtvMontage montage, string name, List<ChannelCorrespondance> montageDescription, string fileName = "")
        {
            List<BtvMontage> montages = session.Montages;
            // Generate unique name
            if (montages.Any(m => m.Name == name && m != montage))
            {
                int count = 1;
                string newName = string.Format("{0}({1})", name, count);
                while (montages.Any(m => m.Name == newName && m != montage))
                {
                    count++;
                    newName = string.Format("{0}({1})", name, count);
                }
                name = newName;
            }
            BtvProgram[] baseFiles = GetDefaultMontage(session).EegFiles;
            LoadingManager.Load(async progress =>
            {
                (BtvProgram[] eegFiles, string errors) = await Task.Run(() => GenerateMontage(baseFiles, montageDescription, fileName, progress));

                if (TryPublishEditedMontage(session, montage, name, eegFiles, montageDescription))
                    ReportMontageErrors(name, errors);
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

        // Main thread only (after the await). Expression errors used to go to BtvLog, which is
        // compiled out of release builds, so a montage with unparsable channels silently showed
        // base values for them.
        private static void ReportMontageErrors(string montageName, string errors)
        {
            if (string.IsNullOrEmpty(errors)) return;
            Debug.LogWarning("Montage " + montageName + " built with errors:\n" + errors);
            const int maxLines = 12;
            string[] lines = errors.TrimEnd('\n').Split('\n');
            string shown = string.Join("\n", lines.Take(maxLines));
            if (lines.Length > maxLines)
                shown += string.Format("\n... and {0} more.", lines.Length - maxLines);
            ApplicationState.displayMessage("Montage built with errors", "NOK",
                "Montage \"" + montageName + "\" was created, but some channel expressions could not be evaluated and keep their base values:\n\n" + shown);
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
            LoadMontage(Session.Current, path);
        }

        public static void LoadMontage(Session session, string path)
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
            AddMontage(session, name, montageDescription);
        }
        /// <summary>
        /// Builds the montage files by evaluating each channel's correspondance expression.
        /// Runs on a worker thread (CPU-bound, only touches the snapshot it was given); progress
        /// reports are marshalled back to the main thread by the caller's Progress instance.
        ///
        /// A montage used to start from a deep copy of every loaded file, then overwrite the
        /// copy sample by sample: each montage cost the full size of the recordings (3.9 GiB for
        /// a 6 h, 183-channel TRC) even when it changed a single channel, and files outside
        /// fileName were copied only to be left unchanged. Unchanged channels and files are now
        /// shared with the base files, and expressions are evaluated as their samples are read
        /// (DerivedSampleSource): building a montage reads nothing but the stored samples.
        /// </summary>
        public static (BtvProgram[] files, string errors) GenerateMontage(BtvProgram[] baseFiles, List<ChannelCorrespondance> montageDescription, string fileName, IProgress<(float progress, string message)> onChangeProgress)
        {
            int globalProgress = 0;
            int totalNumberOfValidFiles = string.IsNullOrEmpty(fileName) ? baseFiles.Count(f => f != null) : 1;
            BtvProgram[] eegFiles = new BtvProgram[EegSlots.Count];
            string errorList = "";
            for (int i = 0; i < EegSlots.Count; ++i)
            {
                BtvProgram baseEEGFile = baseFiles[i];

                if (baseEEGFile == null)
                    continue;

                onChangeProgress.Report(((float)globalProgress / totalNumberOfValidFiles, string.Format("Preparing file {0}", baseEEGFile.Description)));

                if (!string.IsNullOrEmpty(fileName) && baseEEGFile.Description != fileName)
                {
                    eegFiles[i] = new BtvProgram(baseEEGFile, baseEEGFile.Channels);
                    continue;
                }

                List<BtvChannel> channels = new List<BtvChannel>(baseEEGFile.Channels.Count);
                long sampleCount = baseEEGFile.Channels.Count > 0 ? baseEEGFile.Channels[0].Source.SampleCount : 0;
                // Expression channels, evaluated lazily by one source per file: their slots in
                // `channels` are filled once every expression of the file is known.
                List<DerivedSampleSource.Expression> expressions = new List<DerivedSampleSource.Expression>();
                List<int> expressionSlots = new List<int>();

                float localProgress = 0;
                float localProgressStep = 1f / baseEEGFile.Channels.Count;
                foreach (BtvChannel baseChannel in baseEEGFile.Channels)
                {
                    onChangeProgress.Report(((float)(globalProgress + localProgress) / totalNumberOfValidFiles, string.Format("File {0} (channel {1})", baseEEGFile.Description, baseChannel.Label)));

                    ChannelCorrespondance correspondance = montageDescription.FirstOrDefault(c => c.BaseLabel == baseChannel.Label);
                    try
                    {
                        if (correspondance == null)
                        {
                            channels.Add(baseChannel);
                        }
                        else
                        {
                            Node expression = Parser.Parse(correspondance.CorrespondingLabel);
                            BtvChannel shared = SharedMontageChannel(baseChannel, baseEEGFile.Channels, expression);
                            if (shared == null)
                            {
                                // Checked and evaluated on the stored samples here, so a bad
                                // expression is reported and the base channel kept, as before.
                                expressions.Add(DerivedSampleSource.Expression.Prepare(expression, baseEEGFile.Channels, sampleCount));
                                expressionSlots.Add(channels.Count);
                            }
                            channels.Add(shared);
                        }
                    }
                    catch (Exception e)
                    {
                        // The base values used to be restored by re-parsing the channel's own label,
                        // which threw again for a label the parser rejects (a leading digit, a
                        // space) and aborted the whole montage.
                        errorList += string.Format("Could not parse correspondance {0} of channel {1} in file {2}. Keeping base values. Reason: {3}\n", correspondance.CorrespondingLabel, correspondance.BaseLabel, baseEEGFile.Description, e.Message);
                        channels.Add(baseChannel);
                    }
                    localProgress += localProgressStep;
                }

                if (expressions.Count > 0)
                {
                    BtvChannel first = baseEEGFile.Channels[0];
                    DerivedSampleSource derived = new DerivedSampleSource(expressions, first.Frequency, sampleCount);
                    BlockCache cache = new BlockCache(derived);
                    for (int k = 0; k < expressions.Count; k++)
                    {
                        BtvChannel baseChannel = baseEEGFile.Channels[expressionSlots[k]];
                        channels[expressionSlots[k]] = new BtvChannel(baseChannel.Label, baseChannel.ID, derived, k, expressions[k].Stats, cache);
                    }
                }
                eegFiles[i] = new BtvProgram(baseEEGFile, channels);
                globalProgress++;
            }
            // Reported to the user by the caller, back on the main thread.
            return (eegFiles, errorList);
        }

        /// <summary>
        /// A montage channel that is a bare channel name shares that channel's samples: the
        /// channel itself when it maps to its own label, a renamed view otherwise. Null for any
        /// other expression, which a DerivedSampleSource evaluates on the samples being read.
        /// Its median/min/max come from the montage values (the base channel's used to be kept, so
        /// a bipolar trace was centred and scaled with the referential signal's statistics), and
        /// it no longer allocates the whole recording: a bipolar montage of a 6 h, 183-channel
        /// TRC used to cost 3.9 GiB.
        /// </summary>
        public static BtvChannel SharedMontageChannel(BtvChannel baseChannel, List<BtvChannel> baseChannels, Node expression)
        {
            if (expression is NodeVariable variable)
            {
                BtvChannel source = baseChannels.FirstOrDefault(c => c.Label == variable.VariableName);
                if (source == baseChannel)
                    return baseChannel;
                if (source != null)
                    return new BtvChannel(source, baseChannel.Label, baseChannel.ID);
            }
            return null;
        }
    }
}
