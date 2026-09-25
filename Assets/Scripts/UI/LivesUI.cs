using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Asteroids.Events;

namespace Asteroids.UI
{
    /* Shows remaining lives as a row of ship icons. Icons are created on demand and
     * then only toggled, so gaining lives back later (e.g. extra-life pickups) reuses
     * existing icons instead of instantiating new ones every change.
     */
    public class LivesUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onLivesChangedChannel;

        [Header("UI Elements")]
        [Tooltip("Parent for the icons; give it a Horizontal Layout Group to space them out.")]
        [SerializeField] private RectTransform iconContainer;
        [SerializeField] private Image lifeIconPrefab;

        [Tooltip("Optional. Overrides the prefab's sprite so the icon can match the ship in use.")]
        [SerializeField] private Sprite shipSprite;

        private readonly List<Image> icons = new();

        private void OnEnable()
        {
            if (onLivesChangedChannel != null)
                onLivesChangedChannel.OnEventRaised += UpdateLivesIcons;
        }

        private void OnDisable()
        {
            if (onLivesChangedChannel != null)
                onLivesChangedChannel.OnEventRaised -= UpdateLivesIcons;
        }

        private void UpdateLivesIcons(int newLives)
        {
            if (iconContainer == null || lifeIconPrefab == null) return;

            int lives = Mathf.Max(newLives, 0);

            while (icons.Count < lives)
            {
                Image icon = Instantiate(lifeIconPrefab, iconContainer);
                if (shipSprite != null)
                {
                    icon.sprite = shipSprite;
                }
                icons.Add(icon);
            }

            for (int i = 0; i < icons.Count; i++)
            {
                icons[i].gameObject.SetActive(i < lives);
            }
        }
    }
}
