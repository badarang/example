using UnityEngine;

namespace Moleio.Core
{
    public sealed class MoleBodySegment : MonoBehaviour
    {
        public int OwnerId;
        public bool IsHead;

        private void Awake()
        {
            ApplyVisual();
        }

        public void ApplyVisual()
        {
            Color color = IsHead ? new Color(0.2f, 0.9f, 0.35f, 1f) : new Color(0.15f, 0.65f, 0.95f, 1f);
            int order = IsHead ? 20 : 10;
            MoleVisualUtil.EnsureSpriteRenderer(gameObject, color, order);
        }
    }
}
