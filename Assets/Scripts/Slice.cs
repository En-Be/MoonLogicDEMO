using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Slice : ScriptableObject
{
    public int firstFrame;
    public int lastFrame;
    public int passThreshold;
    public int nextSlice;
    public int loopSlice;

    //public bool isHoldSlice;
    public bool isLastSlice;
    //public bool startsTouching;
    public bool loopOnRelease;
    public bool passOnRelease;
}
