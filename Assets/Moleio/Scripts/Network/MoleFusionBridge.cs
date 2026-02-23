using UnityEngine;

namespace Moleio.Network
{
#if MOLEIO_FUSION
    using Fusion;

    [RequireComponent(typeof(NetworkObject))]
    public sealed class MoleFusionBridge : NetworkBehaviour
    {
        [Networked] private Vector2 MoveInput { get; set; }
        [Networked] private NetworkBool DashInput { get; set; }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out Vector2 data))
            {
                MoveInput = data;
            }
        }
    }
#else
    public sealed class MoleFusionBridge : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string note = "Photon Fusion installed 후 Scripting Define Symbols에 MOLEIO_FUSION 추가 시 활성화됩니다.";
    }
#endif
}
