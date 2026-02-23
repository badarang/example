using Moleio.Network;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Moleio.EditorTools
{
    public static class MoleFusionAutoWireEditor
    {
        private const string PlayerPrefabPath = "Assets/Moleio/Prefabs/MolePlayer.prefab";
        private const string SegmentPrefabPath = "Assets/Moleio/Prefabs/MoleBodySegment.prefab";
        private const string FoodPrefabPath = "Assets/Moleio/Prefabs/MoleFood.prefab";
        private const string RunnerPrefabPath = "Assets/Moleio/Prefabs/NetworkRunner.prefab";
        private const string MoleioGameScenePath = "Assets/Moleio/Scenes/MoleioGame.unity";

        [MenuItem("Moleio/Networking/Auto Wire Fusion Prefabs")]
        public static void AutoWireFusionPrefabs()
        {
            GameObject playerPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            GameObject segmentPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(SegmentPrefabPath);
            GameObject foodPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FoodPrefabPath);
            GameObject runnerPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(RunnerPrefabPath);

            if (playerPrefabAsset == null || segmentPrefabAsset == null || foodPrefabAsset == null || runnerPrefabAsset == null)
            {
                Debug.LogError("[MoleFusionAutoWireEditor] Missing prefab asset(s).");
                return;
            }

            var trail = playerPrefabAsset.GetComponent<Moleio.Core.MoleBodyTrail>();
            if (trail == null)
            {
                Debug.LogError("[MoleFusionAutoWireEditor] MoleBodyTrail is missing on player prefab.");
                return;
            }

            var trailSo = new SerializedObject(trail);
            trailSo.FindProperty("segmentPrefab").objectReferenceValue = segmentPrefabAsset.transform;
            trailSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(playerPrefabAsset);

            var playerNetObj = playerPrefabAsset.GetComponent<Fusion.NetworkObject>();
            if (playerNetObj == null)
            {
                Debug.LogError("[MoleFusionAutoWireEditor] NetworkObject is missing on player prefab.");
                return;
            }
            var foodComponent = foodPrefabAsset.GetComponent<Moleio.Core.MoleFood>();
            if (foodComponent == null)
            {
                Debug.LogError("[MoleFusionAutoWireEditor] MoleFood is missing on food prefab.");
                return;
            }

            var runnerComponent = runnerPrefabAsset.GetComponent<Fusion.NetworkRunner>();
            if (runnerComponent == null)
            {
                Debug.LogError("[MoleFusionAutoWireEditor] NetworkRunner is missing on runner prefab.");
                return;
            }

            var bootstraps = Object.FindObjectsByType<MoleFusionBootstrap>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            for (int i = 0; i < bootstraps.Length; i++)
            {
                var so = new SerializedObject(bootstraps[i]);
                so.FindProperty("playerPrefab").objectReferenceValue = playerNetObj;
                so.FindProperty("runnerPrefab").objectReferenceValue = runnerComponent;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(bootstraps[i]);
            }

            var gameManagers = Object.FindObjectsByType<Moleio.Core.MoleGameManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            for (int i = 0; i < gameManagers.Length; i++)
            {
                var so = new SerializedObject(gameManagers[i]);
                so.FindProperty("foodPrefab").objectReferenceValue = foodComponent;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gameManagers[i]);
            }

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.Refresh();

            Debug.Log($"[MoleFusionAutoWireEditor] Auto-wire complete. Bootstraps: {bootstraps.Length}, GameManagers: {gameManagers.Length}");
        }

        [MenuItem("Moleio/Networking/Prepare 2P Rehearsal")]
        public static void PrepareTwoPlayerRehearsal()
        {
            bool changed = false;

            var buildScenes = EditorBuildSettings.scenes;
            bool hasMoleioGame = false;
            for (int i = 0; i < buildScenes.Length; i++)
            {
                if (buildScenes[i].path == MoleioGameScenePath)
                {
                    hasMoleioGame = true;
                    if (!buildScenes[i].enabled)
                    {
                        buildScenes[i].enabled = true;
                        changed = true;
                    }
                }
            }

            if (!hasMoleioGame)
            {
                var updated = new EditorBuildSettingsScene[buildScenes.Length + 1];
                for (int i = 0; i < buildScenes.Length; i++)
                {
                    updated[i] = buildScenes[i];
                }

                updated[buildScenes.Length] = new EditorBuildSettingsScene(MoleioGameScenePath, true);
                EditorBuildSettings.scenes = updated;
                changed = true;
            }
            else
            {
                EditorBuildSettings.scenes = buildScenes;
            }

            var gameManagers = Object.FindObjectsByType<Moleio.Core.MoleGameManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            for (int i = 0; i < gameManagers.Length; i++)
            {
                var so = new SerializedObject(gameManagers[i]);
                var prop = so.FindProperty("autoSpawnLocalPlayer");
                if (prop != null && prop.boolValue)
                {
                    prop.boolValue = false;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(gameManagers[i]);
                    changed = true;
                }
            }

            if (changed)
            {
                EditorSceneManager.MarkAllScenesDirty();
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Scene active = SceneManager.GetActiveScene();
            Debug.Log(
                $"[MoleFusionAutoWireEditor] Rehearsal prep complete. Scene='{active.path}', MoleioGameInBuild={true}, AutoSpawnDisabledOnManagers={gameManagers.Length}"
            );
        }

        [MenuItem("Moleio/Networking/Build Windows64 Client")]
        public static void BuildWindowsClient()
        {
            var enabledScenes = new System.Collections.Generic.List<string>();
            var scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled)
                {
                    enabledScenes.Add(scenes[i].path);
                }
            }

            if (enabledScenes.Count == 0)
            {
                Debug.LogError("[MoleFusionAutoWireEditor] Build aborted: no enabled scenes in Build Settings.");
                return;
            }

            string outputPath = "Builds/Windows/MoleioClient.exe";
            var options = new BuildPlayerOptions
            {
                scenes = enabledScenes.ToArray(),
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[MoleFusionAutoWireEditor] Build succeeded: {outputPath}");
                return;
            }

            Debug.LogError($"[MoleFusionAutoWireEditor] Build failed: {report.summary.result}");
        }
    }
}
