using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if USING_URP
using UnityEngine.Rendering.Universal;
#elif USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

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

    [HideInInspector]
    public int exposureTime = 250;

#if USING_URP
    private string _renderPipelineAssetPath;
    private GUID _transparencyRenderer;
#endif

    #region Public Methods

    public async Task<Texture2D> Render()
    {
        var cam = GetComponent <Camera>();

        PrepareCamera(cam, out Color bgColor, out CameraClearFlags flags,
            out List <GameObject> destroy);

#if USING_URP
        UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();

        PrepareCameraData(camData, out var rendererIndex);
#elif USING_HDRP
        var camData = GetComponent <HDAdditionalCameraData>();

        PrepareCameraData(camData, out HDAdditionalCameraData.ClearColorMode colorMode, out Color bgColorHDR, out var colorBuffer);
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
#elif USING_HDRP
        ResetCameraData(camData, colorMode, bgColorHDR, colorBuffer);
#endif

        return render;
    }

    public async void RenderAndSave(string path)
    {
        Save(await Render(), path);
    }

    public async void RenderAndSaveUrp(
        string path,
        string renderPipelineAssetPath,
        GUID transparencyRenderer)
    {
        Save(await RenderUrp(renderPipelineAssetPath, transparencyRenderer), path);
    }

    public async Task<Texture2D> RenderUrp(string renderPipelineAssetPath, GUID transparencyRenderer)
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

    public void Save(Texture2D texture, string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        lastPath = path[..path.LastIndexOf("/", StringComparison.Ordinal)];
        ScreenshotUtility.SaveTexture(texture, path);

        if (backgroundType is BackgroundType.Transparent or BackgroundType.SolidColor)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
    }

    #endregion
    
    #region Private Methods

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
    private void PrepareCameraData(UniversalAdditionalCameraData camData, out int rendererIndex)
    {
        rendererIndex = -1;

        if (backgroundType is BackgroundType.None)
            return;

#if UNITY_EDITOR
        if (!ScreenshotUtility.TryGetScriptableRendererIndex(_renderPipelineAssetPath,
                camData.scriptableRenderer, out rendererIndex))
            return;

        if (ScreenshotUtility.TryGetScriptableRendererIndex(_renderPipelineAssetPath,
                _transparencyRenderer, out var index))
            camData.SetRenderer(index);
#endif
    }
#elif USING_HDRP
    private void PrepareCameraData(HDAdditionalCameraData camData, out HDAdditionalCameraData.ClearColorMode colorMode, out Color bgColor, out string colorBuffer)
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
                camData.backgroundColorHDR = backgroundColor;
                break;

            case BackgroundType.Transparent:
                camData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
                camData.backgroundColorHDR = new Color(0,0,0,0);
                break;

            case BackgroundType.Image:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if (backgroundType is not (BackgroundType.SolidColor or BackgroundType.Transparent))
            return;

        ScreenshotUtility.WriteColorBufferFormat("    colorBufferFormat: 48", out colorBuffer);
    }
#endif

#if USING_URP
    private void ResetCameraData(UniversalAdditionalCameraData camData, int rendererIndex)
    {
        if (backgroundType is BackgroundType.None)
            return;

        camData.SetRenderer(rendererIndex);
    }
#elif USING_HDRP
    private void ResetCameraData(HDAdditionalCameraData camData, HDAdditionalCameraData.ClearColorMode colorMode, Color bgColor, string colorBuffer)
    {
        camData.clearColorMode = colorMode;
        camData.backgroundColorHDR = bgColor;
        
        if (backgroundType is not (BackgroundType.SolidColor or BackgroundType.Transparent))
            return;

        ScreenshotUtility.WriteColorBufferFormat(colorBuffer, out var _);
    }
#endif

    #endregion
}
