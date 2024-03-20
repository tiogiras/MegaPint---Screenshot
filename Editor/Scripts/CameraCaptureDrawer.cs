using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

#if USING_URP
using UnityEngine.Rendering.Universal;
#endif

#if UNITY_EDITOR
namespace Editor.Scripts
{

[CustomEditor(typeof(CameraCapture))]
internal class CameraCaptureDrawer : UnityEditor.Editor
{
    private const string Path = "Screenshot/User Interface/CameraCapture";
    private const string PathError = "Screenshot/User Interface/MultiplePipelines";
    
    private ColorField _backgroundColor;
    private ObjectField _backgroundImage;

    private EnumField _backgroundType;

    private Button _btnExportPath;
    private Button _btnRender;
    private Button _btnSave;

#if USING_URP
    private UniversalAdditionalCameraData _camData;
#endif

    private DropdownField _depth;
    private IntegerField _height;
    private Label _imageResolution;

    private DropdownField _imageType;
    private Label _path;
    private FloatField _pixelPerUnit;
    private AspectRatioPanel _preview;

    private Texture2D _render;

    private CameraCapture _target;

    private VisualElement _transparencyHint;
    private VisualElement _transparencyHintHdrp;

    private IntegerField _exposureTime;

    private IntegerField _width;
    #region Public Methods

    public override VisualElement CreateInspectorGUI()
    {
#if USING_URP && USING_HDRP
        return Resources.Load <VisualTreeAsset>(PathError).Instantiate();   
#endif
        
        var template = Resources.Load <VisualTreeAsset>(Path);
        TemplateContainer root = template.Instantiate();

        _preview = root.Q <AspectRatioPanel>("Preview");
        _btnRender = root.Q <Button>("BTN_Render");
        _btnSave = root.Q <Button>("BTN_Save");

        _btnExportPath = root.Q <Button>("BTN_ExportPath");
        _path = root.Q <Label>("Path");

        _width = root.Q <IntegerField>("Width");
        _height = root.Q <IntegerField>("Height");
        _depth = root.Q <DropdownField>("Depth");

        _backgroundType = root.Q <EnumField>("BackgroundType");
        _backgroundColor = root.Q <ColorField>("BackgroundColor");
        _backgroundImage = root.Q <ObjectField>("BackgroundImage");
        _imageResolution = root.Q <Label>("ImageResolution");

        _imageType = root.Q <DropdownField>("ImageType");
        _pixelPerUnit = root.Q <FloatField>("PixelPerUnit");

        _transparencyHint = root.Q <VisualElement>("TransparencyHint");

        _transparencyHintHdrp = root.Q <VisualElement>("TransparencyHintHDRP");
        _exposureTime = root.Q <IntegerField>("ExposureTime");

        _target = (CameraCapture)target;

#if USING_URP
        _camData = _target.GetComponent <Camera>().GetUniversalAdditionalCameraData();
#endif

        _width.value = _target.width;
        _height.value = _target.height;
        _depth.value = _target.depth.ToString();
        _backgroundType.value = _target.backgroundType;
        _backgroundColor.value = _target.backgroundColor;
        _backgroundImage.value = _target.backgroundImage;
        _imageType.value = _target.imageType;
        _pixelPerUnit.value = _target.pixelPerUnit;

#if USING_HDRP
        _exposureTime.value = _target.exposureTime;
#endif

        _btnSave.style.display = DisplayStyle.None;

        UpdateTransparencyHint();
        UpdateTransparencyHintHdrp();

        UpdatePath();
        UpdateBackgroundColor();
        UpdateBackgroundImage();

        RegisterCallbacks();

        return root;
    }

    #endregion
    #region Private Methods

    private void ChangePath()
    {
        var path = EditorUtility.OpenFolderPanel("Choose Path", _target.lastPath, "");

        if (!path.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog(
                "Folder not in project",
                "The folder must be within the Assets folder",
                "ok");

            return;
        }

        _target.lastPath = path.Remove(0, Application.dataPath.Length - 6);
        UpdatePath();
    }

