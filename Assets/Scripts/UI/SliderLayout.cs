using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.UI
{
    // Sizes a sprite-based slider from one Size value and lays out every child to match, so the children never need touching.
    // Expected structure (what GameObject > UI > Styled Slider builds):
    //   Slider
    //   ├─ Background                 sliced track sprite (may have an outline the fill sits inside)
    //   ├─ Fill Area
    //   │  └─ Fill                    Slider.fillRect, RectMask2D only (clips instead of squashing the fill's end caps)
    //   │     └─ Fill Image           sliced fill sprite, always full length
    //   └─ Handle Slide Area
    //      └─ Handle                  Slider.handleRect, full slider height, width from the sprite's aspect
    // Assumes a horizontal (Left To Right) slider.
    [ExecuteAlways]
    [RequireComponent(typeof(Slider))]
    public class SliderLayout : MonoBehaviour
    {
        [Tooltip("Width/Height of the slider at Size 1. The handle is the full height.")]
        [SerializeField] private Vector2 baseSize = new Vector2(160f, 20f);

        [Tooltip("Makes the whole slider bigger or smaller. Drives the RectTransform's Width/Height (baseSize * size).")]
        [Min(0.1f)]
        [SerializeField] private float size = 1f;

        [Tooltip("Track (Background) height as a fraction of the slider's height.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float barThickness = 0.72f;

        [Tooltip("How far the fill sits inside the track, in the Background sprite's pixels (its outline thickness). 0 = fill covers the whole track.")]
        [Min(0f)]
        [SerializeField] private float fillInset = 12f;

        [Header("Parts (auto-filled)")]
        [SerializeField] private Image background;
        [SerializeField] private RectTransform fillArea;
        [SerializeField] private Image fillImage;
        [SerializeField] private RectTransform handleSlideArea;
        [SerializeField] private Image handle;

        private Slider slider;
        private RectTransform rectTransform;
        private DrivenRectTransformTracker tracker;
        private Vector2 lastSize;
        private Sprite lastBackgroundSprite;
        private Sprite lastFillSprite;
        private Sprite lastHandleSprite;
        private bool dirty = true;

        private void Reset()
        {
            // Start from whatever size the slider already has.
            baseSize = ((RectTransform)transform).sizeDelta;
            size = 1f;
            FindParts();
            dirty = true;
        }

        private void OnValidate()
        {
            dirty = true;
        }

        private void OnEnable()
        {
            slider = GetComponent<Slider>();
            rectTransform = (RectTransform)transform;

            if (background == null || fillArea == null || fillImage == null || handleSlideArea == null || handle == null)
            {
                FindParts();
            }

            // Greys out Width/Height on the RectTransform so Size is the one place to change it.
            tracker.Clear();
            tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDelta);

            // Driven values are saved as zero, so restore the size straight away rather than waiting a frame.
            ApplySize();
            dirty = true;
        }

        private void OnDisable()
        {
            tracker.Clear();
        }

        private void LateUpdate()
        {
            ApplySize();

            // Swapping a sprite changes the multiplier it needs, so re-layout on that too.
            Sprite backgroundSprite = background != null ? background.sprite : null;
            Sprite fillSprite = fillImage != null ? fillImage.sprite : null;
            Sprite handleSprite = handle != null ? handle.sprite : null;

            Vector2 currentSize = rectTransform.rect.size;
            if (!dirty && currentSize == lastSize && backgroundSprite == lastBackgroundSprite
                && fillSprite == lastFillSprite && handleSprite == lastHandleSprite) return;

            lastSize = currentSize;
            lastBackgroundSprite = backgroundSprite;
            lastFillSprite = fillSprite;
            lastHandleSprite = handleSprite;
            dirty = false;
            Apply(currentSize);
        }

        private void ApplySize()
        {
            Vector2 targetSize = baseSize * size;
            if (rectTransform.sizeDelta != targetSize)
            {
                rectTransform.sizeDelta = targetSize;
            }
        }

        private void FindParts()
        {
            slider = GetComponent<Slider>();

            Transform backgroundTransform = transform.Find("Background");
            if (backgroundTransform != null)
            {
                background = backgroundTransform.GetComponent<Image>();
            }

            if (slider.fillRect != null)
            {
                fillArea = slider.fillRect.parent as RectTransform;

                foreach (Transform child in slider.fillRect)
                {
                    if (child.TryGetComponent(out Image image))
                    {
                        fillImage = image;
                        break;
                    }
                }
            }

            if (slider.handleRect != null)
            {
                handleSlideArea = slider.handleRect.parent as RectTransform;
                handle = slider.handleRect.GetComponent<Image>();
            }
        }

        private void Apply(Vector2 size)
        {
            float barHeight = size.y * barThickness;
            float yMin = 0.5f - barThickness / 2f;
            float yMax = 0.5f + barThickness / 2f;

            // Convert the inset from Background-sprite pixels to UI units at the track's current height.
            float inset = 0f;
            if (background != null && background.sprite != null)
            {
                inset = fillInset * barHeight / background.sprite.rect.height;
            }

            if (background != null)
            {
                Stretch(background.rectTransform, yMin, yMax, 0f, 0f);
                background.pixelsPerUnitMultiplier = FitMultiplier(background, barHeight);
            }

            if (fillArea != null)
            {
                Stretch(fillArea, yMin, yMax, inset, inset);
            }

            // The Slider drives Fill's anchors; its offsets must be zero so the mask edge lands exactly on the value.
            if (slider.fillRect != null)
            {
                slider.fillRect.anchoredPosition = Vector2.zero;
                slider.fillRect.sizeDelta = Vector2.zero;
            }

            if (fillImage != null)
            {
                RectTransform fillRect = fillImage.rectTransform;
                fillRect.anchorMin = new Vector2(0f, 0f);
                fillRect.anchorMax = new Vector2(0f, 1f);
                fillRect.pivot = new Vector2(0f, 0.5f);
                fillRect.anchoredPosition = Vector2.zero;
                fillRect.sizeDelta = new Vector2(size.x - 2f * inset, 0f);
                fillImage.pixelsPerUnitMultiplier = FitMultiplier(fillImage, barHeight - 2f * inset);
            }

            // Handle is the full slider height, as wide as its sprite's proportions allow.
            float handleWidth = size.y;
            if (handle != null && handle.sprite != null)
            {
                handleWidth = size.y * handle.sprite.rect.width / handle.sprite.rect.height;
            }

            if (slider.handleRect != null)
            {
                slider.handleRect.sizeDelta = new Vector2(handleWidth, 0f);
            }

            // Keep the whole handle inside the track at both ends. The fill's cut edge then always stays under the handle.
            if (handleSlideArea != null)
            {
                Stretch(handleSlideArea, 0f, 1f, handleWidth / 2f, 0f);
            }
        }

        // Pixels Per Unit Multiplier that makes the sprite's full height fit the rect, so sliced end caps keep their shape.
        private static float FitMultiplier(Image image, float height)
        {
            if (image.sprite == null || height <= 0f) return image.pixelsPerUnitMultiplier;

            float referencePixelsPerUnit = image.canvas != null ? image.canvas.referencePixelsPerUnit : 100f;
            return image.sprite.rect.height * referencePixelsPerUnit / (image.sprite.pixelsPerUnit * height);
        }

        private static void Stretch(RectTransform rect, float yMin, float yMax, float xInset, float yInset)
        {
            rect.anchorMin = new Vector2(0f, yMin);
            rect.anchorMax = new Vector2(1f, yMax);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(-2f * xInset, -2f * yInset);
        }
    }
}
