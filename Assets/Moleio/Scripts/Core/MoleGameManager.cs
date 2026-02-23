using System.Collections.Generic;
using UnityEngine;

namespace Moleio.Core
{
    public sealed class MoleGameManager : MonoBehaviour
    {
        public static MoleGameManager Instance { get; private set; }

        [Header("World")]
        [SerializeField] private Vector2 worldSize = new(26f, 26f);
        [SerializeField] private Transform[] spawnPoints;

        [Header("Prefabs")]
        [SerializeField] private MolePlayerController playerPrefab;
        [SerializeField] private MoleFood foodPrefab;
        [SerializeField] private bool autoSpawnLocalPlayer = true;

        [Header("Food")]
        [SerializeField] private int initialFoodCount = 120;
        [SerializeField] private int maxFoodCount = 160;
        [SerializeField] private int deathDropStep = 2;

        private readonly List<MoleFood> foods = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (autoSpawnLocalPlayer && playerPrefab != null && FindAnyObjectByType<MolePlayerController>() == null)
            {
                Instantiate(playerPrefab, GetRandomSpawnPoint(), Quaternion.identity);
            }

            BootstrapFood();
        }

        public void OnFoodConsumed(MoleFood food)
        {
            foods.Remove(food);
            if (food != null)
            {
                Destroy(food.gameObject);
            }

            if (foods.Count < maxFoodCount)
            {
                SpawnFoodAtRandom();
            }
        }

        public void OnPlayerDied(MolePlayerController player)
        {
            if (player == null)
            {
                return;
            }

            Vector3[] dropPositions = player.ConsumeBodySegmentPositions();
            int step = Mathf.Max(1, deathDropStep);
            for (int i = 0; i < dropPositions.Length; i += step)
            {
                SpawnFood(dropPositions[i]);
            }
        }

        public Vector3 GetRandomSpawnPoint()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                int index = Random.Range(0, spawnPoints.Length);
                if (spawnPoints[index] != null)
                {
                    return spawnPoints[index].position;
                }
            }

            return new Vector3(
                Random.Range(-worldSize.x * 0.5f, worldSize.x * 0.5f),
                Random.Range(-worldSize.y * 0.5f, worldSize.y * 0.5f),
                0f);
        }

        private void BootstrapFood()
        {
            foods.RemoveAll(item => item == null);
            int target = Mathf.Clamp(initialFoodCount, 0, maxFoodCount);
            for (int i = foods.Count; i < target; i++)
            {
                SpawnFoodAtRandom();
            }
        }

        private void SpawnFoodAtRandom()
        {
            SpawnFood(GetRandomSpawnPoint());
        }

        private void SpawnFood(Vector3 position)
        {
            if (foodPrefab == null)
            {
                return;
            }

            MoleFood instance = Instantiate(foodPrefab, position, Quaternion.identity);
            foods.Add(instance);
        }
    }
}
