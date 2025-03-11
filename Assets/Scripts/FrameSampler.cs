using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class FrameSampler : MonoBehaviour
{
    public int frameRate = 24;
    public int currentFrame;
    public int sliceCount;
    public int currentSlice;

    public SpriteRenderer rend;
    public ButtonBehaviour button;
    public AudioSource aud;

    public Sprite[] frames;
    public Sprite[] buttonFrames;
    public AudioClip clip;
    public Object[] slices;


    void Start()
    {
        Time.fixedDeltaTime = 1f/frameRate;

        rend = GetComponent<SpriteRenderer>();
        button = transform.parent.GetComponentInChildren<ButtonBehaviour>();
        aud = GetComponent<AudioSource>();
        aud.clip = clip;

        aud.Play();
    }

    void FixedUpdate()
    {   
        Slice s = (Slice)slices[currentSlice];

        Debug.Log("current frame = " + currentFrame);
        rend.sprite = frames[currentFrame];
        UpdateButton(s);

        if(button.touchedFrames < button.playedFrames) 
        {
            Debug.Log("played frames: " + button.playedFrames);
            Debug.Log("touched frames: " + button.touchedFrames);
            if(s.loopOnRelease)
            {
                currentSlice = s.loopSlice;
                SliceEnd();
            }
            else if(s.passOnRelease)
            {
                currentSlice = s.nextSlice;
                //s = (Slice)slices[currentSlice];
                SliceEnd();
            }
        }
        
        s = (Slice)slices[currentSlice];
        currentFrame++;

        if(currentFrame > s.lastFrame) // if last slice of frame has been played
        {   
            Debug.Log(s);
            Debug.Log("what");
            EndChecks(s);
        }  


    
                 
    }

    void UpdateButton(Slice s)
    {
        button.UpdateFrame(buttonFrames[currentFrame]);

        if(currentFrame >= s.buttonStart && currentFrame <= s.buttonFinish)
        {
            button.UpdateCollider(s);
        }
        else
        {
            button.RemoveCollider();
        }
    }

    void EndChecks(Slice s)
    {
        //Debug.Log("end checks");
        if(s.isLastSlice)
        {
            Finish();
        }
        else if(s.passThreshold == 0 || button.touchedFrames >= s.passThreshold && s.passOnRelease == false) // swap this conditional for actual threshold checks
        {
            currentSlice = s.nextSlice;
        }
        else
        {
            currentSlice = s.loopSlice;
        }

        SliceEnd();
    }

    void SliceEnd()
    {
        Debug.Log("slice end");
        Slice ns = (Slice)slices[currentSlice];
        currentFrame = ns.firstFrame;
        float targetTime = (float)currentFrame/frameRate;
        aud.time = targetTime;

        button.Reset();

        Debug.Log("Starting slice " + currentSlice);
    }

    void Finish()
    {
        Debug.Log("Exiting scene");
        SceneManager.LoadScene("Menu");
    }
}
