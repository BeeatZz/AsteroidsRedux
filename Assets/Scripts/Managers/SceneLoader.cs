using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Asteroids.Pooling;

namespace Asteroids.Managers
{
    /* Fades the screen, and all audio with it, to black between scenes and back in afterwards.
     *
     * LoadSceneWithLoadingScreen goes through the loading scene, and keeps it up for the scene's whole
     * setup, not just its file load: the target loads additively underneath the loading screen, its
     * Awakes run, and then everything in it is held switched off (apart from its camera and object pool)
     * while the pool prewarms across frames. When the pool reports ready, and the minimum time has
     * passed, the screen fades to black, the loading scene is dropped, the held scene is switched on
     * (so its Start methods, music and intro all begin now) and the screen fades back in.
     *
     * Lives on a prefab placed in every scene, so any scene can be played on its own in the editor.
     * The first copy survives scene loads and later copies remove themselves.
     * Runs on real time, since the pause and game-over screens load scenes with time stopped.
     */
    public class SceneLoader : MonoBehaviour
    {
        // A load hitch mustn't skip most of a fade in one frame.
        private const float MaxFadeStep = 1f / 30f;

        public static SceneLoader Instance { get; private set; }

        [Header("Fade")]
        [Tooltip("CanvasGroup on the full-screen black image, on a canvas drawn above everything else.")]
        [SerializeField] private CanvasGroup fadeGroup;
        [Min(0f)]
        [SerializeField] private float fadeOutDuration = 0.5f;
        [Min(0f)]
        [SerializeField] private float fadeInDuration = 0.6f;
        [Tooltip("Fade all audio with the screen, so music doesn't cut off at the scene change.")]
        [SerializeField] private bool fadeAudio = true;
        [Tooltip("Frames to wait on black after a scene switches in, so its Start methods have run " +
                 "before anything is shown.")]
        [Min(1)]
        [SerializeField] private int settleFrames = 2;

        [Header("Loading Screen")]
        [Tooltip("Must be added to File > Build Profiles > Scene List.")]
        [SerializeField] private string loadingSceneName = "LoadingScene";
        [Tooltip("Shortest time the loading screen stays up, in seconds, so it never just flickers past.")]
        [Min(0f)]
        [SerializeField] private float minLoadingDuration = 2f;
        [Tooltip("Longest the loading screen waits for the scene's setup, in seconds, before it carries on anyway.")]
        [Min(1f)]
        [SerializeField] private float maxLoadingDuration = 20f;

