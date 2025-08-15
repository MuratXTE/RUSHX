using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoundManager))]
public class SoundManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SoundManager soundManager = (SoundManager)target;
        
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Auto-Find Sound Files"))
        {
            AutoAssignSounds(soundManager);
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Test Sounds", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Test Positive Gate Sound"))
            {
                soundManager.PlayPositiveGateSound();
            }
            
            if (GUILayout.Button("Test Negative Gate Sound"))
            {
                soundManager.PlayNegativeGateSound();
            }
            
            if (GUILayout.Button("Test Soldier Death Sound"))
            {
                soundManager.PlaySoldierDeathSound();
            }
        }
        else
        {
            EditorGUILayout.LabelField("Enter Play Mode to test sounds", EditorStyles.helpBox);
        }
    }
    
    void AutoAssignSounds(SoundManager soundManager)
    {
        // Find sound files in the Casual Game Sounds folder
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Casual Game Sounds U6" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            
            if (clip != null)
            {
                // Auto-assign based on filename
                string fileName = clip.name.ToLower();
                
                if (fileName.Contains("levelup") || fileName.Contains("pop"))
                {
                    if (soundManager.positiveGateSound == null)
                    {
                        soundManager.positiveGateSound = clip;
                        Debug.Log($"Assigned {clip.name} as positive gate sound");
                    }
                }
                else if (fileName.Contains("kill"))
                {
                    if (soundManager.soldierDeathSound == null)
                    {
                        soundManager.soldierDeathSound = clip;
                        Debug.Log($"Assigned {clip.name} as soldier death sound");
                    }
                }
                else if (fileName.Contains("swoosh"))
                {
                    if (soundManager.negativeGateSound == null)
                    {
                        soundManager.negativeGateSound = clip;
                        Debug.Log($"Assigned {clip.name} as negative gate sound");
                    }
                }
            }
        }
        
        EditorUtility.SetDirty(soundManager);
    }
}
