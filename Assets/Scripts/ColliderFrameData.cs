using UnityEngine;

[CreateAssetMenu]
public class ColliderFrameData : ScriptableObject
{
    public ColliderFrame[] frames;
}

[System.Serializable]
public class ColliderFrame
{
    public int frameNumber;   
    public Vector2[] points;
}
