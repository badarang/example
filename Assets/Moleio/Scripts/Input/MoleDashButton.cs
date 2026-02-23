using UnityEngine;
using UnityEngine.EventSystems;

namespace Moleio.InputSystem
{
    public sealed class MoleDashButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool IsHeld { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsHeld = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsHeld = false;
        }
    }
}
