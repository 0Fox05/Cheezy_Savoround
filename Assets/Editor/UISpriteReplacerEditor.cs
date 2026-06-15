using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.U2D;

public class UISpriteReplacerEditor : EditorWindow
{
    public SpriteAtlas atlas;

    [MenuItem("Tools/Replace UI Sprites")]
    public static void ShowWindow()
    {
        GetWindow<UISpriteReplacerEditor>("Replace UI Sprites");
    }

    void OnGUI()
    {
        atlas = (SpriteAtlas)EditorGUILayout.ObjectField("Atlas", atlas, typeof(SpriteAtlas), false);

        if (GUILayout.Button("Replace All"))
        {
            Image[] images = Resources.FindObjectsOfTypeAll<Image>();
            foreach (var img in images)
            {
                if (img.sprite != null)
                {
                    Sprite newSprite = atlas.GetSprite(img.sprite.name);
                    if (newSprite != null)
                        img.sprite = newSprite;
                }
            }
            Debug.Log("Sprites replaced!");
        }
    }
}
