using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad;
    Vector3 mousePos;

    void OnEnable()
    {
        mousePos = Input.mousePosition; // prevents prevous touch position triggering before enabled
    }

    public void OnMouseDown()
    {
        // Load scene
        if(mousePos.x != Input.mousePosition.x)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public void OnMouseEnter()
    {
        // Load scene
        if(mousePos.x != Input.mousePosition.x)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
