#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace GrishaGuWorkshop
{
    public class ToolsMenu
    {
        [MenuItem("Grisha WSH/Clear Prefs", priority = 200)]
        public static void ClearPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
        
        [MenuItem("Grisha WSH/Set DEV", priority = 201)]
        public static void SetDev()
        {
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), 
                new string[]
            {
                "ODIN_INSPECTOR",
                "ODIN_INSPECTOR_3",
                "USE_UNITY_IAP",
                "APP_METRICA_TRACK_LOCATION_DISABLED",
                "DEV"
            });
        }
        
        [MenuItem("Grisha WSH/Set RELEASE", priority = 202)]
        public static void SetRelease()
        {
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup),
                new string[]
            {
                "USE_UNITY_IAP",
                "APP_METRICA_TRACK_LOCATION_DISABLED"
            });
        }
    }
}
#endif