using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ClipScript : MonoBehaviour
{
    public int currentSlide;

    [System.Serializable]
    public class TutorialSlides
    {
        public RawImage borderImg;
        public RawImage videoClip;
        public GameObject playerRenderer;
        public TextMeshProUGUI textMeshPro;
        [Header("Attributes")]
        public int targetOpacity;
        public int startOpacity;
        public float clipOpacitySpeed;
        public float typeSpeed = 0.05f;
    }
    private Coroutine typingCoroutine;
    public TutorialSlides clip;
    private string fullText;
    [SerializeField]
    private TutorialScript tutorialScript;
    private void OnEnable()
    {
        if (tutorialScript == null)
        {
            tutorialScript = GameObject.Find("Game Manager").GetComponent<TutorialScript>();
        }
        currentSlide = tutorialScript.slideInt;
        StartASlide(clip);
    }

    private void OnDisable()
    {

        EndSlide(clip);
    }

    private void AddSelf()
    {
        if (tutorialScript == null)
        {
            tutorialScript = GameObject.Find("Game Manager").GetComponent<TutorialScript>();
        }

        if (tutorialScript != null)
        {
            tutorialScript.currentSlide = this; // `this` refers to the current instance of this script
        }
    }

    private void EndSlide(TutorialSlides clip)
    {
        clip.borderImg.gameObject.SetActive(false);
        clip.videoClip.gameObject.SetActive(false);
        clip.playerRenderer.gameObject.SetActive(false);
        clip.textMeshPro.gameObject.SetActive(false);

        fullText = "";
        StopAllCoroutines();
    }

    private void StartASlide(TutorialSlides clip)
    {
        clip.borderImg.gameObject.SetActive(true);
        Color color = clip.borderImg.color;
        color.a = clip.startOpacity;
        clip.borderImg.color = color;
        StartCoroutine(FadeImageAlpha0(clip.targetOpacity, clip.clipOpacitySpeed, clip.borderImg, clip));
    }

    private void SetVideoClipOpacity(TutorialSlides clip)
    {
        clip.videoClip.gameObject.SetActive(true);
        Color color = clip.videoClip.color;
        color.a = clip.startOpacity;
        clip.videoClip.color = color;
        clip.playerRenderer.SetActive(true);
        StartCoroutine(FadeImageAlpha1(clip.targetOpacity, clip.clipOpacitySpeed, clip.videoClip));
        clip.textMeshPro.gameObject.SetActive(true);
        TextStart(clip);
        AddSelf();

        if(tutorialScript != null)
        {
            if(!tutorialScript.nextButton.interactable)
            {
                tutorialScript.nextButton.interactable = true;
            }

            if (!tutorialScript.prevButton.interactable)
            {
                tutorialScript.prevButton.interactable = true;
            }
        }
    }

    private IEnumerator FadeImageAlpha0(float targetAlpha, float duration, RawImage image, TutorialSlides clip)
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
        SetVideoClipOpacity(clip);
    }
    private IEnumerator FadeImageAlpha1(float targetAlpha, float duration, RawImage image)
    {
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
        }
    }

    private void TextStart(TutorialSlides clip)
    {
        // Get the full text from the TextMeshPro component
        fullText = clip.textMeshPro.text;
        clip.textMeshPro.text = ""; // Clear the text at start
        StartCoroutine(TypeText(clip));
    }

    private IEnumerator TypeText(TutorialSlides clip)
    {
        foreach (char letter in fullText.ToCharArray())
        {
            clip.textMeshPro.text += letter;
            yield return new WaitForSeconds(clip.typeSpeed);
        }
    }
}
