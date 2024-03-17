using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class ScreenshotUtility
{
    #region Public Methods

    public static int Gcd(ulong a, ulong b)
    {
        while (a != 0 && b != 0)
        {
            if (a > b)
                a %= b;
            else
                b %= a;
        }

        return Convert.ToInt32(a | b);
    }

    public static Texture2D RenderCamera(Camera camera, int width, int height, int depth)
    {
        RenderTexture cameraTarget = camera.targetTexture;

        var myRenderTarget = new RenderTexture(width, height, depth);

        camera.targetTexture = myRenderTarget;

        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = myRenderTarget;

        camera.Render();

        var image = new Texture2D(myRenderTarget.width, myRenderTarget.height);
        image.ReadPixels(new Rect(0, 0, myRenderTarget.width, myRenderTarget.height), 0, 0);
        image.Apply();

        RenderTexture.active = activeRenderTexture;
        camera.targetTexture = cameraTarget;

        return image;
    }

    public static void SaveTexture(Texture2D texture, string filePath)
    {
        var bytes = texture.EncodeToPNG();
        var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        var writer = new BinaryWriter(stream);

        foreach (var t in bytes)
            writer.Write(t);

        writer.Close();
        stream.Close();

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

#if USING_URP
    /*public static int ScriptableRendererIndex(string renderPipelineAssetPath, GUID renderer)
    {
        var guid = renderer.ToString();
        var lines = File.ReadAllLines(renderPipelineAssetPath);

        var foundRenderers = false;
        var index = 0;

        Debug.Log($"looking for {guid}");
        
        foreach (var line in lines)
        {
            if (!foundRenderers)
            {
                foundRenderers = line.StartsWith("  m_RendererDataList:");
                continue;
            }

            Debug.Log($"renderer {index} | {line}");

            if (line.Contains(guid))
                break;

            index++;
        }

        return index;
    }*/

    public static int ScriptableRendererIndex(string renderPipelineAssetPath, ScriptableRenderer renderer)
    {
        var pipelineAsset = AssetDatabase.LoadAssetAtPath <UniversalRenderPipelineAsset>(renderPipelineAssetPath);

        // TODO  currently being endless due to renderer being null
        // TODO somehow get what renderer is used for transparency and convert them into a scriptableRenderer
        
        /*var index = 0;
        while (true)
        {
            ScriptableRenderer scriptableRenderer = pipelineAsset.GetRenderer(index);

            if (scriptableRenderer == null)
                return -1;

            if (scriptableRenderer == renderer)
                break;

            index++;
        }*/

        return 0;
    }
    
#endif

    #endregion
}
