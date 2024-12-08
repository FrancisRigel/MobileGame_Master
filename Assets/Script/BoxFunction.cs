using System.Collections;
using UnityEngine;

public class BoxFunction : MonoBehaviour
{
    public GameManager manager;
    public CameraMovement cameraMovement;
    public GameObject poofParticle;
    public MeshRenderer meshRenderer;
    public GameObject prefab;
    public AudioSource audioSource;

    [Header("Checkers")]
    public bool isHitZone;
    public bool isClicked = false;

    private void OnEnable()
    {
        TouchManager.OnTouchBegan += HandleTouchBegan;
        TouchManager.OnTouchMoved += HandleTouchMoved;
        TouchManager.OnTouchEnded += HandleTouchEnded;
    }

    private void OnDisable()
    {
        TouchManager.OnTouchBegan -= HandleTouchBegan;
        TouchManager.OnTouchMoved -= HandleTouchMoved;
        TouchManager.OnTouchEnded -= HandleTouchEnded;
    }

    private void HandleTouchBegan(Touch touch)
    {
        if (isClicked || cameraMovement.holdingObject || cameraMovement.isSettingsOn || !manager.turnOnPerfect && manager.touchManager == null) return;
        RaycastHandler(touch);
    }

    private void HandleTouchMoved(Touch touch)
    {
        if (isHitZone && !isClicked)
        {
            RaycastHandler(touch);
        }
    }

    private void HandleTouchEnded(Touch touch)
    {
        if (isHitZone && !isClicked)
        {
            TriggerInteraction();
        }
    }

    private void RaycastHandler(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity) && hit.collider.CompareTag("Box") && hit.collider.gameObject == gameObject)
        {
            isHitZone = true;
        }
        else
        {
            isHitZone = false;
        }
    }

    private void TriggerInteraction()
    {
        GameObject instance = Instantiate(poofParticle);
        ParticleSystem poof = instance.GetComponent<ParticleSystem>();
        instance.transform.position = transform.position;
        meshRenderer.enabled = false;
        isClicked = true;
        Instantiate(prefab, transform.position, Quaternion.identity);
        audioSource.Play();
        StartCoroutine(DestroyAfterParticles());
    }

    private IEnumerator DestroyAfterParticles()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
