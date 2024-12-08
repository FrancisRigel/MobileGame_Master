using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    public GameObject fader;
    private GameObject faderObj;
    public GameObject[] slides;

    public Button nextButton;
    public Button prevButton;
    public Button backButton;


    public ClipScript currentSlide;
    public int slideInt;
    private bool backClick = false;
    private void Start()
    {
        StartScene();
    }

    private void LateUpdate()
    {
        if (backClick) return;
        if(currentSlide != null)
        {
            if (slideInt >= 4)
            {
                if (nextButton.gameObject.activeSelf)
                {
                    nextButton.gameObject.SetActive(false);
                }
            }
            else
            {
                if (!nextButton.gameObject.activeSelf)
                {
                    nextButton.gameObject.SetActive(true);
                }
            }


            if(slideInt != 0)
            {
                if(!prevButton.gameObject.activeSelf)
                {
                    prevButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (prevButton.gameObject.activeSelf)
                {
                    prevButton.gameObject.SetActive(false);
                }
            }
        }
    }


    private void StartScene()
    {
        fader = Instantiate(fader, GameObject.Find("Canvas").transform);
        Image image = fader.GetComponent<Image>();
        Color color = image.color;
        color.a = 1;
        image.color = color;
        StartCoroutine(FadeImageAlpha(0, 3f, image));
    }

    private IEnumerator FadeImageAlpha(float targetAlpha, float duration, Image image)
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
        Destroy(faderObj);
        slides[0].SetActive(true);
        backButton.gameObject.SetActive(true);
    }

    private IEnumerator FadeImageAlpha1(float targetAlpha, float duration, Image image)
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
        SceneManager.LoadScene(0);
    }



    public void NextButton()
    {
        if(currentSlide != null)
        {
            currentSlide.gameObject.SetActive(false);
            currentSlide = null;
        }


        slideInt++;
        nextButton.interactable = false;
        prevButton.interactable = false;

        slides[slideInt].gameObject.SetActive(true);
    }

    public void PrevButton()
    {
        if (currentSlide != null)
        {
            currentSlide.gameObject.SetActive(false);
            currentSlide = null;
        }


        slideInt--;
        nextButton.interactable = false;
        prevButton.interactable = false;

        slides[slideInt].gameObject.SetActive(true);
    }

    public void BackButton()
    {
        backClick = true;
        nextButton.interactable = false;
        prevButton.interactable = false;
        backButton.gameObject.SetActive(false);
        fader = Instantiate(fader, GameObject.Find("Canvas").transform);
        Image image = fader.GetComponent<Image>();
        Color color = image.color;
        color.a = 0;
        image.color = color;

        StartCoroutine(FadeImageAlpha1(3, 3f, image));
    }
}
