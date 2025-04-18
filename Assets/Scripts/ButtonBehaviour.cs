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

    void Start()
    {
        col = GetComponent<PolygonCollider2D>();
        rend = GetComponent<SpriteRenderer>();
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
        playedFrames++;
    }

    public bool UpdateCollider(Slice s, int b, List<Vector2[]> colliderPaths)
    {
        if (colliderPaths == null || colliderPaths.Count == 0)
        {
            RemoveCollider();
            return false;
        }

        if (col == null)
        {
            col = gameObject.AddComponent<PolygonCollider2D>();
        }
        else
        {
            (col as PolygonCollider2D).pathCount = colliderPaths.Count;
        }

        PolygonCollider2D polyCol = col as PolygonCollider2D;

        for (int i = 0; i < colliderPaths.Count; i++)
        {
            polyCol.SetPath(i, colliderPaths[i]);
        }

        if (Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            if (RayHasHit(b))
            {
                touchedFrames++;
                float score = (float)touchedFrames / s.passThreshold;
                if (s.lastFrame - s.firstFrame > 11)
                {
                    foreach (Image i in touchScoreIndicators)
                    {
                        i.fillAmount = score;
                    }
                }
                return true;
            }
        }
        return false;
    }


    public void RemoveCollider()
    {
        Destroy(col);
    }

    bool RayHasHit(int b)
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
            Debug.Log("Touched object: " + hit.collider.gameObject.name);

            string objectName = hit.collider.gameObject.name;
            if (objectName == "Button_" + b)
            {
                return(true);
            }
            else
            {
                return(false);
            }
        }
        else
        {
            return(false);
        }

    }

    public void Reset()
    {
        playedFrames = 0;
        touchedFrames = 0;
        foreach(Image i in touchScoreIndicators)
        {
            i.fillAmount = 0;
        }
    }
    
}
