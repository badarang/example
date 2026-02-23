using UnityEngine;

namespace Moleio.Core
{
    public sealed class MoleFood : MonoBehaviour
    {
        [SerializeField] private int growthAmount = 1;
        public int GrowthAmount => Mathf.Max(1, growthAmount);

        private void Awake()
        {
            MoleVisualUtil.EnsureSpriteRenderer(gameObject, new Color(1f, 0.75f, 0.2f, 1f), 5);
            if (transform.localScale == Vector3.one)
            {
                transform.localScale = Vector3.one * 0.35f;
            }
        }
    }
}
