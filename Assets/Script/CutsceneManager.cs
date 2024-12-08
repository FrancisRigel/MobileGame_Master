using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public Image image;
    public Button button;
    public RectTransform newButtonPosition;
    public float timer = 0;
    Coroutine coroutine;
    Vector2 oldButtonPosition;

    private void Start()
    {
        oldButtonPosition = button.transform.position;
    }

    private void Awake()
    {
        playableDirector.Play();
        StartCoroutine(FadeImageAlpha(0, 1.5f));
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began && coroutine == null)
            {
                button.interactable = false;
                OnScreenTap();
            }
        }
    }

    private IEnumerator FadeImageAlpha(float targetAlpha, float duration)
    {
        // Get the current color of the image
        Color currentColor = image.color;
        // Store the starting alpha value
        float startAlpha = currentColor.a;

        // Timer for the fade duration
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // Interpolate the alpha value
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);

            // Update the image color with the new alpha value
            currentColor.a = alpha;
            image.color = currentColor;

            yield return null;
        }

        // Ensure the final alpha is exactly the target alpha
        image.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
        image.enabled = false;
    }

    private IEnumerator FadeImageAlphaS(float targetAlpha, float duration)
    {
        // Get the current color of the image
        Color currentColor = image.color;
        // Store the starting alpha value
        float startAlpha = currentColor.a;

        // Timer for the fade duration
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // Interpolate the alpha value
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);

            // Update the image color with the new alpha value
            currentColor.a = alpha;
            image.color = currentColor;

            yield return null;
        }

        // Ensure the final alpha is exactly the target alpha
        image.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
        SceneManager.LoadScene(2); 
    }

    public void ChangeScene()
    {
        image.enabled = true;
        StartCoroutine(FadeImageAlphaS(1, 1.5f));

    }

    void OnScreenTap()
    {
        coroutine = StartCoroutine(StartSkipAppear());
    }

    private IEnumerator StartSkipAppear()
    {
        float t = 0f;
        float duration = 1f;
        while (t < duration)
        {
            t += Time.deltaTime;
            button.transform.position = Vector3.Lerp(button.transform.position, newButtonPosition.position, t / duration);
            yield return null;
        }

        float tt = 0;
        float d = 1f;
        button.interactable = true;
        while (tt < d)
        {
            tt += Time.deltaTime;
            timer = tt;
            yield return null;
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    tt = 0;
                }
            }
        }
        timer = 0f;
        button.interactable = false;
        StartCoroutine(SkipRemove());
    }

    private IEnumerator SkipRemove()
    {
        float timer = 0;
        float duration = 1f;
        while(timer < duration)
        {
            timer += Time.deltaTime;
            button.transform.position = Vector3.Lerp( button.transform.position, oldButtonPosition, timer / duration);
            yield return null;
        }
        coroutine = null;
    }
}
