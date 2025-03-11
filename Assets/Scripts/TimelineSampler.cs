using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

public class TimelineSampler : MonoBehaviour
{
    public Slice currentSlice;

    public float currentSliceScore;

    static AudioSource aud;
    public static PlayableDirector pD;
    public List<IMarker> markers = new List<IMarker>();

    public TouchTracker toTr;

    void Start()
    {
        Application.targetFrameRate = 24;

        pD = GetComponentInParent<PlayableDirector>();
        toTr = GetComponentInChildren<TouchTracker>();
        TimelineAsset tL = pD.playableAsset as TimelineAsset;
        IMarker[] allMarkers = tL.GetOutputTrack(1).GetMarkers().OrderBy((marker) => marker.time).ToArray();
        for(int i = 0; i < allMarkers.Length; i++)
        {
            if(i%2 == 0)
            {
                markers.Add(allMarkers[i]); // only add the even markers (leave out the odds as they are slice ends)
            }
        }
        Debug.Log("number of markers = " + markers.Count);
    }
    
    void Update()
    {

    }

    public void SliceStart(Slice slice)
    {
        currentSlice = slice;
        Debug.Log("Starting slice " + currentSlice);
        //toTr.Reset(currentSlice.startsTouching);
        CheckSliceParams();
    }

    public void SliceEnd()
    {
        if(currentSlice.passOnRelease)
        {
            if(toTr.touching)
            {
                GoToCheckpointSlice();
            }
            else
            {
                // play next slice
            }
        }
        else if(toTr.CheckIfThresholdPassed(currentSlice))
        {
            if(currentSlice.isLastSlice)
            {
                Finish();
            }
            else
            {
                // play next slice
            }            
        }
        else
        {
            GoToCheckpointSlice();
        }
    }

    public void GoToCheckpointSlice()
    {
        pD.time = markers[currentSlice.loopSlice].time;
        //SliceStart(currentSlice);
    }

    public void TouchStarted()
    {

    }

    public void TouchFinished()
    {
        if(currentSlice.loopOnRelease)
        {
            GoToCheckpointSlice();
        }
    }

    public void CheckSliceParams()
    {

    }

    public void Finish()
    {
        SceneManager.LoadScene("Menu");
    }
}
