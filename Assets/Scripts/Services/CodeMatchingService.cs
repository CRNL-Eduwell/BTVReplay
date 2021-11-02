using Assets.Scripts.Data.Factory;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BTV.Services.CodeMatchingService
{
    public static class CodeMatchingService
    {
        public static bool HasCodes { get { return m_CodeComment.Count > 0; } }
        private static Dictionary<int, string> m_CodeComment = new Dictionary<int, string>();

        public static void Reset()
        {
            m_CodeComment = new Dictionary<int, string>();
        }

        public static void Load(string filePath)
        {
            if (File.Exists(filePath))
            {
                ICodeCommentContext file = CodeMatchingFactory.GetMatchingContext(filePath);

                m_CodeComment = new Dictionary<int, string>();
                for (int i = 0; i < file.Pairs.Count; i++)
                {
                    m_CodeComment.Add(file.Pairs[i].Code, file.Pairs[i].Comment);
                }
            }
        }

        public static string GetCommentFromCode(int code)
        {
            if (m_CodeComment.ContainsKey(code))
            {
                return m_CodeComment[code];
            }
            return "";
        }
    }
}