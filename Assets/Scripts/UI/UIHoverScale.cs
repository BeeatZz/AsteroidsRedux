using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Asteroids.UI
{
    /* Eases this object up to a slightly larger size while the pointer is over it (or it's
     * selected with the keyboard or a gamepad), and back down when it leaves.
     * Runs on unscaled time, so it still works in the pause and game over menus.
     */
    public class UIHoverScale : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Tooltip("Scale while hovered, as a multiple of the starting size (1.08 = 8% bigger).")]
        [Min(1f)]
        [SerializeField] private float hoverScale = 1.08f;

        [Tooltip("How quickly the size catches up. Higher is snappier.")]
        [Min(0.1f)]
        [SerializeField] private float smoothing = 12f;

        private Selectable selectable;
        private Vector3 baseScale;
        private bool isPointerOver;
        private bool isSelected;

        private void Awake()
        {
            baseScale = transform.localScale;
            selectable = GetComponent<Selectable>();
        }

        private void OnDisable()
        {
            isPointerOver = false;
            isSelected = false;
            transform.localScale = baseScale;
        }

        public void OnPointerEnter(PointerEventData eventData) => isPointerOver = true;
        public void OnPointerExit(PointerEventData eventData) => isPointerOver = false;
        public void OnSelect(BaseEventData eventData) => isSelected = true;
        public void OnDeselect(BaseEventData eventData) => isSelected = false;

        private void Update()
        {
            bool interactable = selectable == null || selectable.IsInteractable();
            float target = interactable && (isPointerOver || isSelected) ? hoverScale : 1f;

            Vector3 targetScale = baseScale * target;
            float blend = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, blend);
        }
    }
}
