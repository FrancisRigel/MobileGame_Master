using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ComputerCaseManager : MonoBehaviour
{
    [Header("Settings")]
    public CameraMovement cameraMovement;
    public Transform newPosition;
    public Vector3 newRotation;
    public float rotationSpeed = 0.1f;
    public ComponentManager motherboard;
    public GameManager gameManager;
    [Header("Checkers")]
    public bool isDragging = false;
    public bool isCameraCloser = false;
    public bool caseDetailsOn = false;
    public bool isCaseTouched;
    public bool isGlassAnimating = false;
    public bool isGlassAttached = true;
    public Animator removeScrewAnimation;
    public ScrewFunction[] screwFunction;
    public Transform glasPanelOrginalPosition, glassPanelRemovedPosition, glassPanelRemovingPosition;
    [HideInInspector]public Touch touch;

    [HideInInspector]public ComponentManager[] colliders;

    public bool glassConstraint = true;
    private void LateUpdate()
    {
        if (gameManager.instance != null || cameraMovement.isSettingsOn) return;
        cameraMovement.ButtonHandler(cameraMovement.holdingObject, isCameraCloser, cameraMovement.caseDetailsAnimation);
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (cameraMovement.holdingObject) return;
        if (Input.touchCount == 0)
        {
            cameraMovement.holdingObject = false;
            return;
        }

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
                if (!caseDetailsOn)
                    CaseCloserFunction();
                else
                    HandleTouchEnded(touch);
                break;
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        if (cameraMovement.componentDetails) return;

        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        int highestPriority = int.MinValue;
        GameObject selectedObject = null;
        foreach(var hit in hits)
        {
            Collider hitCollider = hit.collider;
            int priority = hitCollider.layerOverridePriority;
            if(priority > highestPriority)
            {
                highestPriority = priority;
                selectedObject = hitCollider.gameObject;
            }
        }

        if (selectedObject != gameObject) return;
        if (selectedObject == gameObject)
        {
            isDragging = true;
            return;
        }
    }

    private void HandleTouchMoved(Touch touch)
    {
        if(cameraMovement.componentDetails) return;

        if(isDragging && isCameraCloser)
        {
            float rotationX = touch.deltaPosition.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, -rotationX, Space.World);
        }
    }

    private void HandleTouchEnded(Touch touch)
    {
        if (isDragging)
        {
            isDragging = false;
        }
    }

    private void CaseCloserFunction()
    {
        if (isDragging)
        {
            caseDetailsOn = true;
            isCameraCloser = true;
            StartCaseDetailsAnimation(newPosition.position, newRotation, true);
            isDragging = false;
        }
    }
    public bool AllScrewRemoved()
    {
        foreach (ScrewFunction screws in screwFunction)
        {
            if (!screws.isScrewRemoved)
            {
                return false;
            }
        }
        return true;
    }

    private void StartCaseDetailsAnimation(Vector3 targetPosition, Vector3 targetRotation, bool isStarting)
    {
        isCaseTouched = false;
        cameraMovement.caseDetailsAnimation = true;
        StartCoroutine(AnimateCaseDetails(targetPosition, targetRotation, isStarting));
    }

    private IEnumerator AnimateCaseDetails(Vector3 targetPosition, Vector3 targetRotation, bool isStarting)
    {
        Quaternion targetRot = Quaternion.Euler(targetRotation);
        float duration = .5f;
        float timer = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        Quaternion startRotation = Camera.main.transform.rotation;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            Camera.main.transform.rotation = Quaternion.Lerp(startRotation, targetRot, t);
            yield return null;
        }

        Camera.main.transform.position = targetPosition;
        Camera.main.transform.rotation = targetRot;

        cameraMovement.SetRotation(Camera.main.transform.localRotation);
        caseDetailsOn = isStarting;
        cameraMovement.caseDetailsAnimation = false;
    }


    public void OnRightButtonClicked()
    {
        StartCaseDetailsAnimation(newPosition.position, newRotation, true);
        cameraMovement.rightButton.interactable = false;
        isCameraCloser = true;
    }

    public void OnLeftButtonClicked()
    {
        StartCaseDetailsAnimation(cameraMovement.oldPosition, cameraMovement.oldRotation, false);
        cameraMovement.leftButton.interactable = false;
        isCameraCloser = false;
    }
}
