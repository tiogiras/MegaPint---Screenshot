using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if USING_URP
using UnityEngine.Rendering.Universal;
#endif

#if USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MegaPint
{

/// <summary> Holds settings and information to render the image of the attached <see cref="Camera" /> component </summary>
[RequireComponent(typeof(Camera))]
public class CameraCapture : MonoBehaviour
{
    [HideInInspector]
    public int width = 1920;

    [HideInInspector]
    public int height = 1080;

    [HideInInspector]
    public int depth = 32;

    [HideInInspector]
    public BackgroundType backgroundType;

    [HideInInspector]
    public string lastPath = "Assets";

    [HideInInspector]
    public Color backgroundColor;

    [HideInInspector]
    public Sprite backgroundImage;

    [HideInInspector]
    public string imageType = "Simple";

    [HideInInspector]
    public float pixelPerUnit = 1;

    [HideInInspector]
    public bool listenToShortcut;

#if USING_HDRP
    [HideInInspector]
    public int exposureTime = 250;
#endif

#if USING_URP
    private string _renderPipelineAssetPath;
#if UNITY_EDITOR
      private GUID _transparencyRenderer;
#endif
#endif

    #region Public Methods

    /// <summary> Render the camera's image </summary>
    /// <returns> Rendered image </returns>
    public async Task <Texture2D> Render()
    {
        var cam = GetComponent <Camera>();

        PrepareCamera(
            cam,
            out Color bgColor,
            out CameraClearFlags flags,
            out List <GameObject> destroy);

#if USING_URP
        UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();

        PrepareCameraData(camData, out var rendererIndex);
#endif
#if USING_HDRP
        var camDataHdrp = GetComponent <HDAdditionalCameraData>();

        PrepareCameraData(
            camDataHdrp,
            out HDAdditionalCameraData.ClearColorMode colorMode,
            out Color bgColorHDR,
            out var colorBuffer);
#endif

        // ReSharper disable once RedundantAssignment
        Texture2D render = ScreenshotUtility.RenderCamera(cam, width, height, depth);

#if USING_HDRP
        await Task.Delay(exposureTime);

        render = ScreenshotUtility.RenderCamera(cam, width, height, depth);
#endif

        ResetCamera(cam, bgColor, flags, destroy);

#if USING_URP
        ResetCameraData(camData, rendererIndex);
#endif
#if USING_HDRP
        ResetCameraData(camDataHdrp, colorMode, bgColorHDR, colorBuffer);
#endif

        return render;
    }

    /// <summary> Render the camera and save </summary>
    /// <param name="path"> Export path </param>
    public async void RenderAndSave(string path)
    {
        Save(await Render(), path);
    }

#if UNITY_EDITOR
    /// <summary> Render the camera and save </summary>
    /// <param name="path"> Export path </param>
    /// <param name="renderPipelineAssetPath"> Path to the renderPipelineAsset </param>
    /// <param name="transparencyRenderer"> Path to the renderer </param>
    public async void RenderAndSaveUrp(
        string path,
        string renderPipelineAssetPath,
        GUID transparencyRenderer)
    {
        Save(await RenderUrp(renderPipelineAssetPath, transparencyRenderer), path);
    }

    /// <summary> Render the camera's image </summary>
    /// <param name="renderPipelineAssetPath"> Path to the renderPipelineAsset </param>
    /// <param name="transparencyRenderer"> Path to the renderer </param>
    /// <returns> Rendered image </returns>
    public async Task <Texture2D> RenderUrp(
        string renderPipelineAssetPath,
        GUID transparencyRenderer)
    {
#if USING_URP
        var isUrpAsset = QualitySettings.renderPipeline is UniversalRenderPipelineAsset;

        if (!isUrpAsset &&
            backgroundType is BackgroundType.Transparent or BackgroundType.SolidColor or BackgroundType.Image)
        {
            Debug.LogWarning(
                "You have no UniversalRenderPipelineAsset selected in you Quality settings. Therefor the camera can't render modes with possible transparency.");

            return null;
        }
        
        _renderPipelineAssetPath = renderPipelineAssetPath;
        _transparencyRenderer = transparencyRenderer;
#endif

        return await Render();
    }
#endif

    /// <summary> Save the rendered image </summary>
    /// <param name="texture"> Texture to save </param>
    /// <param name="path"> Export path </param>
    public void Save(Texture2D texture, string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        lastPath = path[..path.LastIndexOf("/", StringComparison.Ordinal)];
        ScreenshotUtility.SaveTexture(texture, path);

#if UNITY_EDITOR
        if (!path.StartsWith("Assets/"))
            return;

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.SaveAndReimport();
#endif
    }

    #endregion

    #region Private Methods

    /// <summary> Reset the camera </summary>
    /// <param name="cam"> Targeted camera </param>
    /// <param name="bgColor"> Background color </param>
    /// <param name="flags"> ClearFlags </param>
    /// <param name="destroy"> Objects to destroy </param>
    private static void ResetCamera(
        Camera cam,
        Color bgColor,
        CameraClearFlags flags,
        IReadOnlyList <GameObject> destroy)
    {
        cam.backgroundColor = bgColor;
        cam.clearFlags = flags;

        for (var i = destroy.Count - 1; i >= 0; i--)
        {
            GameObject obj = destroy[i];
            DestroyImmediate(obj);
        }
    }

    /// <summary> Prepare the camera to render </summary>
    /// <param name="cam"> Targeted camera </param>
    /// <param name="bgColor"> Background color </param>
    /// <param name="flags"> ClearFlags </param>
    /// <param name="destroy"> Objects to destroy </param>
    /// <exception cref="ArgumentOutOfRangeException"> Background not found </exception>
    private void PrepareCamera(
        Camera cam,
        out Color bgColor,
        out CameraClearFlags flags,
        out List <GameObject> destroy)
    {
        bgColor = cam.backgroundColor;
        flags = cam.clearFlags;
        destroy = new List <GameObject>();

        switch (backgroundType)
        {
            case BackgroundType.None:
                cam.clearFlags = CameraClearFlags.Skybox;

                break;

            case BackgroundType.SolidColor:
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = backgroundColor;

                break;

            case BackgroundType.Transparent:
                cam.clearFlags = CameraClearFlags.Depth;

                break;

            case BackgroundType.Image:
                cam.clearFlags = CameraClearFlags.Depth;

                var canvas = new GameObject("RenderCanvas").AddComponent <Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;

                var image = new GameObject("bgImage").AddComponent <Image>();
                image.sprite = backgroundImage;
                image.type = imageType.Equals("Simple") ? Image.Type.Simple : Image.Type.Tiled;
                image.pixelsPerUnitMultiplier = pixelPerUnit;

                RectTransform rect = image.rectTransform;
                Transform parent = canvas.transform;

                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(width, height);
                rect.transform.SetParent(parent);

                rect.localPosition = Vector3.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;

                destroy.Add(canvas.gameObject);
                destroy.Add(image.gameObject);

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

#if USING_URP
    /// <summary> Prepare the camera data to render </summary>
    /// <param name="camData"> Targeted camera data </param>
    /// <param name="rendererIndex"> Index of the transparency renderer </param>
    private void PrepareCameraData(UniversalAdditionalCameraData camData, out int rendererIndex)
    {
        rendererIndex = -1;

        if (backgroundType is BackgroundType.None || !camData.renderPostProcessing)
            return;

#if UNITY_EDITOR
        if (string.IsNullOrEmpty(_renderPipelineAssetPath) || _transparencyRenderer.Empty())
            return;

        if (!ScreenshotUtility.TryGetScriptableRendererIndex(_renderPipelineAssetPath,
                camData.scriptableRenderer, out rendererIndex))
            return;

        if (ScreenshotUtility.TryGetScriptableRendererIndex(_renderPipelineAssetPath,
                _transparencyRenderer, out var index))
            camData.SetRenderer(index);
#endif
    }
    
    private void ResetCameraData(UniversalAdditionalCameraData camData, int rendererIndex)
    {
        if (backgroundType is BackgroundType.None || !camData.renderPostProcessing)
            return;
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(_renderPipelineAssetPath) || _transparencyRenderer.Empty())
            return;
#endif

        camData.SetRenderer(rendererIndex);
    }
#endif
#if USING_HDRP
    /// <summary> Prepare the camera data for rendering </summary>
    /// <param name="camData"> Targeted camera data </param>
    /// <param name="colorMode"> Color Mode </param>
    /// <param name="bgColor"> Background color </param>
    /// <param name="colorBuffer"> Color buffer </param>
    /// <exception cref="ArgumentOutOfRangeException"> Background not found </exception>
    private void PrepareCameraData(
        HDAdditionalCameraData camData,
        out HDAdditionalCameraData.ClearColorMode colorMode,
        out Color bgColor,
        out string colorBuffer)
    {
        colorMode = camData.clearColorMode;
        bgColor = camData.backgroundColorHDR;
        colorBuffer = "";

        switch (backgroundType)
        {

            case BackgroundType.None:
                camData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Sky;

                break;

            case BackgroundType.SolidColor:
                camData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
                camData.backgroundColorHDR =
 new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, backgroundColor.a);

                break;

            case BackgroundType.Transparent:
                camData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
                camData.backgroundColorHDR = new Color(0, 0, 0, 0);

                break;

            case BackgroundType.Image:
                camData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
                camData.backgroundColorHDR = new Color(0, 0, 0, 0);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if (backgroundType is BackgroundType.None)
            return;

        ScreenshotUtility.WriteColorBufferFormat("    colorBufferFormat: 48", out colorBuffer);
    }
    
    private void ResetCameraData(
        HDAdditionalCameraData camData,
        HDAdditionalCameraData.ClearColorMode colorMode,
        Color bgColor,
        string colorBuffer)
    {
        camData.clearColorMode = colorMode;
        camData.backgroundColorHDR = bgColor;

        if (backgroundType is BackgroundType.None)
            return;

        ScreenshotUtility.WriteColorBufferFormat(colorBuffer, out var _);
    }
#endif

    #endregion
}

}
