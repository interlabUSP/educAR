using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using glTFLoader;
using System.IO;
using UnityEngine.XR.ARFoundation;

public class Loading : MonoBehaviour
{

    private string filePathModel;
    private string filePathImage;
    private string filePathTrigger;

    private WWW _wwwRequest;

    public GameObject loadingScreen;
    public Slider slider;

    private DownloadManager m_downloadManager;

    void Awake()
    {
        string IDcanal = ApplicationModel.cena.id;
        m_downloadManager = GetComponent<DownloadManager>();
        filePathModel = $"{Application.temporaryCachePath}/Models/" + IDcanal;
        filePathImage = $"{Application.temporaryCachePath}/Images/" + IDcanal;
        filePathTrigger = $"{Application.temporaryCachePath}/Trigger/" + IDcanal;
    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(m_downloadManager.GetTriggers(slider, loadingScreen));
        LoaderUtility.Initialize();
        SceneManager.LoadScene(3);
    }

}
