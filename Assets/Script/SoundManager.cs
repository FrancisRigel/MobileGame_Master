using System.Collections;
using TMPro;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    public class AudioClipWithWeight
    {
        public AudioClip clip;
        public int weight;  // Higher values mean more likely to be selected
        public string dialogue;
    }

    public AudioSource audioSource;  // The audio source to play clips
    public AudioClipWithWeight[] goodDialogues;  // List of good dialogue audio clips
    public AudioClipWithWeight[] badDialogues;   // List of bad dialogue audio clips
    public GameObject dialogueBox;
    private GameObject dialogueBoxPrefab;
    [Range(0f, 100f)]
    public float playSoundChance = 50f;  // The chance of playing any sound

    private AudioClip lastPlayedClip; // Store the last played audio clip

    [Header("For SFX")]
    public GameObject pickupSFX;
    public AudioClip pickup;
    public AudioClip drop;
    public AudioClip attach;
    // Public method to play a random good dialogue
    [System.Serializable]
    public class EndDialogues
    {
        public AudioClip clip;
        public string dialogue;
        [Range(1, 9)]
        public float dialogueDuration;
    }

    public EndDialogues goodEnding;
    public EndDialogues badEnding;

    public void PlayBadEndingDialogue()
    {
        StartCoroutine(WaitEnding(badEnding));
    }

    public void PlayGoodEndingDialogue()
    {
        StartCoroutine(WaitEnding(goodEnding));
    }

    IEnumerator WaitEnding(EndDialogues dialogue)
    {
        yield return new WaitForSeconds(1f);
        PlayEnding(dialogue);
    }
    public void PlayEnding(EndDialogues endDialogues)
    {
        StopAllCoroutines();
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = endDialogues.clip;
        audioSource.Play();
        if(dialogueBoxPrefab != null)
        {
            Destroy(dialogueBoxPrefab);
        }

        if(dialogueBoxPrefab == null)
        {
            dialogueBoxPrefab = Instantiate(dialogueBox);
            TextMeshProUGUI text = dialogueBoxPrefab.GetComponentInChildren<TextMeshProUGUI>();
            text.text = endDialogues.dialogue;
            StartCoroutine(enumerator(endDialogues.dialogueDuration));
        }

    }
    public void PlayRandomGoodDialogue()
    {
        StartCoroutine(WaitForMiliSecs(goodDialogues));
    }

    // Public method to play a random bad dialogue
    public void PlayRandomBadDialogue()
    {
        StartCoroutine(WaitForMiliSecs(badDialogues));
    }

    private IEnumerator WaitForMiliSecs(AudioClipWithWeight[] selections)
    {
        yield return new WaitForSeconds(.3f);
        PlayRandomClip(selections);
    }

    // Private method to handle playing a random clip from the provided dialogue array
    private void PlayRandomClip(AudioClipWithWeight[] selectedDialogues)
    {
        // Check if the audio source is currently playing a clip
        if (audioSource.isPlaying)
        {
            Debug.Log("Audio is already playing. No new clip will be played.");
            return; // Exit early if audio is already playing
        }

        // Determine if a sound should be played
        if (Random.Range(0f, 100f) > playSoundChance)
        {
            Debug.Log("No sound played (chance not met).");
            return; // Exit early if the chance is not met
        }

        // Calculate total weight
        int totalWeight = 0;
        foreach (var clip in selectedDialogues)
        {
            totalWeight += clip.weight;
        }

        // Pick a random weight value
        int randomWeight = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        // Select the clip based on the random weight, ensuring it isn't the same as the last played
        foreach (var clip in selectedDialogues)
        {
            cumulativeWeight += clip.weight;

            if (randomWeight < cumulativeWeight && clip.clip != lastPlayedClip) // Ensure it's not the last played clip
            {
                audioSource.clip = clip.clip;
                audioSource.Play();
                lastPlayedClip = clip.clip; // Update the last played clip
                if(dialogueBoxPrefab == null)
                {
                    dialogueBoxPrefab = Instantiate(dialogueBox);
                    TextMeshProUGUI text = dialogueBoxPrefab.GetComponentInChildren<TextMeshProUGUI>();
                    text.text = clip.dialogue;
                    StartCoroutine(enumerator(2));
                }
                return; // Exit after playing
            }
        }

        // If no valid clip found, you may want to handle it here (optional)
        Debug.LogWarning("No new clip found, retrying...");
    }

    private IEnumerator enumerator(float i)
    {
        yield return new WaitForSeconds(i);
        if(dialogueBoxPrefab != null)
        {
            Destroy(dialogueBoxPrefab);
        }
    }

    public void SelectedObjectSFX(int i)
    {
        if (i == 1)
        {
            GameObject obj = Instantiate(pickupSFX, Camera.main.transform);
            AudioSource source = obj.GetComponent<AudioSource>();
            source.clip = pickup;
            source.Play();
        }
        else if (i == 2)
        {
            GameObject obj = Instantiate(pickupSFX, Camera.main.transform);
            AudioSource source = obj.GetComponent<AudioSource>();
            source.clip = drop;
            source.Play();
        }
        else if (i == 3)
        {
            GameObject obj = Instantiate(pickupSFX, Camera.main.transform);
            AudioSource source = obj.GetComponent<AudioSource>();
            source.clip = attach;
            source.volume = 0.3f;
            source.Play();
        } 

    }
}
