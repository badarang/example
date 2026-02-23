using UnityEngine;

namespace Moleio.Network
{
#if FUSION_WEAVER
    using Fusion;

    public struct MoleFusionInput : INetworkInput
    {
        public Vector2 Move;
        public NetworkBool Dash;
    }
#endif
}
