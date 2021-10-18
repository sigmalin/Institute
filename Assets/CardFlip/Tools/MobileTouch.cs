using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileTouch : ITouch
{
    public bool isTouchScreen()
    {
        return 0 < Input.touchCount;
    }

    public Vector2 getTouchPos()
    {
        if (!isTouchScreen() == false) return Vector2.zero;
        return Input.GetTouch(0).position;
    }
}
