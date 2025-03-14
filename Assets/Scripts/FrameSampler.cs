using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public string loadsTo;

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
        Sprite foundSprite = FindSpriteWithNumber(buttonFrames, currentFrame);
        if(foundSprite == null)
        {
            Debug.Log("No sprite found");
            button.RemoveFrame();
            button.RemoveCollider();
        }
        else
        {
            Debug.Log("found sprite " + foundSprite);
            button.UpdateFrame(foundSprite);
            button.UpdateCollider(s);
        }
    }

    Sprite FindSpriteWithNumber(Sprite[] sprites, int number)
    {
        string sceneName = SceneManager.GetActiveScene().name; // Get scene name in lowercase
        string expectedName = $"{sceneName}_buttonFrames_{number:D4}"; // Format number as 4-digit (e.g., 0001)
        return sprites.FirstOrDefault(sprite => sprite.name == expectedName);
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
        SceneManager.LoadScene(loadsTo);
    }
}
