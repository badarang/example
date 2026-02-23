using System.Collections;
using UnityEngine;

namespace Moleio.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(MoleBodySegment))]
    [RequireComponent(typeof(MoleBodyTrail))]
    public sealed class MolePlayerController : MonoBehaviour
    {
        private static int nextPlayerId = 1;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float rotationLerp = 12f;

        [Header("Dash")]
        [SerializeField] private float dashMultiplier = 1.75f;
        [SerializeField] private float maxDash = 100f;
        [SerializeField] private float dashDrainPerSecond = 30f;
        [SerializeField] private float dashRegenPerSecond = 20f;
        [SerializeField] private float minDashToActivate = 8f;

        [Header("Respawn")]
        [SerializeField] private float respawnDelaySeconds = 3f;

        private Rigidbody2D rb;
        private MoleBodyTrail bodyTrail;
        private MoleBodySegment headSegment;
        private Collider2D headCollider;
        private IMoleInput input;
        private float currentDash;
        private bool isDead;
        private int playerId;

        public int PlayerId => playerId;
        public bool IsDead => isDead;
        public float DashNormalized => maxDash <= Mathf.Epsilon ? 0f : currentDash / maxDash;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            bodyTrail = GetComponent<MoleBodyTrail>();
            headSegment = GetComponent<MoleBodySegment>();
            headCollider = GetComponent<Collider2D>();
            input = GetComponent<IMoleInput>();

            playerId = nextPlayerId++;
            headSegment.OwnerId = playerId;
            headSegment.IsHead = true;
            bodyTrail.SetOwner(playerId);

            currentDash = maxDash;
        }

        private void FixedUpdate()
        {
            if (isDead)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            Vector2 move = input != null ? input.Move : Vector2.zero;
            if (move.sqrMagnitude < 0.0001f)
            {
                rb.velocity = Vector2.zero;
                RegenerateDash();
                return;
            }

            move.Normalize();
            bool wantsDash = input != null && input.DashHeld;
            bool isDashing = wantsDash && currentDash >= minDashToActivate;
            float speed = moveSpeed * (isDashing ? dashMultiplier : 1f);
            rb.velocity = move * speed;

            float angle = Mathf.Atan2(move.y, move.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerp * Time.fixedDeltaTime);

            if (isDashing)
            {
                currentDash = Mathf.Max(0f, currentDash - dashDrainPerSecond * Time.fixedDeltaTime);
            }
            else
            {
                RegenerateDash();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isDead)
            {
                return;
            }

            MoleFood food = other.GetComponent<MoleFood>();
            if (food != null)
            {
                bodyTrail.Grow(food.GrowthAmount);
                MoleGameManager.Instance?.OnFoodConsumed(food);
                return;
            }

            MoleBodySegment otherSegment = other.GetComponent<MoleBodySegment>();
            if (otherSegment == null || otherSegment.OwnerId == playerId || otherSegment.IsHead)
            {
                return;
            }

            KillPlayer();
        }

        public void KillPlayer()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            rb.velocity = Vector2.zero;
            headCollider.enabled = false;
            MoleGameManager.Instance?.OnPlayerDied(this);
            StartCoroutine(RespawnRoutine());
        }

        public Vector3[] ConsumeBodySegmentPositions()
        {
            return bodyTrail.ConsumeSegmentPositions();
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelaySeconds);
            Vector3 point = MoleGameManager.Instance != null
                ? MoleGameManager.Instance.GetRandomSpawnPoint()
                : Vector3.zero;

            transform.position = point;
            transform.rotation = Quaternion.identity;
            isDead = false;
            currentDash = maxDash;
            bodyTrail.RebuildInitialSegments();
            bodyTrail.SetOwner(playerId);
            headCollider.enabled = true;
            enabled = true;
        }

        private void RegenerateDash()
        {
            currentDash = Mathf.Min(maxDash, currentDash + dashRegenPerSecond * Time.fixedDeltaTime);
        }
    }
}
