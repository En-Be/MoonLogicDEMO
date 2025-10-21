#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public static class SliceDataUtility
{
    // Constants for PlayerPrefs
    private const int BOOL_TRUE = 1;
    private const int BOOL_FALSE = 0;

    // Constants for Manifest File Generation
    private const string ManifestFilename = "sliceKeys.txt";
    private const string ResourcesFolderPath = "Assets/Resources";
    private static readonly string ManifestFilePath = Path.Combine(ResourcesFolderPath, ManifestFilename);

    /// <summary>
    /// Finds all 'Slice' ScriptableObjects in the project assets and generates 
    /// a manifest file (sliceKeys.txt) in the Resources folder containing a 
    /// comma-separated list of all slice names.
    /// </summary>
    public static void GenerateKeyManifestFile()
    {
        // 1. Find all Slices using the AssetDatabase
        string[] guids = AssetDatabase.FindAssets("t:Slice");
        
        // 2. Extract asset names
        List<string> sliceNames = new List<string>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // Load as Object since 'Slice' class definition isn't provided here
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path); 
            
            if (asset != null)
            {
                sliceNames.Add(asset.name);
            }
        }
        
        // 3. Combine names into a comma-separated string, using a newline for readability
        string content = string.Join(",\n", sliceNames);
        
        // 4. Ensure the Resources folder exists
        if (!Directory.Exists(ResourcesFolderPath))
        {
            Directory.CreateDirectory(ResourcesFolderPath);
            AssetDatabase.Refresh();
        }

        // 5. Write to file
        File.WriteAllText(ManifestFilePath, content);
        
        // 6. Refresh AssetDatabase so the file appears in Unity's Project window
        AssetDatabase.Refresh();
    }
    
    // -------------------------------------------------------------------------
    // PlayerPrefs Management Functions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Finds all 'Slice' ScriptableObjects in the project assets, creates a PlayerPrefs 
    /// entry for each, initializing its state to FALSE (0), and then generates 
    /// the required key manifest file.
    /// </summary>
    [MenuItem("Tools/Slices/Generate Initial PlayerPrefs Data")]
    public static void GenerateInitialSliceData()
    {
        // 1. Find all Slices using the AssetDatabase
        string[] guids = AssetDatabase.FindAssets("t:Slice");
        
        List<string> generatedKeys = new List<string>();
        
        // 2. Load the actual assets and extract names to use as keys
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path); 
            
            if (asset != null)
            {
                string keyName = asset.name; // Use Object.name
                
                // Set the individual PlayerPrefs key to 0 (false)
                PlayerPrefs.SetInt(keyName, BOOL_FALSE);
                generatedKeys.Add(keyName);
            }
        }
        
        // 3. Save the keys to disk
        PlayerPrefs.Save();

        // 4. GENERATE THE MANIFEST FILE
        GenerateKeyManifestFile();

        // 5. Log the combined success message
        Debug.Log($"✅ [SliceDataUtility] Generated/reset {generatedKeys.Count} Slice keys in PlayerPrefs AND updated the key manifest file at {ManifestFilePath}.");
        Debug.Log($"Keys created/reset with value 0 (false):\n{string.Join(", ", generatedKeys)}");
    }
    
    /// <summary>
    /// Finds all 'Slice' ScriptableObjects and deletes their corresponding PlayerPrefs keys.
    /// </summary>
    [MenuItem("Tools/Slices/Clear All Slice PlayerPrefs Keys")]
    public static void ClearAllSliceKeys()
    {
        string[] guids = AssetDatabase.FindAssets("t:Slice");
        List<string> clearedKeys = new List<string>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            
            if (asset != null)
            {
                string keyName = asset.name;
                
                if (PlayerPrefs.HasKey(keyName))
                {
                    PlayerPrefs.DeleteKey(keyName);
                    clearedKeys.Add(keyName);
                }
            }
        }
        
        PlayerPrefs.Save();
        
        if (clearedKeys.Count > 0)
        {
            Debug.Log($"[SliceDataUtility] Successfully cleared {clearedKeys.Count} PlayerPrefs keys.");
            Debug.Log($"Keys cleared:\n{string.Join(", ", clearedKeys)}");
        }
        else
        {
            Debug.LogWarning("[SliceDataUtility] No Slice keys were found to clear.");
        }
    }
    
    /// <summary>
    /// Finds all 'Slice' ScriptableObjects, retrieves their corresponding PlayerPrefs 
    /// values, and prints them to the console for debugging purposes.
    /// </summary>
    [MenuItem("Tools/Slices/Print All Slice PlayerPrefs")]
    public static void PrintAllSlicePlayerPrefs()
    {
        // 1. Find all Slices using the AssetDatabase to get the list of known keys
        string[] guids = AssetDatabase.FindAssets("t:Slice");
        
        List<string> outputLines = new List<string>();
        
        outputLines.Add("--- Slice PlayerPrefs Status ---");
        
        // 2. Loop through assets, get the key name, and retrieve the value from PlayerPrefs
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path); 
            
            if (asset != null)
            {
                string keyName = asset.name;
                // Get the integer value, defaulting to 0 (BOOL_FALSE) if the key doesn't exist
                int value = PlayerPrefs.GetInt(keyName, BOOL_FALSE);
                
                string status = (value == BOOL_TRUE) ? "COMPLETED" : "FALSE (Not Collected)";
                
                outputLines.Add($"[{keyName}] = {value} ({status})");
            }
        }
        
        if (outputLines.Count == 1) // Only the header line exists
        {
            outputLines.Add("No Slice assets found in the project.");
        }

        // 3. Print the results to the console
        Debug.Log(string.Join("\n", outputLines));
    }
}
#endif
