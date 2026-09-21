using Assets.Scripts.Data.Factory;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BTV.Services.CodeMatchingService
{
    public static class CodeMatchingService
    {
        public static bool HasCodes { get { return CodeComments.Count > 0; } }
        private static Dictionary<int, string> CodeComments { get { return Session.Current.CodeComments; } }

        public static void Reset()
        {
            Session.Current.CodeComments = new Dictionary<int, string>();
        }

        public static void Load(string filePath)
        {
            if (File.Exists(filePath))
            {
                ICodeCommentContext file = CodeMatchingFactory.GetMatchingContext(filePath);

                Session.Current.CodeComments = new Dictionary<int, string>();
                for (int i = 0; i < file.Pairs.Count; i++)
                {
                    CodeComments.Add(file.Pairs[i].Code, file.Pairs[i].Comment);
                }
            }
        }

        public static string GetCommentFromCode(int code)
        {
            if (CodeComments.ContainsKey(code))
            {
                return CodeComments[code];
            }
            return "";
        }

        public static List<KeyValuePair<int, string>> GetCodesAndComment()
        {
            List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();

            foreach (var kvp in CodeComments)
            {
                result.Add(new KeyValuePair<int, string>(kvp.Key, kvp.Value));
            }

            return result;
        }
    }
}
