#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Editor.Scripts.Internal
{

[CustomEditor(typeof(CameraCapture))]
internal class CameraCaptureDrawer : UnityEditor.Editor
{
    private const string Path = "Screenshot/User Interface/CameraCapture";

    private Camera _camera;
    private AspectRatioPanel _preview;
    private Button _btnRender;
    private Button _btnSave;

    private IntegerField _width;
    private IntegerField _height;
    private DropdownField _depth;

    private EnumField _backgroundType;
    private ColorField _backgroundColor;
    private ObjectField _backgroundImage;
    private Label _imageResolution;

    private DropdownField _imageType;
    private FloatField _pixelPerUnit;
    
    private CameraCapture _target;

    private Texture2D _render;

    #region Public Methods

    public override VisualElement CreateInspectorGUI()
    {
        var template = Resources.Load <VisualTreeAsset>(Path);
        TemplateContainer root = template.Instantiate();

        _preview = root.Q <AspectRatioPanel>("Preview");
        _btnRender = root.Q <Button>("BTN_Render");
        _btnSave = root.Q <Button>("BTN_Save");

        _width = root.Q <IntegerField>("Width");
        _height = root.Q <IntegerField>("Height");
        _depth = root.Q <DropdownField>("Depth");

        _backgroundType = root.Q <EnumField>("BackgroundType");
        _backgroundColor = root.Q <ColorField>("BackgroundColor");
        _backgroundImage = root.Q <ObjectField>("BackgroundImage");
        _imageResolution = root.Q <Label>("ImageResolution");

        _imageType = root.Q <DropdownField>("ImageType");
        _pixelPerUnit = root.Q <FloatField>("PixelPerUnit");

        _target = (CameraCapture)target;
        
        _camera = _target.gameObject.GetComponent <Camera>();

        _width.value = _target.width;
        _height.value = _target.height;
        _depth.value = _target.depth.ToString();
        _backgroundType.value = _target.backgroundType;
        _backgroundColor.value = _target.backgroundColor;
        _backgroundImage.value = _target.backgroundImage;
        _imageType.value = _target.imageType;
        _pixelPerUnit.value = _target.pixelPerUnit;
        
        _btnSave.style.display = DisplayStyle.None;

        UpdateBackgroundColor();
        UpdateBackgroundImage();

        RegisterCallbacks();

        return root;
    }

    private void UpdateBackgroundImage()
    {
        var isImage = _target.backgroundType == BackgroundType.Image;
        _backgroundImage.style.display = isImage ? DisplayStyle.Flex : DisplayStyle.None;
        _imageType.style.display = isImage ? DisplayStyle.Flex : DisplayStyle.None;

        Sprite image = _target.backgroundImage;
        _imageResolution.style.display = isImage && image != null ? DisplayStyle.Flex : DisplayStyle.None;
        _pixelPerUnit.style.display = isImage && _target.imageType.Equals("Tiled") ? DisplayStyle.Flex : DisplayStyle.None;
        
        _imageResolution.text = _target.backgroundImage == null ? "" : $"{image.rect.width} x {image.rect.height}";
    }

    private void UpdateBackgroundColor()
    {
        _backgroundColor.style.display = _target.backgroundType == BackgroundType.SolidColor ? DisplayStyle.Flex : DisplayStyle.None;
    }

    #endregion

    private void RegisterCallbacks()
    {
        _btnRender.clicked += Render;
        _btnSave.clicked += Save;
        
        _width.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue < 0)
                    _width.SetValueWithoutNotify(0);

                _target.width = evt.newValue;
            });
        
        _height.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue < 0)
                    _height.SetValueWithoutNotify(0);
                
                _target.height = evt.newValue;
            });

        _depth.RegisterValueChangedCallback(evt => {_target.depth = int.Parse(evt.newValue);});

        _backgroundType.RegisterValueChangedCallback(evt =>
        {
            _target.backgroundType = (BackgroundType)evt.newValue;
            UpdateBackgroundColor();
            UpdateBackgroundImage();
        });

        _backgroundColor.RegisterValueChangedCallback(evt => {_target.backgroundColor = evt.newValue;});
        
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
    }

    private static void SaveTexture(Texture2D texture, string filePath) {
        var bytes = texture.EncodeToPNG();
        var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        var writer = new BinaryWriter(stream);
        
        foreach (var t in bytes)
        {
            writer.Write(t);
        }
        
        writer.Close();
        stream.Close();
        
        AssetDatabase.Refresh();
    }

    private void Save()
    {
        var path = EditorUtility.SaveFilePanelInProject("Save Screenshot", "", "png", "", _target.lastPath);
        
        if (string.IsNullOrEmpty(path))
            return;
        
        _target.lastPath = path[..path.LastIndexOf("/", StringComparison.Ordinal)];
        SaveTexture(_render, path);
    }

    private void Render()
    {
        PrepareCamera(out Color backgroundColor, out CameraClearFlags flags, out List <GameObject> destroy);

        var width = _target.width;
        var height = _target.height;

        var gcd = Utility.Gcd((ulong)width, (ulong)height);

        _render = Utility.RenderCamera(_camera, width, height, _target.depth);
        _preview.style.backgroundImage = _render;
        _preview.aspectRatioX = width / gcd;
        _preview.aspectRatioY = height / gcd;
        _preview.FitToParent();

        _btnSave.style.display = DisplayStyle.Flex;

        ResetCamera(backgroundColor, flags, destroy);
    }

    private void PrepareCamera(out Color backgroundColor, out CameraClearFlags flags, out List<GameObject> destroy)
    {
        backgroundColor = _camera.backgroundColor;
        flags = _camera.clearFlags;
        destroy = new List <GameObject>();

        switch (_target.backgroundType)
        {
            case BackgroundType.None:
                _camera.clearFlags = CameraClearFlags.Skybox;
                break;
            
            case BackgroundType.SolidColor:
                _camera.clearFlags = CameraClearFlags.SolidColor;
                _camera.backgroundColor = _target.backgroundColor;
                break;
            
            case BackgroundType.Transparent: 
                _camera.clearFlags = CameraClearFlags.Depth;
                break;
            
            case BackgroundType.Image: 
                _camera.clearFlags = CameraClearFlags.Depth;

                var canvas = new GameObject("RenderCanvas").AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = _camera;

                var image = new GameObject("bgImage").AddComponent <Image>();
                image.sprite = _target.backgroundImage;
                image.type = _target.imageType.Equals("Simple") ? Image.Type.Simple : Image.Type.Tiled;
                image.pixelsPerUnitMultiplier = _target.pixelPerUnit;

                RectTransform rect = image.rectTransform;
                Transform parent = canvas.transform;
                
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(_target.width, _target.height);
                rect.transform.SetParent(parent);

                rect.localPosition = Vector3.zero;
                rect.localScale = Vector3.one;
                
                destroy.Add(canvas.gameObject);
                destroy.Add(image.gameObject);
                break;
            
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void ResetCamera(Color backgroundColor, CameraClearFlags flags, IReadOnlyList <GameObject> destroy)
    {
        _camera.backgroundColor = backgroundColor;
        _camera.clearFlags = flags;

        for (var i = destroy.Count - 1; i >= 0; i--)
        {
            GameObject obj = destroy[i];
            DestroyImmediate(obj);
        }
    }
}

}
#endif
