using BTV.Services.UserPreferencesService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    public void TakeScreenshot()
    {
        try
        {
            string preferencePath = UserPreferencesService.UserPreferences.GeneralPreferences.ExportPath;
            if (string.IsNullOrEmpty(preferencePath)) preferencePath = Application.dataPath + "/..";

            string folderPath = Path.GetFullPath(preferencePath + "/Screenshots/");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string screenshotPath = folderPath + string.Format("{0}_FullView.png", ApplicationState.Module3D.Patient.PatientName);
            GenerateUniqueSavePath(ref screenshotPath);
            ScreenCapture.CaptureScreenshot(screenshotPath);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
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
