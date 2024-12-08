using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddonsForCooler : MonoBehaviour
{
    public CameraMovement cameraMovement;
    public ComponentManager manager;
    public Collider targetCollider;
    public GameObject syringe;
    private ComponentManager thisObj;

    private void Start()
    {
        thisObj = GetComponent<ComponentManager>();
    }

    private void LateUpdate()
    {
        if (cameraMovement.selectedObject == syringe) return;
        if (thisObj.isHolding)
        {
            if (manager.isAttached && !targetCollider.enabled)
            {
                targetCollider.enabled = true;
            }
        }
        else
        {
            if (manager.isAttached && targetCollider.enabled)
            {
                targetCollider.enabled = false;
            }
        }

    }
}
