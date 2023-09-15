using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SlidingPanel : MonoBehaviour
{
    public void SlideIn(Vector2 pos, Vector2 size)
    {
        // easing
        // https://www.youtube.com/watch?v=YqMpVCPX2ls
        // https://codepen.io/jhnsnc/pen/LpVXGM
        float animTime = 0.65f;
        transform.LeanMoveLocal(pos, animTime).setEaseOutExpo();
        transform.LeanScale(size, animTime).setEaseOutExpo();
        transform.GetComponent<Image>().gameObject.LeanAlpha(1.0f, animTime);
    }
    public void SlideOut(Vector2 pos, Vector2 size)
    {
        float animTime = 0.65f;
        transform.LeanMoveLocal(pos, animTime).setEaseOutExpo();
        transform.LeanScale(size, animTime).setEaseOutExpo();
        transform.GetComponent<Image>().gameObject.LeanAlpha(0.3f, animTime);
    }
}
