using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Asteroids.UI
{
    /* Makes a pressed button feel physically pushed in: its label and icons drop down by the depth
     * of the button art, and the whole button darkens slightly. Works alongside the Button's own
     * Sprite Swap transition, which still handles the pressed sprite.
     * Like Button's own pressed state, it stays pressed for as long as the pointer is held down,
     * even if it's dragged off the button, so the label never comes apart from the sprite.
     */
    [RequireComponent(typeof(Selectable))]
    public class ButtonPressEffect : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, ISubmitHandler
    {
        [Tooltip("How far the content drops, as a fraction of the button's height. The Kenney depth " +
                 "buttons have an 8px lip on a 128px sprite, which is 0.0625.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float pressDepth = 0.0625f;

        [Tooltip("Brightness while pressed (1 = unchanged). Alpha is kept.")]
        [Range(0f, 1f)]
        [SerializeField] private float pressedBrightness = 0.85f;

        [Tooltip("Seconds the pressed look is held when the button is activated from the keyboard or a gamepad.")]
        [Min(0f)]
        [SerializeField] private float submitPressDuration = 0.1f;

        private Selectable selectable;
        private RectTransform rectTransform;
        private bool isPointerDown;
        private bool isPressedLook;
        private Coroutine submitRoutine;

        // Captured when the press starts rather than in Awake, so layout or colour changes made
        // by other scripts while the button is up are respected.
        private readonly Dictionary<RectTransform, Vector2> originalPositions = new();
        private readonly Dictionary<Graphic, Color> originalColors = new();

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            rectTransform = (RectTransform)transform;
        }

        // A button that hides its own panel (e.g. Settings) is disabled mid-press and never gets
        // its pointer-up, so it has to put itself back here.
        private void OnDisable()
        {
            isPointerDown = false;
            submitRoutine = null;
            SetPressedLook(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            isPointerDown = true;
            Refresh();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            isPointerDown = false;
            Refresh();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!selectable.IsInteractable()) return;

            if (submitRoutine != null) StopCoroutine(submitRoutine);
            submitRoutine = StartCoroutine(SubmitPress());
        }

        private IEnumerator SubmitPress()
        {
            SetPressedLook(true);
            yield return new WaitForSecondsRealtime(submitPressDuration);
            submitRoutine = null;
            Refresh();
        }

        private void Refresh()
        {
            if (submitRoutine != null) return;
            SetPressedLook(isPointerDown && selectable.IsInteractable());
        }

        private void SetPressedLook(bool pressed)
        {
            if (pressed == isPressedLook) return;
            isPressedLook = pressed;

            if (pressed)
            {
                float drop = rectTransform.rect.height * pressDepth;

                // Direct children only: they carry any nested elements with them.
                foreach (RectTransform child in transform)
                {
                    originalPositions[child] = child.anchoredPosition;
                    child.anchoredPosition += Vector2.down * drop;
                }

                foreach (var graphic in GetComponentsInChildren<Graphic>())
                {
                    Color color = graphic.color;
                    originalColors[graphic] = color;
                    color.r *= pressedBrightness;
                    color.g *= pressedBrightness;
                    color.b *= pressedBrightness;
                    graphic.color = color;
                }
            }
            else
            {
                foreach (var pair in originalPositions)
                {
                    if (pair.Key != null) pair.Key.anchoredPosition = pair.Value;
                }

                foreach (var pair in originalColors)
                {
                    if (pair.Key != null) pair.Key.color = pair.Value;
                }

                originalPositions.Clear();
                originalColors.Clear();
            }
        }
    }
}
