using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace needle.Weaver
{
    public class WeaverSettingsProvider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create()
        {
            try
            {
                WeaverSettings.instance.Save();
                return new WeaverSettingsProvider("Project/Needle/Weaver", SettingsScope.Project);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }

            return null;
        }

        private WeaverSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
        

        private Vector2 scroll;

        public override void OnGUI(string searchContext)
        {
            InitStyles();

            EditorGUILayout.BeginVertical();
            
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.BeginVertical();
            
            WeaverSettings.instance.PatchOnBuild = EditorGUILayout.Toggle("Allow Patch on Build", WeaverSettings.instance.PatchOnBuild);

            var pm = PatchMethodDatabase.AllPatchedMethods();
            foreach (var patch in pm)
            {
                var enabled = EditorGUILayout.ToggleLeft(patch.FullName, patch.FoundTarget);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private GUIStyle patchTitleInactive, patchTitleActive, descriptionStyle;

        private void InitStyles()
        {
            if (descriptionStyle == null)
                descriptionStyle = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true,
                    normal = {textColor = new Color(.5f, .5f, .5f)}
                };

            if (patchTitleActive == null)
            {
                patchTitleActive = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold,
                };
            }

            if (patchTitleInactive == null)
                patchTitleInactive = new GUIStyle(patchTitleActive)
                {
                    normal = {textColor = new Color(.5f, .5f, .5f),},
                };
        }
    }
}