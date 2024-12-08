using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsFunction : MonoBehaviour
{
    public RectTransform rectTransform;
    public Button[] buttons;

    public GameManager gameManager;
    public CameraMovement cameraMovement;
    private void Awake()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>(); 
        StartCoroutine(SettingsPopUp());
    }

    private IEnumerator SettingsPopUp()
    {
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, new Vector3(1f, 1f, 1f), timer / duration);
            yield return null;
        }

        foreach(var button in buttons)
        {
            button.interactable = true;
        }
    } 

    private IEnumerator SettingsDisappear()
    {
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, new Vector3(0f, 0f, 0f), timer / duration);
            yield return null;
        }
        gameManager.gearIcon.interactable = true;
        cameraMovement.isSettingsOn = false;
        Destroy(gameManager.instance);
        gameManager.instance = null;
    }


    public void ResumeButton()
    {
        gameManager.ReturnArrowsFunction();
        foreach (var button in buttons)
        {
            button.interactable = false;
        }
        gameManager.GearIconAppearFunction();
        StartCoroutine(SettingsDisappear());
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitButton()  // Quit functionality
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // Stop play mode in Unity Editor
#else
               SceneManager.LoadScene(0);
#endif
    }
}
