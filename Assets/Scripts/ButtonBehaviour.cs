using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour
{
    public PolygonCollider2D col;
    public int playedFrames;
    public int touchedFrames;
    public SpriteRenderer rend;
    Vector3 mousePos;
    public Camera cam;
    public Image[] touchScoreIndicators;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<PolygonCollider2D>();
        rend = GetComponent<SpriteRenderer>();
        //Debug.Log(Application.platform);
        Reset();
    }

    public void UpdateFrame(Sprite s)
    {
        rend.sprite = s;
        playedFrames++;
    }

    public void RemoveFrame()
    {
        rend.sprite = null;
        Debug.Log("rend.sprite = " + rend.sprite);
        playedFrames++;
    }

    public void UpdateCollider(Slice s)
    {
        Destroy(col);
        col = gameObject.AddComponent<PolygonCollider2D>();

        if(Input.touchCount > 0 || (Input.GetMouseButton(0)))
        {   
            //Debug.Log("Frame is touched");
            if(RayHasHit())
            {
                touchedFrames++;
                float score = (float)touchedFrames/s.passThreshold;
                foreach(Image i in touchScoreIndicators)
                {
                    i.fillAmount = score;
                }
            }
        }
        //Debug.Log("Button collider updated");
    }

    public void RemoveCollider()
    {
        Destroy(col);
        //Debug.Log("Button collider removed");
    }

    bool RayHasHit()
    {
        RaycastHit2D hit;

        if(Application.platform == RuntimePlatform.WindowsEditor)
        {
            hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        }
        else
        {
            Touch touch = Input.GetTouch(0);
            hit = Physics2D.Raycast(cam.ScreenToWorldPoint(touch.position), Vector2.zero);
        }

        if(hit)
        {
            return(true);
        }
        else
        {
            return(false);
        }
    }

    public void Reset()
    {
        //Debug.Log("touched frames = " + touchedFrames);
        playedFrames = 0;
        touchedFrames = 0;
        foreach(Image i in touchScoreIndicators)
        {
            i.fillAmount = 0;
        }
    }
    
}
