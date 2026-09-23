using BTV.Services.SubjectInfoService;
using BTV.Services.UserPreferencesService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BTV.Services;

public class ScreenshotTaker : MonoBehaviour
{
    private Session m_PatientSession = null;

    private void Awake()
    {
        Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
    }

    private void OnLoaderMessage(LoaderMessage message)
    {
        if (message.Task == LoaderMessage.LoaderTask.MediaLoader && Session.IsCurrent(message.PatientSession))
            m_PatientSession = message.PatientSession;
    }

    public void TakeScreenshot()
    {
        if (!Session.IsCurrent(m_PatientSession)) return;
        try
        {
            string preferencePath = UserPreferencesService.UserPreferences.GeneralPreferences.ExportPath;
            if (string.IsNullOrEmpty(preferencePath)) preferencePath = Application.dataPath + "/..";

            string folderPath = Path.GetFullPath(preferencePath + "/Screenshots/");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string screenshotPath = folderPath + string.Format("{0}_FullView.png", SubjectInfoService.GetSubjectName(m_PatientSession));
            GenerateUniqueSavePath(ref screenshotPath);
            ScreenCapture.CaptureScreenshot(screenshotPath);
        }
        catch (Exception e)
        {
            BtvLog.Handled("Screenshot could not be saved", e);
            ApplicationState.displayMessage("NOK", "Screenshots could not be saved", "Please verify your rights");
        }
    }

    private void GenerateUniqueSavePath(ref string path)
    {
        string extension = Path.GetExtension(path);
        string pathWithoutExtension = Path.GetFullPath(path).Remove(Path.GetFullPath(path).Length - extension.Length);
        int count = 0;
        while (File.Exists(path))
        {
            string temp = string.Format("{0}({1})", pathWithoutExtension, ++count);
            path = temp + extension;
        }
    }
}
