using UnityEngine;

namespace Moleio.Core
{
    public sealed class MoleFood : MonoBehaviour
    {
        [SerializeField] private int growthAmount = 1;
        public int GrowthAmount => Mathf.Max(1, growthAmount);
    }
}
