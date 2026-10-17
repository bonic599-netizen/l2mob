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
            EditorApplication.delayCall += FixMissingPrefabsInActiveScene;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += (s, m) => FixMissingPrefabsInActiveScene();
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

        [MenuItem("Tools/Fix Missing Prefabs in Active Scene")]
        public static void FixMissingPrefabsInActiveScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return;

            int count = 0;
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var t in transforms)
                {
                    if (t == null) continue;
                    if (PrefabUtility.IsPartOfMissingPrefab(t.gameObject))
                    {
                        var nearest = PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject);
                        if (nearest != null)
                        {
                            PrefabUtility.UnpackPrefabInstance(nearest, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                            count++;
                        }
                    }
                }
            }

            if (count > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                Debug.Log($"[MetaUpgradeHelper] Successfully unpacked {count} missing prefab instance(s) in '{scene.name}' and saved the scene.");
            }
            else
            {
                Debug.Log($"[MetaUpgradeHelper] No missing prefab instances found in '{scene.name}'.");
            }
        }
    }
}
