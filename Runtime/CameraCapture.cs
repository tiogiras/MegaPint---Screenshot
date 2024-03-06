using System;
using System.Collections.Generic;
using PlasticGui.Configuration.CloudEdition.Welcome;
using UnityEngine;
using UnityEngine.UIElements;

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
    public string lastPath;

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

    public void Capture()
    {
        Debug.Log("Capturing");
    }
}