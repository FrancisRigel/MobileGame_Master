using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DialogueManager;

public class PhoneManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip phoneDial;
    public AudioClip beep;
    public Animator animator;
    DialogueManager dialogueManager;
    public GameObject dialogeBoxPrefab;
    private GameObject dialogueBoxInstance;

    [Header("Fade")]
    public GameObject faderPrefab;
    private GameObject fader;
    Image image;

    [Header("Evaluation")]
    public GameObject evaluationPrefab;
    private GameObject evaluation;
    private void Awake()
    {
        dialogueManager = GameObject.Find("Game Manager").GetComponent<DialogueManager>();
        audioSource.Play();
        StartCoroutine(WaitForNextClip());
    }

    private IEnumerator WaitForNextClip()
    {
        while (audioSource.isPlaying)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1);

        audioSource.clip = phoneDial;
        audioSource.Play();

        while (audioSource.isPlaying)
        {
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(1);

        FinalDialogueStart();
    }
    [System.Serializable]
    public class FinalDialogue
    {
        public AudioClip clip;
        public string dialogueText;
    }

    public FinalDialogue[] finals;

    public void FinalDialogueStart()
    {
        StartCoroutine(FinalDialogueTimer(finals));
    }
    private IEnumerator FinalDialogueTimer(FinalDialogue[] final)
    {
        int dialogueCount = 0;

        while (dialogueCount < final.Length)
        {
            audioSource.clip = final[dialogueCount].clip;
            audioSource.Play();
            if (dialogueBoxInstance == null)
            {
                dialogueBoxInstance = Instantiate(dialogeBoxPrefab);
            }
            TextMeshProUGUI text = dialogueBoxInstance.GetComponentInChildren<TextMeshProUGUI>();
            text.text = final[dialogueCount].dialogueText;
            yield return new WaitForSeconds(final[dialogueCount].clip.length);

            dialogueCount++;
        }
        Destroy(dialogueBoxInstance);
        dialogueBoxInstance = null;

        yield return new WaitForSeconds(1);

        audioSource.clip = beep;
        audioSource.Play();

        while (audioSource.isPlaying)
        {
            yield return null;
        }
        animator.SetTrigger("End");
        StartFade();
    }

    private void StartFade()
    {
        fader = Instantiate(faderPrefab, GameObject.Find("Canvas").transform);
        image = fader.GetComponent<Image>();
        StartCoroutine(FadeImageAlpha(1, 3f));
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
        evaluation = Instantiate(evaluationPrefab);
    }
}
