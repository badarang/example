using UnityEngine;

namespace Moleio.Network
{
    public sealed class MoleFusionBridge : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string note = "Legacy bridge. MoleFusionBootstrap + MoleFusionInputAdapter를 사용하세요.";
    }
}
