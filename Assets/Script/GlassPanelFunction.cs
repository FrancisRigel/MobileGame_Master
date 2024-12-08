using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassPanelFunction : MonoBehaviour
{
    public CameraMovement cameraMovement;
    public ComputerCaseManager computerCaseManager;
    public bool isDragging;
    public bool isAttached = true;
    public float distanceFromCamera = 1.5f;
    public float followSpeed = 10;
    public LayerMask layerMask;
    private Vector3 touchPosition;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private cakeslice.Outline outline;
    Collider col;
    private void Awake()
    {
        col = GetComponent<Collider>();
        computerCaseManager =GameObject.Find("Case").GetComponent<ComputerCaseManager>();
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        originalRotation = transform.rotation;
        originalPosition = transform.position;
        layerMask = LayerMask.GetMask("Case");
    }

    private void Update()
    {
        HandleTouchInput();
    }


    private void HandleTouchInput()
    {
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
                HandleTouchEnded(touch);
                break;
        }
    }


    private void HandleTouchBegan(Touch touch)
    {
        if (cameraMovement.componentDetails) return;
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        if (hits.Length > 0)
        {
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
            if(selectedObject != gameObject) return;
            if(selectedObject == gameObject)
            {
                isDragging = true;
                cameraMovement.holdingObject = true;
                gameObject.transform.SetParent(null);

                outline = gameObject.GetComponent<cakeslice.Outline>();
                if (outline == null)
                {
                    outline = gameObject.AddComponent<cakeslice.Outline>();
                }
                computerCaseManager.isGlassAttached = false;
                computerCaseManager.colliders = null;
                outline.enabled = true;
                outline.eraseRenderer = false;
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
        if(isDragging)
        {
            computerCaseManager.isGlassAnimating = true;
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;
            bool hitZone = Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
            if (hitZone && hit.collider != null)
            {
                transform.SetParent(computerCaseManager.gameObject.transform);
                isDragging = false;
                cameraMovement.holdingObject = false;
                StartCoroutine(MoveToCase());
                col.enabled = true;
                isAttached = true;
            }
            else
            {
                StartCoroutine(MoveToRemovedPosition());
                isDragging = false;
                cameraMovement.holdingObject = false;
                col.enabled = true;
                isAttached = false;
            }
            if (outline != null)
            {
                outline.enabled = false;
                outline.eraseRenderer = true;
                outline = gameObject.GetComponent<cakeslice.Outline>();
                Destroy(outline);
            }
            cameraMovement.holdingObject = false;
        }
    }


    private IEnumerator MoveToRemovedPosition()
    {
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, computerCaseManager.glassPanelRemovedPosition.position, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, computerCaseManager.glassPanelRemovedPosition.rotation, timer / duration);
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
                        transform.SetParent(null);
                        yield break;
                    }
                }
            }
        }
        computerCaseManager.isGlassAnimating = false;
    }

    private IEnumerator MoveToCase()
    {
        float timer = 0;
        float duration = 0.5f;
        while(timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, computerCaseManager.glassPanelRemovingPosition.position, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, computerCaseManager.glassPanelRemovingPosition.rotation, timer / duration);
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
                        transform.SetParent(null);
                        yield break;
                    }
                }
            }
        }

        timer = 0;
        while(timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, computerCaseManager.glasPanelOrginalPosition.position, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, computerCaseManager.glasPanelOrginalPosition.rotation, timer / duration);
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
                        transform.SetParent(null);
                        yield break;
                    }
                }
            }
        }
        computerCaseManager.isGlassAttached = true;
        computerCaseManager.isGlassAnimating = false;
    }
}
