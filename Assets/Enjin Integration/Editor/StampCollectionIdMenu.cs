using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HappyHarvest.EnjinIntegration.Data;
using UnityEditor;
using UnityEngine;

namespace HappyHarvest.EnjinIntegration.EditorTools
{
    /// <summary>
    /// One-time setup helper. Calls the sample game server's
    /// <c>GET /api/setup/collection-id</c> endpoint and stamps the returned
    /// on-chain collection id onto the <c>collectionId</c> field of every
    /// <see cref="EnjinItem"/> ScriptableObject in the project.
    ///
    /// In a real game you'd run this once after bringing up a fresh server
    /// (or after a chain reset that forced re-allocation of the collection).
    /// It's an editor-time operation, not a runtime fetch: the resulting
    /// values get serialised into the assets and ship with the build.
    ///
    /// Menu path: <b>Enjin &gt; Stamp Collection ID onto EnjinItem Assets</b>.
    /// </summary>
    internal static class StampCollectionIdMenu
    {
        // Where to store the most-recently-used server host so we don't have
        // to ask every run. The key is intentionally per-machine
        // (EditorPrefs); it isn't worth persisting in the project.
        private const string HostPrefKey = "Enjin.Setup.ServerHost";
        private const string DefaultHost = "http://localhost:3000";

        [MenuItem("Enjin/Stamp Collection ID onto EnjinItem Assets", priority = 50)]
        public static void Run()
        {
            var host = EditorPrefs.GetString(HostPrefKey, DefaultHost);
            host = EditorInputDialog.Show(
                title: "Stamp Collection ID",
                message: "Game server base URL (e.g. http://localhost:3000). The Editor will call " +
                         "<host>/api/setup/collection-id and write the value onto every EnjinItem asset.",
                defaultValue: host);
            if (string.IsNullOrWhiteSpace(host))
            {
                Debug.Log("[StampCollectionId] Cancelled.");
                return;
            }
            host = host.TrimEnd('/');
            EditorPrefs.SetString(HostPrefKey, host);

            string collectionId;
            try
            {
                // Run on a background thread to avoid sync-over-async
                // deadlocking the Unity main thread / editor UI on macOS.
                collectionId = Task.Run(() => FetchCollectionIdAsync(host))
                    .GetAwaiter().GetResult();
            }
            catch (AggregateException agg) when (agg.InnerException != null)
            {
                EditorUtility.DisplayDialog(
                    "Stamp Collection ID",
                    $"Failed to fetch collection id from {host}/api/setup/collection-id:\n\n{agg.InnerException.Message}\n\n" +
                    "Make sure the server is running and has finished bootstrap.",
                    "OK");
                return;
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(
                    "Stamp Collection ID",
                    $"Failed to fetch collection id from {host}/api/setup/collection-id:\n\n{ex.Message}\n\n" +
                    "Make sure the server is running and has finished bootstrap.",
                    "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(collectionId))
            {
                EditorUtility.DisplayDialog(
                    "Stamp Collection ID",
                    "Server returned an empty collection id.",
                    "OK");
                return;
            }

            var stamped = StampAllEnjinItems(collectionId);

            var summary = stamped.Count == 0
                ? "No EnjinItem assets found in the project. Nothing to stamp."
                : $"Stamped collection id {collectionId} onto {stamped.Count} EnjinItem asset(s):\n\n  " +
                  string.Join("\n  ", stamped);
            Debug.Log("[StampCollectionId] " + summary.Replace("\n\n", " "));
            EditorUtility.DisplayDialog("Stamp Collection ID", summary, "OK");
        }

        private static async Task<string> FetchCollectionIdAsync(string host)
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var url = host + "/api/setup/collection-id";
            using var resp = await http.GetAsync(url).ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP {(int)resp.StatusCode}: {body}");
            }
            // Server returns {"collectionId":"..."} (System.Text.Json camelCase
            // by default). JsonUtility handles this shape fine and is safe to
            // call off the main thread for plain [Serializable] POD types.
            var parsed = JsonUtility.FromJson<CollectionIdResponse>(body);
            return parsed?.collectionId;
        }

        /// <summary>
        /// Finds every EnjinItem asset in the project and writes the supplied
        /// id into its <c>collectionId.value</c> private SerializeField via
        /// SerializedObject (so Undo + dirty tracking behave correctly).
        /// </summary>
        private static List<string> StampAllEnjinItems(string collectionId)
        {
            var stamped = new List<string>();
            var guids = AssetDatabase.FindAssets("t:" + nameof(EnjinItem));
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<EnjinItem>(path);
                if (asset == null) continue;

                var so = new SerializedObject(asset);
                var prop = so.FindProperty("collectionId.value");
                if (prop == null)
                {
                    Debug.LogWarning($"[StampCollectionId] {path}: no collectionId.value property found; skipping.");
                    continue;
                }
                if (prop.stringValue == collectionId)
                {
                    // Already up to date; no-op.
                    stamped.Add(Path.GetFileNameWithoutExtension(path) + " (unchanged)");
                    continue;
                }
                Undo.RecordObject(asset, "Stamp Collection ID");
                prop.stringValue = collectionId;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(asset);
                stamped.Add(Path.GetFileNameWithoutExtension(path));
            }
            AssetDatabase.SaveAssets();
            return stamped;
        }

        // JsonUtility target for the /api/setup/collection-id response.
        // Kept private + nested because nothing else needs it.
        [Serializable]
        private class CollectionIdResponse
        {
            public string collectionId;
        }
    }

    /// <summary>
    /// Minimal modal text-input dialog. EditorUtility has no built-in
    /// equivalent; this is a tiny wrapper around EditorWindow.
    /// </summary>
    internal class EditorInputDialog : EditorWindow
    {
        private string _value = string.Empty;
        private string _message = string.Empty;
        private bool _confirmed;
        private bool _initialFocus;

        public static string Show(string title, string message, string defaultValue)
        {
            var win = CreateInstance<EditorInputDialog>();
            win.titleContent = new GUIContent(title);
            win._message = message;
            win._value = defaultValue ?? string.Empty;
            win.minSize = new Vector2(420, 140);
            win.maxSize = new Vector2(720, 200);
            win.ShowModalUtility();
            return win._confirmed ? win._value : null;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(_message, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(6);

            GUI.SetNextControlName("EnjinInputField");
            _value = EditorGUILayout.TextField(_value);
            if (!_initialFocus)
            {
                EditorGUI.FocusTextInControl("EnjinInputField");
                _initialFocus = true;
            }

            EditorGUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Cancel", GUILayout.Width(80)))
                {
                    _confirmed = false;
                    Close();
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("OK", GUILayout.Width(80)) ||
                    (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
                {
                    _confirmed = true;
                    Close();
                }
            }
        }
    }
}
