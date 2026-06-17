using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace EnjinSdkSmoke.Editor
{
    /// <summary>
    /// Headless entry-point for the SDK smoke test. Drives the same
    /// <see cref="SdkSmokeRunner"/> used in the interactive scene, but from an
    /// editor batch invocation so CI (or a developer without Unity open) can
    /// verify the published UPM package end-to-end.
    ///
    /// Usage:
    ///   Unity -batchmode -nographics -projectPath . \
    ///         -executeMethod EnjinSdkSmoke.Editor.SdkSmokeBatch.Run \
    ///         -logFile /path/to/smoke.log
    ///
    /// The SDK calls run on a thread-pool thread (no Unity SynchronizationContext
    /// to deadlock against) and all output is mirrored to the Unity log via
    /// Debug.Log with the [SdkSmoke] prefix. Exit code is 0 on completion, 2 if
    /// the config asset is missing, 1 on an unhandled fault.
    /// </summary>
    public static class SdkSmokeBatch
    {
        private const string ConfigPath = "Assets/EnjinSdkSmoke/SdkSmokeConfig.asset";

        public static void Run()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<SdkSmokeConfig>(ConfigPath);
            if (cfg == null)
            {
                Debug.LogError($"[SdkSmokeBatch] No config at {ConfigPath}. Cannot run.");
                EditorApplication.Exit(2);
                return;
            }

            var go = new GameObject("SdkSmokeBatch");
            var runner = go.AddComponent<SdkSmokeRunner>();
            runner.config = cfg;

            int code = 0;
            try
            {
                // Start() does not fire in edit mode, so drive the run explicitly.
                // Task.Run keeps continuations off the editor main thread.
                Task.Run(async () => await runner.RunSmokeAsync()).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SdkSmokeBatch] FATAL: {e}");
                code = 1;
            }

            EditorApplication.Exit(code);
        }
    }
}
