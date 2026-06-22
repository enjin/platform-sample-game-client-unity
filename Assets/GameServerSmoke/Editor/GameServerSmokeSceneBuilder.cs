using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameServerSmoke.Editor
{
    /// <summary>
    /// Editor convenience: builds (or loads) a minimal smoke-test scene wired
    /// to a <see cref="GameServerSmokeConfig"/> asset and the existing
    /// EnjinManager prefab, so you don't have to construct it by hand.
    ///
    /// Menu paths:
    ///   Enjin > Open Game Server Smoke Scene
    ///   Enjin > Create Game Server Smoke Config
    /// </summary>
    internal static class GameServerSmokeSceneBuilder
    {
        private const string ScenePath = "Assets/GameServerSmoke/GameServerSmoke.unity";
        private const string ConfigPath = "Assets/GameServerSmoke/GameServerSmokeConfig.asset";
        private const string EnjinManagerPrefabPath = "Assets/Enjin Integration/Prefabs/EnjinManager.prefab";

        [MenuItem("Enjin/Open Game Server Smoke Scene", priority = 110)]
        public static void OpenScene()
        {
            EnsureConfigExists();

            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "GameServerSmoke";

            var go = new GameObject("GameServerSmokeRunner");
            var runner = go.AddComponent<GameServerSmokeRunner>();
            runner.config = AssetDatabase.LoadAssetAtPath<GameServerSmokeConfig>(ConfigPath);
            runner.enjinManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnjinManagerPrefabPath);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GameServerSmoke] Created {ScenePath}. " +
                      $"Fill in {ConfigPath} with email/password, then press Play.");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(ConfigPath);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        [MenuItem("Enjin/Create Game Server Smoke Config", priority = 111)]
        public static void CreateConfig()
        {
            EnsureConfigExists();
            var cfg = AssetDatabase.LoadAssetAtPath<GameServerSmokeConfig>(ConfigPath);
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

            var asset = ScriptableObject.CreateInstance<GameServerSmokeConfig>();
            AssetDatabase.CreateAsset(asset, ConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GameServerSmoke] Created {ConfigPath}. " +
                      "Open it in the Inspector and fill in the email + password.");
        }
    }
}
