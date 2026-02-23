using Moleio.Core;
using UnityEngine;

namespace Moleio.Network
{
#if FUSION_WEAVER
    using Fusion;

    [RequireComponent(typeof(NetworkObject))]
    public sealed class MoleFusionInputAdapter : NetworkBehaviour, IMoleInput
    {
        [Networked] private Vector2 NetMove { get; set; }
        [Networked] private NetworkBool NetDash { get; set; }

        public Vector2 Move => NetMove;
        public bool DashHeld => NetDash;

        public override void FixedUpdateNetwork()
        {
            if (GetInput<MoleFusionInput>(out var input))
            {
                NetMove = input.Move;
                NetDash = input.Dash;
                return;
            }

            if (Object.HasStateAuthority)
            {
                NetMove = Vector2.zero;
                NetDash = default;
            }
        }
    }
#else
    public sealed class MoleFusionInputAdapter : MonoBehaviour, IMoleInput
    {
        public Vector2 Move => Vector2.zero;
        public bool DashHeld => false;
    }
#endif
}