    private void RegisterCallbacks()
    {
        _btnRender.clicked += Render;

        _btnSave.clickable = new Clickable(
            () =>
            {
                var path = EditorUtility.SaveFilePanelInProject(
                    "Save Screenshot",
                    "",
                    "png",
                    "",
                    _target.lastPath);

                _target.Save(_render, path);
            });

        _width.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue < 0)
                {
                    _width.SetValueWithoutNotify(0);
                    _target.width = 0;

                    return;
                }

                _target.width = evt.newValue;
            });

        _height.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue < 0)
                {
                    _height.SetValueWithoutNotify(0);
                    _target.height = 0;

                    return;
                }

                _target.height = evt.newValue;
            });

        _exposureTime.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue < 0)
                {
                    _exposureTime.SetValueWithoutNotify(0);
                    _target.exposureTime = 0;

                    return;
                }

                _target.exposureTime = evt.newValue;
            });

        _depth.RegisterValueChangedCallback(evt => {_target.depth = int.Parse(evt.newValue);});

        _backgroundType.RegisterValueChangedCallback(
            evt =>
            {
                _target.backgroundType = (BackgroundType)evt.newValue;
                UpdateBackgroundColor();
                UpdateBackgroundImage();
                UpdateTransparencyHint();
            });

        _backgroundColor.RegisterValueChangedCallback(
            evt => {_target.backgroundColor = evt.newValue;});

        _imageType.RegisterValueChangedCallback(
            evt =>
            {
                _target.imageType = evt.newValue;
                UpdateBackgroundImage();
            });

        _pixelPerUnit.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue < 0)
                    _pixelPerUnit.SetValueWithoutNotify(0);

                _target.pixelPerUnit = evt.newValue;
            });

        _backgroundImage.RegisterValueChangedCallback(
            evt =>
            {
                _target.backgroundImage = (Sprite)evt.newValue;
                UpdateBackgroundImage();
            });

        _btnExportPath.clicked += ChangePath;
    }

    private async void Render()
    {
        var width = _target.width;
        var height = _target.height;

        var gcd = ScreenshotUtility.Gcd((ulong)width, (ulong)height);

#if USING_URP
        _render = await _target.RenderUrp(ScreenshotData.RenderPipelineAssetPath,
            AssetDatabase.GUIDFromAssetPath(ScreenshotData.RendererDataPath));
#else
        _render = await _target.Render();
#endif

        _preview.style.backgroundImage = _render;
        _preview.aspectRatioX = width / gcd;
        _preview.aspectRatioY = height / gcd;
        _preview.FitToParent();

        _btnSave.style.display = DisplayStyle.Flex;
    }

    private void UpdateBackgroundColor()
    {
        _backgroundColor.style.display = _target.backgroundType == BackgroundType.SolidColor
            ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void UpdateBackgroundImage()
    {
        var isImage = _target.backgroundType == BackgroundType.Image;
        _backgroundImage.style.display = isImage ? DisplayStyle.Flex : DisplayStyle.None;
        _imageType.style.display = isImage ? DisplayStyle.Flex : DisplayStyle.None;

        Sprite image = _target.backgroundImage;

        _imageResolution.style.display =
            isImage && image != null ? DisplayStyle.Flex : DisplayStyle.None;

        _pixelPerUnit.style.display = isImage && _target.imageType.Equals("Tiled")
            ? DisplayStyle.Flex : DisplayStyle.None;

        _imageResolution.text = _target.backgroundImage == null ? ""
            : $"{image.rect.width} x {image.rect.height}";
    }

    private void UpdatePath()
    {
        _path.text = _target.lastPath;
        _path.tooltip = _target.lastPath;
    }

    private void UpdateTransparencyHint()
    {
#if USING_URP
        if (_target.backgroundType is not BackgroundType.None)
            _transparencyHint.style.display = _camData.renderPostProcessing ? DisplayStyle.Flex
                : DisplayStyle.None;
        else
            _transparencyHint.style.display = DisplayStyle.None;
#else
        _transparencyHint.style.display = DisplayStyle.None;
#endif
    }

    private void UpdateTransparencyHintHdrp()
    {
#if USING_HDRP
        _transparencyHintHdrp.style.display = DisplayStyle.Flex;
#else
        _transparencyHintHdrp.style.display = DisplayStyle.None;
#endif
    }

    #endregion
}

}
#endif
