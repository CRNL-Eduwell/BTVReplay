using Assets.Scripts.Data.Factory;
using BTV.Data;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Data.Files
{
    public class MatchFile : ICodeCommentContext
    {
        public string FilePath { get; set; } = "";

        public List<CodeCommentPair> Pairs { get; set; } = new List<CodeCommentPair>();

        public MatchFile()
        { 
        
        }

        public MatchFile(string filePath)
        {
            FilePath = filePath;

            if (File.Exists(FilePath))
                Load(FilePath);
            else
                Debug.LogError("MatchFile => Filepath : " + FilePath + " does not exist ");

        }

        private int Load(string FilePath)
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(FilePath))
                {
                    Pairs = JsonConvert.DeserializeObject<List<CodeCommentPair>>(streamReader.ReadToEnd(), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("The chosen MatchFile could not be read:");
                Console.WriteLine(e.Message);
                Pairs = new List<CodeCommentPair>();
                return -1;
            }
        }

    }
}