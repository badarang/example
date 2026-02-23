using Moleio.Network;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Moleio.InputSystem;
using System.IO;

namespace Moleio.EditorTools
{
    public static class MoleFusionAutoWireEditor
    {
        private const string PlayerPrefabPath = "Assets/Moleio/Prefabs/MolePlayer.prefab";
        private const string SegmentPrefabPath = "Assets/Moleio/Prefabs/MoleBodySegment.prefab";
        private const string FoodPrefabPath = "Assets/Moleio/Prefabs/MoleFood.prefab";
        private const string RunnerPrefabPath = "Assets/Moleio/Prefabs/NetworkRunner.prefab";
        private const string MoleioGameScenePath = "Assets/Moleio/Scenes/MoleioGame.unity";
        private const string TouchSpriteAssetPath = "Assets/Moleio/Generated/TouchUI/touchpad.png";

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

        [MenuItem("Moleio/Networking/Setup Touch UI")]
        public static void SetupTouchUi()
        {
            Transform uiRoot = EnsureGameObject("UIRoot", null).transform;
            Canvas canvas = EnsureCanvas(uiRoot);
            EnsureEventSystem();

            GameObject joystickGo = EnsureGameObject("VirtualJoystick", canvas.transform);
            var joystickRect = joystickGo.GetComponent<RectTransform>();
            ConfigureRect(joystickRect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(160f, 160f), new Vector2(130f, 130f));
            var joystickBg = EnsureImage(joystickGo, new Color(0f, 0f, 0f, 0.35f));

            GameObject handleGo = EnsureGameObject("Handle", joystickGo.transform);
            var handleRect = handleGo.GetComponent<RectTransform>();
            ConfigureRect(handleRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(72f, 72f), Vector2.zero);
            var handleImage = EnsureImage(handleGo, new Color(1f, 1f, 1f, 0.9f));

            var joystick = joystickGo.GetComponent<MoleVirtualJoystick>();
            if (joystick == null)
            {
                joystick = joystickGo.AddComponent<MoleVirtualJoystick>();
            }

            SetPrivateField(joystick, "handle", handleRect);
            SetPrivateField(joystick, "radius", 70f);

            GameObject dashGo = EnsureGameObject("DashButton", canvas.transform);
            var dashRect = dashGo.GetComponent<RectTransform>();
            ConfigureRect(dashRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(140f, 140f), new Vector2(-130f, 130f));
            var dashImage = EnsureImage(dashGo, new Color(0.95f, 0.25f, 0.2f, 0.8f));
            if (dashGo.GetComponent<MoleDashButton>() == null)
            {
                dashGo.AddComponent<MoleDashButton>();
            }

            GameObject routerGo = EnsureGameObject("MoleInputRouter", uiRoot);
            var router = routerGo.GetComponent<MoleInputRouter>();
            if (router == null)
            {
                router = routerGo.AddComponent<MoleInputRouter>();
            }

            var routerSo = new SerializedObject(router);
            routerSo.FindProperty("joystick").objectReferenceValue = joystick;
            routerSo.FindProperty("dashButton").objectReferenceValue = dashGo.GetComponent<MoleDashButton>();
            routerSo.FindProperty("enableKeyboardFallback").boolValue = true;
            routerSo.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(joystickGo);
            EditorUtility.SetDirty(handleGo);
            EditorUtility.SetDirty(dashGo);
            EditorUtility.SetDirty(routerGo);
            EditorUtility.SetDirty(canvas.gameObject);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.Refresh();

            _ = joystickBg;
            _ = handleImage;
            _ = dashImage;

            Debug.Log("[MoleFusionAutoWireEditor] Touch UI setup complete.");
        }

        private static Canvas EnsureCanvas(Transform uiRoot)
        {
            Canvas existing = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (existing != null)
            {
                return existing;
            }

            GameObject go = EnsureGameObject("GameCanvas", uiRoot);
            var canvas = go.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = go.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (go.GetComponent<CanvasScaler>() == null)
            {
                var scaler = go.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (go.GetComponent<GraphicRaycaster>() == null)
            {
                go.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        private static GameObject EnsureGameObject(string name, Transform parent)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                if (parent != null && found.transform.parent != parent)
                {
                    found.transform.SetParent(parent, false);
                }

                return found;
            }

            if (parent != null)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                return go;
            }

            return new GameObject(name);
        }

        private static Image EnsureImage(GameObject target, Color color)
        {
            var image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.AddComponent<Image>();
            }

            image.sprite = EnsureTouchUiSprite();
            image.type = Image.Type.Simple;
            image.color = color;
            image.raycastTarget = true;
            return image;
        }

        private static Sprite EnsureTouchUiSprite()
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TouchSpriteAssetPath);
            if (sprite != null)
            {
                return sprite;
            }

            string absolutePath = Path.GetFullPath(TouchSpriteAssetPath);
            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color baseColor = new Color(1f, 1f, 1f, 1f);
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    float nx = (x / 63f) * 2f - 1f;
                    float ny = (y / 63f) * 2f - 1f;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);
                    float alpha = r <= 1f ? 1f : 0f;
                    tex.SetPixel(x, y, new Color(baseColor.r, baseColor.g, baseColor.b, alpha));
                }
            }

            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(absolutePath, png);

            AssetDatabase.ImportAsset(TouchSpriteAssetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(TouchSpriteAssetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(TouchSpriteAssetPath);
        }

        private static void ConfigureRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 size, Vector2 anchoredPos)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;
        }

        private static void SetPrivateField<T>(Object target, string fieldName, T value)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                return;
            }

            if (typeof(Object).IsAssignableFrom(typeof(T)))
            {
                prop.objectReferenceValue = value as Object;
            }
            else if (typeof(T) == typeof(float))
            {
                prop.floatValue = (float)(object)value;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
