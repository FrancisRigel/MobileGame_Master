using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetComponentManager : MonoBehaviour
{
    public string perfectSlotComponent;
    public string perfectSlotComponent2;
    public Color color = Color.white;
    public GameObject targetSlot;
    public Renderer render;
    private Material material;
    public bool isOccupied;
    public bool isPerfectlyAttached;
    void Start()
    {
        if (render != null)
        {
            // Store the material reference for later use
            material = render.material;

            // Ensure emission is enabled on the material
            material.EnableKeyword("_EMISSION");
        }
    }

    void LateUpdate()
    {
        if (!isOccupied && targetSlot.activeInHierarchy
            )
        {
            // Example: Change colors over time
            float intensity = Mathf.PingPong(Time.time * 2, 1.0f); // A value between 0 and 1

            // Update the main color (non-HDR)
            Color dynamicBaseColor = color * intensity;
            material.SetColor("_Color", dynamicBaseColor);

            // Update the emission color (HDR)
            Color dynamicEmissionColor = color * Mathf.LinearToGammaSpace(intensity);
            material.SetColor("_EmissionColor", dynamicEmissionColor);
        }
    }

    public void SetMaterialColor(Color newBaseColor)
    {
        // Update both the base color and the emission color directly if needed
        color = newBaseColor;

        // Apply the new colors to the material
        material.SetColor("_Color", color);
        material.SetColor("_EmissionColor", color);
    }
}
