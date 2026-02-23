using Moleio.Core;
using UnityEngine;

namespace Moleio.InputSystem
{
    public sealed class MoleInputRouter : MonoBehaviour, IMoleInput
    {
        [SerializeField] private MoleVirtualJoystick joystick;
        [SerializeField] private MoleDashButton dashButton;
        [SerializeField] private bool enableKeyboardFallback = true;

        public Vector2 Move
        {
            get
            {
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

                return enableKeyboardFallback && Input.GetKey(KeyCode.Space);
            }
        }
    }
}
