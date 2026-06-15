using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.U2D;
using System.Collections.Generic;

public class UISpriteGlobalRestorerEditor : EditorWindow
{
    [MenuItem("Tools/Restore All Atlas Sprites")]
    public static void ShowWindow()
    {
        GetWindow<UISpriteGlobalRestorerEditor>("Restore All Atlas Sprites");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Restore All Sprites"))
        {
            RestoreAllSprites();
        }
    }

    void RestoreAllSprites()
    {
        // Find all atlases in the project
        string[] atlasGuids = AssetDatabase.FindAssets("t:SpriteAtlas");
        List<SpriteAtlas> atlases = new List<SpriteAtlas>();
        foreach (string guid in atlasGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            if (atlas != null) atlases.Add(atlas);
        }

        // Find all UI Images in the project
        Image[] images = Resources.FindObjectsOfTypeAll<Image>();

        foreach (var img in images)
        {
            if (img.sprite == null) continue;

            string spriteName = img.sprite.name;

            // Check if this sprite exists in any atlas
            foreach (var atlas in atlases)
            {
                Sprite atlasSprite = atlas.GetSprite(spriteName);
                if (atlasSprite != null && img.sprite == atlasSprite)
                {
                    // Try to find a non-atlas sprite with the same name
                    string[] guids = AssetDatabase.FindAssets(spriteName + " t:Sprite");
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Sprite candidate = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                        // Replace only if it's not the atlas version
                        if (candidate != null && candidate != atlasSprite)
                        {
                            img.sprite = candidate;
                            Debug.Log($"Restored {spriteName} from {path}");
                            break;
                        }
                    }
                }
            }
        }

        Debug.Log("All atlas sprites restored to loose versions!");
    }
}
