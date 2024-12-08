using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogeBoxPrefab;
    public AudioSource audioSource;
    private GameObject dialogueBoxInstance;
    [System.Serializable]
    public class Dialogue
    {
        public AudioClip clip;
        [Range(0, 10)]
        public float dialogueDelay;
        public string dialogueText;
    }
    public Dialogue monitorDialogue;
    public Dialogue turnOnFinal;
    public Dialogue monologueWhileTurningOn;

    public void DialogueStart(Dialogue a)
    {
        StartCoroutine(DialogueTime(a));
    }

    private IEnumerator DialogueTime(Dialogue a)
    {
        yield return new WaitForSeconds(a.dialogueDelay);
        if (dialogueBoxInstance == null)
        {
            dialogueBoxInstance = Instantiate(dialogeBoxPrefab);
            TextMeshProUGUI text = dialogueBoxInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (a.clip != null)
            {
                audioSource.clip = a.clip;
                audioSource.Play();
            }
            text.text = a.dialogueText;
        }

        yield return new WaitForSeconds(a.clip.length);

        if(dialogueBoxInstance != null )
        {
            Destroy(dialogueBoxInstance);
        }
    }
}
