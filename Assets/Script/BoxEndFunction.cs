using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoxEndFunction : MonoBehaviour
{
    GameManager gameManager;
    public GameObject timeClockHeader;
    public TextMeshProUGUI timeClock;
    public RectTransform rectTransform;
    public GameObject[] buttons;
    private void Awake()
    {
        rectTransform.localScale = new Vector3(rectTransform.localScale.x, 0, rectTransform.localScale.z);
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        StartCoroutine(ScaleYOverTime(1, 1));
    }

    private IEnumerator ScaleYOverTime(float targetY, float duration)
    {
        float elapsedTime = 0f;
        Vector3 initialScale = rectTransform.localScale;

        while (elapsedTime < duration)
        {
            // Interpolate only the Y component
            float newY = Mathf.Lerp(initialScale.y, targetY, elapsedTime / duration);
            rectTransform.localScale = new Vector3(initialScale.x, newY, initialScale.z);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        // Ensure final scale is set at the end
        rectTransform.localScale = new Vector3(initialScale.x, targetY, initialScale.z);
        yield return new WaitForSeconds(.7f);
        timeClockHeader.SetActive(true);
        yield return new WaitForSeconds(.7f);
        HandleTimer();
        yield return new WaitForSeconds(1.5f);
        EnableButtons();
    }

    private void EnableButtons()
    {
        foreach(GameObject button in buttons)
        {
            button.SetActive(true);
        }
    }


    private void HandleTimer()
    {
        timeClock.gameObject.SetActive(true);
        timeClock.text = gameManager.formatTimeText;
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene(0);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
