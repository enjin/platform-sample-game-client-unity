using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnjinSdkSmoke.Editor
{
    /// <summary>
    /// Editor convenience: builds (or loads) a minimal smoke-test scene wired
    /// to a SdkSmokeConfig asset, so you don't have to construct it by hand.
    ///
    /// Menu path: Enjin ▸ Open SDK Smoke Scene
    /// </summary>
    internal static class SdkSmokeSceneBuilder
    {
        private const string ScenePath = "Assets/EnjinSdkSmoke/SdkSmoke.unity";
        private const string ConfigPath = "Assets/EnjinSdkSmoke/SdkSmokeConfig.asset";

        [MenuItem("Enjin/Open SDK Smoke Scene", priority = 100)]
        public static void OpenScene()
        {
            EnsureConfigExists();

            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                return;
            }

            // Save the active scene first so we don't lose unsaved work.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "SdkSmoke";

            var go = new GameObject("SdkSmokeRunner");
            var runner = go.AddComponent<SdkSmokeRunner>();
            runner.config = AssetDatabase.LoadAssetAtPath<SdkSmokeConfig>(ConfigPath);

            // Ensure the scene path is unique on disk if a prior orphan exists.
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SdkSmoke] Created {ScenePath}. Fill in {ConfigPath} with your platform URL + token, then press Play.");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(ConfigPath);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        [MenuItem("Enjin/Create SDK Smoke Config", priority = 101)]
        public static void CreateConfig()
        {
            EnsureConfigExists();
            var cfg = AssetDatabase.LoadAssetAtPath<SdkSmokeConfig>(ConfigPath);
            Selection.activeObject = cfg;
            EditorGUIUtility.PingObject(cfg);
        }

        private static void EnsureConfigExists()
        {
            if (File.Exists(ConfigPath)) return;

            var dir = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var asset = ScriptableObject.CreateInstance<SdkSmokeConfig>();
            AssetDatabase.CreateAsset(asset, ConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SdkSmoke] Created {ConfigPath}. Open it in the Inspector and fill in the platform URL + token.");
        }
    }
}
