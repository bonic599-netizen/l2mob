using UnityEditor;
using UnityEngine;

namespace L2Mobile.Editor
{
    [InitializeOnLoad]
    public static class MetaUpgradeHelper
    {
        private const string PrefKey = "MetaUpgradeHelper_Done_v2";

        static MetaUpgradeHelper()
        {
            EditorApplication.delayCall += CheckAndUpgrade;
        }

        private static void CheckAndUpgrade()
        {
            if (!EditorPrefs.GetBool(PrefKey, false))
            {
                EditorPrefs.SetBool(PrefKey, true);
                Debug.Log("[MetaUpgradeHelper] Upgrading and re-saving meta files for FMOD...");
                AssetDatabase.ForceReserializeAssets(new string[] { "Assets/Plugins/FMOD" });
                AssetDatabase.SaveAssets();
                Debug.Log("[MetaUpgradeHelper] FMOD meta upgrade finished.");
            }
        }

        [MenuItem("Tools/Upgrade All Meta Files")]
        public static void UpgradeAll()
        {
            AssetDatabase.ForceReserializeAssets();
            AssetDatabase.SaveAssets();
            Debug.Log("[MetaUpgradeHelper] All project meta files reserialized and saved.");
        }
    }
}
