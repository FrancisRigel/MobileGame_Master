using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class USBFunction : MonoBehaviour
{
    CameraMovement cameraMovement;
    GameManager gameManager;
    public float distanceFromCamera = 2f;
    public float followSpeed = 10;
    public LayerMask targetLayer;
    [Header("Checkers")]
    public bool isDragging;
    public bool isAttached;
    public Vector3 newRotation;
    public Vector3 newPosition;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private Vector3 touchPosition;
    Collider targetCollider;

    private cakeslice.Outline outline;
    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void Update()
    {
        if (cameraMovement.caseDetailsAnimation || cameraMovement.isSettingsOn) return;
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

    private void RaycastHandler(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity) && hit.collider.CompareTag("USB") && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            cameraMovement.holdingObject = true;
            cameraMovement.selectedObject = gameObject;

            if(targetCollider != null)
            {
                targetCollider.enabled = true;
                targetCollider = null;
            }

            if (outline == null)
            {
                outline = gameObject.AddComponent<cakeslice.Outline>();
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


            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;
            bool isHitting = Physics.Raycast(ray, out hit, Mathf.Infinity ,LayerMask.NameToLayer("Case"));
            if (isHitting && hit.collider != null && hit.collider.CompareTag("CASEO"))
            {
                SetAlpha(gameObject, 0.2f);
            }
            else
            {
                SetAlpha(gameObject, 1);
            }
        }
    }
    Coroutine coroutine;
    private void HandleTouchEnded(Touch touch)
    {
        if (isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;
            bool hitZone = Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer);
            if (hitZone && hit.collider != null)
            {
                transform.SetParent(hit.transform.parent);
                isDragging = false;
                StopAllCoroutines();
                coroutine = StartCoroutine(MoveToNewPosition(hit.transform));
                isAttached = true;
                targetCollider = hit.collider;

                if (targetCollider != null)
                {
                    targetCollider.enabled = false;
                }

                if (!gameManager.USBFunctions.Contains(this))
                {
                    gameManager.USBFunctions.Add(this);
                }
            }
            else
            {
                transform.SetParent(null);
                isDragging = false;
                StopAllCoroutines();
                coroutine = StartCoroutine(MoveToOldPosition());
                isAttached = false;
                if (gameManager.USBFunctions.Contains(this))
                {
                    gameManager.USBFunctions.Remove(this);
                }
            }

            if (outline != null)
            {
                Destroy(outline);
            }
            SetAlpha(gameObject, 1);
        }
    }

    private IEnumerator MoveToNewPosition(Transform hitpos)
    {
        float timer = 0;
        float duration = .5f;
        Quaternion targetRotation;
        Vector3 targetPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            targetPosition = hitpos.transform.position + newPosition;
            transform.position = Vector3.Lerp(transform.position, targetPosition, timer / duration);
            targetRotation = hitpos.rotation * Quaternion.Euler(newRotation);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, timer / duration);
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
