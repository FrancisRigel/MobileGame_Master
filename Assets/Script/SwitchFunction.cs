using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchFunction : MonoBehaviour
{
    RectTransform thisPos;
    Button button;
    public Color offColor = Color.white;
    public Color onColor = Color.white;
    public Image switchHead;
    public RectTransform switchOn;
    public RectTransform switchOff;
    public RectTransform switchSlider;
    public bool toggleState = false;
    AudioSource audioSource;
    public AudioClip switchSound;
    GameManager gameManager;
    public Image thisImage;
    private void Awake()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        audioSource = GetComponent<AudioSource>();
        button = GetComponent<Button>();
        thisPos = GetComponent<RectTransform>();
        StartCoroutine(AwakeAnimation(true));
        gameManager.switchFunction = this;
    }

    public IEnumerator AwakeAnimation(bool i)
    {
        float timer = 0;
        float duration = 1f;
        Vector3 targetScale;
        if(i)
        {
            targetScale = new Vector3(1, 1, 1);
        }
        else
        {
            targetScale = Vector3.zero;
        }


        while(timer < duration)
        {
            timer += Time.deltaTime;
            thisPos.localScale = Vector3.Lerp(thisPos.localScale, targetScale, timer / duration);
            yield return null;
        }
        if (i)
        {
            thisPos.localScale = targetScale;
            button.interactable = true;
            button.onClick.AddListener(Toggle);
        }
        else
        {
            Destroy(gameObject);
        }

    }

 
    public void Toggle()
    {
        toggleState = !toggleState;

        if (!toggleState)
        {
            gameManager.isSwitchedTurnedOn = false;
            switchHead.color = offColor;
            switchSlider.position = switchOn.position;
            if (gameManager.exploded) return;
            audioSource.clip = switchSound;
            audioSource.Play();
        }
        else
        {
            gameManager.isSwitchedTurnedOn = true;
            switchHead.color = onColor;
            switchSlider.position = switchOff.position;
            if (gameManager.exploded) return;
            audioSource.clip = switchSound;
            audioSource.Play();
        }
    }

    public void Toggler(bool t)
    {
        t = !t;
        if (!t)
        {
            gameManager.isSwitchedTurnedOn = false;
            switchHead.color = offColor;
            switchSlider.position = switchOn.position;
            audioSource.clip = switchSound;
            audioSource.Play();
        }
        else
        {
            gameManager.isSwitchedTurnedOn = true;
            switchHead.color = onColor;
            switchSlider.position = switchOff.position;
            audioSource.clip = switchSound;
            audioSource.Play();
        }
    }
}
