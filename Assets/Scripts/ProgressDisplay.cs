using UnityEngine;
using TMPro;
using System.Linq; 
using System.Collections.Generic;

/// <summary>
/// Loads the list of all slice keys from a TextAsset manifest, reads their 
/// boolean status from PlayerPrefs, and displays the completion percentage.
/// </summary>
public class ProgressDisplay : MonoBehaviour
{
    [Tooltip("The TextMeshPro UI element used to display the final percentage progress.")]
    public TextMeshProUGUI scoreText;

    // The name of the TextAsset in Resources containing the comma-separated list of all slice keys.
    private const string KeyManifestFilename = "sliceKeys"; 
    
    // PlayerPrefs uses integers (0 or 1) to store booleans
    private const int BOOL_TRUE = 1;

    void Start()
    {
        if (scoreText == null)
        {
            Debug.LogError("ProgressDisplay requires a TextMeshProUGUI component assigned to scoreText.");
            return;
        }
    }

    void Update()
    {
        LoadAndCalculateProgress();
    }
    
    /// <summary>
    /// Reads the key manifest and checks PlayerPrefs to calculate progress.
    /// </summary>
    private void LoadAndCalculateProgress()
    {
        // 1. Load the manifest of all slice keys from Resources.
        // NOTE: This file is essential because PlayerPrefs does not provide an API 
        // to retrieve a list of all existing keys, which is needed for the 'totalSlices' count.
        TextAsset keyManifest = Resources.Load<TextAsset>(KeyManifestFilename);

        if (keyManifest == null)
        {
            scoreText.text = "ERROR: Manifest Missing!";
            Debug.LogError($"Could not find required manifest file: Assets/Resources/{KeyManifestFilename}.txt. Cannot calculate total slices.");
            return;
        }

        // 2. Parse the keys (assuming keys are separated by commas or newlines)
        // Clean up the text by removing whitespace and splitting by common delimiters
        string[] rawKeys = keyManifest.text.Split(new char[] { ',', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        // Remove leading/trailing whitespace from keys
        List<string> sliceKeys = rawKeys.Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k)).ToList();
        
        int totalSlices = sliceKeys.Count;
        int completedSlices = 0;

        // 3. Check the status of each key in PlayerPrefs
        foreach (string key in sliceKeys)
        {
            // PlayerPrefs.GetInt retrieves the boolean status (1 for true, 0 for false/missing).
            // The default value of 0 correctly treats missing keys as BOOL_FALSE (uncollected).
            if (PlayerPrefs.GetInt(key, 0) == BOOL_TRUE)
            {
                completedSlices++;
            }
        }

        // 4. Calculate Percentage.
        float percentage = 0f;
        
        if (totalSlices > 0)
        {
            percentage = (float)completedSlices / totalSlices * 100f;
        }

        // 5. Update the UI.
        // :F1 formats the float to one decimal place.
        scoreText.text = $"Progress: {percentage:F1}%";

        //Debug.Log($"Progress loaded successfully from PlayerPrefs. Total progress is: {percentage:F1}% ({completedSlices} out of {totalSlices} possible items).");
    }
}
