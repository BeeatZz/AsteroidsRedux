using UnityEngine;
using Asteroids.Managers;

namespace Asteroids.UI
{
    /* Hides the mouse cursor while the ship is being flown and shows it whenever a menu needs it.
     * The cursor image itself is the Default Cursor in Project Settings > Player, so any scene
     * without this component (main menu, loading) gets the custom cursor automatically.
     * Lives in the game scene only.
     */
    public class GameplayCursorHider : MonoBehaviour
    {
        private void Update()
        {
            // Pause and game over both freeze time; a scene fade needs the cursor back too.
            bool leavingScene = SceneLoader.Instance != null && SceneLoader.Instance.IsTransitioning;
            bool menuOpen = Time.timeScale == 0f || leavingScene;

            Cursor.visible = menuOpen;
        }

        private void OnDisable()
        {
            // The setting is global and survives scene loads, so never leave it hidden behind us.
            Cursor.visible = true;
        }
    }
}
