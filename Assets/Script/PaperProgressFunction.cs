using cakeslice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PaperProgressFunction : MonoBehaviour
{
    public Camera cam;
    public CameraMovement cameraMovement;
    public GameManager manager;
    public GameObject paperProgressPrefab;
    public GameManager gameManager;
    private GameObject paper;
    public Canvas canvas;
    public AudioSource source;
    public AudioClip paperSlide, paperDrop;
    [Header("Checkers")]
    public bool isHitZone;
    public bool isClicked = false;

    private void LateUpdate()
    {
        if (gameManager.isSwitchedTurnedOn) return;
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            cameraMovement.holdingObject = false;
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                HandleTouchBegan(touch);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                HandleTouchMoved(touch);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                HandleTouchEnded();
                break;
        }
    }
    private void RaycastHandler(Touch touch)
    {
        if (isClicked) return;
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity);
        if (hitSomething && hit.collider != null && hit.collider.CompareTag("Paper"))
        {
            isHitZone = true;
        }
        else
        {
            if (isHitZone)
            {
                isHitZone = false;
            }
        }
    }
    private void HandleTouchBegan(Touch touch)
    {
        if (cameraMovement.componentDetails) return;
        RaycastHandler(touch);
    }

    private void HandleTouchMoved(Touch touch)
    {
        if (isHitZone)
        {
            RaycastHandler(touch);
        }
    }

    private void HandleTouchEnded()
    {
        if (isHitZone)
        {
            if(paper == null)
            {
                paper = Instantiate(paperProgressPrefab, canvas.transform);
                StartCoroutine(PaperAnimation());
                StartCoroutine(manager.ButtonsRemove());
                cameraMovement.selectedObject = paperProgressPrefab;
                cameraMovement.isSettingsOn = true;
                if (!source.isPlaying)
                {
                    source.clip = paperSlide;
                    source.Play();
                }
            }
            isClicked = false;
            isHitZone = false;
        }
    }

    private IEnumerator PaperAnimation()
    {
        float timer = 0f;
        float duration = 1f;

        // Get the RectTransform component from the 'paper' GameObject
        RectTransform rect = paper.GetComponent<RectTransform>();

        // Starting and target positions for Y
        float startY = -500f;
        float targetY = 0f;
        // Store the initial position (X stays constant)
        Vector2 startPos = rect.anchoredPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Interpolate only the Y position while keeping the X position constant
            rect.anchoredPosition = new Vector2(startPos.x, Mathf.Lerp(startY, targetY, t));
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 65, t);
            yield return null;
        }

        // Ensure the final position is set to the target value after the loop
        rect.anchoredPosition = new Vector2(startPos.x, targetY);
        yield return new WaitForSeconds(0.5f);
        Button child = paper.transform.Find("Button").gameObject.GetComponent<Button>();
        child.interactable = true;
        child.onClick.AddListener(BackButtonPaper);
    }

    public void BackButtonPaper()
    {
        StartCoroutine(PaperAnimationDone());
        StartCoroutine(manager.ButtonsBack());

        if (!source.isPlaying)
        {
            source.clip = paperDrop;
            source.Play();
        }
    }

    private IEnumerator PaperAnimationDone()
    {
        float timer = 0f;
        float duration = 1f;

        // Get the RectTransform component from the 'paper' GameObject
        RectTransform rect = paper.GetComponent<RectTransform>();

        // Starting and target positions for Y
        float startY = 0;
        float targetY = -500f;

        // Store the initial position (X stays constant)
        Vector2 startPos = rect.anchoredPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Interpolate only the Y position while keeping the X position constant
            rect.anchoredPosition = new Vector2(startPos.x, Mathf.Lerp(startY, targetY, t));

            yield return null;
        }
        Destroy(paper.gameObject);
        cameraMovement.selectedObject = null;
        cameraMovement.isSettingsOn = false;
    }

}
