using cakeslice;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class PrimeComponentManager : MonoBehaviour
{
    CameraMovement cameraMovement;
    GameManager gameManager;
    [Header("Checkers")]
    public bool isDragging;
    public bool inNewPosition;
    public bool isMonitor = false;

    [Header("Settings")]
    public float distanceFromCamera = 5f;
    private Vector3 touchPosition;
    public float followSpeed = 10;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private float touchTime;
    public LayerMask targetLayer;
    public Transform newPosition;
    public GameObject monitorStand;
    [Header("Details Of The Component")]
    public bool rotatingByTouch;
    public bool autoRotating;
    private float objectTimerRotation = 0;
    private Vector3 rotationAxis;
    private Button button;
    private float objectTimer;
    Collider col;
    Vector3 previousTouchPosition;

    [Header("Item Description")]
    [TextArea(3, 10)]
    public string descriptionText;
    public float textSize;

    [Header("Wire")]
    public GameObject wire;
    public GameObject usb;
    public Transform startWire;
    public Transform usbPos;
    private GameObject wirePrefab;
    private GameObject usbPrefab;
    [Header("Monitor Only")]
    public GameObject plugEndPrefab;
    private GameObject plugEnd;
    public GameObject hdmiprefab;
    private GameObject hdmi;
    public Material monitorScreen;
    [Header("Outline Host")]
    public GameObject mesh;
    private cakeslice.Outline outline;
    private void Awake()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        button = GameObject.Find("Button Component Details").GetComponent<Button>();
        col = GetComponent<Collider>();
        originalRotation = transform.rotation;
        originalPosition = transform.position;
        if(gameObject.layer == LayerMask.NameToLayer("Keyboard"))
        {
            newPosition = GameObject.Find("Keyboard New Position").transform;
            usbPos = GameObject.Find("USB1").transform;
            gameManager.primeObjects.Add(gameObject);
            return;
        }
        else if(gameObject.layer == LayerMask.NameToLayer("Monitor"))
        {
            newPosition = GameObject.Find("Monitor New Position").transform;
            Vector3 pos = new Vector3(0, 180, 0);
            transform.rotation = Quaternion.Euler(pos);
            usbPos = GameObject.Find("USB2").transform;
            Instantiate(monitorStand);
            isMonitor = true;
            gameManager.primeObjects.Add(gameObject);
            monitorScreen.color = Color.black;
            gameManager.monitorMaterial = monitorScreen;
            return;
        }
        else if(gameObject.layer == LayerMask.NameToLayer("Mouse"))
        {
            newPosition = GameObject.Find("Mouse New Position").transform;
            usbPos = GameObject.Find("USB3").transform;
            gameManager.primeObjects.Add(gameObject);
            return;
        }
    }

    private void Update()
    {
        if (cameraMovement.caseDetailsAnimation || cameraMovement.isSettingsOn) return;
        HandleTouchInput();

        if (autoRotating && cameraMovement.componentDetails)
        {
            RotateAutomatically();
        }
    }

    private void RopeHandler()
    {

        if (isMonitor && wirePrefab == null)
        {
            GameObject w = Instantiate(wire);
            var wScript = w.GetComponent<VerletRope>();
            plugEnd = Instantiate(plugEndPrefab);
            var plugScript = plugEnd.GetComponent<PlugEndFunction>();
            wScript.startLine = startWire;
            wScript.ropeEndPoint = plugScript.line;

            wirePrefab = Instantiate(wire);
            hdmi = Instantiate(hdmiprefab);
            var a = wirePrefab.GetComponent<VerletRope>();
            var b = hdmi.GetComponent<SocketFunction>();
            a.startLine = startWire;
            a.ropeEndPoint = b.endlineMonitor;
        }
        else
        {
            if (wirePrefab == null)
            {
                wirePrefab = Instantiate(wire);
                usbPrefab = Instantiate(usb);
                usbPrefab.transform.position = usbPos.position;
                var a = wirePrefab.GetComponent<VerletRope>();
                var b = usbPrefab.GetComponentInChildren<SocketFunction>();
                a.startLine = startWire;
                a.ropeEndPoint = b.endlineUSB;
            }
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
                    HandleTouchEnded(touch);
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


    private void RaycastHandler(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)&& hit.collider.CompareTag("Prime") && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            cameraMovement.holdingObject = true;
            cameraMovement.selectedObject = gameObject;
            touchTime = Time.time;
            gameManager.isPrimeDragging = true;
            if (mesh != null)
            {
                outline = mesh.AddComponent<cakeslice.Outline>();
            }
            else
            {
                if(outline == null)
                {
                    outline = gameObject.AddComponent<cakeslice.Outline>();
                }
            }
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        if (cameraMovement.holdingObject || cameraMovement.isSettingsOn) return;
        RaycastHandler(touch);

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
            if (Time.time - touchTime < 0.2f)
            {
                ActivateComponentDetails();
                isDragging = false;
            }
            else
            {
                CheckAttachmentOrReturn(touch);
            }

            if(mesh != null)
            {
                Destroy(outline);
            }
            else
            {
                Destroy(outline);
            }
        }
    }

    private void CheckAttachmentOrReturn(Touch touch)
    {
        if (isDragging)
        {
            Ray dragRay = Camera.main.ScreenPointToRay(touch.position);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;
            bool hitZone = Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer);
            if (hitZone && hit.collider != null)
            {
                StopAllCoroutines();
                inNewPosition = true;
                StartCoroutine(MoveToNewPosition());
                RopeHandler();
            }
            else
            {
                StartCoroutine(MoveToOldPosition());
            }
        }
        gameManager.isPrimeDragging = false;
        cameraMovement.selectedObject = null;
        isDragging = false;
        cameraMovement.holdingObject = false;
    }

    private IEnumerator MoveToNewPosition()
    {
        float timer = 0;
        float duration = .5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, newPosition.position, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, newPosition.rotation, timer / duration);
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

    private void ActivateComponentDetails()
    {
        cameraMovement.componentDetails = true;
        rotationAxis = GetRandomDirection();
        cameraMovement.MoveDetailsStart();
        cameraMovement.smallArrow.gameObject.SetActive(true);
        cameraMovement.smallArrow.onClick.AddListener(cameraMovement.SmallArrowFunctionStart);
        StartCoroutine(MoveToCenter());
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
        if (inNewPosition)
        {
            StartCoroutine(MoveToNewPosition());
        }
        else
        {
            StartCoroutine(MoveToOldPosition());
        }
        cameraMovement.selectedObject = null;
    }

    private Vector3 GetRandomDirection()
    {
        float x = Random.Range(-1000, 1000);
        float y = Random.Range(-1000, 1000);
        float z = Random.Range(-1000, 1000);

        return new Vector3(x, y, z).normalized;
    }
}
