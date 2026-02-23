using UnityEngine;
using UnityEngine.EventSystems;

namespace Moleio.InputSystem
{
    public sealed class MoleVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform handle;
        [SerializeField] private float radius = 90f;

        private RectTransform rect;
        private Vector2 direction;

        public Vector2 Direction => direction;

        private void Awake()
        {
            rect = transform as RectTransform;
            ResetHandle();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (rect == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                return;
            }

            Vector2 clamped = Vector2.ClampMagnitude(localPoint, radius);
            direction = clamped / Mathf.Max(radius, 1f);

            if (handle != null)
            {
                handle.anchoredPosition = clamped;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            direction = Vector2.zero;
            ResetHandle();
        }

        private void ResetHandle()
        {
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }
    }
}
