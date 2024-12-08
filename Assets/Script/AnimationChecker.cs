using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationChecker : MonoBehaviour
{
    public CameraMovement manager;

    public void CPUAnimationTrue()
    {
        manager.isCPUAnimating = true;
    }

    public void CPUAnimationFalse()
    {
        manager.isCPUAnimating = false;
    }
}
