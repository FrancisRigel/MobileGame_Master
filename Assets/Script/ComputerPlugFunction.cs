using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ComputerPlugFunction : MonoBehaviour
{
    public CameraMovement cameraMovement;
    public PowerSupplyManager powerSupplyManager;
    public GameManager gameManager;
    public Rope rope;

    [Header("Checkers")]
    public bool isDragging;
    public bool isAttached;
    public float distanceFromCamera = 1.5f;
    public float followSpeed = 10;
    public LayerMask layerMask;
    public Transform insertingPlugPosition;
    public Transform insertedPlugPosition;
    private cakeslice.Outline outline;
    Collider col;
    private Vector3 touchPosition;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    [HideInInspector]public Coroutine coroutine;
    private void Start()
    {
        col = GetComponent<Collider>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    private void Update()
    {
        if (cameraMovement.isSettingsOn || gameManager.isSwitchedTurnedOn) return;
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
                HandleTouchEnded(touch);
                break;
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        if (cameraMovement.componentDetails && cameraMovement.selectedObject != gameObject) return;
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        if(hits.Length > 0)
        {
            int highestPriority = int.MinValue;
            GameObject selectedObject = null;
            foreach (var hit in hits)
            {
                Collider hitCollider = hit.collider;
                int priority = hitCollider.layerOverridePriority;
                if (priority > highestPriority)
                {
                    highestPriority = priority;
                    selectedObject = hitCollider.gameObject;
                }
            }

            if (selectedObject != gameObject) return;
            if (selectedObject == gameObject)
            {
                isDragging = true;
                cameraMovement.holdingObject = true;
                gameObject.transform.SetParent(null);
                cameraMovement.selectedObject = gameObject;
                outline = gameObject.GetComponent<cakeslice.Outline>();
                if (outline == null)
                {
                    outline = gameObject.AddComponent<cakeslice.Outline>();
                }
                col.enabled = false;
                isAttached = false;
                return;
            }
        }
    }

    private void HandleTouchMoved(Touch touch)
    {
        if (cameraMovement.componentDetails) return;
        if (isDragging)
        {
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
        }
    }

    private void HandleTouchEnded(Touch touch)
    {
        if (isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;
            bool hitZone = Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
            if(hitZone && hit.collider != null && powerSupplyManager.isAttached)
            {
                transform.SetParent(insertedPlugPosition.transform.parent);
                isDragging = false;
                StopAllCoroutines();
                coroutine = StartCoroutine(MoveToNewPosition());
                isAttached = true;
            }
            else
            {
                transform.SetParent(null);
                isDragging = false;
                StopAllCoroutines();
                coroutine = StartCoroutine(MoveToOldPosition());
                isAttached = false;
            }

            if (outline != null)
            {
                outline.enabled = false;
                outline.eraseRenderer = true;
                outline = gameObject.GetComponent<cakeslice.Outline>();
                Destroy(outline);
            }
            col.enabled = true;
            cameraMovement.holdingObject = false;
            cameraMovement.selectedObject = null;
        }
    }

    private IEnumerator MoveToNewPosition()
    {
        float timer = 0;
        float duration = .5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, insertingPlugPosition.position, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, insertingPlugPosition.rotation, timer / duration);
            yield return null;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject == gameObject && hit.collider != null)
                    {
                        gameObject.transform.SetParent(null);
                        yield break;
                    }
                }
            }
        }

        timer = 0;
        while(timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, insertedPlugPosition.position, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, insertedPlugPosition.rotation, timer / duration);
            yield return null;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit[] hits = Physics.RaycastAll(ray);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != null && cameraMovement.selectedObject == gameObject && isDragging)
                    {
                        StopAllCoroutines();
                        transform.SetParent(null);
                        yield break;
                    }
                }
            }
        }
    }

    public IEnumerator MoveToOldPosition()
    {
        float timer = 0;
        float duration = 0.5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, originalPosition, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, timer / duration);
            yield return null;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit[] hits = Physics.RaycastAll(ray);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != null && cameraMovement.selectedObject == gameObject && isDragging)
                    {
                        StopAllCoroutines();
                        transform.SetParent(null);
                        yield break;
                    }
                }
            }
        }
    }
}
