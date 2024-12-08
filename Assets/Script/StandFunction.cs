using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandFunction : MonoBehaviour
{
    public Transform standPosition;
    public GameObject currentComponent;
    public bool isStandOccupied = false;
    private void LateUpdate()
    {
        if(currentComponent != null)
        {
            isStandOccupied = true;
        }
        else
        {
            isStandOccupied = false;
        }
    }



}
