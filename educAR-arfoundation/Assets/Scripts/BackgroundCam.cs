using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundCam : MonoBehaviour
{
    private bool camAvailable;
    private WebCamTexture backCam;
    private Texture defaultBackground;

    public RawImage background;
    public AspectRatioFitter fit;
    public Quaternion baseRotation;
    
    void Start()
    {
        defaultBackground = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.Log("Camera nao detectada");
            camAvailable = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if(!devices[i].isFrontFacing)
            {
                backCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
            }
        }

        if (backCam == null)
        {
            Debug.Log("Nao encontrou uma camera");
            return;
        }

        float antiRotate = -(360 - backCam.videoRotationAngle);
        Quaternion quatRot = new Quaternion();
        quatRot.eulerAngles = new Vector3(0, 0, antiRotate);
        background.transform.rotation = quatRot;
        backCam.Play();
        background.texture = backCam;

        camAvailable = true;
    }
    void Update()
    {
        if (!camAvailable)
            return;
        
        float ratio = (float)backCam.width / (float)backCam.height;
        fit.aspectRatio = ratio;

        float scaleY = backCam.videoVerticallyMirrored ? -1f: 1f;
        background.rectTransform.localScale = new Vector3(1f , scaleY ,  1f);

        int orient = -backCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3 (0, 0, orient);
    }

}

