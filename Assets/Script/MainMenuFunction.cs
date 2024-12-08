using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuFunction : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject WirePrefab;
    public Transform lookAtPos;
    public Transform positionB;
    public float speed;
    public float waitTime;
    public Transform positionA;
    [System.Serializable]
    public class WireManagement
    {
        public string name;
        public Transform startLine;
        public Transform endLine;
    }

    public WireManagement[] wireManagement;


    [Header("Fader")]
    public GameObject faderPrefab;
    private GameObject fader;
    public GameObject[] titles;
    public RectTransform titleItself;
    public RectTransform newTitlePosition;
    public GameObject playButton;
    public GameObject howToPlayButton;
    public RectTransform playButtonPosition;
    public RectTransform howToPlayButtonPosition;
    private void Start()
    {
        HandleWires(wireManagement);
        StartCoroutine(MoveBackAndForth());
        StartFade();
    }

    private IEnumerator MoveBackAndForth()
    {
        while (true)
        {
            // Move to position B
            yield return MoveToPosition(positionB);
            yield return new WaitForSeconds(waitTime);

            // Move to position A
            yield return MoveToPosition(positionA);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator MoveToPosition(Transform targetPosition)
    {
        while (Vector3.Distance(mainCamera.transform.position, targetPosition.position) > .1f)
        {
            mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, targetPosition.position, speed * Time.deltaTime);
            mainCamera.transform.LookAt(lookAtPos);
            yield return null; // Wait until the next frame
        }
    }


    private void HandleWires(WireManagement[] wireManagements)
    {
        foreach(WireManagement wireManagement in wireManagements)
        {
            GameObject wire =  Instantiate(WirePrefab);
            VerletRope wireSystem = wire.GetComponent<VerletRope>();

            wireSystem.startLine = wireManagement.startLine;
            wireSystem.ropeEndPoint = wireManagement.endLine;
        }
    }

    private void StartFade()
    {
        fader = Instantiate(faderPrefab, GameObject.Find("Canvas").transform);
        Image image = fader.GetComponent<Image>();
        Color color = image.color;
        color.a = 1;
        image.color = color;
        StartCoroutine(FadeImageAlpha(0, 3f, image));
    }

    private void StartTitleFade()
    {
        foreach(GameObject title in titles)
        {
            title.SetActive(true);
            Image image = title.GetComponent<Image>();
            if (image == null) return;
            Color targetColor = image.color;

            Color color = image.color;
            color.a = 0;
            image.color = color;

            Coroutine coroutine = StartCoroutine(FadeTitles(targetColor.a, 1.5f, image));
        }
    }

    private void PlayButtonFunction()
    {
        playButton.SetActive(true);
        howToPlayButton.SetActive(true);
        StartCoroutine(AnimatePlay());
    }

    private IEnumerator AnimatePlay()
    {
        float timer = 0;
        float duration = 1.5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            playButton.transform.position = Vector3.Lerp(playButton.transform.position, playButtonPosition.position, timer / duration);
            howToPlayButton.transform.position = Vector3.Lerp(howToPlayButton.transform.position, howToPlayButtonPosition.position, timer / duration);
            yield return null;
        }
    }

    private IEnumerator FadeTitles(float targetAlpha, float duration, Image image)
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

        timer = 0;
        duration = 1.5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            titleItself.position = Vector3.Lerp(titleItself.position, newTitlePosition.position, timer / duration);
            yield return null;
        }
        PlayButtonFunction();
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
        StartTitleFade();
    }

    public void LoadPlayScene()
    {
        StartCoroutine(FadeLast(1, 3f, 1));
        Button button = playButton.GetComponent<Button>();
        button.interactable = false;
        Button button1 = howToPlayButton.GetComponent<Button>();
        button1.interactable = false;
    }

    public void LoadTutorial()
    {
        StartCoroutine(FadeLast(1, 3f, 3));
        Button button = playButton.GetComponent<Button>();
        button.interactable = false;
        Button button1 = howToPlayButton.GetComponent<Button>();
        button1.interactable = false;
    }


    private IEnumerator FadeLast(float targetAlpha, float duration, int i)
    {
        Image image = fader.GetComponent<Image>();

        Color currentColor = image.color;
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
        SceneManager.LoadScene(i);
    }
}
