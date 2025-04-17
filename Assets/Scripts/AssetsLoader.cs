#if UNITY_EDITOR // => Ignore from here to next endif if not in editor

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SpriteGroup
{
    public List<Sprite> sprites = new List<Sprite>();
}

public class AssetsLoader : MonoBehaviour
{
    public FrameSampler sampler; 
    public string sceneName;

    [CustomEditor(typeof(AssetsLoader))]
    public class AssetsLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            AssetsLoader assetsLoader = target as AssetsLoader;
            assetsLoader.sampler = assetsLoader.GetComponentInChildren<FrameSampler>();
            assetsLoader.sceneName = SceneManager.GetActiveScene().name;

            if (GUILayout.Button("Load All"))
            {
                assetsLoader.LoadAll();
            }

            if (GUILayout.Button("Load Frames"))
            {
                assetsLoader.LoadFrames();
            }

            if (GUILayout.Button("Load Button Frames"))
            {
                assetsLoader.LoadButtonFrames();
            }

            if (GUILayout.Button("Load Audio"))
            {
                assetsLoader.LoadAudio();
            }

            if (GUILayout.Button("Load Slices"))
            {
                assetsLoader.LoadSlices();
            }
        }
    }

    public void LoadAll()
    {
        LoadFrames();
        LoadButtonFrames();
        LoadAudio();
        LoadSlices();
    }

    public void LoadFrames()
    {
        List<Sprite> loadedImages = new List<Sprite>();

        // Get all image files in the specified folder
        string[] imagePaths = Directory.GetFiles("Assets/Scenes/" + sceneName + "/Frames/", "*.jpg"); // Adjust file extension as needed
        foreach (string imagePath in imagePaths)
        {
            // Load the image
            Sprite image = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            if (image != null)
            {
                loadedImages.Add(image);
            }
        }

        // Assign the loaded images to the array
        sampler.frames = loadedImages.ToArray();
        Debug.Log("Loaded " + loadedImages.Count + " images.");
    }

    public void LoadButtonFrames()
    {
        // Clear the list first (optional but cleaner)
        sampler.buttonFrames.Clear();
        
        int folderIndex = 0;

        while (true)
        {
            string folderPath = $"Assets/Scenes/{sceneName}/ButtonFrames_{folderIndex}/";
            
            if (!Directory.Exists(folderPath))
            {
                // No more folders found, stop searching
                break;
            }

            // Create a new SpriteGroup
            SpriteGroup newGroup = new SpriteGroup();

            // Get all image files in the folder
            string[] imagePaths = Directory.GetFiles(folderPath, "*.jpg"); // Change extension if needed

            foreach (string imagePath in imagePaths)
            {
                // Unity expects forward slashes in paths
                string assetPath = imagePath.Replace("\\", "/");

                Sprite image = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (image != null)
                {
                    newGroup.sprites.Add(image);
                }
            }

            // Add this group to the sampler
            sampler.buttonFrames.Add(newGroup);

            Debug.Log($"Loaded {newGroup.sprites.Count} images from {folderPath}");

            folderIndex++; // Move to next folder
        }

        Debug.Log($"Finished loading {sampler.buttonFrames.Count} button frame groups.");
    }


    public void LoadAudio()
    {
        AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Scenes/" + sceneName + "/" + sceneName + ".wav");
        if(audioClip == null)
        {
            Debug.Log("No clip found");
        }
        else
        {
            sampler.clip = audioClip;
            Debug.Log("Loaded " + audioClip);
        }
    }

    public void LoadSlices()
    {
        int[] sliceStarts = SliceInfo();
        int sliceCount = sliceStarts.Length;
        Slice[] slices = new Slice[sliceCount];

        for(int i = 0; i < sliceCount - 1; i++)
        {
            slices[i] = (Slice)AssetDatabase.LoadAssetAtPath("Assets/Scenes/" + sceneName + "/Slices/" + sceneName + "_Slice " + i + ".asset", typeof(Slice));
            if(slices[i] == null)
            {
                Debug.Log("slice " + i + " not found");
                Debug.Log("creating slice " + i);
                slices[i] = ScriptableObject.CreateInstance<Slice>();
                slices[i].firstFrame = sliceStarts[i];
                slices[i].lastFrame = sliceStarts[i + 1] - 1;
                slices[i].passThreshold = 0;
                slices[i].nextSlice = i + 1;
                slices[i].failSlice = i;
                AssetDatabase.CreateAsset(slices[i], "Assets/Scenes/" + sceneName + "/Slices/" + sceneName + "_Slice " + i + ".asset");
            }
            else if (slices[i] != null)
            {
                slices[i].firstFrame = sliceStarts[i];
                slices[i].lastFrame = sliceStarts[i + 1] - 1;
            }
        }
        AssetDatabase.SaveAssets();
        sampler.slices = slices;
        Debug.Log("Loaded " + sliceCount + " slices.");
    }

    public int[] SliceInfo()
    {
        string filePath = "Assets/Scenes/" + sceneName + "/slices.txt";
        List<int> frameNumbers = new List<int>();

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (int.TryParse(line, out int frameNumber))
                {
                    frameNumbers.Add(frameNumber);
                }
                else
                {
                    Debug.LogError("Invalid frame number format: " + line);
                }
            }
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }

        return frameNumbers.ToArray();
    }
}

#endif
