// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace CasualBlast
{
    public class Transition : MonoBehaviour
    {
        // Reference to the canvas used for transition
        private static GameObject m_canvas;

        // Singleton instance of the Transition class
        private static Transition instance;

        // Reference to the overlay object used for fading effect
        private GameObject m_overlay;

        private void Awake()
        {
            // Create a new canvas if it does not exist
            if (m_canvas == null)
            {
                m_canvas = new GameObject("TransitionCanvas");
                var canvas = m_canvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                DontDestroyOnLoad(m_canvas);
            }
        }

        // Method to load a new level with a transition effect
        public static void LoadLevel(string level, float duration, Color color)
        {
            if (instance == null)
            {
                var fade = new GameObject("Transition");
                instance = fade.AddComponent<Transition>();
                instance.StartFade(level, duration, color);
                fade.transform.SetParent(m_canvas.transform, false);
                fade.transform.SetAsLastSibling();
            }
        }

        // Method to start the fade effect
        private void StartFade(string level, float duration, Color fadeColor)
        {
            StartCoroutine(RunFade(level, duration, fadeColor));
        }

        // Coroutine to handle the fade effect and scene transition
        private IEnumerator RunFade(string level, float duration, Color fadeColor)
        {
            // Create the overlay for fade effect
            CreateOverlay(fadeColor);

            // Fade in
            yield return Fade(0f, 1f, duration / 2);

            // Load the new scene
            SceneManager.LoadScene(level);

            // Fade out
            yield return Fade(1f, 0f, duration / 2);

            // Clean up the overlay and transition object
            Destroy(m_overlay);
            Destroy(gameObject);
            instance = null;
        }

        // Method to create the overlay used for fading
        private void CreateOverlay(Color fadeColor)
        {
            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, fadeColor);
            bgTex.Apply();

            m_overlay = new GameObject("Overlay");
            var image = m_overlay.AddComponent<Image>();
            var rect = new Rect(0, 0, bgTex.width, bgTex.height);
            var sprite = Sprite.Create(bgTex, rect, new Vector2(0.5f, 0.5f), 1);
            image.sprite = sprite;
            image.canvasRenderer.SetAlpha(0.0f);

            m_overlay.transform.localScale = new Vector3(1, 1, 1);
            var overlayRect = m_overlay.GetComponent<RectTransform>();
            var canvasRect = m_canvas.GetComponent<RectTransform>();
            overlayRect.sizeDelta = canvasRect.sizeDelta;
            m_overlay.transform.SetParent(m_canvas.transform, false);
            m_overlay.transform.SetAsFirstSibling();
        }

        // Coroutine to perform the fade effect
        private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
        {
            var image = m_overlay.GetComponent<Image>();
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
                image.canvasRenderer.SetAlpha(alpha);
                yield return null;
            }

            image.canvasRenderer.SetAlpha(endAlpha);
        }
    }
}
