using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenVector2
{
    Vector2 src;
    Vector2 dst;
    float time;
    float elapsed;

    public TweenVector2()
    {
        src = Vector2.zero;
        dst = Vector2.zero;
        time = 0f;
        elapsed = 0f;
    }

    public TweenVector2 Get(Vector2 _value)
    {
        src = _value;
        elapsed = 0f;
        return this;
    }

    public TweenVector2 To(Vector2 _value, float _time)
    {
        dst = _value;
        time = _time;
        return this;
    }

    public Vector2 Move(float _delta)
    {
        if (time <= 0f) return dst;

        elapsed = Mathf.Min(elapsed + _delta, time);

        float t = elapsed / time;
        return Vector2.Lerp(src, dst, t*t);
    }
}
