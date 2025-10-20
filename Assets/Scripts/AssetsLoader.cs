#if UNITY_EDITOR // => Ignore from here to next endif if not in editor

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;


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
        sampler.buttonColliderDatas.Clear();

        int folderIndex = 0;

        while (true)
        {
            string folderPath = $"Assets/Scenes/{sceneName}/ButtonFrames_{folderIndex}/";
            
            if (!Directory.Exists(folderPath))
            {
                Debug.Log("No more button frame folders");
                // No more folders found, stop searching
                break;
            }

            // Create a new SpriteGroup
            SpriteGroup newGroup = new SpriteGroup();

            List<ColliderFrame> colliderFrames = new List<ColliderFrame>();

            // Get all image files in the folder
            string[] imagePaths = Directory.GetFiles(folderPath, "*.jpg"); // Change extension if needed

            foreach (string imagePath in imagePaths)
            {
                string assetPath = imagePath.Replace("\\", "/");

                Sprite image = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (image != null)
                {
                    newGroup.sprites.Add(image);

                    // ---- Collider baking here ----
                    PolygonCollider2D tempCollider = new GameObject().AddComponent<PolygonCollider2D>();
                    tempCollider.pathCount = 1;
                    
                    List<Vector2> path = new List<Vector2>();
                    image.GetPhysicsShape(0, path);

                    if (path.Count == 0)
                    {
                        Debug.LogWarning($"No physics shape found for sprite: {image.name}");
                    }
                    else
                    {
                        Debug.Log($"Collider path count for {image.name}: {path.Count}");
                    }
                    
                    int frameNumber = ParseFrameNumberFromImageName(image.name); // <- You need to extract the number from the sprite's name
                    ColliderFrame frame = new ColliderFrame
                    {
                        frameNumber = frameNumber,
                        points = path.ToArray()
                    };

                    // Check if ColliderFrame is populated before adding it
                    if (frame.points.Length > 0)
                    {
                        colliderFrames.Add(frame);
                        Debug.Log($"Added collider frame for {image.name} with {frame.points.Length} points.");
                    }
                    else
                    {
                        Debug.LogWarning($"No points for collider frame of {image.name}. Frame was not added.");
                    }



                    GameObject.DestroyImmediate(tempCollider.gameObject);
                }
            }

            // Add this group to the sampler
            sampler.buttonFrames.Add(newGroup);

            // Save ColliderFrameData ScriptableObject
            string colliderDataPath = $"{folderPath}/ButtonFrameColliders_{folderIndex}.asset";
            ColliderFrameData colliderData = AssetDatabase.LoadAssetAtPath<ColliderFrameData>(colliderDataPath);
            if (colliderData == null)
            {
                colliderData = ScriptableObject.CreateInstance<ColliderFrameData>();
                AssetDatabase.CreateAsset(colliderData, colliderDataPath);
            }
            sampler.buttonColliderDatas.Add(colliderData);

            colliderData.frames = colliderFrames.ToArray();
            EditorUtility.SetDirty(colliderData);

            Debug.Log($"Loaded {newGroup.sprites.Count} images and baked {colliderFrames.Count} colliders from {folderPath}");

            folderIndex++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Finished loading {sampler.buttonFrames.Count} button frame groups.");
    }


    int ParseFrameNumberFromImageName(string imageName)
    {
        // Split by underscores
        string[] parts = imageName.Split('_');
        if (parts.Length >= 3)
        {
            if (int.TryParse(parts[2], out int number))
                return number;
        }
        Debug.LogWarning($"Failed to parse frame number from {imageName}");
        return -1;
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
                slices[i].nextSlice = new int[1];   
                slices[i].nextSlice[0] = i + 1;
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
