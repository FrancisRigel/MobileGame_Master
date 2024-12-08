using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    public GameObject selectedObject;
    public float objectRotationSpeed = 10;
    public bool invertedTouch = false;
    public float rotationSpeed; // Sensitivity of the camera rotation
    public bool holdingObject;
    public bool componentDetails;
    public bool isSettingsOn;
    public bool invertControls = false;  // Option to invert both vertical and horizontal controls
    public bool isBoardAttached = false;

    public float rotationX = 0f;
    public float rotationY = 0f;
    public ComputerCaseManager computerCaseManager;
    [HideInInspector] public Vector3 oldPosition;
    [HideInInspector] public Vector3 oldRotation;
    public bool caseDetailsAnimation = false;
    Camera cam;

    [HideInInspector] public Vector2 oldButtonBackPosition;
    public RectTransform buttonBackPosition;
    public Button buttonback;
    public Button smallArrow;
    public GameObject smallArrowPosition;
    public RectTransform smallArrowPositionDescription;
    public RectTransform descriptionBox;
    public RectTransform descriptionBoxPosition;
    Vector2 oldDescriptionBox;
    Vector2 oldSmallArrowPosition;
    public GameManager gameManager;
    [Header("For Animations")]
    public bool isCPUAnimating = false;
    public bool isDescriptionStarted = false;

    [Header("UI Elements")]
    public Button rightButton;
    public Button leftButton;
    public Button gearButton;
    public RectTransform buttonNewPosition;
    public RectTransform gearButtonHidePosition;
    [HideInInspector]public Vector2 oldRightButtonPosition;
    [HideInInspector]public Vector2 oldLeftButtonPosition;
    [HideInInspector]public Vector2 oldGearPosition;


    [Header("Type Writer Effect")]
    public TextMeshProUGUI uiText;
    public float typeSpeed;
    public float fontSize = 15;
    private Coroutine typingCoroutine;

    [Header("Pinch Zoom")]
    public float zoomSpeed = 0.5f; // Adjust zoom speed
    public float minZoom = 20f; // Minimum zoom (field of view)
    public float maxZoom = 60f; // Maximum zoom (field of view)
    private float initialDistance; // Distance between two fingers
    private float currentZoom; // Current field of view

    private void Awake()
    {
        Application.targetFrameRate = 60;
        currentZoom = Camera.main.fieldOfView;
        oldRightButtonPosition = rightButton.transform.position;
        oldLeftButtonPosition = leftButton.transform.position;
        oldGearPosition = gearButton.transform.position;
        oldDescriptionBox = descriptionBox.position;
        oldButtonBackPosition = buttonback.transform.position;
        oldSmallArrowPosition = smallArrow.transform.position;
        cam = GetComponent<Camera>();
        oldRotation = transform.localEulerAngles;
        oldPosition = transform.position;

        rotationX = transform.localEulerAngles.y;
        rotationY = transform.localEulerAngles.x;
    }

    void Update()
    {
        if (holdingObject || componentDetails || caseDetailsAnimation || computerCaseManager.isDragging || isSettingsOn) return;
        CameraFunction();

    }

    void CameraFunction()
    {
#if UNITY_ANDROID || UNITY_IOS
        HandleTouchInput();
#else
        HandleMouseInput();
#endif
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1) // Single finger touch for rotation
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                // Calculate rotation based on touch movement
                float deltaX = touch.deltaPosition.x * rotationSpeed * Time.deltaTime;
                float deltaY = touch.deltaPosition.y * rotationSpeed * Time.deltaTime;

                // Apply inversion for both axes if enabled
                if (invertControls)
                {
                    deltaX = -deltaX;
                    deltaY = -deltaY;
                }

                // Apply rotation
                rotationX += deltaX;
                rotationY -= deltaY;  // Inverting Y because camera looks "up" with negative Y

                // Clamp vertical rotation to prevent flipping
                rotationY = Mathf.Clamp(rotationY, -90f, 90f);

                // Apply the rotation to the camera
                transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
            }
        }
        else if (Input.touchCount == 2) // Two finger touch for zooming
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // Check if at least one finger moved
            if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                // Calculate the distance between two touches in each frame
                float currentDistance = Vector2.Distance(touch1.position, touch2.position);

                // If this is the first pinch, save the initial distance
                if (initialDistance == 0)
                {
                    initialDistance = currentDistance;
                }

                // Calculate the difference between the initial and current distance
                float distanceDifference = currentDistance - initialDistance;

                // Zoom based on the distance change
                currentZoom -= distanceDifference * zoomSpeed * Time.deltaTime;

                // Clamp the zoom value between minZoom and maxZoom
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

                // Apply the zoom to the camera's field of view
                Camera.main.fieldOfView = currentZoom;

                // Reset the initial distance for the next frame
                initialDistance = currentDistance;
            }
        }
        else
        {
            // Reset initial distance when touch count goes back to 0 or 1
            initialDistance = 0;
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButton(0))
        {
            // Calculate rotation based on mouse movement
            float deltaX = Input.GetAxis("Mouse X") * rotationSpeed;
            float deltaY = Input.GetAxis("Mouse Y") * rotationSpeed;

            // Apply inversion for both axes if enabled
            if (invertControls)
            {
                deltaX = -deltaX;
                deltaY = -deltaY;
            }

            // Apply rotation
            rotationX += deltaX;
            rotationY -= deltaY;  // Inverting Y because camera looks "up" with negative Y

            // Clamp vertical rotation to prevent flipping
            rotationY = Mathf.Clamp(rotationY, -90f, 90f);

            // Apply the rotation to the camera
            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }
    }

    public void SetRotation(Quaternion newRotation)
    {
        transform.localRotation = newRotation;

        // Decompose the Quaternion back into rotationX and rotationY
        Vector3 eulerAngles = newRotation.eulerAngles;
        rotationX = eulerAngles.y;  // Y is the horizontal axis in Unity's Euler Angles
        rotationY = eulerAngles.x;  // X is the vertical axis in Unity's Euler Angles
    }

    public void MoveDetailsStart()
    {
        isDescriptionStarted = true;
        StartCoroutine(MoveAwayButtonPosition());
        StartCoroutine(CameraFOVChange(65, !componentDetails));
        StartCoroutine(ArrowDescription(smallArrowPosition.transform.position, !componentDetails));
    }

    public void MoveDetailsEnd()
    {
        StartCoroutine(MoveBackButtonPosition());
        StartCoroutine(ArrowDescription(oldSmallArrowPosition, componentDetails));
        StartCoroutine(CameraFOVChange(50, componentDetails));
    }

    public IEnumerator CameraFOVChange(float fov, bool details)
    {
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, timer / duration);
            yield return null;

            if (details)
            {
                yield break;
            }
        }
    }

    public IEnumerator ButtonMoveScreen(Vector3 pos, bool details)
    {
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            buttonback.transform.position = Vector2.Lerp(buttonback.transform.position, pos, timer / duration);
            yield return null;
            if (details)
            {
                yield break;
            }
        }
    }

    public IEnumerator ArrowDescription(Vector2 pos, bool details)
    {
        var a = smallArrow.GetComponent<Button>();
        a.interactable = false;
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            smallArrow.gameObject.transform.position = Vector2.Lerp(smallArrow.gameObject.transform.position, pos, timer / duration);
            yield return null;
            if (details)
            {
                yield break;
            }
        }
        a.interactable = true;
    }
    public void SmallArrowFunctionStart()
    {
        isDescriptionStarted = true;
        smallArrow.gameObject.SetActive(false);
        StartCoroutine(ButtonMoveScreen(oldButtonBackPosition, !isDescriptionStarted));
        StartCoroutine(DescriptionStart());
        Quaternion newRotation = Quaternion.Euler(smallArrow.gameObject.transform.eulerAngles.x, smallArrow.gameObject.transform.eulerAngles.y, 0f);
        smallArrow.gameObject.transform.rotation = newRotation;
        smallArrow.onClick.RemoveAllListeners();
    }

    public void SmallArrowFunctionEnd()
    {
        if (IsTyping())
        {
            uiText.text = "";
            StopCoroutine(typingCoroutine);
        }
        else
        {
            uiText.text = "";
        }
        StartCoroutine(ButtonMoveScreen(buttonBackPosition.position, componentDetails));
        StartCoroutine(DescriptionBoxEnd());
        Quaternion newRotation = Quaternion.Euler(smallArrow.gameObject.transform.eulerAngles.x, smallArrow.gameObject.transform.eulerAngles.y, 180f);
        smallArrow.gameObject.transform.rotation = newRotation;
        smallArrow.onClick.RemoveAllListeners();
        isDescriptionStarted = false;
    }
    private IEnumerator DescriptionStart()
    {
        buttonback.interactable = false;
        Vector3 worldPosition = Vector3.zero;
        if (!gameManager.turnOnPerfect)
        {
            var a = selectedObject.GetComponent<ComponentManager>();
            var b = selectedObject.GetComponent<PowerSupplyManager>();
            if (a != null)
            {
                DisplayText(a.descriptionText, a.textSize);
                Vector3 screenLeft = new Vector3(-Screen.width * -0.3f, Screen.height / 2, a.distanceFromCamera);
                worldPosition = cam.ScreenToWorldPoint(screenLeft);
            }
            else if (b != null)
            {
                DisplayText(b.descriptionText, b.textSize);
                Vector3 screenLeft = new Vector3(-Screen.width * -0.3f, Screen.height / 2, b.distanceFromCamera);
                worldPosition = cam.ScreenToWorldPoint(screenLeft);
                worldPosition += b.offsetPosition;
            }
            else
            {
                var c = selectedObject.GetComponent<SyringeFunction>();
                DisplayText(c.descriptionText, c.textSize);
                Vector3 screenLeft = new Vector3(-Screen.width * -0.3f, Screen.height / 2, c.distanceFromCamera);
                worldPosition = cam.ScreenToWorldPoint(screenLeft);
            }
        }
        else
        {
            var a = selectedObject.GetComponent<PrimeComponentManager>();
            DisplayText(a.descriptionText, a.textSize);
            Vector3 screenLeft = new Vector3(-Screen.width * -0.3f, Screen.height / 2, a.distanceFromCamera);
            worldPosition = cam.ScreenToWorldPoint(screenLeft);
        }

        float timer = 0;
        float duration = 1.5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            // Interpolate between the initial position and the target position
            descriptionBox.position = Vector2.Lerp(descriptionBox.position, descriptionBoxPosition.position, timer / duration);
            selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, worldPosition, timer / duration);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60, timer / duration);
            yield return null;
            if (!isDescriptionStarted)
            {
                StopAllCoroutines();
                yield break;
            }
        }

        smallArrow.gameObject.transform.position = smallArrowPositionDescription.position;
        smallArrow.gameObject.SetActive(true);
        smallArrow.onClick.AddListener(SmallArrowFunctionEnd);
    }

    IEnumerator DescriptionBoxEnd()
    {
        if (!gameManager.turnOnPerfect)
        {
            var a = selectedObject.GetComponent<ComponentManager>();
            var b = selectedObject.GetComponent<PowerSupplyManager>();
            if (a != null)
            {
                StartCoroutine(a.MoveToCenter());
            }
            else if (b != null)
            {

                StartCoroutine(b.MoveToCenter());
            }
            else
            {
                var c = selectedObject.GetComponent<SyringeFunction>();
                StartCoroutine(c.MoveToCenter());
            }
        }
        else
        {
            var a = selectedObject.GetComponent<PrimeComponentManager>();
            StartCoroutine(a.MoveToCenter());
        }
        smallArrow.gameObject.SetActive(false);
        smallArrow.gameObject.transform.position = smallArrowPosition.transform.position;
        float timer = 0;
        float duration = 1.5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            descriptionBox.position = Vector2.Lerp(descriptionBox.position, oldDescriptionBox, timer / duration);
            yield return null;
            if (isDescriptionStarted)
            {
                StopAllCoroutines();
                yield break;
            }
        }
        buttonback.interactable = true;
        smallArrow.onClick.AddListener(SmallArrowFunctionStart);
        smallArrow.gameObject.SetActive(true);
    }
    public void DisplayText(string newText, float? newFontSize = null, float? customTypeSpeed = null)
    {
        // Stop the previous typing coroutine if it's still running
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // Set the font size if provided
        if (newFontSize.HasValue)
        {
            fontSize = newFontSize.Value;
        }

        // Apply the font size to the TextMeshPro component
        uiText.fontSize = fontSize;

        // Use custom typeSpeed if provided, otherwise use the default one
        float effectiveTypeSpeed = customTypeSpeed.HasValue ? customTypeSpeed.Value : typeSpeed;

        // Start a new typing coroutine
        typingCoroutine = StartCoroutine(TypeText(newText, effectiveTypeSpeed));
    }

    // Coroutine to handle the typing effect
    IEnumerator TypeText(string textToType, float effectiveTypeSpeed)
    {
        uiText.text = ""; // Clear the text before starting

        // Loop through each character in the string
        foreach (char letter in textToType.ToCharArray())
        {
            uiText.text += letter; // Add the next character to the UI text
            yield return new WaitForSeconds(effectiveTypeSpeed); // Wait based on the typeSpeed between characters
        }

        // Once done, set the coroutine reference to null
        typingCoroutine = null;
    }

    // Method to check if typing is still ongoing
    public bool IsTyping()
    {
        return typingCoroutine != null;
    }


