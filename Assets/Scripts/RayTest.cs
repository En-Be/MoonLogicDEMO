using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTest : MonoBehaviour
{   
    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    { 
        foreach(Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                // Construct a ray from the current touch coordinates
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray))
                {
                    Debug.Log("hit collider");
                }
                else
                {
                    Debug.Log("missed collider");
                }
            }
        }
    }

    void OnMouseDown()
    {
        // Construct a ray from the current touch coordinates
        //Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2f);
        RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        Debug.Log(hit.collider.tag);
        if (hit.collider.tag == "Button")
        {
            Debug.Log("hit collider");
        }
        else
        {
            Debug.Log("missed collider");
        }
    }


}
