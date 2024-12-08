using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovedScrewPosition : MonoBehaviour
{
    public bool isOccupied = false;
    public bool isScrewAttached = false;
    public Collider col;
    private void Start()
    {
        this.col = GetComponent<Collider>();
    }
}
