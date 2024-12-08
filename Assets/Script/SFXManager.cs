using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    AudioSource source;
    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void LateUpdate()
    {
        if(!source.isPlaying)
        {
            Destroy(this.gameObject);
        }
    }
}
