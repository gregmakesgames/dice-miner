#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace GrishaGuWorkshop
{
    public class ToolsMenu
    {
        [MenuItem("Dev Tools/Clear Prefs")]
        public static void ClearPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
        
        [MenuItem("Dev Tools/Set DEV")]
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
        
        [MenuItem("Dev Tools/Set RELEASE")]
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