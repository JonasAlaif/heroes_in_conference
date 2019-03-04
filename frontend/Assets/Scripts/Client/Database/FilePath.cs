using System;
using System.IO;
using UnityEngine;

[Serializable]
public class FilePath {
    public readonly bool IsRemote;
    public readonly string Path;

    public FilePath(bool isRemote, string path) {
        IsRemote = isRemote;
        Path = path;
    }

    #region Image Loading
    /// <summary>
    /// Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
    /// </summary>
    /// <param name="filePath">Where to load from</param>
    /// <param name="pixelsPerUnit"></param>
    /// <returns>The loaded Dprite</returns>
    public static Sprite loadNewSprite(string filePath, float pixelsPerUnit = 100.0f) {
        Texture2D spriteTexture = loadTexture(filePath);
        Sprite newSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), pixelsPerUnit);

        return newSprite;
    }

    /// <summary>
    /// Load a PNG or JPG file from disk to a Texture2D
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>The loaded Texture2D, null if load failed</returns>
    public static Texture2D loadTexture(string filePath) {
        Texture2D tex2D;
        byte[] fileData;

        if (File.Exists(filePath)) {
            fileData = File.ReadAllBytes(filePath);
            tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (tex2D.LoadImage(fileData))           // Load the imagedata into the texture (size is set automatically)
                return tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }
    #endregion
}