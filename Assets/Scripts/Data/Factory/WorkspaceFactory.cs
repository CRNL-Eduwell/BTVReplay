using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace Assets.Scripts.Data.Factory
{
    public class WorkspaceFactory
    {
        public static IWorkspaceContext GetWorkspaceContext(string FilePath)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".workspace":
                    return new WorkspaceFile(FilePath);
                default:
                    throw new ArgumentException("WorkspaceFactory.GetWorkspaceContext : file extension not supported => " + fileInfo.Extension); ;
            }
        }

        public static void SaveWorkspace(string FilePath, Workspace workspace)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".workspace":
                    WorkspaceFile.Save(FilePath, workspace);
                    break;
                default:
                    throw new ArgumentException("WorkspaceFactory.SaveWorkspace : file extension not supported => " + fileInfo.Extension); ;
            }
        }
    }
}