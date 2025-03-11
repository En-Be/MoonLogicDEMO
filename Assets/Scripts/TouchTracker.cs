using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchTracker : MonoBehaviour
{
    public TimelineSampler tS;
    public int touchedFrames;
    public bool touching;
    public bool colliding;
    public Image[] touchScoreIndicators;
    public int frameCount;
    
    Vector3 mousePos;

    void Start()
    {
        Reset();
    }

    void Update()
    {
        if(Input.touchCount != 0 || Application.platform == RuntimePlatform.WindowsEditor)
        {
            colliding = true;
        }
        else
        {
            colliding = false;
        }

        if(touching && colliding)
        {
            touchedFrames++;
            float score = (float)touchedFrames/tS.currentSlice.passThreshold;
            foreach(Image i in touchScoreIndicators)
            {
                i.fillAmount = score;
            }
        }
        /*
        if(tS.currentSlice.isHoldSlice)
        {
            foreach(Image i in touchScoreIndicators)
            {
                i.fillAmount = 0;
            }
        }
        */
    }

    void OnMouseDown()
    {
        Debug.Log("Sprite Clicked!");
        if(mousePos != Input.mousePosition) // prevents prevous touch position triggering a false positive
        {
            touching = true;
            tS.TouchStarted();
        }
    }

    void OnMouseEnter()
    {
        Debug.Log(mousePos);
        Debug.Log(Input.mousePosition);
        if(mousePos.x != Input.mousePosition.x) // prevents prevous touch position triggering a false positive
        {
            Debug.Log("Sprite Entered!");
            touching = true;
            tS.TouchStarted();
        }
    }

    void OnMouseUp()
    {
        mousePos = Input.mousePosition;
        Debug.Log("Sprite Released!");
        if(touching)
        {
            touching = false;
            tS.TouchFinished();
        }
    }

    void OnMouseExit()
    {
        mousePos = Input.mousePosition;
        Debug.Log("Sprite Exited!");
        if(touching)
        {
            touching = false;
            tS.TouchFinished();
        }
    }

    public void Reset(bool startsTouching = false)
    {
        touchedFrames = 0;
        //touchScoreIndicator.fillAmount = 0;
        foreach(Image i in touchScoreIndicators)
        {
            i.fillAmount = 0;
        }
        if(startsTouching == false)
        {
            touching = false;
        }
        mousePos = Input.mousePosition;
        frameCount = 0;
    }

    public bool CheckIfThresholdPassed(Slice slice)
    {
        Debug.Log("slice threshold = " + slice.passThreshold);
        if(touchedFrames != 0 || slice.passThreshold == 0)
        {
            print("touchedFrames = " + touchedFrames);

            if(touchedFrames >= slice.passThreshold)
            {
                Debug.Log("pass");
                return true;
            }
            else
            {
                Debug.Log("fail");
                return false;
            }
            
        }
        else
        {
            print("no touch");
            return false;
        }
    }
}
