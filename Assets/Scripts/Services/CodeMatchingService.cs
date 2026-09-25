using Assets.Scripts.Data.Factory;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BTV.Services.CodeMatchingService
{
    public static class CodeMatchingService
    {
        public static bool HasCodes { get { return HasCodesFor(Session.Current); } }
        private static Dictionary<int, string> CodeComments { get { return Session.Current.CodeComments; } }

        public static void Reset()
        {
            Session.Current.CodeComments = new Dictionary<int, string>();
        }

        public static void Load(string filePath)
        {
            Load(Session.Current, filePath);
        }

        public static void Load(Session session, string filePath)
        {
            if (File.Exists(filePath))
            {
                ICodeCommentContext file = CodeMatchingFactory.GetMatchingContext(filePath);

                session.CodeComments = new Dictionary<int, string>();
                for (int i = 0; i < file.Pairs.Count; i++)
                {
                    session.CodeComments.Add(file.Pairs[i].Code, file.Pairs[i].Comment);
                }
            }
        }

        public static string GetCommentFromCode(int code)
        {
            return GetCommentFromCode(Session.Current, code);
        }

        public static bool HasCodesFor(Session session)
        {
            return session.CodeComments.Count > 0;
        }

        public static string GetCommentFromCode(Session session, int code)
        {
            if (session.CodeComments.ContainsKey(code))
            {
                return session.CodeComments[code];
            }
            return "";
        }

        public static List<KeyValuePair<int, string>> GetCodesAndComment()
        {
            return GetCodesAndComment(Session.Current);
        }

        public static List<KeyValuePair<int, string>> GetCodesAndComment(Session session)
        {
            List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();

            foreach (var kvp in session.CodeComments)
            {
                result.Add(new KeyValuePair<int, string>(kvp.Key, kvp.Value));
            }

            return result;
        }
    }
}
