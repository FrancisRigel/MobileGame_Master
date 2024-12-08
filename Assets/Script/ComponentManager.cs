using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentManager : MonoBehaviour
{
    public bool isAttached = false;
    private CameraMovement cameraMovement;
    public Camera mainCamera;
    public float distanceFromCamera = 5f;
    public GameObject newPosition1;
    public Transform newPosition2;
    private Vector3 oldPosition;
    public float followSpeed = 10;
    public Vector3 newRotationWhileDragging;
    public Vector3 position1Offset;
    public Vector3 position2Offset;
    public Vector3 newOriginalRotation;
    private Vector3 touchPosition;
    public string targetTag;
    private GameObject selectedObject;
    private static GameObject currentlyDraggedObject;
    [HideInInspector] public Collider col;

    public bool isHolding = false;
    private Quaternion oldRotation;
    public List<ComponentManager> attachableComponents = new List<ComponentManager>();

    [Header("Details Of The Component")]
    public bool rotatingByTouch;
    public bool autoRotating;
    private float objectTimerRotation = 0;
    private Vector3 rotationAxis;
    public Button button;
    private float objectTimer;
    [Header("For Component With Animations Attached")]
    public Animator[] animators;
    public LayerMask targetComponent;
    Vector3 previousTouchPosition;
    private Quaternion originalRotation;
    float touchTime;

    [Header("Components on Stand")]
    public StandFunction standFunction;
    public Vector3 newStandRotation;
    public cakeslice.Outline outline;
    public LayerMask standLayer;

    [Header("Sub-Attachment Highlights")]
    public TargetComponentManager[] TargetsComponentManager;

    [Header("Vectors for Wrong Slots")]
    public Vector3 newPositionCPU;
    public Vector3 newRotationCPU;
    public bool inCPU;

    public Vector3 newPositionGPU;
    public Vector3 newRotationGPU;
    public bool inGPU;

    public Vector3 newPositionRAM;
    public Vector3 newRotationRAM;
    public bool inRAM;

    public Vector3 newPositionM2;
    public Vector3 newRotationM2;
    public bool inM2;

    [Header("Particle Spark")]
    public ParticleSystem spark;

    [Header("Item Description")]
    [TextArea(3, 10)]
    public string descriptionText;
    public float textSize;

    private cakeslice.Outline standOutline;

    [Header("This Attached into")]
    public Collider thisObjectAttachedTo;


    TargetComponentManager targetSlot;
    Collider targetSlotCollider;
    SoundManager soundManager;
    Vector3 originalScale;
    GameManager gameManager;
    private void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        soundManager = GameObject.Find("Game Manager").GetComponent<SoundManager>();
        standOutline = standFunction.gameObject.GetComponent<cakeslice.Outline>();
        col = GetComponent<Collider>();
        cameraMovement = mainCamera.GetComponent<CameraMovement>();
        oldPosition = transform.position;
        oldRotation = transform.rotation;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }
    private void Update()
    {
        if (cameraMovement.caseDetailsAnimation || cameraMovement.isSettingsOn || gameManager.isSwitchedTurnedOn) return;
        HandleTouchInput();

        if (autoRotating && cameraMovement.componentDetails)
        {
            RotateAutomatically(cameraMovement.selectedObject);
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
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
                    HandleTouchBeganDetails(touch, gameObject);
                else
                    HandleTouchBegan(touch);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (isComponentDetails)
                    HandleTouchMovedDetails(touch, gameObject);
                else
                    HandleTouchMoved(touch, gameObject);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isComponentDetails)
                    HandleTouchEndedDetails(touch, gameObject);
                else
                    HandleTouchEnded(touch, gameObject);
                break;
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        if (cameraMovement.componentDetails) return;

        Ray ray = mainCamera.ScreenPointToRay(touch.position);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        if (hits.Length > 0)
        {
            selectedObject = null;
            int highestPriority = int.MinValue;

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

            if (currentlyDraggedObject == null && selectedObject == gameObject)
            {
                currentlyDraggedObject = gameObject;
                cameraMovement.holdingObject = true;
                cameraMovement.selectedObject = currentlyDraggedObject;
                touchTime = Time.time;
                isHolding = true;
                OutlineHandler(true, gameObject);
                soundManager.SelectedObjectSFX(1);
            }

            if (standFunction.currentComponent == currentlyDraggedObject)
            {
                standFunction.currentComponent = null;
            }
        }
    }
    public LayerMask highlightTarget;
    void HighlightChangerColor(Touch touch)
    {
        if (cameraMovement.isBoardAttached)
        {
            Ray ray = mainCamera.ScreenPointToRay(touch.position);
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, highlightTarget);

            foreach (TargetComponentManager targets in TargetsComponentManager)
            {
                if (!targets.isOccupied && !targets.isPerfectlyAttached)
                {
                    targets.targetSlot.SetActive(true);
                }

                if (hitSomething && hit.collider != null && hit.collider.CompareTag(targetTag))
                {
                    // Check if the hit object is the current target
                    if (hit.collider.gameObject == targets.gameObject)
                    {
                        targets.SetMaterialColor(Color.white);
                    }
                    else
                    {
                        targets.SetMaterialColor(Color.green);
                    }
                }
                else
                {
                    targets.SetMaterialColor(Color.green);
                }
            }

            if (targetSlot != null && cameraMovement.isBoardAttached)
            {
                targetSlot.targetSlot.SetActive(true);
                targetSlot.enabled = true;
                targetSlot.isOccupied = false;
                if (targetSlotCollider != null)
                {
                    targetSlotCollider.enabled = true;
                }
                if (targetSlot.perfectSlotComponent == gameObject.name || targetSlot.perfectSlotComponent2 == gameObject.name)
                {
                    targetSlot.isPerfectlyAttached = false;
                }
            }
        }
     
    }
    private void HandleTouchMoved(Touch touch, GameObject thisComponent)
    {
        if (cameraMovement.componentDetails) return;
        if (isHolding && currentlyDraggedObject == thisComponent)
        {
            touchPosition = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, distanceFromCamera));
            transform.position = Vector3.Lerp(transform.position, touchPosition, followSpeed * Time.deltaTime);

            Vector3 direction = touchPosition - transform.position;
            direction.x = -direction.x;
            direction.y = -direction.y;

            HighlightChangerColor(touch);
            CheckIfTargetHit(touch, thisComponent);
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
            if (cameraMovement.isBoardAttached)
            {
                BoardAttachedFunction(currentlyDraggedObject, false);
            }
            CPUDrag(touch, thisComponent);
        }
    }
    private void CheckIfTargetHit(Touch touch, GameObject thisObject)
    {
        Ray ray = mainCamera.ScreenPointToRay(touch.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetComponent))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag(targetTag))
            {
                OutlineHandler(true, currentlyDraggedObject);
            }

            if (cameraMovement.isBoardAttached)
            {
                OutlineHandler(true, currentlyDraggedObject);
                SetAlpha(thisObject, 0.2f);
            }
        }
        else
        {
            OutlineHandler(false, currentlyDraggedObject);
            SetAlpha(thisObject, 1);
        }
        bool hitStand = Physics.Raycast(ray, out hit, Mathf.Infinity, standLayer);
        if (!hitStand && hit.collider == null && !standFunction.isStandOccupied)
        {
            standOutline.enabled = true;
            standOutline.eraseRenderer = false;
            standOutline.color = 2;
        }
        else
        {
            standOutline.color = 0;
            standOutline.enabled = false;
            standOutline.eraseRenderer = true;
        }
    }
    private void HandleTouchEnded(Touch touch, GameObject thisObject)
    {
        if (isHolding && thisObject == this.gameObject)
        {
            AttachedObjectChecker();
            if (Time.time - touchTime < 0.2f)
            {
                OutlineHandler(true, currentlyDraggedObject);
                SetAlpha(thisObject, 1);
                ActivateComponentDetails();
                if (cameraMovement.isCPUAnimating)
                {
                    CPUDragEnded();
                }
                outline.enabled = true;
            }
            else
            {
                outline.enabled = true;
                OutlineHandler(true, currentlyDraggedObject);
                CheckAttachmentOrReturn(touch, thisObject);
            }
        }
        isHolding = false;
        standOutline.enabled = false;
        standOutline.eraseRenderer = true;
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
        AttachedObjectChecker();
        if (cameraMovement.isBoardAttached)
        {
            BoardAttachedFunction(currentlyDraggedObject, false);
        }
        foreach (TargetComponentManager targets in TargetsComponentManager)
        {
            if (!targets.isOccupied && cameraMovement.isBoardAttached)
            {
                targets.targetSlot.SetActive(false);
            }
        }
    }
    private void CheckAttachmentOrReturn(Touch touch, GameObject thisObject)
    {
        if (isHolding && thisObject == currentlyDraggedObject)
        {
            Ray dragRay = mainCamera.ScreenPointToRay(touch.position);
            RaycastHit[] hits = Physics.RaycastAll(dragRay);
            bool targetFound = false;
            foreach (RaycastHit hit in hits)
            {
                if ((hit.collider != null && hit.collider.gameObject.CompareTag(targetTag) && cameraMovement.isBoardAttached) || (thisObject.name == "Motherboard Main" && hit.collider != null && hit.collider.gameObject.CompareTag(targetTag)))
                {
                    SetAlpha(gameObject, 1);
                    isAttached = true;
                    newPosition1 = hit.collider.gameObject;
                    newPosition2 = newPosition1.transform.GetChild(0);
                    targetFound = true;
                    BoardAttachedFunction(currentlyDraggedObject, false);
                    targetSlot = null;
                    targetSlotCollider = null;
                    if (targetSlot == null && cameraMovement.isBoardAttached)
                    {
                        targetSlot = hit.collider.gameObject.GetComponent<TargetComponentManager>();
                        targetSlot.targetSlot.SetActive(false);
                        targetSlot.enabled = false;
                        targetSlotCollider = hit.collider;
                        targetSlotCollider.enabled = false;
                        if (targetSlot.perfectSlotComponent == thisObject.name || targetSlot.perfectSlotComponent2 == thisObject.name)
                        {
                            targetSlot.isPerfectlyAttached = true;
                            soundManager.PlayRandomGoodDialogue();
                            AttachedObjectChecker();
                        }
                        else
                        {
                            targetSlot.isOccupied = true;
                        }
                    }
                    if (hit.collider.gameObject.name == "CPU New Position" && !targetSlot.isPerfectlyAttached)
                    {
                        inCPU = true;
                        inGPU = false;
                        inRAM = false;
                        inM2 = false;
                        NewPositionForSlots(newRotationCPU, newPositionCPU);
                    }
                    else if (hit.collider.gameObject.name == "GPU New Position" && !targetSlot.isPerfectlyAttached)
                    {
                        inGPU = true;
                        inCPU = false;
                        inRAM = false;
                        inM2 = false;
                        NewPositionForSlots(newRotationGPU, newPositionGPU);
                    }
                    else if (hit.collider.gameObject.name == "RAM New Position" && !targetSlot.isPerfectlyAttached)
                    {
                        inRAM = true;
                        inCPU = false;
                        inGPU = false;
                        NewPositionForSlots(newRotationRAM, newPositionRAM);
                    }
                    else if (hit.collider.gameObject.name == "M2 New Position" && !targetSlot.isPerfectlyAttached)
                    {
                        inM2 = true;
                        inGPU = false;
                        inCPU = false;
                        inRAM = false;
                        NewPositionForSlots(newRotationM2, newPositionM2);
                    }
                    else
                    {
                        if((thisObject.name != "Motherboard Main"))
                        {
                            StartCoroutine(NewPosition());
                            inRAM = false;
                            inCPU = false;
                            inGPU = false;
                            inM2 = false;
                        }
                        else
                        {
                            ComputerCaseManager compCase = GameObject.Find("Case").GetComponent<ComputerCaseManager>();
                            if (!compCase.isGlassAttached)
                            {
                                StartCoroutine(NewPosition());
                                inRAM = false;
                                inCPU = false;
                                inGPU = false;
                                inM2 = false;
                                compCase = null;
                            }
                            else
                            {
                                SetAlpha(gameObject, 1);
                                BoardAttachedFunction(currentlyDraggedObject, false);
                                isAttached = false;
                                StartCoroutine(MoveBackToOriginalPos());
                                compCase = null;
                            }
                        }
                    }
                    break;
                }
                if (!standFunction.isStandOccupied && hit.collider.gameObject == standFunction.gameObject && thisObject == currentlyDraggedObject)
                {
                    SetAlpha(gameObject, 1);
                    BoardAttachedFunction(currentlyDraggedObject, true);
                    standFunction.currentComponent = currentlyDraggedObject;
                    AttachedObjectChecker();
                    StartCoroutine(StandAnimation());
                    targetFound = true;
                    if (targetSlot != null && cameraMovement.isBoardAttached)
                    {
                        targetSlot.targetSlot.SetActive(true);
                        targetSlot.enabled = true;
                        targetSlot.isOccupied = false;
                        if (targetSlotCollider != null)
                        {
                            targetSlotCollider.enabled = true;
                        }
                        if (targetSlot.perfectSlotComponent == thisObject.name || targetSlot.perfectSlotComponent2 == thisObject.name)
                        {
                            targetSlot.isPerfectlyAttached = false;
                        }
                        targetSlot = null;
                    }
                    inRAM = false;
                    inCPU = false;
                    inGPU = false;
                    inM2 = false;
                    isAttached = false;
                    CPUDragEnded();
                    break;
                }
            }
            if (!targetFound)
            {
                SetAlpha(gameObject, 1);
                BoardAttachedFunction(currentlyDraggedObject, false);
                isAttached = false;
                StartCoroutine(MoveBackToOriginalPos());
                if (targetSlot != null && isHolding && thisObject == currentlyDraggedObject && cameraMovement.isBoardAttached)
                {
                    targetSlot.targetSlot.SetActive(true);
                    targetSlot.enabled = true;
                    targetSlot.isOccupied = false;
                    if (targetSlotCollider != null)
                    {
                        targetSlotCollider.enabled = true;
                    }
                    if (targetSlot.perfectSlotComponent == thisObject.name || targetSlot.perfectSlotComponent2 == thisObject.name)
                    {
                        targetSlot.isPerfectlyAttached = false;
                    }
                    targetSlot = null;
                }
                inRAM = false;
                inCPU = false;
                inGPU = false;
                inM2 = false;
                CPUDragEnded();
                soundManager.SelectedObjectSFX(2);
                if (thisObjectAttachedTo != null)
                {
                    thisObjectAttachedTo.enabled = true;
                    thisObjectAttachedTo = null;
                }
            }
            foreach (TargetComponentManager targets in TargetsComponentManager)
            {
                if (!targets.isOccupied && cameraMovement.isBoardAttached)
                {
                    targets.targetSlot.SetActive(false);
                }
            }
            isHolding = false;
            currentlyDraggedObject = null;
            AttachedObjectChecker();
        }
    }

    void NewPositionForSlots(Vector3 rot, Vector3 pos)
    {
        if (inCPU || inRAM || inGPU || inM2)
        {
            StartCoroutine(NewPositionWrongSlot(rot, pos));
        }
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
        button.onClick.AddListener(ButtonBack);
        StartCoroutine(cameraMovement.ButtonMoveScreen(cameraMovement.buttonBackPosition.position, !cameraMovement.componentDetails));
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, distanceFromCamera);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenCenter);
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

    private IEnumerator NewPositionWrongSlot(Vector3 rot, Vector3 pos)
    {
        cameraMovement.selectedObject = null;
        float timer = 0;
        float duration = 0.5f;

        Quaternion targetRotation = newPosition1.transform.parent.rotation * Quaternion.Euler(rot);
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, (pos + newPosition1.transform.position), timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, timer / duration);
            yield return null;
        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            // Check if the specific object is hit
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null && cameraMovement.selectedObject == currentlyDraggedObject && isHolding)
                {
                    // Break the coroutine if the object's collider is touched
                    yield break;
                }
            }
        }
        soundManager.PlayRandomBadDialogue();
        SparkHandler();
        AttachedObjectChecker();
        CPUDragEnded();
    }

    private IEnumerator NewPosition()
    {
        cameraMovement.selectedObject = null;
        float timer = 0;
        float duration = 0.5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            Vector3 pos;
            if(position1Offset != Vector3.zero)
            {
                pos = position1Offset + newPosition1.transform.position;
            }
            else
            {
                pos = newPosition1.transform.position;
            }
            transform.position = Vector3.Lerp(transform.position, pos, timer / duration);
            Quaternion targetRotation;
            if (newOriginalRotation != Vector3.zero)
            {
                // Combine newPosition1's rotation with the offset (newOriginalRotation)
                targetRotation = newPosition1.transform.parent.localRotation * Quaternion.Euler(newOriginalRotation);
            }
            else
            {
                // No offset, just use newPosition1's rotation
                targetRotation = newPosition1.transform.rotation;
            }
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, timer / duration);
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
                    if (hit.collider != null && cameraMovement.selectedObject == currentlyDraggedObject && isHolding)
                    {
                        // Break the coroutine if the object's collider is touched
                        yield break;
                    }
                }
            }
        }
        soundManager.SelectedObjectSFX(3);
        timer = 0;
        duration = 0.5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            Vector3 pos;
            if (position2Offset != Vector3.zero)
            {
                pos = position2Offset + newPosition2.transform.position;
            }
            else
            {
                pos = newPosition2.transform.position;
            }
            transform.position = Vector3.Lerp(transform.position, pos, timer / duration);
            Quaternion targetRotation;
            if (newOriginalRotation != Vector3.zero)
            {
                // Combine newPosition1's rotation with the offset (newOriginalRotation)
                targetRotation = newPosition1.transform.parent.localRotation * Quaternion.Euler(newOriginalRotation);
            }
            else
            {
                // No offset, just use newPosition1's rotation
                targetRotation = newPosition1.transform.rotation;
            }
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, timer / duration);
            yield return null;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit[] hits = Physics.RaycastAll(ray);

                // Check if the specific object is hit
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != null && cameraMovement.selectedObject == currentlyDraggedObject && isHolding)
                    {
                        // Break the coroutine if the object's collider is touched
                        yield break;
                    }
                }
            }
        }
        AttachedObjectChecker();
        CPUDragEnded();
    }
    private void SparkHandler()
    {
        if (targetSlot != null && !targetSlot.isPerfectlyAttached && spark != null)
        {
            var a = Instantiate(spark);
            StartCoroutine(SparkRemover(a));
            a.transform.position = transform.position;
        }
    }
    private IEnumerator SparkRemover(ParticleSystem a)
    {
        a.Play();
        while (a.isPlaying)
        {
            yield return null;
        }
        Destroy(a.gameObject);
    }

    private IEnumerator MoveBackToOriginalPos()
    {
        cameraMovement.selectedObject = null;
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
                Ray ray = mainCamera.ScreenPointToRay(touch.position);
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
        selectedObject = null;
    }

    private void CPUDrag(Touch touch, GameObject obj)
    {
        TargetComponentManager hitComponent;
        Ray ray = mainCamera.ScreenPointToRay(touch.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetComponent))
        {
            hitComponent = hit.collider.gameObject.GetComponent<TargetComponentManager>();
            if (hit.collider != null && hit.transform.gameObject.name == "CPU New Position" && cameraMovement.isBoardAttached && !hitComponent.isOccupied)
            {
                foreach (Animator anim in animators)
                {
                    anim.SetBool("Detect", true);
                }
            }
            else
            {
                foreach (Animator anim in animators)
                {
                    anim.SetBool("Detect", false);
                }
            }
        }
        else
        {
            foreach (Animator anim in animators)
            {
                anim.SetBool("Detect", false);
            }
        }
    }

    private void CPUDragEnded()
    {
        if (animators != null)
        {
            foreach (Animator anim in animators)
            {
                anim.SetBool("Detect", false);
            }
        }
    }

    private void AttachedObjectChecker()
    {
        if (this.isAttached)
        {
            Transform parentTransform = this.newPosition2.transform.parent;
            this.transform.SetParent(parentTransform, true);
            if (this.gameObject.name == "CPU Cooler")
            {
                Vector3 localScale = originalScale;
                Vector3 parentScale = parentTransform.localScale;

                // Adjust the scale to counteract the parent's scale
                transform.localScale = new Vector3(
                    localScale.x / parentScale.x,
                    localScale.y / parentScale.y,
                    localScale.z / parentScale.z
                );
            }
        }
        else
        {
            this.transform.SetParent(null, true);
        }
    }



    private void HandleTouchBeganDetails(Touch touch, GameObject thisObject)
    {
        Ray ray = mainCamera.ScreenPointToRay(touch.position);
        RaycastHit hitInfo;
        // Perform the raycast
        if (Physics.Raycast(ray, out hitInfo))
        {
            // Check if the hit collider is not null and the hit object's name matches
            if (hitInfo.collider == this.col && cameraMovement.selectedObject.name == thisObject.name)
            {
                previousTouchPosition = touch.position;
                rotatingByTouch = true;
            }
        }
        else
        {
            // If no hit, ensure rotatingByTouch is set to false
            rotatingByTouch = false;
        }
    }

    private void HandleTouchMovedDetails(Touch touch, GameObject thisObject)
    {
        if (cameraMovement.componentDetails && rotatingByTouch && cameraMovement.selectedObject.name == thisObject.name)
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

                thisObject.transform.Rotate(Vector3.up, rotationY, Space.World);
                thisObject.transform.Rotate(Vector3.right, rotationX, Space.World);

                previousTouchPosition = currentTouchPosition;
            }
        }
    }

    private void HandleTouchEndedDetails(Touch touch, GameObject thisObject)
    {
        if (cameraMovement.componentDetails && rotatingByTouch && cameraMovement.selectedObject.name == thisObject.name)
        {
            objectTimer = 2;
            autoRotating = true;
            objectTimerRotation = 0;
            rotatingByTouch = false;
        }
    }
    public void ButtonBack()
    {
        // Reset states
        objectTimerRotation = 0;
        autoRotating = false;
        rotatingByTouch = false;

        // Handle slot positioning based on current component
        HandleComponentPosition();

        // Reset button and object states
        button.onClick.RemoveAllListeners();
        button.interactable = false;
        currentlyDraggedObject = null;
        cameraMovement.componentDetails = false;

        // Start camera movement and details ending routines
        StartCoroutine(cameraMovement.ButtonMoveScreen(cameraMovement.oldButtonBackPosition, cameraMovement.componentDetails));
        cameraMovement.MoveDetailsEnd();
        cameraMovement.selectedObject = null;
    }

    private void HandleComponentPosition()
    {
        // Check for the other conditions first, and if any are true, bypass the isAttached logic
        if (inCPU)
        {
            NewPositionForSlots(newRotationCPU, newPositionCPU);
            return;
        }
        else if (inGPU)
        {
            NewPositionForSlots(newRotationGPU, newPositionGPU);
            return;
        }
        else if (inRAM)
        {
            NewPositionForSlots(newRotationRAM, newPositionRAM);
            return;
        }
        else if (inM2)
        {
            NewPositionForSlots(newRotationM2, newPositionM2);
            return;
        }

        // If none of the above conditions are true, handle the isAttached logic
        if (isAttached)
        {
            StartCoroutine(NewPosition());
            return;
        }

        // If none of the conditions match, move the object back to its original position
        StartCoroutine(MoveBackToOriginalPos());
    }


    private void RotateAutomatically(GameObject thisObject)
    {
        if (selectedObject.gameObject == thisObject)
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

    private void OutlineHandler(bool enabler, GameObject thisObject)
    {
        if (outline == null) return;
        if (isHolding && thisObject.name == this.gameObject.name)
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
    }

    private IEnumerator StandAnimation()
    {
        cameraMovement.selectedObject = null;
        Quaternion newRot = Quaternion.Euler(newStandRotation);
        float timer = 0;
        float duration = 0.5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, standFunction.standPosition.position, timer / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, timer / duration);
            yield return null;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit[] hits = Physics.RaycastAll(ray);

                // Check if the specific object is hit
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != null && cameraMovement.selectedObject == currentlyDraggedObject && isHolding)
                    {
                        // Break the coroutine if the object's collider is touched
                        yield break;
                    }
                }
            }
        }
    }

    private void BoardAttachedFunction(GameObject motherBoard, bool isAttached)
    {
        if (motherBoard.name == "Motherboard Main")
        {
            cameraMovement.isBoardAttached = isAttached;
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