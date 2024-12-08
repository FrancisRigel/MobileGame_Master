using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Checkers")]
    public bool isAllSubAttachmentsAttached;
    public bool isAllPerfectlyAttached;
    public bool isWireAttached;
    public bool isBoardAttached;
    public bool isSomethingAttachedAndNotRight;
    public bool isAllScrewAttached;
    public bool isPasted;
    public bool isPerfectlyPasted;
    public bool isSwitchedTurnedOn;
    public bool exploded;
    public bool turnOnPerfect;
    public bool isPrimeDragging;
    public bool isAllUSBConnected;
    public bool isTwoCableAttached;
    public bool isFinalScene;
    public ComponentManager[] components;
    public TargetComponentManager[] componentsSlots;
    public PlugEndFunction endPlug;
    public ComputerPlugFunction startPlug;
    public ComponentManager motherBoard;
    public CameraMovement cameraMovement;
    public ScrewFunction[] screwFunction;
    public Camera cam;
    public SoundManager soundManager;
    public DialogueManager dialogueManager;
    [Header("UI Modifiers")]
    public Button leftArrow;
    public Button rightArrow;
    public RectTransform arrowPosition;

    [Header("Settings Modifier")]
    public GameObject settingsPrefab;
    public Button gearIcon;
    public Canvas canvas;

    public GameObject instance;

    [Header("Intro Functions")]
    public GameObject fader;
    private GameObject faderPrefab;
    private Image image;
    public List<MonoBehaviour> scriptsToManage = new List<MonoBehaviour>();

    [Header("Papers")]
    public GameObject paperPrefab;
    private GameObject paper;
    public Button[] buttons;
    public AudioClip paperDrop;

    [Header("Glass Functions")]
    public GameObject glass;
    public GlassPanelFunction glassPanelFunction;
    public ComputerCaseManager computerCaseManager;

    [Header("Switch Function")]
    public PowerSupplyManager powerSupplyManager;
    public GameObject switchToggle;
    private GameObject switchTogglePrefab;

    [Header("Script Remover")]
    public LayerMask includeLayers;
    public LayerMask excludeLayers;
    public GameObject[] objectsToRemoveScripts;

    [Header("Explosion Functions")]
    public GameObject house;
    public GameObject explosionPrefab;
    public GameObject plugSparkPrefab;
    public Transform explosionPosition;
    private bool explodeOnce = false;
    private GameObject plugSparkParticle;
    private GameObject explosionParticle;
    public AudioSource wallSocket;
    [Header("Boxes")]
    public TouchManager touchManager;
    public GameObject[] partsInBox;
    public GameObject[] enableObjects;
    public GameObject[] leftOvers;
    public GameObject usbPrime;
    public Collider primeCollider;

    public SwitchFunction switchFunction;

    public List<USBFunction> USBFunctions = new List<USBFunction>();
    public List<PlugEndFunction> plugFunctions = new List<PlugEndFunction>();
    public List<GameObject> primeObjects = new List<GameObject>();
    [Header("Monitor")]
    public Material monitorMaterial;
    public AudioSource mainMusic;
    public AudioClip newMusic;

    [Header("Phone")]
    public GameObject phonePrefab;
    private GameObject phoneObject;

    [Header("Timer")]
    float timeElapsed;
    public string formatTimeText;
    private void Awake()
    {
        StartScene();
    }

    private void LateUpdate()
    {
        if (primeCollider.gameObject.activeSelf)
        {
            if (isPrimeDragging)
            {
                if (!primeCollider.enabled)
                {
                    primeCollider.enabled = true;
                }
            }
            else
            {
                if (primeCollider.enabled)
                {
                    primeCollider.enabled = false;
                }
            }

        }
        HandleGameTimer();
        HandleFinalScene();
        HandleAllCables();
        if (turnOnPerfect) return;
        Init();
        GlassHandler();
        TurnOnSomethingWrong();
        TurnOnPerfect();
        SwitchHandler();
        CasePartsHandler();
    }

    private void HandleGameTimer()
    {
        if (isFinalScene) return;
        timeElapsed += Time.deltaTime;
        string formatTime = FormatTime(timeElapsed);

        formatTimeText = formatTime;
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0}:{1:00}", minutes, seconds); // Formats time as "0:00"
    }

    private void HandleFinalScene()
    {
        if (isFinalScene) return;
        if (isAllUSBConnected && isTwoCableAttached)
        {
            Button button = switchFunction.gameObject.GetComponent<Button>();
            if (!button.interactable)
            {
                button.interactable = true;
                button.onClick.AddListener(AddButtonListenerFinalPerfect);
                isFinalScene = true;
            }
        }
    }

    private void AddButtonListenerFinalPerfect()
    {
        foreach (GameObject obj in primeObjects)
        {
            Collider col = obj.GetComponent<Collider>();
            col.enabled = false;
        }
        Button button = switchFunction.gameObject.GetComponent<Button>();
        button.interactable = false;
        StartCoroutine(WaitForResult());
    }

    private IEnumerator WaitForResult()
    {
        yield return new WaitForSeconds(1f);
        dialogueManager.DialogueStart(dialogueManager.turnOnFinal);
        mainMusic.Stop();
        yield return new WaitForSeconds(1.5f);
        mainMusic.clip = newMusic;
        mainMusic.Play();
        StartCoroutine(ButtonsRemove());
        StartCoroutine(switchFunction.AwakeAnimation(false));
        float timer = 0;
        float duration = 5f;
        while(timer < duration)
        {
            timer += Time.deltaTime;
            monitorMaterial.color = Color.Lerp(monitorMaterial.color, Color.white, timer / duration);
            yield return null;
        }
        dialogueManager.DialogueStart(dialogueManager.monologueWhileTurningOn);
        yield return new WaitForSeconds(2f);
        HandlePhone();
    }

    private void HandlePhone()
    {
        if(phoneObject == null)
        {
            phoneObject = Instantiate(phonePrefab);
            phoneObject.transform.SetParent(cameraMovement.transform, true);
            Animator animator = phoneObject.GetComponent<Animator>();
          
        }
    }

    private void HandleAllCables()
    {
        if (turnOnPerfect)
        {
            isAllUSBConnected = HandleAllUSB();
            isTwoCableAttached = HandleTwoCables();
        }
    }

    private bool HandleAllUSB()
    {
        if(USBFunctions.Count <= 2)
        {
            return false;
        }
        foreach (USBFunction function in USBFunctions)
        {
            if (!function.isAttached)
            {
                return false;
            }
        }

        return true; 
    }
    private bool HandleTwoCables()
    {
        if(plugFunctions.Count <= 1)
        {
            return false;
        }
        foreach(PlugEndFunction plugFunction in plugFunctions)
        {
            if (!plugFunction.isAttached)
            {
                return false;
            }
        }

        return true;
    }

    private void CasePartsHandler()
    {
        if (exploded || turnOnPerfect) return;
        foreach (var a in components)
        {
            if (a.isAttached && motherBoard.isAttached)
            {
                if (a.enabled)
                {
                    a.enabled = false;
                }
            }
            else
            {
                if (a.isAttached && !motherBoard.isAttached)
                {
                    if (!a.enabled)
                    {
                        a.enabled = true;
                    }
                }
            }
        }
    }

    private void TurnOnSomethingWrong()
    {
        if (isAllPerfectlyAttached) return;
        if (isSwitchedTurnedOn && isSomethingAttachedAndNotRight && motherBoard.isAttached)
        {
            foreach (ComponentManager component in components)
            {
                if (component != null)
                {
                    RemoveScriptsRecursively(component.gameObject);
                    Rigidbody rb = component.gameObject.AddComponent<Rigidbody>();
                    rb.includeLayers = includeLayers;
                    rb.excludeLayers = excludeLayers;
                }
            }

            if (motherBoard != null)
            {
                RemoveScriptsRecursively(motherBoard.gameObject);
                Rigidbody rb = motherBoard.gameObject.AddComponent<Rigidbody>();
                rb.includeLayers = includeLayers;
                rb.excludeLayers = excludeLayers;
            }

            if (glassPanelFunction != null)
            {
                Collider collider = glassPanelFunction.gameObject.GetComponent<Collider>();
                collider.enabled = true;
                RemoveScriptsRecursively(glassPanelFunction.gameObject);
                Rigidbody rb = glassPanelFunction.gameObject.AddComponent<Rigidbody>();
                rb.includeLayers = includeLayers;
                rb.excludeLayers = excludeLayers;
            }

            foreach(ScrewFunction screw in screwFunction)
            {
                if (screw != null)
                {
                    RemoveScriptsRecursively(screw.gameObject);
                    Rigidbody rb = screw.gameObject.AddComponent<Rigidbody>();
                    rb.includeLayers = includeLayers;
                    rb.excludeLayers = excludeLayers;
                }
            }

            HandleHouseColliders(house);
            foreach (var a in objectsToRemoveScripts)
            {
                RemoveScriptsRecursively(a.gameObject);
            }
            if (explosionParticle == null && plugSparkParticle == null)
            {
                if (!explodeOnce)
                {
                    explosionParticle = Instantiate(explosionPrefab);
                    plugSparkParticle = Instantiate(plugSparkPrefab);
                    explosionParticle.transform.position = explosionPosition.position;
                    soundManager.PlayBadEndingDialogue();
                    explodeOnce = true;
                    exploded = true;
                    wallSocket.Play();
                }
            }
        }
    }

    private void TurnOnPerfect()
    {
        if (isSomethingAttachedAndNotRight) return;
        if (isAllPerfectlyAttached && isBoardAttached && isAllScrewAttached && glassPanelFunction.isAttached && isSwitchedTurnedOn && isPasted)
        {
            if (turnOnPerfect) return;
            foreach(var a in screwFunction)
            {
                RemoveScriptsRecursively(a.gameObject);
            }

            foreach(var a in components)
            {
                RemoveScriptsRecursively(a.gameObject);
            }

            RemoveScriptsRecursively(motherBoard.gameObject);
            RemoveScriptsRecursively(glassPanelFunction.gameObject);
            RemoveScriptsRecursively(startPlug.gameObject);
            foreach(var a in objectsToRemoveScripts)
            {
                RemoveScriptsRecursively(a.gameObject);
            }

            soundManager.PlayGoodEndingDialogue();
            dialogueManager.DialogueStart(dialogueManager.monitorDialogue);
            if(touchManager == null)
            {
                touchManager = gameObject.AddComponent<TouchManager>();
            }
            HandleBoxes();
            HandleLeftoverObjects(leftOvers);
            usbPrime.SetActive(true);
            if (!primeCollider.gameObject.activeSelf)
            {
                primeCollider.gameObject.SetActive(true);
            }

            foreach(GameObject a in enableObjects)
            {
                if (!a.activeSelf)
                {
                    a.SetActive(true);
                }
            }
            StartCoroutine(WaitSeconds());
            turnOnPerfect = true;
        }
    }

    private IEnumerator WaitSeconds()
    {
        yield return new WaitForSeconds(1.5f);
        if (switchFunction != null)
        {
            Button button = switchFunction.gameObject.GetComponent<Button>();
            button.interactable = false;
            switchFunction.Toggler(true);
            switchFunction.toggleState = false;
        }
    }

    private void HandleLeftoverObjects(GameObject[] obj)
    {
        foreach (var a in obj)
        {
            StartCoroutine(ShrinkObject(a));
        }
    }

    IEnumerator ShrinkObject(GameObject obj)
    {
        float timer = 0;
        float duration = 1;
        Vector3 scale = Vector3.zero;
        while(timer < duration)
        {
            timer += Time.deltaTime;
            obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, scale, timer / duration);
            yield return null;
        }
        Destroy(obj);
    }

    private void HandleBoxes()
    {
        foreach(var items in partsInBox)
        {
            Collider itemCollider = items.GetComponent<Collider>();
            if (!itemCollider.enabled)
            {
                itemCollider.enabled = true;
            }
            BoxFunction boxFunction = items.GetComponent<BoxFunction>();
            if (!boxFunction.enabled)
            {
                boxFunction.enabled = true;
            }
        }
    }

    private void HandleHouseColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponents<Collider>();

        foreach (Collider collider in colliders)
        {
            if(!collider.enabled)
            {
                collider.enabled = true;
            }
        }
    }
    Coroutine coroutine;
    private void SwitchHandler()
    {
        if (exploded) return;
        if (powerSupplyManager.isAttached && (startPlug.isAttached && endPlug.isAttached))
        {
            if (switchTogglePrefab == null && coroutine == null)
            {
                coroutine = StartCoroutine(WaitSwitch());
            }
        }
        else
        {
            if (switchTogglePrefab != null)
            {
                Destroy(switchTogglePrefab);
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        if (switchTogglePrefab == null)
        {
            if (isSwitchedTurnedOn)
            {
                isSwitchedTurnedOn = false;
            }
        }
    }

    private IEnumerator WaitSwitch()
    {
        yield return new WaitForSeconds(.5f);
        switchTogglePrefab = Instantiate(switchToggle, canvas.transform);
    }

    private void SubComponentsHandler()
    {
        foreach (ComponentManager component in components)
        {
            if (component.isAttached && motherBoard.isAttached)
            {
                component.enabled = false;
            }
            else
            {
                if (component.isAttached && !motherBoard.isAttached)
                {
                    component.enabled = true;
                }
            }
        }
    }

    private void GlassHandler()
    {
        if (isSwitchedTurnedOn || exploded || turnOnPerfect) return;
        if (computerCaseManager.AllScrewRemoved())
        {
            if(glassPanelFunction != null)
            {
                glassPanelFunction.enabled = true;
                Collider collider = glassPanelFunction.GetComponent<Collider>();
                if (!collider.enabled)
                {
                    collider.enabled = true;
                }
            }
        }
        else
        {
            if (glassPanelFunction != null)
            {
                glassPanelFunction.enabled = false;
                Collider collider = glassPanelFunction.GetComponent<Collider>();
                if (collider.enabled)
                {
                    collider.enabled = false;
                }
            }
        }

        if (motherBoard.isAttached)
        {
            if (glassPanelFunction.isAttached)
            {
                motherBoard.enabled = false;
            }
            else
            {
                motherBoard.enabled = true;
            }
        }
    }

    private void ScriptHandler(bool enabler)
    {
        foreach(MonoBehaviour behaviour in scriptsToManage)
        {
            behaviour.enabled = enabler;
        }
    }

    private void StartScene()
    {
        cameraMovement.selectedObject = paperPrefab;
        StartCoroutine(WaitSec());
        faderPrefab = Instantiate(fader, canvas.transform);
        cameraMovement.selectedObject = paperPrefab;
        image = faderPrefab.GetComponent<Image>();
        Color color = image.color;
        color.a = Mathf.Clamp01(1);
        image.color = color;
        StartCoroutine(FadeImageAlpha(0, 2f));
        ScriptHandler(false);
    }

    private IEnumerator WaitSec()
    {
        yield return new WaitForSeconds(0.5f);
        paper = Instantiate(paperPrefab, canvas.transform);
        StartCoroutine(PaperAnimation());
        StartCoroutine(ButtonsRemove());
        foreach(Button button in buttons)
        {
            button.interactable = false;
        }
    }



    private IEnumerator PaperAnimation()
    {
        float timer = 0f;
        float duration = 1f;

        // Get the RectTransform component from the 'paper' GameObject
        RectTransform rect = paper.GetComponent<RectTransform>();

        // Starting and target positions for Y
        float startY = -500f;
        float targetY = 0f;
        // Store the initial position (X stays constant)
        Vector2 startPos = rect.anchoredPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Interpolate only the Y position while keeping the X position constant
            rect.anchoredPosition = new Vector2(startPos.x, Mathf.Lerp(startY, targetY, t));
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 65, t);
            yield return null;
        }
      
        // Ensure the final position is set to the target value after the loop
        rect.anchoredPosition = new Vector2(startPos.x, targetY);
        yield return new WaitForSeconds(1.5f);
        Button child = paper.transform.Find("Button").gameObject.GetComponent<Button>();
        child.interactable = true;
        child.onClick.AddListener(BackButtonPaper);
    }

    public IEnumerator ButtonsRemove()
    {
        float timer = 0;
        float duration = 1f;
        while(timer < duration)
        {
            timer += Time.deltaTime;
            cameraMovement.leftButton.transform.position = Vector2.Lerp(cameraMovement.leftButton.transform.position, cameraMovement.buttonNewPosition.position, timer / duration);
            cameraMovement.rightButton.transform.position = Vector2.Lerp(cameraMovement.rightButton.transform.position, cameraMovement.buttonNewPosition.position, timer / duration);
            cameraMovement.gearButton.transform.position = Vector2.Lerp(cameraMovement.gearButton.transform.position, cameraMovement.gearButtonHidePosition.position, timer / duration);
            yield return null;
        }
    }

    public IEnumerator ButtonsBack()
    {
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            cameraMovement.leftButton.transform.position = Vector2.Lerp(cameraMovement.leftButton.transform.position, cameraMovement.oldLeftButtonPosition, t);
            cameraMovement.rightButton.transform.position = Vector2.Lerp(cameraMovement.rightButton.transform.position, cameraMovement.oldRightButtonPosition, t);
            cameraMovement.gearButton.transform.position = Vector2.Lerp(cameraMovement.gearButton.transform.position, cameraMovement.oldGearPosition, t);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 50, t);
            yield return null;
        }

        foreach(Button button in buttons)
        {
            button.interactable = true;
        }
        cameraMovement.selectedObject = null;
    }

    public void BackButtonPaper()
    {
        StartCoroutine(PaperAnimationDone());
        StartCoroutine(ButtonsBack());

        AudioSource source = paper.GetComponent<AudioSource>();
        source.clip = paperDrop;
        source.Play();
    }

    private IEnumerator PaperAnimationDone()
    {
        float timer = 0f;
        float duration = 1f;

        // Get the RectTransform component from the 'paper' GameObject
        RectTransform rect = paper.GetComponent<RectTransform>();

        // Starting and target positions for Y
        float startY = 0;
        float targetY = -500f;

        // Store the initial position (X stays constant)
        Vector2 startPos = rect.anchoredPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Interpolate only the Y position while keeping the X position constant
            rect.anchoredPosition = new Vector2(startPos.x, Mathf.Lerp(startY, targetY, t));

            yield return null;
        }
        Destroy(paper.gameObject);
        ScriptHandler(true);
        cameraMovement.selectedObject = null;
    }

    private IEnumerator FadeImageAlpha(float targetAlpha, float duration)
    {
        // Get the current color of the image
        Color currentColor = image.color;
        // Store the starting alpha value
        float startAlpha = currentColor.a;

        // Timer for the fade duration
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // Interpolate the alpha value
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);

            // Update the image color with the new alpha value
            currentColor.a = alpha;
            image.color = currentColor;

            yield return null;
        }

        // Ensure the final alpha is exactly the target alpha
        image.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
        //ScriptHandler(true);
        Destroy(faderPrefab);
    }

    private void Init()
    {
        if (PerfectAttachmentChecker())
        {
            isAllPerfectlyAttached = true;
        }
        else
        {
            isAllPerfectlyAttached = false;
        }

        if(WirePluggedChecker())
        {
            isWireAttached = true;
        }
        else
        {
            isWireAttached = false;
        }

        if (BoardAttachedChecker())
        {
            isBoardAttached = true;
        }
        else
        {
            isBoardAttached = false;
        }

        if (SubAttachmentsChecker())
        {
            isAllSubAttachmentsAttached = true;
        }
        else
        {
            isAllSubAttachmentsAttached = false;
        }

        if (AttachmentChecker())
        {
            isSomethingAttachedAndNotRight = true;
        }
        else
        {
            isSomethingAttachedAndNotRight = false;
        }

        if (ScrewsFunction())
        {
            isAllScrewAttached = true;
        }
        else
        {
            isAllScrewAttached = false;
        }
    }

    private bool ScrewsFunction()
    {
        foreach(ScrewFunction screws in screwFunction)
        {
            if (screws.isScrewRemoved)
            {
                return false;
            }
        }
        return true;
    }

    private bool PerfectAttachmentChecker()
    {
        foreach (TargetComponentManager comp in componentsSlots)
        {
            if (!comp.isPerfectlyAttached)
            {
                return false;
            }
        }
        return true;
    }

    private bool SubAttachmentsChecker()
    {
        foreach(ComponentManager components in components)
        {
            if (!components.isAttached)
            {
                return false;
            }
        }
        return true;
    }

    private bool AttachmentChecker()
    {
        foreach (TargetComponentManager comp in componentsSlots)
        {
            if (comp.isOccupied) 
            {
                return true;
            }
        }
        return false;
    }

    private bool WirePluggedChecker()
    {
        if(startPlug.isAttached && endPlug.isAttached)
        {
            return true;
        }
        return false;
    }

    private bool BoardAttachedChecker()
    {
        if (motherBoard.isAttached)
        {
            return true;
        }
        return false;
    }

    public void SettingsClick()
    {
        if (cameraMovement.selectedObject != null) return; 
        cameraMovement.isSettingsOn = true;
        instance = Instantiate(settingsPrefab, canvas.transform);
        StartCoroutine(MoveArrows());
        gearIcon.interactable = false;
        StartCoroutine(GearIconRemove());
    }

    public void ReturnArrowsFunction()
    {
        StartCoroutine(ReturnArrows());
    }

    public void GearIconAppearFunction()
    {
        StartCoroutine(GearIconAppear());
    }

    IEnumerator GearIconRemove()
    {
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            gearIcon.transform.localScale = Vector3.Lerp(gearIcon.transform.localScale, new Vector3(0f, 0f, 0f), timer / duration);
            yield return null;
        }
    }

    IEnumerator GearIconAppear()
    {
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            gearIcon.transform.localScale = Vector3.Lerp(gearIcon.transform.localScale, new Vector3(1f, 1f, 1f), timer / duration);
            yield return null;
        }
    }

    IEnumerator ReturnArrows()
    {
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            leftArrow.transform.position = Vector2.Lerp(leftArrow.transform.position, cameraMovement.oldLeftButtonPosition, timer / duration);
            rightArrow.transform.position = Vector2.Lerp(rightArrow.transform.position, cameraMovement.oldRightButtonPosition, timer / duration);
            yield return null;
        }
    }

    IEnumerator MoveArrows()
    {
        float timer = 0;
        float duration = 1f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            leftArrow.transform.position = Vector2.Lerp(leftArrow.transform.position, arrowPosition.position, timer / duration);
            rightArrow.transform.position = Vector2.Lerp(rightArrow.transform.position, arrowPosition.position, timer / duration);
            yield return null;
        }
    }

    private void RemoveScriptsRecursively(GameObject obj)
    {
        // Get all MonoBehaviour scripts on the current object
        MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();

        // Loop through each script and destroy it
        foreach (MonoBehaviour script in scripts)
        {
            Destroy(script);
        }

        // Loop through each child and call this method recursively
        foreach (Transform child in obj.transform)
        {
            RemoveScriptsRecursively(child.gameObject);
        }
    }
}
