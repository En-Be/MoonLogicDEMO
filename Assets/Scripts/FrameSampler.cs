using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

[System.Serializable]
public class SpriteGroup
{
    public List<Sprite> sprites = new List<Sprite>();
}

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
    public List<SpriteGroup> buttonFrames = new List<SpriteGroup>();
    public List<ColliderFrameData> buttonColliderDatas;

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

        for(int i = 0; i < buttons.Length; i++)
        {
            ButtonBehaviour button = buttons[i];

            if(button.touchedFrames < button.playedFrames) 
            {

                if(s.loopOnRelease)
                {
                    CheckReleaseChances(s);
                }
                else if(s.passOnRelease)
                {
                    PlayerPrefs.SetInt(s.name, 1);
                    currentSlice = s.nextSlice[i];
                    SliceEnd(s);
                }
            }
        }
        
        currentFrame++;

        if(currentFrame > s.lastFrame)
        {   
            EndChecks(s);
        }     
                 
    }

    void UpdateButton(Slice s)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            ButtonBehaviour button = buttons[i];

            Sprite foundSprite = FindSpriteWithNumber(i, currentFrame);

            List<Vector2[]> colliderPaths = FindColliderWithNumber(i, currentFrame);
            
            if (foundSprite == null || colliderPaths == null)
            {
                button.RemoveFrame();
                button.RemoveCollider();
                //Debug.Log("sprite or paths is null");
            }
            else
            {
                button.UpdateFrame(foundSprite);
                bool touching = button.UpdateCollider(s, i, colliderPaths, currentFrame);

                if (touching && s.loopOnRelease)
                {
                    releaseChancesToUse = s.releaseChances;
                    button.touchedFrames = button.playedFrames;
                }
                if (touching && !s.loopOnRelease && !s.passOnRelease)
                {
                    rend.color = Color.white;
                }
            }
        }
    }

    Sprite FindSpriteWithNumber(int button, int number)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string expectedName = $"{sceneName}_button{button}_{number:D4}";
        return buttonFrames[button].sprites.FirstOrDefault(sprite => sprite.name == expectedName);
    }

    List<Vector2[]> FindColliderWithNumber(int button, int number)
    {

        var dataGroup = buttonColliderDatas[button];

        var frameData = dataGroup.frames.FirstOrDefault(frame => frame.frameNumber == number);

        if (frameData == null)
            return null;

        return new List<Vector2[]> { frameData.points };
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
        if(s.isLastSlice)
        {
            PlayerPrefs.SetInt(s.name, 1);
            Finish();
        }
        else
        {
            for(int i = 0; i < buttons.Length; i++)
            {
                ButtonBehaviour button = buttons[i];
                if(s.passThreshold == 0 || button.touchedFrames >= s.passThreshold && s.passOnRelease == false)
                {
                    //Debug.Log("button = " + i);
                    //Debug.Log("next slice = " + s.nextSlice[i]);
                    PlayerPrefs.SetInt(s.name, 1);
                    currentSlice = s.nextSlice[i];
                    Slice ns = (Slice)slices[currentSlice];
                    loopChancesToUse = ns.loopChances;
                    releaseChancesToUse = ns.releaseChances;
                    break;
                }
            }
        }

        if (s.loopChances > 0)
        {
            CheckLoopChances(s);
        }

        SliceEnd(s);
    }

    void CheckLoopChances(Slice s)
    {
        loopChancesToUse--;
        if(s.fadesWithChances)
        {
            float f = (float)1/s.loopChances;
            f = (float)f*loopChancesToUse;
            Color c = new Color(f, f, f, 1);
            rend.color = c;
        }
        
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
            //Debug.Log("resetting button/touch indicator");
            button.Reset(); // might need to change this to prevent touch score indicator flicker
        }
        //Debug.Log("Starting slice " + currentSlice);
    }

    void Finish()
    {
        //Debug.Log("Exiting scene");
        SceneManager.LoadScene(loadsTo);
    }
}
