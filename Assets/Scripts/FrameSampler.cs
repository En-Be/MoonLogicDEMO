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
    public int loopChancesToUse;
    public int releaseChancesToUse;

    public SpriteRenderer rend;
    public ButtonBehaviour[] buttons;
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
        buttons = transform.parent.GetComponentsInChildren<ButtonBehaviour>();
        aud = GetComponent<AudioSource>();
        aud.clip = clip;

        aud.Play();

        Slice s = (Slice)slices[currentSlice];
        currentFrame = s.firstFrame;
        loopChancesToUse = s.loopChances;
        releaseChancesToUse = s.releaseChances;
        float targetTime = (float)currentFrame/frameRate;
        aud.time = targetTime;
    }

    void FixedUpdate()
    {   
        Slice s = (Slice)slices[currentSlice];

        //Debug.Log("current frame = " + currentFrame);
        rend.sprite = frames[currentFrame];

        if(s.loopChances == 0 || s.loopChances == loopChancesToUse)
        {
            rend.color = Color.white;           
        }

        UpdateButton(s);

        foreach(ButtonBehaviour button in buttons)
        {
            if(button.touchedFrames < button.playedFrames) 
            {
                //Debug.Log("played frames: " + button.playedFrames);
                //Debug.Log("touched frames: " + button.touchedFrames);
                if(s.loopOnRelease)
                {
                    CheckReleaseChances(s);
                }
                else if(s.passOnRelease)
                {
                    currentSlice = s.nextSlice;
                    //Debug.Log("current slice updated to next slice ");
                    SliceEnd(s);
                }
            }
        }
        
        currentFrame++;

        if(currentFrame > s.lastFrame)
        {   
            //Debug.Log(s);
            EndChecks(s);
        }     
                 
    }

    void UpdateButton(Slice s)
    {
        foreach(ButtonBehaviour button in buttons)
        {
            Sprite foundSprite = FindSpriteWithNumber(buttonFrames, currentFrame);
            if(foundSprite == null)
            {
                //Debug.Log("No sprite found");
                button.RemoveFrame();
                button.RemoveCollider();
            }
            else
            {
                //Debug.Log("found sprite " + foundSprite);
                button.UpdateFrame(foundSprite);
                bool touching = button.UpdateCollider(s);
                Debug.Log("touching = " + touching);
                if(touching && s.loopOnRelease)
                {
                    releaseChancesToUse = s.releaseChances;
                    button.touchedFrames = button.playedFrames;
                }
                if(touching && !s.loopOnRelease && !s.passOnRelease)
                {
                    rend.color = Color.white;
                }
            }
        }
    }

    Sprite FindSpriteWithNumber(Sprite[] sprites, int number)
    {
        string sceneName = SceneManager.GetActiveScene().name; 
        string expectedName = $"{sceneName}_buttonFrames_{number:D4}"; 
        return sprites.FirstOrDefault(sprite => sprite.name == expectedName);
    }

    void CheckReleaseChances(Slice s)
    {
        if(releaseChancesToUse == 0)
        {
            currentSlice = s.failSlice;
            Slice ns = (Slice)slices[currentSlice];
            loopChancesToUse = ns.loopChances;
            releaseChancesToUse = ns.releaseChances;
            
            SliceEnd(s);
        }
        else
        {
            releaseChancesToUse--;
            float f = (float)1/s.releaseChances;
            f = (float)f*releaseChancesToUse;
            Color c = new Color(f, f, f, 1);
            rend.color = c;
        }
    }

    void EndChecks(Slice s)
    {
        foreach(ButtonBehaviour button in buttons)
        {
            if(s.isLastSlice)
            {
                Finish();
            }
            else if(s.passThreshold == 0 || button.touchedFrames >= s.passThreshold && s.passOnRelease == false)
            {
                currentSlice = s.nextSlice;
                //Debug.Log("current slice updated to next slice");
                Slice ns = (Slice)slices[currentSlice];
                loopChancesToUse = ns.loopChances;
                releaseChancesToUse = ns.releaseChances;
            }
            else if (s.loopChances > 0)
            {
                CheckLoopChances(s);
            }
        }

        SliceEnd(s);
    }

    void CheckLoopChances(Slice s)
    {
        loopChancesToUse--;
        float f = (float)1/s.loopChances;
        f = (float)f*loopChancesToUse;
        //Debug.Log("loop chances to use = " + loopChancesToUse);
        //Debug.Log("colour value = " + f);
        Color c = new Color(f, f, f, 1);
        rend.color = c;
        
        if(loopChancesToUse == 0)
        {
            currentSlice = s.failSlice;
            Slice ns = (Slice)slices[currentSlice];
            loopChancesToUse = ns.loopChances;
            releaseChancesToUse = ns.releaseChances;
        }
        
    }

    void SliceEnd(Slice s)
    {
        Slice ns = (Slice)slices[currentSlice];
        currentFrame = ns.firstFrame;

        float targetTime = (float)currentFrame/frameRate;
        aud.time = targetTime;
        foreach(ButtonBehaviour button in buttons)
        {
            button.Reset();
        }
        Debug.Log("Starting slice " + currentSlice);
    }

    void Finish()
    {
        Debug.Log("Exiting scene");
        SceneManager.LoadScene(loadsTo);
    }
}
