using System;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    public static event Action<Touch> OnTouchBegan;
    public static event Action<Touch> OnTouchMoved;
    public static event Action<Touch> OnTouchEnded;

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchBegan?.Invoke(touch);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    OnTouchMoved?.Invoke(touch);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnTouchEnded?.Invoke(touch);
                    break;
            }
        }
    }
}
