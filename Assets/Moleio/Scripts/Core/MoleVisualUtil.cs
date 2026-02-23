using UnityEngine;

namespace Moleio.Core
{
    public static class MoleVisualUtil
    {
        private static Sprite cachedSprite;

        public static SpriteRenderer EnsureSpriteRenderer(GameObject target, Color color, int sortingOrder)
        {
            if (target == null)
            {
                return null;
            }

            SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = target.AddComponent<SpriteRenderer>();
            }

            if (renderer.sprite == null)
            {
                renderer.sprite = GetOrCreateDefaultSprite();
            }

            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static Sprite GetOrCreateDefaultSprite()
        {
            if (cachedSprite != null)
            {
                return cachedSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply(false, true);

            cachedSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);

            return cachedSprite;
        }
    }
}
