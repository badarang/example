using System;
using System.Collections.Generic;
using Moleio.Core;
using Moleio.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Moleio.Network
{
#if FUSION_WEAVER
    using Fusion;
    using Fusion.Sockets;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public sealed class MoleFusionBootstrap : MonoBehaviour, INetworkRunnerCallbacks
    {
        [Header("Session")]
        [SerializeField] private string sessionName = "moleio-room";
        [SerializeField] private int maxPlayers = 10;
        [SerializeField] private GameMode gameMode = GameMode.AutoHostOrClient;

        [Header("Prefabs")]
        [SerializeField] private NetworkRunner runnerPrefab;
        [SerializeField] private NetworkObject playerPrefab;

        private NetworkRunner runner;

        private void OnValidate()
        {
            AutoWireReferences();
            ValidatePlayerPrefab(out _);
        }

        [ContextMenu("MoleFusion/Auto Wire References")]
        private void AutoWireReferencesMenu()
        {
            AutoWireReferences();
        }

        [ContextMenu("MoleFusion/Validate Player Prefab")]
        private void ValidatePlayerPrefabMenu()
        {
            if (ValidatePlayerPrefab(out string message))
            {
                Debug.Log($"[MoleFusionBootstrap] {message}", this);
                return;
            }

            Debug.LogError($"[MoleFusionBootstrap] {message}", this);
        }

        private async void Start()
        {
            if (runner != null)
            {
                return;
            }

            AutoWireReferences();

            if (!ValidatePlayerPrefab(out string prefabValidation))
            {
                Debug.LogError($"[MoleFusionBootstrap] {prefabValidation}", this);
                return;
            }

            runner = runnerPrefab != null
                ? Instantiate(runnerPrefab)
                : new GameObject("NetworkRunner").AddComponent<NetworkRunner>();

            runner.name = "NetworkRunner";
            runner.ProvideInput = true;
            runner.AddCallbacks(this);

            NetworkSceneManagerDefault sceneManager = runner.GetComponent<NetworkSceneManagerDefault>();
            if (sceneManager == null)
            {
                sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            }

            var args = new StartGameArgs
            {
                GameMode = gameMode,
                SessionName = sessionName,
                PlayerCount = maxPlayers,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = sceneManager
            };

            var result = await runner.StartGame(args);
            if (!result.Ok)
            {
                Debug.LogError($"Fusion StartGame failed: {result.ShutdownReason}");
                return;
            }

            Debug.Log($"[MoleFusionBootstrap] StartGame ok. mode={gameMode}, session={sessionName}", this);
        }

        public void OnPlayerJoined(NetworkRunner networkRunner, PlayerRef player)
        {
            Debug.Log($"[MoleFusionBootstrap] OnPlayerJoined player={player.PlayerId} isServer={networkRunner.IsServer}", this);

            if (!networkRunner.IsServer || playerPrefab == null)
            {
                return;
            }

            Vector3 spawn = MoleGameManager.Instance != null
                ? MoleGameManager.Instance.GetRandomSpawnPoint()
                : Vector3.zero;

            if (!ValidatePlayerPrefab(out string prefabValidation))
            {
                Debug.LogError($"[MoleFusionBootstrap] Spawn blocked: {prefabValidation}", this);
                return;
            }

            networkRunner.Spawn(playerPrefab, spawn, Quaternion.identity, player);
            Debug.Log($"[MoleFusionBootstrap] Spawned player object for player={player.PlayerId}", this);
        }

        public void OnInput(NetworkRunner networkRunner, NetworkInput networkInput)
        {
            networkInput.Set(new MoleFusionInput
            {
                Move = MoleInputRouter.LocalMove,
                Dash = MoleInputRouter.LocalDashHeld
            });
        }

        public void OnPlayerLeft(NetworkRunner networkRunner, PlayerRef player) { }
        public void OnInputMissing(NetworkRunner networkRunner, PlayerRef player, NetworkInput networkInput) { }
        public void OnShutdown(NetworkRunner networkRunner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner networkRunner) { }
        public void OnDisconnectedFromServer(NetworkRunner networkRunner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner networkRunner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner networkRunner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner networkRunner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner networkRunner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner networkRunner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner networkRunner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner networkRunner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner networkRunner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner networkRunner) { }
        public void OnSceneLoadStart(NetworkRunner networkRunner) { }
        public void OnObjectEnterAOI(NetworkRunner networkRunner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner networkRunner, NetworkObject obj, PlayerRef player) { }

        private void AutoWireReferences()
        {
#if UNITY_EDITOR
            bool changed = false;

            if (runnerPrefab == null && TryFindRunnerPrefab(out NetworkRunner foundRunner))
            {
                runnerPrefab = foundRunner;
                changed = true;
            }

            if (playerPrefab == null && TryFindPlayerPrefab(out NetworkObject foundPlayer))
            {
                playerPrefab = foundPlayer;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(this);
            }
#endif
        }

        private bool ValidatePlayerPrefab(out string message)
        {
            if (playerPrefab == null)
            {
                message = "playerPrefab is not assigned.";
                return false;
            }

            GameObject prefabRoot = playerPrefab.gameObject;
            var missing = new List<string>();

            if (prefabRoot.GetComponent<NetworkObject>() == null)
            {
                missing.Add(nameof(NetworkObject));
            }

            if (prefabRoot.GetComponent<MoleFusionInputAdapter>() == null)
            {
                missing.Add(nameof(MoleFusionInputAdapter));
            }

            if (prefabRoot.GetComponent<MolePlayerController>() == null)
            {
                missing.Add(nameof(MolePlayerController));
            }

            if (prefabRoot.GetComponent<Rigidbody2D>() == null)
            {
                missing.Add(nameof(Rigidbody2D));
            }

            if (prefabRoot.GetComponent<Collider2D>() == null)
            {
                missing.Add(nameof(Collider2D));
            }

            if (prefabRoot.GetComponent<MoleBodySegment>() == null)
            {
                missing.Add(nameof(MoleBodySegment));
            }

            if (prefabRoot.GetComponent<MoleBodyTrail>() == null)
            {
                missing.Add(nameof(MoleBodyTrail));
            }

            if (missing.Count > 0)
            {
                message = $"playerPrefab '{prefabRoot.name}' is missing: {string.Join(", ", missing)}";
                return false;
            }

            message = $"playerPrefab '{prefabRoot.name}' validation passed.";
            return true;
        }

#if UNITY_EDITOR
        private static bool TryFindRunnerPrefab(out NetworkRunner found)
        {
            found = null;
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                NetworkRunner candidate = prefab.GetComponent<NetworkRunner>();
                if (candidate != null)
                {
                    found = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindPlayerPrefab(out NetworkObject found)
        {
            found = null;
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Moleio", "Assets" });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                NetworkObject networkObject = prefab.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    continue;
                }

                if (prefab.GetComponent<MoleFusionInputAdapter>() == null)
                {
                    continue;
                }

                found = networkObject;
                return true;
            }

            return false;
        }
#endif
    }
#else
    public sealed class MoleFusionBootstrap : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string note = "Photon Fusion 미설치 상태입니다. 설치 후 자동으로 활성화됩니다.";
    }
#endif
}
