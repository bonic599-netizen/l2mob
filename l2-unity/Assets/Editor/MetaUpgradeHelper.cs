using UnityEditor;
using UnityEngine;

namespace L2Mobile.Editor
{
    [InitializeOnLoad]
    public static class MetaUpgradeHelper
    {
        private const string PrefKey = "MetaUpgradeHelper_Done_v3";

        static MetaUpgradeHelper()
        {
            EditorApplication.delayCall += CheckAndUpgrade;
        }

        private static void CheckAndUpgrade()
        {
            if (!EditorPrefs.GetBool(PrefKey, false))
            {
                EditorPrefs.SetBool(PrefKey, true);
                Debug.Log("[MetaUpgradeHelper] Refreshing assets and reserializing...");
                AssetDatabase.Refresh();
                AssetDatabase.ForceReserializeAssets(new string[] { "Assets/Plugins/FMOD", "Assets/Materials" });
                AssetDatabase.SaveAssets();
                Debug.Log("[MetaUpgradeHelper] Material and meta upgrade finished.");
            }
        }

        [MenuItem("Tools/Fix Purple Squares & Refresh Materials")]
        public static void FixPurpleSquares()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.ForceReserializeAssets(new string[] { "Assets/Materials" });
            AssetDatabase.SaveAssets();
            Debug.Log("[MetaUpgradeHelper] Lobby materials refreshed and reserialized.");
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
