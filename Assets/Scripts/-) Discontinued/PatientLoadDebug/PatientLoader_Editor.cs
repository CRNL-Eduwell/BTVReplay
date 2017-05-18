//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//using System;
//using UnityEngine;
//using UnityEngine.UI;


//#if UNITY_EDITOR
//[CustomEditor(typeof(PatientLoader))]
//public class PatientLoader_Editor : Editor
//{
//    SerializedProperty PatientLoader;
//    Pat currentPat;

//    bool showGeneralPaths = false;
//    bool showGeneralPaths2 = false;

//    void OnEnable()
//    {
//        currentPat = new Pat();
//        PatientLoader = serializedObject.FindProperty("PatientLoader");
//    }

//    public override void OnInspectorGUI()
//    {
//        PatientLoader myTarget = (PatientLoader)target;

//        EditorGUILayout.Space();
//        GUILayout.BeginHorizontal();
//        if (GUILayout.Button("Load This \nPatient", new GUILayoutOption[] { GUILayout.Width(120), GUILayout.Height(40) }))
//        {
//            myTarget.addPat(currentPat);
//            Debug.Log("Number of Pat : " + myTarget.currentPats.Count);
//            myTarget.SaveList();
//            Debug.Log("File Saved");
//        }
//        GUILayout.EndHorizontal();

//        EditorGUILayout.Space();

//        currentPat.lhemi = EditorGUILayout.TextField("LHemi", currentPat.lhemi);
//        currentPat.rhemi = EditorGUILayout.TextField("RHemi", currentPat.rhemi);
//        currentPat.pts = EditorGUILayout.TextField("PTS",currentPat.pts);
//        currentPat.pos = EditorGUILayout.TextField("POS",currentPat.pos);
//        currentPat.sm0 = EditorGUILayout.TextField("sm0",currentPat.sm0);
//        currentPat.sm250 = EditorGUILayout.TextField("sm250",currentPat.sm250);
//        currentPat.sm500 = EditorGUILayout.TextField("sm500",currentPat.sm500);
//        currentPat.sm1000 = EditorGUILayout.TextField("sm1000",currentPat.sm1000);
//        currentPat.sm2500 = EditorGUILayout.TextField("sm2500",currentPat.sm2500);
//        currentPat.sm5000 = EditorGUILayout.TextField("sm5000",currentPat.sm5000);
//        currentPat.prov = EditorGUILayout.TextField("prov",currentPat.prov);
//        currentPat.video = EditorGUILayout.TextField("video",currentPat.video);

//        GUILayout.BeginHorizontal();
//        if (GUILayout.Button("Load List", new GUILayoutOption[] { GUILayout.Width(120), GUILayout.Height(40) }))
//        {
//            myTarget.LoadList();
//            Debug.Log(myTarget.currentPats.Count);
//            Debug.Log("List Loaded");
//        }
//        if (GUILayout.Button("Save List", new GUILayoutOption[] { GUILayout.Width(120), GUILayout.Height(40) }))
//        {
//            myTarget.SaveList();
//            Debug.Log("File Saved");
//        }
//        GUILayout.EndHorizontal();

//        EditorGUILayout.Space();

//        showGeneralPaths = EditorGUILayout.Foldout(showGeneralPaths, "Patient Loaded");
//        if (showGeneralPaths)
//        {
//            EditorGUI.indentLevel++;
//            for (int i = 0; i < myTarget.currentPats.Count; i++)
//            {
//                GUILayout.BeginHorizontal();
//                showGeneralPaths2 = EditorGUILayout.Foldout(showGeneralPaths2, "Pat " + i);
//                if (GUILayout.Button("Load Me", new GUILayoutOption[] { GUILayout.Width(120), GUILayout.Height(40) }))
//                {
//                    BTVMedia media = GameObject.Find("Canvas").transform.GetChild(6).GetChild(1).GetComponent<BTVMedia>();
//                    GameObject.Find("Canvas").transform.GetChild(6).gameObject.SetActive(true);
//                    media.loadValueCheat(myTarget.currentPats[i]);
//                    Debug.Log("Loading This Patient");
//                }
//                if (GUILayout.Button("Delete Me", new GUILayoutOption[] { GUILayout.Width(120), GUILayout.Height(40) }))
//                {
//                    myTarget.removePatAt(i);
//                    Debug.Log("Removed: " + i);
//                }

//                GUILayout.EndHorizontal();

//                if (showGeneralPaths2)
//                {
//                    myTarget.currentPats[i].lhemi = EditorGUILayout.TextField("LHemi", myTarget.currentPats[i].lhemi);
//                    myTarget.currentPats[i].rhemi = EditorGUILayout.TextField("RHemi", myTarget.currentPats[i].rhemi);
//                    myTarget.currentPats[i].pts = EditorGUILayout.TextField("PTS", myTarget.currentPats[i].pts);
//                    myTarget.currentPats[i].pos = EditorGUILayout.TextField("POS", myTarget.currentPats[i].pos);
//                    myTarget.currentPats[i].sm0 = EditorGUILayout.TextField("sm0", myTarget.currentPats[i].sm0);
//                    myTarget.currentPats[i].sm250 = EditorGUILayout.TextField("sm250", myTarget.currentPats[i].sm250);
//                    myTarget.currentPats[i].sm500 = EditorGUILayout.TextField("sm500", myTarget.currentPats[i].sm500);
//                    myTarget.currentPats[i].sm1000 = EditorGUILayout.TextField("sm1000", myTarget.currentPats[i].sm1000);
//                    myTarget.currentPats[i].sm2500 = EditorGUILayout.TextField("sm2500", myTarget.currentPats[i].sm2500);
//                    myTarget.currentPats[i].sm5000 = EditorGUILayout.TextField("sm5000", myTarget.currentPats[i].sm5000);
//                    myTarget.currentPats[i].prov = EditorGUILayout.TextField("prov", myTarget.currentPats[i].prov);
//                    myTarget.currentPats[i].video = EditorGUILayout.TextField("video", myTarget.currentPats[i].video);
//                }
//            }
//        }


//    }
//}
//#endif
