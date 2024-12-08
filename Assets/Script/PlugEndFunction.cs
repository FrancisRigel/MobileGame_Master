using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlugEndFunction : MonoBehaviour
{
    public CameraMovement cameraMovement;
    public ComputerPlugFunction computerPlugFunction;
    public GameManager gameManager;
    public Transform line;
    [Header("Checkers")]
    public float distanceFromCamera = 1.5f;
    public float followSpeed = 10;
    public bool isDragging;
    public bool isAttached;
    public LayerMask targetLayer;
    private cakeslice.Outline outline;
    private Vector3 touchPosition;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    [HideInInspector] public Coroutine coroutine;
    Collider col;
    Collider targetCollider;
    private void Awake()
    {
        col = GetComponent<Collider>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void Update()
    {
        if (cameraMovement.isSettingsOn) return;
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
        if (hits.Length > 0)
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
                isAttached = false;
                cameraMovement.selectedObject = gameObject;
                outline = gameObject.GetComponent<cakeslice.Outline>();
                if (outline == null)
                {
                    outline = gameObject.AddComponent<cakeslice.Outline>();
                }
                col.enabled = false;

                if(targetCollider != null)
                {
                    targetCollider.enabled = true;
                }
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
            bool hitZone = Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer);
          
            if (hitZone && hit.collider != null)
            {
                SocketFunction socket = hit.transform.gameObject.GetComponent<SocketFunction>();
                transform.SetParent(socket.transform.parent);
                isDragging = false;
                StopAllCoroutines();
                coroutine = StartCoroutine(MoveToNewPosition(socket));
                isAttached = true;
                targetCollider = hit.collider;
                if(targetCollider != null)
                {
                    targetCollider.enabled = false;
                }

                if (!gameManager.plugFunctions.Contains(this))
                {
                    gameManager.plugFunctions.Add(this);
                }
            }
            else
            {
                transform.SetParent(null);
                isDragging = false;
                StopAllCoroutines();
                coroutine = StartCoroutine(MoveToOldPosition());
                isAttached = false;

                if (gameManager.plugFunctions.Contains(this))
                {
                    gameManager.plugFunctions.Remove(this);
                }
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
            if (cameraMovement.selectedObject == gameObject)
            {
                cameraMovement.selectedObject = null;
            }
        }
    }
    private IEnumerator MoveToNewPosition(SocketFunction socket)
    {
        float timer = 0;
        float duration = .5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, socket.insertingTransform.position, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, socket.insertingTransform.rotation, timer / duration);
            yield return null;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject == gameObject && hit.collider != null && isDragging)
                    {
                        gameObject.transform.SetParent(null);
                        yield break;
                    }
                }
            }
        }

        timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, socket.insertedTransform.position, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, socket.insertedTransform.rotation, timer / duration);
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
