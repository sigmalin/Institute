using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowsEditorTouch : ITouch
{
    public bool isTouchScreen()
    {
        return Input.GetMouseButton(0) || Input.GetMouseButtonDown(0);
    }

    public Vector2 getTouchPos()
    {
        return Input.mousePosition;
    }
}
