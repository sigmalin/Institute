using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITouch
{
    bool isTouchScreen();

    Vector2 getTouchPos();
}