public void ButtonHandler(bool holdingObject, bool isCameraCloser, bool caseDetailsAnim)
    {
        if (holdingObject || caseDetailsAnim)
        {
            // Disable both buttons if holding an object or during animation
            leftButton.interactable = false;
            rightButton.interactable = false;
            return;
        }

        if (isCameraCloser)
        {
            // Enable left button, disable right button if the camera is closer
            leftButton.interactable = true;
            rightButton.interactable = false;
        }
        else
        {
            // Enable right button, disable left button if the camera is not closer
            leftButton.interactable = false;
            rightButton.interactable = true;
        }
    }
    public IEnumerator MoveAwayButtonPosition()
    {
        float timer = 0;
        float duration = 2f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            leftButton.transform.position = Vector2.Lerp(leftButton.transform.position, buttonNewPosition.position, timer / duration);
            rightButton.transform.position = Vector2.Lerp(rightButton.transform.position, buttonNewPosition.position, timer / duration);
            gearButton.transform.position = Vector2.Lerp(gearButton.transform.position, gearButtonHidePosition.position, timer / duration);
            yield return null;

            if (!componentDetails || isSettingsOn)
            {
                yield break;
            }
        }
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
    }

    public IEnumerator MoveBackButtonPosition()
    {
        if (!leftButton.gameObject.activeSelf && !rightButton.gameObject.activeSelf)
        {
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
        }
        float timer = 0;
        float duration = 2f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            leftButton.transform.position = Vector2.Lerp(leftButton.transform.position, oldLeftButtonPosition, timer / duration);
            rightButton.transform.position = Vector2.Lerp(rightButton.transform.position, oldRightButtonPosition, timer / duration);
            gearButton.transform.position = Vector2.Lerp(gearButton.transform.position, oldGearPosition, timer / duration);
            yield return null;

            if (componentDetails || isSettingsOn)
            {
                yield break;
            }
        }
    }
}