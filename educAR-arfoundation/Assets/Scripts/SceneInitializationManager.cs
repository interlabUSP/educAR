using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Siccity.GLTFUtility;

public class SceneInitializationManager : MonoBehaviour
{
    public GameObject placeholder;

    private int imageCount;

    private ARTrackedImageManager m_TrackedImageManager;

    private List<String> imagesList = new List<String>();
    
    // Map<nome_marcador, nome_overlays>
    public Dictionary<String, List<GameObject>> m_PrefabsDictionary { get; private set; } = new Dictionary<String, List<GameObject>>();

    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
    }
    
    private IEnumerator Start() 
    {
        yield return StartCoroutine(createTriggerLibrary());
        InitializePrefabsDictionary();
    }

    private IEnumerator createTriggerLibrary() {
        List<ApplicationModel.Scene> scenes = ApplicationModel.cena.scenes;

        // load images and initialize image library
        var library = m_TrackedImageManager.CreateRuntimeLibrary() as MutableRuntimeReferenceImageLibrary;

        var counter = 0;
        foreach (ApplicationModel.Scene scene in scenes)
        {
            string triggerUrl = scene.trigger;

            if (File.Exists(triggerUrl))
            {
                Debug.Log("Loading image: " + triggerUrl.ToString());
                UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + triggerUrl);
                yield return uwr.SendWebRequest();
                var texture = DownloadHandlerTexture.GetContent(uwr);

                if (texture)
                {
                    Debug.Log("Image loaded: " + triggerUrl.ToString());
                    Debug.Log(texture);
                    var status = AddReferenceImageJobStatus.None;
                    var addImageJob = library.ScheduleAddImageWithValidationJob(texture, scene.name, 1);
                    while (!addImageJob.jobHandle.IsCompleted) {}
                    // As vezes o status eh ErrorUnknown, e ai a imagem nao eh adicionada... Esse loop so ficou infinito kkk
                    Debug.Log(addImageJob.status);
                    counter++;
                    Debug.Log("Image added. Library: " + library.count);
                }
                else
                {
                    Debug.Log("Nao foi possivel carregar a imagem:  " + triggerUrl.ToString());
                }
            }
            else
            {
                Debug.Log("Imagem nao encontrada:  " + triggerUrl.ToString());
            }
        }
        m_TrackedImageManager.referenceLibrary = library;
        Debug.Log("Library: " + m_TrackedImageManager.referenceLibrary.count);
    }

    private void InitializePrefabsDictionary()
    {
        imageCount = m_TrackedImageManager.referenceLibrary.count;
        Debug.Log("Imagecount:" + imageCount);
        for (int i = 0; i < imageCount; i++)
        {
            m_PrefabsDictionary.Add(m_TrackedImageManager.referenceLibrary[i].name,  new List<GameObject>{ placeholder });
            List<GameObject> _;
            m_PrefabsDictionary.TryGetValue(m_TrackedImageManager.referenceLibrary[i].name, out _);
            Debug.Log("trigger: overlay:" + m_TrackedImageManager.referenceLibrary[i].name + " " + _.Count);
        }
        Debug.Log("m_PrefabsDict:" + m_PrefabsDictionary.Count);
    }

    private GameObject LoadModel(string path)
    {
        GameObject model = Importer.LoadFromFile(path);
        return model;
    }

}
