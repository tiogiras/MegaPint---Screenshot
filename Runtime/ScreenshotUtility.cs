using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if USING_URP
using UnityEngine.Rendering.Universal;
#endif

#if USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

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
    
    public static void WriteColorBufferFormat(string newColorBuffer, out string oldColorBuffer)
    {
        oldColorBuffer = "";

#if USING_HDRP
        RenderPipelineAsset pipelineAsset = QualitySettings.renderPipeline;

        if (pipelineAsset is not HDRenderPipelineAsset)
            return;

        var pipelineAssetPath = AssetDatabase.GetAssetPath(pipelineAsset);

        AssetDatabase.SaveAssetIfDirty(pipelineAsset);

        var lines = File.ReadAllLines(pipelineAssetPath);

        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].Contains("colorBufferFormat:"))
                continue;

            oldColorBuffer = lines[i];
            lines[i] = newColorBuffer;

            break;
        }

        File.WriteAllLines(pipelineAssetPath, lines);

        AssetDatabase.SaveAssetIfDirty(pipelineAsset);
        AssetDatabase.Refresh();
#endif
    }

#if USING_URP
#if UNITY_EDITOR
    public static bool TryGetScriptableRendererIndex(string renderPipelineAssetPath, GUID renderer, out int index)
    {
        EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(renderPipelineAssetPath));
        AssetDatabase.SaveAssets();
        
        var guid = renderer.ToString();
        var lines = File.ReadAllLines(renderPipelineAssetPath);

        var foundRenderers = false;

        index = 0;
        
        foreach (var line in lines)
        {
            if (!foundRenderers)
            {
                foundRenderers = line.StartsWith("  m_RendererDataList:");
                continue;
            }
            
            if (!line.StartsWith("  - {fileID:"))
                break;

            if (line.Contains(guid))
                return true;

            index++;
        }

        return false;
    }

    public static bool TryGetScriptableRendererIndex(string renderPipelineAssetPath, ScriptableRenderer renderer, out int index)
    {
        var pipelineAsset =
 AssetDatabase.LoadAssetAtPath <UniversalRenderPipelineAsset>(renderPipelineAssetPath);

        index = 0;
        while (true)
        {
            ScriptableRenderer scriptableRenderer = pipelineAsset.GetRenderer(index);

            if (scriptableRenderer == null)
                break;

            if (scriptableRenderer == renderer)
                return true;

            index++;
        }

        return false;
    }
#endif
#endif

    #endregion
}
