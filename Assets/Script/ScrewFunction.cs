using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ScrewFunction : MonoBehaviour
{
    public ComputerCaseManager computerCaseManager;
    public CameraMovement cameraMovement;
    public float distanceFromCamera;
    public float followSpeed;
    public LayerMask targetLayer;
    public LayerMask thisLayer;

    [Header("Checkers")]
    public bool isDragging;
    public bool isAttached;
    public bool removingScrew;
    public bool isScrewRemoved;
    public bool isHolding;
    public RemovedScrewPosition currentAttachedScrew;

    [Header("Target Locations")]
    public RemovedScrewPosition[] removedScrewPosition;
    public RemovedScrewPosition[] screwOutlets;

    private RemovedScrewPosition currentRemovedPosition;

    public GameObject screwObject;
    public float offsetValue;
    private Vector3 touchPosition;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    public cakeslice.Outline outline;
    private static GameObject selectedObject;
    private Coroutine removingScrewCoroutine;
    GameManager gameManager;
    private void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        originalRotation = transform.rotation;
        originalPosition = transform.position;
    }

    private void Update()
    {
        if (gameManager.isSwitchedTurnedOn) return;
        HandleTouchInput();
    }
    private void HandleTouchInput()
    {
        if (!computerCaseManager.isCameraCloser) return;
        if (Input.touchCount == 0)
        {
            cameraMovement.holdingObject = false;
            if (removingScrewCoroutine != null)
            {
                StopCoroutine(removingScrewCoroutine);
                removingScrewCoroutine = null;
            }
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
                if (isAttached)
                    HandleTouchRemoving(touch, gameObject);
                else
                    HandleTouchRemoved(touch, gameObject);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                HandleTouchEndedDetails(touch);
                break;
        }
    }
    private void HandleTouchBegan(Touch touch)
    {
        if (cameraMovement.componentDetails) return;

        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Collider hitCollider = hit.collider;
            if (hitCollider.gameObject == this.gameObject)
            {
                selectedObject = hitCollider.gameObject;
                outline.enabled = true;
                outline.eraseRenderer = false;
                cameraMovement.holdingObject = true;
                isDragging = true;
                isHolding = true;
                if (!isAttached && currentRemovedPosition != null)
                {
                    currentRemovedPosition.isOccupied = false;
                }
                return;
            }
        }
    }



    private void HandleTouchRemoving(Touch touch, GameObject obj)
    {
        if (cameraMovement.componentDetails) return;
        if (removingScrewCoroutine == null && obj == selectedObject && isHolding)
        {
            removingScrewCoroutine = StartCoroutine(RemovingScrew(touch));
        }
        Ray ray = Camera.main.ScreenPointToRay(touch.position);

        if (isHolding && RaycastHitObject(ray, out RaycastHit hit) && hit.collider.gameObject == selectedObject)
        {
            removingScrew = true;
        }
        else
        {
            outline.enabled = false;
            outline.eraseRenderer = true;
            isHolding = false;
            removingScrew = false;
            return;
        }
    }
    private void HandleTouchRemoved(Touch touch, GameObject thisScrew)
    {
        if (cameraMovement.componentDetails) return;
        if (isDragging && selectedObject == thisScrew)
        {
            isScrewRemoved = true;
            if(currentAttachedScrew != null)
            {
                currentAttachedScrew.isScrewAttached = false;
                currentAttachedScrew.col.enabled = true;
                currentAttachedScrew = null;
            }
            selectedObject.transform.SetParent(null);
            touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, distanceFromCamera));
            transform.position = Vector3.Lerp(transform.position, touchPosition, followSpeed * Time.deltaTime);

            Vector3 direction = touchPosition - transform.position;
            direction.x = -direction.x;
            direction.y = -direction.y;

            if (direction.magnitude > 0.01f) // Threshold to avoid jitter when stationary
            {

                // Compute the target rotation based on movement direction
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Apply tilt to the target rotation
                Quaternion tiltRotation = Quaternion.Euler(direction.normalized * 40);
                targetRotation = originalRotation * tiltRotation;

                // mooothly interpolate towards the target rotation
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, cameraMovement.objectRotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, 5 * Time.deltaTime);
            }
            removingScrew = false;
        }
    }

    private IEnumerator RemovingScrew(Touch touch)
    {
        float timer = 0;
        float duration = 1f;

        // Store the original position and calculate the target position with the offset
        Vector3 originalPosition = screwObject.transform.position;
        Vector3 targetPosition = originalPosition + screwObject.transform.right * offsetValue;
        Quaternion startRotation = screwObject.transform.rotation;
        float startXRotation = startRotation.eulerAngles.x;
        float endXRotation = startXRotation - 360; // Adjust this as needed

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            // Interpolate position and rotation
            screwObject.transform.position = Vector3.Lerp(originalPosition, targetPosition, t);
            float newXRotation = Mathf.Lerp(startXRotation, endXRotation, t);
            Quaternion newRotation = Quaternion.Euler(newXRotation, startRotation.eulerAngles.y, startRotation.eulerAngles.z);
            screwObject.transform.rotation = newRotation;

            yield return null;

            // Check if holding or if touch has ended
            if (!isHolding || touch.phase == TouchPhase.Ended)
            {
                removingScrewCoroutine = null;
                screwObject.transform.position = originalPosition; // Reset to original position
                screwObject.transform.rotation = currentAttachedScrew.transform.rotation; // Reset to original rotation
                yield break;
            }
        }
        screwObject.transform.position = originalPosition; // Reset to original position
        screwObject.transform.rotation = currentAttachedScrew.transform.rotation; // Reset to original rotation
        isAttached = false;
        removingScrewCoroutine = null;
    }



    private void HandleTouchEndedDetails(Touch touch)
    {
        if (!isDragging) return;
        if (isAttached)
        {
            isDragging = false;
            isHolding = false;
            selectedObject = null;
            outline.enabled = false;
            outline.eraseRenderer = true;
        }
        else
        {
            HandleTouchEnded(touch);
        }
    }

    private void HandleTouchEnded(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetLayer) && computerCaseManager.isGlassAttached)
        {
            selectedObject.transform.SetParent(hit.transform.parent);
            HandleRaycastHit(hit);
            isScrewRemoved = false;
        }
        else
        {
            TryMoveToUnoccupiedPosition();
        }

        // Resetting states
        isDragging = false;
        isHolding = false;
        selectedObject = null;
        outline.enabled = false;
        outline.eraseRenderer = true;
    }

    private void HandleRaycastHit(RaycastHit hit)
    {
        currentAttachedScrew = hit.collider.gameObject.GetComponent<RemovedScrewPosition>();

        if (currentAttachedScrew != null && !currentAttachedScrew.isScrewAttached)
        {
            AttachScrew(currentAttachedScrew);
        }
        else
        {
            TryMoveToUnoccupiedPosition();
            if(currentAttachedScrew != null)
            {
                currentAttachedScrew.col.enabled = true;
            }
        }
    }

    private void AttachScrew(RemovedScrewPosition screwPosition)
    {
        isAttached = true;
        screwPosition.isScrewAttached = true;
        currentAttachedScrew.col.enabled = false;
        StartCoroutine(MoveToOldPosition());
    }

    private void TryMoveToUnoccupiedPosition()
    {
        currentRemovedPosition = CheckForUnoccupiedPosition();
        if (currentRemovedPosition != null)
        {
            currentRemovedPosition.isOccupied = true;
            StartCoroutine(MoveToRemovedPosition(currentRemovedPosition.transform));
        }
    }
    private RemovedScrewPosition CheckForUnoccupiedPosition()
    {
        foreach (RemovedScrewPosition pos in removedScrewPosition)
        {
            if (!pos.isOccupied) // If position is not occupied
            {
                currentRemovedPosition = pos;
                return currentRemovedPosition; // Return the first available position
            }
        }
        return null; // No available position found
    }
    private IEnumerator MoveToOldPosition()
    {
        float timer = 0;
        float duration = 1;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            this.transform.position = Vector3.Lerp(this.transform.position, currentAttachedScrew.transform.position, timer / duration);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, currentAttachedScrew.transform.rotation, timer / duration);
            yield return null;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null && hit.collider.gameObject == this.gameObject)
                    {
                        yield break;
                    }
                }
            }
        }
    }

    private IEnumerator MoveToRemovedPosition(Transform UnOccupiedPosition)
    {
        float timer = 0;
        float duration = 1;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            this.transform.position = Vector3.Lerp(this.transform.position, UnOccupiedPosition.position, timer / duration);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, UnOccupiedPosition.rotation, timer / duration);
            yield return null;
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null && hit.collider.gameObject == this.gameObject)
                    {
                        yield break;
                    }
                }
            }
        }
    }
    private bool RaycastHitObject(Ray ray, out RaycastHit hit)
    {
        LayerMask thisLayer = this.gameObject.layer;
        return Physics.Raycast(ray, out hit) && hit.collider != null;
    }
}