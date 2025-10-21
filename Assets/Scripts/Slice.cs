using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Slice : ScriptableObject
{
    public int firstFrame;
    public int lastFrame;
    public int passThreshold;
    public int loopChances;
    public int releaseChances;
    public int[] nextSlice;
    public int failSlice;

    public bool showScore;
    public bool fadesWithChances;
    public bool isLastSlice;
    public bool loopOnRelease;
    public bool passOnRelease;
    public string[] requiredToPass;
}
