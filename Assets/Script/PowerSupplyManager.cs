using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerSupplyManager : MonoBehaviour
{
    public CameraMovement cameraMovement;
    public ComputerPlugFunction computerPlugFunction;
    public ComputerCaseManager caseManager;
    public GameManager gameManager;
    public cakeslice.Outline outline;
    public float distanceFromCamera;
    public string targetTag;
    public LayerMask targetLayer;
    public Button button;
    public Transform newPosition1, newPosition2;
    public Vector3 offsetPosition;
    public bool isAttached;
    private bool isHolding;
    private Quaternion originalRotation;
    float touchTime;
    private Vector3 rotationAxis;
    Collider col;
    private float objectTimerRotation = 0;
    private bool autoRotating;
    private float objectTimer;
    Vector3 oldPosition;
    Quaternion oldRotation;
    Vector3 previousTouchPosition;
    private bool rotatingByTouch;
    SoundManager soundManager;

    [Header("Item Description")]
    [TextArea(3, 10)]
    public string descriptionText;
    public float textSize;
    private void Start()
    {
        soundManager = GameObject.Find("Game Manager").GetComponent<SoundManager>();
        oldPosition = transform.position;
        oldRotation = transform.rotation;
        col = GetComponent<Collider>();
        originalRotation = transform.rotation;
    }

    private void Update()
    {
        if (cameraMovement.caseDetailsAnimation || cameraMovement.isSettingsOn || gameManager.turnOnPerfect) return;
        HandleTouchInput();


        if (autoRotating && cameraMovement.componentDetails)
        {
            RotateAutomatically(gameObject);
        }
    }
    void HandleTouchInput()
    {
        if(Input.touchCount == 0)
        {
            cameraMovement.holdingObject = false;
            return;
        }

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
                    HandleTouchEndedDetails(touch);
                else 
                    HandleTouchEnded(touch);
                break;
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        if (cameraMovement.componentDetails || caseManager.isDragging && cameraMovement.selectedObject != gameObject) return;

        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        bool hitPSU = Physics.Raycast(ray, out hit, Mathf.Infinity);

        if (hitPSU && hit.collider != null && hit.collider.CompareTag("PSU"))
        {
            isAttached = false;
            isHolding = true;
            touchTime = Time.time;
            cameraMovement.selectedObject = gameObject;
            cameraMovement.holdingObject = true;
            soundManager.SelectedObjectSFX(1);
            if (computerPlugFunction.isAttached && !computerPlugFunction.isDragging)
            {
                computerPlugFunction.transform.SetParent(null);
                computerPlugFunction.isAttached = false;
                computerPlugFunction.StopAllCoroutines();
                computerPlugFunction.coroutine = StartCoroutine(computerPlugFunction.MoveToOldPosition());
            }
        }
    }

    private void HandleTouchMoved(Touch touch)
    {
        if (cameraMovement.componentDetails) return;
        if (isHolding)
        {
            transform.SetParent(null);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, distanceFromCamera));
            Vector3 pos = touchPosition + offsetPosition;
            transform.position = Vector3.Lerp(transform.position, pos, 10 * Time.deltaTime);
            CheckIfTargetHit(touch);
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

                // Smoothly interpolate towards the target rotation
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
        if (isHolding)
        {
            if (Time.time - touchTime < 0.2f)
            {
                ActivateComponentDetails();
            }
            else
            {
                CheckAttachmentOrReturn(touch);
            }
            SetAlpha(gameObject, 1f);
            OutlineHandler(true);
        }
        cameraMovement.holdingObject = false;
    }

    private void HandleTouchBeganDetails(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) )
        {
            if(hit.collider == this.col && cameraMovement.selectedObject == gameObject)
            {
                previousTouchPosition = touch.position;
                rotatingByTouch = true;

                if (computerPlugFunction.isAttached && !computerPlugFunction.isDragging)
                {
                    computerPlugFunction.transform.SetParent(null);
                    computerPlugFunction.isAttached = false;
                    computerPlugFunction.StopAllCoroutines();
                    computerPlugFunction.coroutine = StartCoroutine(computerPlugFunction.MoveToOldPosition());
                }
            }
            else
            {
                rotatingByTouch = false;
            }
        }
        isAttached = false;
    }

    private void HandleTouchMovedDetails(Touch touch)
    {
        if(cameraMovement.componentDetails && rotatingByTouch)
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

    private void HandleTouchEndedDetails(Touch touch)
    {
        if(cameraMovement.componentDetails && rotatingByTouch)
        {
            objectTimer = 2;
            autoRotating = true;
            objectTimerRotation = 0;
            rotatingByTouch = false;
        }
    }
    private void CheckIfTargetHit(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            OutlineHandler(true);
            SetAlpha(gameObject, 0.2f);
            transform.SetParent(newPosition1);
        }
        else
        {
            transform.SetParent(null);
            SetAlpha(gameObject, 1f);
            OutlineHandler(false);
        }
    }

    private void OutlineHandler(bool enabler)
    {
        if (enabler)
        {
            outline.enabled = false;
            outline.eraseRenderer = true;
        }
        else
        {
            outline.enabled = true;
            outline.eraseRenderer = false;
        }
    }

    private void ActivateComponentDetails()
    {
        transform.SetParent(null);
        cameraMovement.componentDetails = true;
        rotationAxis = GetRandomDirection();
        cameraMovement.MoveDetailsStart();
        StartCoroutine(MoveToCenter());
        col.enabled = false;

    }
    private Vector3 GetRandomDirection()
    {
        float x = Random.Range(-1000, 1000);
        float y = Random.Range(-1000, 1000);
        float z = Random.Range(-1000, 1000);

        return new Vector3(x, y, z).normalized;
    }
    public IEnumerator MoveToCenter()
    {
        cameraMovement.smallArrow.gameObject.SetActive(true);
        cameraMovement.smallArrow.onClick.AddListener(cameraMovement.SmallArrowFunctionStart);
        StartCoroutine(cameraMovement.ButtonMoveScreen(cameraMovement.buttonBackPosition.position, !cameraMovement.componentDetails));
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, distanceFromCamera);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenCenter);
        Vector3 pos = worldPosition + offsetPosition;
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, pos, timer / duration);
            yield return null;
        }
        button.interactable = true;
        button.onClick.AddListener(ButtonBack);
        objectTimer = 1;
        autoRotating = true;
        col.enabled = true;
    }

    public void ButtonBack()
    {
        objectTimerRotation = 0;
        autoRotating = false;
        button.onClick.RemoveAllListeners();
        button.interactable = false;
        cameraMovement.componentDetails = false;
        cameraMovement.MoveDetailsEnd();
        StartCoroutine(cameraMovement.ButtonMoveScreen(cameraMovement.oldButtonBackPosition, cameraMovement.componentDetails));
        cameraMovement.selectedObject = null;


    }

    private void CheckAttachmentOrReturn(Touch touch)
    {
        if (isHolding)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;
            bool hitNewPosition = Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer);

            if (hitNewPosition && hit.collider != null && hit.collider.CompareTag(targetTag))
            {
                StartCoroutine(NewPosition());
                cameraMovement.selectedObject = null;
            }
            else
            {
                StartCoroutine(MoveBackToOriginalPos());
                cameraMovement.selectedObject = null;
                isAttached = false;
                soundManager.SelectedObjectSFX(2);
            }
        }
        isHolding = false;
    }

    private IEnumerator NewPosition()
    {
        float timer = 0;
        float duration = 0.5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, newPosition1.transform.position, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, newPosition1.transform.rotation, timer / duration);
            yield return null;

            // Check for touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit[] hits = Physics.RaycastAll(ray);

                // Check if the specific object is hit
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != null && cameraMovement.selectedObject == this.gameObject  && isHolding)
                    {
                        // Break the coroutine if the object's collider is touched
                        yield break;
                    }
                }
            }
        }

        timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, newPosition2.transform.position, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, newPosition2.transform.rotation, timer / duration);
            yield return null;

            // Check for touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit[] hits = Physics.RaycastAll(ray);

                // Check if the specific object is hit
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != null && cameraMovement.selectedObject == this.gameObject && isHolding)
                    {
                        // Break the coroutine if the object's collider is touched
                        yield break;
                    }
                }
            }
        }
        isAttached = true;
    }

    private IEnumerator MoveBackToOriginalPos()
    {
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            this.transform.position = Vector3.Lerp(this.transform.position, oldPosition, timer / duration);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, oldRotation, timer / duration);
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
                        // Break the coroutine if the object's collider is touched
                        yield break;
                    }
                }
            }
        }
    }

    private void RotateAutomatically(GameObject thisObject)
    {
        if (cameraMovement.selectedObject == thisObject)
        {
            thisObject.transform.Rotate(rotationAxis, cameraMovement.objectRotationSpeed * 4 * Time.deltaTime, Space.World);
            objectTimerRotation += Time.deltaTime;
            if (objectTimerRotation >= objectTimer)
            {
                rotationAxis = GetRandomDirection();
                objectTimerRotation = 0;
            }
        }
    }
    void SetAlpha(GameObject obj, float alpha)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                Color color = mat.color;
                color.a = alpha;
                mat.color = color;

                if (alpha < 1f)
                {
                    mat.SetFloat("_Mode", 2); // Set to transparent mode
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }
                else
                {
                    mat.SetFloat("_Mode", 0); // Set to opaque mode
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = -1;
                }
            }
        }
    }
}
