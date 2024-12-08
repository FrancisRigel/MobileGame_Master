using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SyringeFunction : MonoBehaviour
{
    public CameraMovement cameraMovement;
    public ComponentManager cpu;
    public ComponentManager cpuCooler;
    public GameManager gameManager;
    public LayerMask targetLayer;
    public float distanceFromCamera = 1.5f;
    public float followSpeed = 10;
    public GameObject[] outlines;
    public Vector3 offset;
    public Button button;
    [Header("Checkers")]
    public bool isDragging;
    public bool isHitting;
    public bool autoRotating;
    public bool rotatingByTouch;

    private Vector3 touchPosition;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    Coroutine pastingAnim;
    Transform hitPos;

    public Animator paste;
    public Animator pasteHolder;


    public GameObject thermalPaste;
    public Vector3 thermalPasteOffset;
    private Vector3 thermalPasteOriginalSize;
    private Vector3 thermalPasteSize = Vector3.zero;
    public Vector3 pastePositionOffset;
    private GameObject tm;
    public float pasteTimer;
    Coroutine pasting;
    float touchTime;
    private Vector3 rotationAxis;
    Collider col;
    private float objectTimer;
    private float objectTimerRotation = 0;
    Vector3 previousTouchPosition;
    public Collider targetCollider;
    [Header("Item Description")]
    [TextArea(3, 10)]
    public string descriptionText;
    public float textSize;

    private void Start()
    {
        col = GetComponent<Collider>();
        thermalPasteOriginalSize = thermalPaste.transform.localScale;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    private void Update()
    {
        if (cameraMovement.isSettingsOn || gameManager.isSwitchedTurnedOn) return;
        HandleTouchInput();
    }
    private void LateUpdate()
    {
        if (gameManager.isSwitchedTurnedOn) return;
        Tite();

        if (autoRotating && cameraMovement.componentDetails)
        {
            RotateAutomatically();
        }
    }

    private void RotateAutomatically()
    {
        transform.Rotate(rotationAxis, cameraMovement.objectRotationSpeed * 4 * Time.deltaTime, Space.World);
        objectTimerRotation += Time.deltaTime;
        if (objectTimerRotation >= objectTimer)
        {
            rotationAxis = GetRandomDirection();
            objectTimerRotation = 0;
        }
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

        bool isComponentDetails = cameraMovement.componentDetails;

        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (isComponentDetails)
                    HandleTouchBeganDetails(touch);
                else
                    HandleTouchBegan(touch);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (isComponentDetails)
                    HandleTouchMovedDetails(touch);
                else
                    HandleTouchMoved(touch);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isComponentDetails)
                    HandleTouchEndedDetails();
                else
                    HandleTouchEnded();
                break;
        }
    }

    private void HandleTouchBeganDetails(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            if (hitInfo.collider == col && hitInfo.collider != null)
            {
                previousTouchPosition = touch.position;
                rotatingByTouch = true;
                autoRotating = false;
            }
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
                cameraMovement.selectedObject = gameObject;
                touchTime = Time.time;
                foreach (GameObject obj in outlines)
                {
                    cakeslice.Outline outline = obj.GetComponent<cakeslice.Outline>();
                    if(outline == null)
                    {
                        outline = obj.AddComponent<cakeslice.Outline>();
                    }
                }

                if(cpu.isAttached && !targetCollider.enabled)
                {
                    targetCollider.enabled = true;
                }
                return;
            }
        }
    }
    private void HandleTouchMovedDetails(Touch touch)
    {
        if (cameraMovement.componentDetails && rotatingByTouch)
        {
            autoRotating = false;
            objectTimerRotation = 0;
            rotationAxis = Vector3.zero;

            Vector3 currentTouchPosition = touch.position;
            Vector3 touchDelta = currentTouchPosition - previousTouchPosition;

            if (touchDelta.magnitude > 10)
            {
                float rotationX = touchDelta.y * cameraMovement.objectRotationSpeed * Time.deltaTime;
                float rotationY = touchDelta.x * cameraMovement.objectRotationSpeed * Time.deltaTime;

                if (cameraMovement.invertedTouch)
                {
                    rotationX = -rotationX;
                    rotationY = -rotationY;
                }

                transform.Rotate(Vector3.up, rotationY, Space.World);
                transform.Rotate(Vector3.right, rotationX, Space.World);

                previousTouchPosition = currentTouchPosition;
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
            HandleDetection(touch);
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
    private void HandleTouchEndedDetails()
    {
        if (cameraMovement.componentDetails && rotatingByTouch)
        {
            objectTimer = 2;
            autoRotating = true;
            objectTimerRotation = 0;
            rotatingByTouch = false;
        }
    }
    private void HandleTouchEnded()
    {
        if (isDragging)
        {
            if (Time.time - touchTime < 0.2f)
            {
                transform.SetParent(null);
                isDragging = false;
                StopAllCoroutines();
                if(pasting!= null)
                {
                    StopCoroutine(pasting);
                }

                ActivateComponentDetails();
            }
            else
            {
                transform.SetParent(null);
                isDragging = false;
                StopAllCoroutines();
                if (pasting != null)
                {
                    StopCoroutine(pasting);
                }
                StartCoroutine(MoveToOldPosition());
                cameraMovement.selectedObject = null;
            }
            if (outlines != null)
            {
                foreach (GameObject outline in outlines)
                {
                    var o = outline.GetComponent<cakeslice.Outline>();
                    Destroy(o);
                }
            }
            if (cpu.isAttached && targetCollider.enabled)
            {
                targetCollider.enabled = false;
            }
        }
    }

    private void ActivateComponentDetails()
    {
        cameraMovement.componentDetails = true;
        rotationAxis = GetRandomDirection();
        cameraMovement.MoveDetailsStart();
        cameraMovement.smallArrow.gameObject.SetActive(true);
        cameraMovement.smallArrow.onClick.AddListener(cameraMovement.SmallArrowFunctionStart);
        StartCoroutine(MoveToCenter());
        col.enabled = false;
    }
    private void HandleDetection(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        bool hitZone = Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer);
        if(hitZone && hit.collider != null && hit.collider.name == "CPU New Position" && cpu.isAttached)
        {
            if (tm == null)
            {
                tm = Instantiate(thermalPaste, hit.transform.parent);
                Vector3 newPos = tm.transform.position + pastePositionOffset;
                tm.transform.position = newPos;
                cpu.col.enabled = false;
            }

            isHitting = true;
            hitPos = hit.transform;
            if(hitPos != null)
            {
                pastingAnim = StartCoroutine(PastingAnimation());
                pasteHolder.SetBool("Pasting", isHitting);
                paste.SetBool("IsHitting", isHitting);
                pasteHolder.speed = 1;
                paste.speed = 1;
                pasting = StartCoroutine(PastingSize());
            }

        }
        else
        {
            paste.speed = 0;
            pasteHolder.speed = 0;
            isHitting = false;
            hitPos = null;
        }
    }

    private IEnumerator PastingSize()
    {
        if (!isHitting)
        {
            yield break;
        }
        float timer = pasteTimer;
        float duration = 5f;
        Vector3 startScale = thermalPasteSize; 
        Vector3 targetScale = thermalPasteOriginalSize + thermalPasteOriginalSize; 

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            tm.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            pasteTimer = t;
            yield return null;
            if (tm == null || !isHitting)
            {
                thermalPasteSize = tm.transform.localScale;
                pasteTimer = t;
                StopCoroutine(pasting);
                yield break;
            }
        }
        thermalPasteSize = tm.transform.localScale;
    }
    public IEnumerator MoveToCenter()
    {
        button.onClick.AddListener(ButtonBack);
        StartCoroutine(cameraMovement.ButtonMoveScreen(cameraMovement.buttonBackPosition.position, !cameraMovement.componentDetails));
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, distanceFromCamera);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenCenter);
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, worldPosition, timer / duration);
            yield return null;
        }
        button.interactable = true;
        objectTimer = 1;
        autoRotating = true;
        col.enabled = true;
    }

    public void ButtonBack()
    {
        // Reset states
        objectTimerRotation = 0;
        autoRotating = false;
        rotatingByTouch = false;


        // Reset button and object states
        button.onClick.RemoveAllListeners();
        button.interactable = false;
        cameraMovement.componentDetails = false;

        // Start camera movement and details ending routines
        StartCoroutine(cameraMovement.ButtonMoveScreen(cameraMovement.oldButtonBackPosition, cameraMovement.componentDetails));
        cameraMovement.MoveDetailsEnd();
        StartCoroutine(MoveToOldPosition());
        cameraMovement.selectedObject = null;
    }

    private void Tite()
    {
        if (tm == null) return;
        if (pasteTimer > .2f)
        {
            gameManager.isPerfectlyPasted = false;
            gameManager.isPasted = true;
        }
        else if (pasteTimer >= .08f)
        {
            gameManager.isPasted = false;
            gameManager.isPerfectlyPasted = true;
        }
    }
    private IEnumerator MoveToOldPosition()
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
                        yield break;
                    }
                }
            }
        }
    }
    public Vector3 newRotation;
    private IEnumerator PastingAnimation()
    {
        float timer = 0;
        float duration = .5f;
        var r = Quaternion.Euler(newRotation);
        Vector3 offSet = hitPos.position + offset;
        while(timer < duration && isHitting)
        {
            timer += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, r, timer / duration);
            transform.position = Vector3.Lerp(transform.position, offSet, timer / duration);
            yield return null;

            if (!isHitting && hitPos == null)
            {
                yield break;
            }
        }
        pastingAnim = null;
    }

    private Vector3 GetRandomDirection()
    {
        float x = Random.Range(-1000, 1000);
        float y = Random.Range(-1000, 1000);
        float z = Random.Range(-1000, 1000);

        return new Vector3(x, y, z).normalized;
    }
}
