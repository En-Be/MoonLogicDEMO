using UnityEngine;
using TMPro;
using System; 
using System.Linq; 

/// <summary>
/// Loads "sliceSave.json" from the Resources folder, dynamically calculates a score
/// based on the count of 'true' boolean values in the JSON, regardless of the key names.
/// 
/// This script uses string parsing to handle JSON with dynamic keys at the root level, 
/// as standard Unity's JsonUtility cannot deserialize a root dictionary.
/// </summary>
public class ProgressDisplay : MonoBehaviour
{
    [Tooltip("The TextMeshPro UI element used to display the final score.")]
    public TextMeshProUGUI scoreText;

    // The name of the JSON file in the Resources folder (without extension)
    private const string JsonFilename = "sliceSave";

    void Start()
    {
        if (scoreText == null)
        {
            Debug.LogError("ScoreDisplay requires a TextMeshProUGUI component assigned to scoreText.");
            return;
        }

        LoadAndCalculateScore();
    }

    private void LoadAndCalculateScore()
    {
        // 1. Load the TextAsset from the Resources folder. (Requires file to be at Assets/Resources/sliceSave.json)
        TextAsset jsonFile = Resources.Load<TextAsset>(JsonFilename);

        if (jsonFile == null)
        {
            scoreText.text = "ERROR: Save file not found!";
            Debug.LogError($"Could not find file: Assets/Resources/{JsonFilename}.json. Check the file location.");
            return;
        }

        // 2. Calculate the score dynamically by parsing the raw JSON text.
        int totalBooleans;
        int score = CalculateDynamicBooleanScore(jsonFile.text, out totalBooleans);

        // 3. Update the UI.
        scoreText.text = $"Slices Collected: {score} / {totalBooleans}";

        Debug.Log($"Score loaded successfully. Total score is: {score} out of {totalBooleans} possible items.");
    }

    /// <summary>
    /// Parses the raw JSON string to dynamically count the number of 'true' boolean values.
    /// </summary>
    /// <param name="jsonText">The raw content of the JSON file.</param>
    /// <param name="totalBooleans">Outputs the total number of boolean entries found.</param>
    /// <returns>The count of boolean values set to true (the score).</returns>
    private int CalculateDynamicBooleanScore(string jsonText, out int totalBooleans)
    {
        int trueCount = 0;
        
        // 1. Clean up the string to remove root brackets and normalize.
        string cleanedJson = jsonText.Trim();
        if (cleanedJson.StartsWith("{") && cleanedJson.EndsWith("}"))
        {
            // Remove the outer curly braces
            cleanedJson = cleanedJson.Substring(1, cleanedJson.Length - 2).Trim();
        }

        // Handle empty file case
        if (string.IsNullOrEmpty(cleanedJson))
        {
            totalBooleans = 0;
            return 0;
        }

        // 2. Split into individual key-value pairs (entries).
        // Using a simple split, assuming simple JSON structure (no nested objects/arrays).
        string[] entries = cleanedJson.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        
        totalBooleans = 0; // Will count only valid boolean entries

        // 3. Process each entry.
        foreach (string entry in entries)
        {
            // Split the entry into key and value around the first colon.
            string[] parts = entry.Split(new char[] { ':' }, 2);

            // Ensure the entry has a key and a value.
            if (parts.Length != 2)
            {
                continue;
            }

            // The value is the second part (parts[1]). Trim whitespace and remove any quotes.
            string value = parts[1].Trim().Replace("\"", "").ToLower();

            // Check if the cleaned value is a recognized boolean value.
            if (value == "true" || value == "false")
            {
                totalBooleans++;

                // If the value is true, increment the score.
                if (value == "true")
                {
                    trueCount++;
                }
            }
        }

        return trueCount;
    }
}
