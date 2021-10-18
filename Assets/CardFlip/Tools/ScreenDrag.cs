using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenDrag : MonoBehaviour
{
    System.Action progress;

    ITouch screenTouch;

    Vector2 touchStart;

    DrawCard card;

    Vector2 worldSize;
    Vector2 dragDir;

    TweenVector2 tween;

    // Start is called before the first frame update
    void Start()
    {
        Debug.LogFormat("screen width : {0}, height : {1}",Screen.width, Screen.height);
        Debug.LogFormat("Camera size : {0}, aspect : {1}", Camera.main.orthographicSize, Camera.main.aspect);

        this.updateWorldSize();

        card = new DrawCard();

        tween = new TweenVector2();

        progress = onNormal;

        if(Application.platform == RuntimePlatform.WindowsEditor)
        {
            screenTouch = new WindowsEditorTouch();
        }
        else
        {
            screenTouch = new MobileTouch();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (screenTouch == null) return;

        if (progress != null)
        {
            progress.Invoke();
        }

        if (card != null)
        {
            this.updateWorldSize();
            card.Flip(worldSize, dragDir);
            card.Render();
        }
    }

    private void OnDestroy()
    {
        if (card != null)
        {
            card.Release();
            card = null;
        }
    }

    private void updateWorldSize()
    {
        worldSize.y = Camera.main.orthographicSize * 2f;
        worldSize.x = Camera.main.aspect * worldSize.y;
    }

    void onNormal()
    {
        moveTween();

        if (screenTouch.isTouchScreen())
        {
            touchStart = screenTouch.getTouchPos();
            progress = onPress;
        }
    }

    void onPress()
    {
        if (screenTouch.isTouchScreen())
        {
            Vector2 touchPos = screenTouch.getTouchPos();
            transScreen2World(touchPos - touchStart);
        }
        else
        {
            progress = onNormal;
            freeDrag();
        }
    }

    void transScreen2World(Vector2 touchDiff)
    {
        dragDir.x = touchDiff.x * worldSize.x / Screen.width;
        dragDir.y = touchDiff.y * worldSize.y / Screen.height;

        //Debug.LogFormat("touch start : {0}, mov : {1}", touchStart, touchDiff);
        //Debug.LogFormat("world diffX : {0}, diffY : {1}", dragDir.x, dragDir.y);
    }

    void freeDrag()
    {
        tween.Get(dragDir).To(Vector2.zero, 0.25f);
    }

    void moveTween()
    {
        dragDir = tween.Move(Time.deltaTime);

        //Debug.LogFormat("world diffX : {0}, diffY : {1}", dragDir.x, dragDir.y);
    }

    ///

    private void OnGUI()
    {
        if (card.isFlop)
        {
            if (GUI.Button(new Rect(0f, 0f, 200f, 200f), "replay"))
            {
                card.isFlop = false;
            }
        }        
    }
}
