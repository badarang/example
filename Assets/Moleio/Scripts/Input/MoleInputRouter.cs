using Moleio.Core;
using UnityEngine;

namespace Moleio.InputSystem
{
    public sealed class MoleInputRouter : MonoBehaviour, IMoleInput
    {
        public static Vector2 LocalMove { get; private set; }
        public static bool LocalDashHeld { get; private set; }

        [SerializeField] private MoleVirtualJoystick joystick;
        [SerializeField] private MoleDashButton dashButton;
        [SerializeField] private bool enableMouseLeftMove = true;
        [SerializeField] private float mouseMoveDeadZone = 0.15f;
        [SerializeField] private bool enableKeyboardFallback = true;

        private MolePlayerController cachedPlayer;
        private float nextPlayerSearchTime;

        public Vector2 Move
        {
            get
            {
                Vector2 mouseMove = GetMouseMove();
                if (mouseMove.sqrMagnitude > 0.0001f)
                {
                    return mouseMove;
                }

                Vector2 value = joystick != null ? joystick.Direction : Vector2.zero;
                if (value.sqrMagnitude > 0.0001f)
                {
                    return value;
                }

                if (!enableKeyboardFallback)
                {
                    return Vector2.zero;
                }

                return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }
        }

        public bool DashHeld
        {
            get
            {
                if (dashButton != null && dashButton.IsHeld)
                {
                    return true;
                }

                return enableKeyboardFallback && (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(1));
            }
        }

        private void LateUpdate()
        {
            LocalMove = Move;
            LocalDashHeld = DashHeld;
        }

        private Vector2 GetMouseMove()
        {
            if (!enableMouseLeftMove || !Input.GetMouseButton(0))
            {
                return Vector2.zero;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                return Vector2.zero;
            }

            if (cachedPlayer == null && Time.time >= nextPlayerSearchTime)
            {
                cachedPlayer = FindAnyObjectByType<MolePlayerController>();
                nextPlayerSearchTime = Time.time + 0.5f;
            }

            if (cachedPlayer == null)
            {
                return Vector2.zero;
            }

            Vector3 playerPos = cachedPlayer.transform.position;
            Vector3 mouse = Input.mousePosition;
            mouse.z = Mathf.Abs(cam.transform.position.z - playerPos.z);
            Vector3 world = cam.ScreenToWorldPoint(mouse);
            Vector2 direction = new Vector2(world.x - playerPos.x, world.y - playerPos.y);

            if (direction.magnitude < mouseMoveDeadZone)
            {
                return Vector2.zero;
            }

            return direction.normalized;
        }
    }
}
