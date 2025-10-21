using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SliceTracker : MonoBehaviour
{
    // Save file inside the project folder
    private const string saveFilePath = "Assets/Resources/sliceSave.json";

    /// <summary>
    /// Generates or updates the slice save JSON file. Editor-only.
    /// </summary>
    public void GenerateSliceSave()
    {
#if UNITY_EDITOR
        // Find all ScriptableObjects of type Slice in the project
        string[] guids = AssetDatabase.FindAssets("t:Slice");
        Debug.Log($"Found {guids.Length} Slice assets.");

        Dictionary<string, bool> sliceDict = new Dictionary<string, bool>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (!sliceDict.ContainsKey(fileName))
                sliceDict[fileName] = false; // default value
        }

        // Build JSON manually
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("{");
        int count = 0;
        foreach (var kvp in sliceDict)
        {
            string line = $"  \"{kvp.Key}\": {kvp.Value.ToString().ToLower()}";
            count++;
            if (count < sliceDict.Count)
                line += ",";
            sb.AppendLine(line);
        }
        sb.AppendLine("}");

        // Write to file
        File.WriteAllText(saveFilePath, sb.ToString());

        // Refresh AssetDatabase so it appears in Unity
        AssetDatabase.Refresh();

        Debug.Log($"✅ Slice save file created/updated at: {saveFilePath}");
#else
        Debug.LogWarning("Slice generation only works in the Editor.");
#endif
    }
}

#if UNITY_EDITOR
// Custom Editor to add a button in the Inspector
[CustomEditor(typeof(SliceTracker))]
public class SliceTrackerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SliceTracker tracker = (SliceTracker)target;

        if (GUILayout.Button("Generate Slice Save"))
        {
            tracker.GenerateSliceSave();
        }
    }
}
#endif
