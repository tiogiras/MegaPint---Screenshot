using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScreenshot : MonoBehaviour
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
    public string lastPath;

    [HideInInspector]
    public Color backgroundColor;

    [HideInInspector]
    public Sprite backgroundImage;
    
    [HideInInspector]
    public string imageType = "Simple";
    
    [HideInInspector]
    public float pixelPerUnit = 1;
}