        private readonly List<GameObject> heldRoots = new();
        private Scene heldScene;

        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // The first scene opens from black too.
            SetFade(1f, silenceAudio: true);
            StartCoroutine(FadeInFirstScene());
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                SceneManager.sceneLoaded -= HoldLoadedScene;
            }
        }

        public void LoadScene(string sceneName)
        {
            Begin(sceneName, useLoadingScreen: false);
        }

        public void LoadSceneWithLoadingScreen(string sceneName)
        {
            Begin(sceneName, useLoadingScreen: true);
        }

        private void Begin(string sceneName, bool useLoadingScreen)
        {
            if (IsTransitioning) return;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"Scene '{sceneName}' isn't in the build's scene list.", this);
                return;
            }

            // Stops the opening fade in, if a button was clicked during it; the fade out continues from there.
            StopAllCoroutines();
            StartCoroutine(Transition(sceneName, useLoadingScreen));
        }

        private IEnumerator FadeInFirstScene()
        {
            yield return Settle();
            yield return Fade(0f, fadeInDuration, silenceAudio: false);
        }

        private IEnumerator Transition(string sceneName, bool useLoadingScreen)
        {
            IsTransitioning = true;

            yield return Fade(1f, fadeOutDuration, silenceAudio: true);
            ResetGlobalState();

            if (useLoadingScreen && Application.CanStreamedLevelBeLoaded(loadingSceneName))
            {
                yield return LoadThroughLoadingScreen(sceneName);
            }
            else
            {
                if (useLoadingScreen)
                {
                    Debug.LogWarning($"Loading scene '{loadingSceneName}' isn't in the build's scene list; loading without it.", this);
                }

                SceneManager.LoadScene(sceneName);
                yield return Settle();
            }

            yield return Fade(0f, fadeInDuration, silenceAudio: false);

            IsTransitioning = false;
        }

        // Ends on black, with the target scene live and the loading scene gone.
        private IEnumerator LoadThroughLoadingScreen(string sceneName)
        {
            SceneManager.LoadScene(loadingSceneName);
            yield return Settle();

            Scene loadingScene = SceneManager.GetActiveScene();
            float shownAt = Time.unscaledTime;

            // The loading screen has no sound, so the audio stays down until the target is in.
            yield return Fade(0f, fadeInDuration, silenceAudio: true);

            // Reading the scene's assets happens off the main thread, so the loading screen keeps animating.
            // The load stops at 0.9 until it's allowed to switch in.
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            load.allowSceneActivation = false;
            while (load.progress < 0.9f)
            {
                yield return null;
            }

            // Two listeners would make Unity complain, and the loading scene is silent anyway.
            DisableAudioListeners(loadingScene);

            // Switching in runs the scene's Awakes. HoldLoadedScene runs right after them, before any Start.
            heldRoots.Clear();
            heldScene = default;
            SceneManager.sceneLoaded += HoldLoadedScene;
            load.allowSceneActivation = true;
            while (!load.isDone)
            {
                yield return null;
            }
            SceneManager.sceneLoaded -= HoldLoadedScene;

            if (!heldScene.IsValid())
            {
                heldScene = SceneManager.GetSceneByName(sceneName);
            }

            // The pool prewarms across frames while the held scene sits switched off.
            float deadline = Time.unscaledTime + maxLoadingDuration;
            while (Time.unscaledTime - shownAt < minLoadingDuration || !IsSceneReady())
            {
                if (Time.unscaledTime > deadline)
                {
                    Debug.LogWarning($"'{sceneName}' took more than {maxLoadingDuration} s to get ready; showing it anyway.", this);
                    break;
                }

                yield return null;
            }

            yield return Fade(1f, fadeOutDuration, silenceAudio: true);

            // Now on black: hand over.
            SceneManager.SetActiveScene(heldScene);
            AsyncOperation unload = SceneManager.UnloadSceneAsync(loadingScene);
            while (unload != null && !unload.isDone)
            {
                yield return null;
            }

            ReleaseHeldRoots();
            yield return Settle();
        }

        // Called for the target scene right after its Awakes and OnEnables, and before its first Start.
        // Switching a root off here runs its OnDisable, and switching it back on later runs OnEnable
        // and then Start, so each object does its real start-up when the player can see it.
        private void HoldLoadedScene(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Additive) return;

            heldScene = scene;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                // The camera has to stay: objects in the pool cache Camera.main as they're created.
                // The pool has to stay: it's what is being prepared.
                if (root.GetComponentInChildren<Camera>(true) != null) continue;
                if (root.GetComponentInChildren<ObjectPool>(true) != null) continue;
                if (!root.activeSelf) continue;

                root.SetActive(false);
                heldRoots.Add(root);
            }
        }

        private void ReleaseHeldRoots()
        {
            foreach (GameObject root in heldRoots)
            {
                // A duplicate SceneLoader removes itself during Awake, so it can already be gone.
                if (root != null)
                {
                    root.SetActive(true);
                }
            }

            heldRoots.Clear();
        }

        private static bool IsSceneReady()
        {
            return ObjectPool.Instance == null || ObjectPool.Instance.IsReady;
        }

        private static void DisableAudioListeners(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (AudioListener listener in root.GetComponentsInChildren<AudioListener>())
                {
                    listener.enabled = false;
                }
            }
        }

        // Both are global and survive scene loads; the next scene expects them at their defaults.
        private static void ResetGlobalState()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        private IEnumerator Settle()
        {
            // One extra frame, since a scene loaded with LoadScene only switches in on the next frame.
            for (int i = 0; i <= settleFrames; i++)
            {
                yield return null;
            }
        }

        private IEnumerator Fade(float targetAlpha, float duration, bool silenceAudio)
        {
            if (fadeGroup == null)
            {
                // Nothing to show, but the audio still has to end up where the fade would leave it.
                SetFade(targetAlpha, silenceAudio);
                yield break;
            }

            fadeGroup.blocksRaycasts = true;
            float startAlpha = fadeGroup.alpha;

            for (float elapsed = 0f; elapsed < duration; elapsed += Mathf.Min(Time.unscaledDeltaTime, MaxFadeStep))
            {
                SetFade(Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration), silenceAudio);
                yield return null;
            }

            SetFade(targetAlpha, silenceAudio);

            // Clicks go through again once the screen is clear.
            fadeGroup.blocksRaycasts = targetAlpha > 0f;
        }

        private void SetFade(float alpha, bool silenceAudio)
        {
            if (fadeGroup != null)
            {
                fadeGroup.alpha = alpha;
            }

            if (fadeAudio)
            {
                AudioListener.volume = silenceAudio ? Mathf.Min(AudioListener.volume, 1f - alpha) : 1f - alpha;
            }
        }
    }
}
