using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Siccity.GLTFUtility;

public class ImageTrackingManager : MonoBehaviour
{

    private DownloadManager downloadManager;
    private ARTrackedImageManager m_TrackedImageManager;
    
    // Map<nome_marcador, modelo3d>
    public Dictionary<String, List<GameObject>> m_PrefabsDictionary;
    private Dictionary<String, GameObject> m_Instantiated = new Dictionary<String, GameObject>();
    private List<ApplicationModel.Scene> scenes;

    public delegate IEnumerator CoroutineCallback(ARTrackedImage updatedImage);

    private CoroutineCallback m_getOverlayCallback;

    void Awake()
    {
        scenes = ApplicationModel.cena.scenes;
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        m_PrefabsDictionary = GetComponent<SceneInitializationManager>().m_PrefabsDictionary;
        downloadManager = GetComponent<DownloadManager>();
    }

    void OnEnable() => m_TrackedImageManager.trackedImagesChanged += OnChanged;

    void OnDisable() => m_TrackedImageManager.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            Debug.Log("Imagem encontrada!");
            var minLocalScalar = Mathf.Min(trackedImage.size.x, trackedImage.size.y);
            trackedImage.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);
            m_getOverlayCallback = UpdatePrefabDictionary;
            Debug.Log("Download comecando!");
            Debug.Log(downloadManager);
            StartCoroutine(downloadManager.GetOverlays(trackedImage, m_getOverlayCallback));
            Debug.Log("Assign Prefab!");
            AssignPrefab(trackedImage);
        }

        foreach (ARTrackedImage updatedImage in eventArgs.updated)
        {
            if(updatedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                UpdateTrackingGameObject(updatedImage);
            }
            else if(updatedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited)
            {
                UpdateLimitedGameObject(updatedImage);
            }
            else
            {
                UpdateNoneGameObject(updatedImage);
            }
        }

        foreach (ARTrackedImage removedImage in eventArgs.removed)
        {
            DestroyGameObject(removedImage);
        }
    }

    void AssignPrefab(ARTrackedImage trackedImage)
    {
        if (m_PrefabsDictionary.TryGetValue(trackedImage.referenceImage.name, out var prefabList))
        {
            GameObject parentObject = new GameObject();
            parentObject.transform.position = trackedImage.transform.position;
            for (int i = 0; i < prefabList.Count; i++)
            {
                var childObject = Instantiate(prefabList[i]);
                childObject.SetActive(true);
                childObject.transform.SetParent(parentObject.transform, false);
                Debug.Log("overlay criado");
            }
            Debug.Log("number of children: " + parentObject.transform.childCount);
            m_Instantiated[trackedImage.referenceImage.name] = parentObject;
        }
    }

    private void UpdateTrackingGameObject(ARTrackedImage updatedImage)
    {
        if (m_Instantiated.TryGetValue(updatedImage.referenceImage.name, out GameObject instantiatedPrefab))
        {
            // Debug.Log("Image name: " + updatedImage.referenceImage.name);
            instantiatedPrefab.transform.position = updatedImage.transform.position;
            instantiatedPrefab.transform.rotation = updatedImage.transform.rotation;
            instantiatedPrefab.SetActive(true);
        }
    }

    private void UpdateLimitedGameObject(ARTrackedImage updatedImage)
    {
        if (m_Instantiated.TryGetValue(updatedImage.referenceImage.name, out GameObject instantiatedPrefab))
        {
            instantiatedPrefab.SetActive(false);
        }
    }

    private void UpdateNoneGameObject(ARTrackedImage updatedImage)
    {
        if (m_Instantiated.TryGetValue(updatedImage.referenceImage.name, out GameObject instantiatedPrefab))
        {
            instantiatedPrefab.SetActive(false);
        }
    }

    private void DestroyGameObject(ARTrackedImage removedImage)
    {
        if (m_Instantiated.TryGetValue(removedImage.referenceImage.name, out GameObject instantiatedPrefab))
        {             
            m_Instantiated.Remove(removedImage.referenceImage.name);
            Destroy(instantiatedPrefab);
        }
    }

    private GameObject LoadModel(string path)
    {
        // https://github.com/Siccity/GLTFUtility/issues/89
        AnimationClip[] animationClips;
        var importSettings = new ImportSettings();
        importSettings.useLegacyClips = true;
        GameObject model = Importer.LoadFromFile(path, importSettings, out animationClips);

        // criar a animacao do model
        if (animationClips.Length > 0)
        {
            Animation animation = model.AddComponent<Animation>();
            animationClips[0].legacy = true;
            animation.AddClip(animationClips[0], animationClips[0].name);
            animation.clip = animation.GetClip(animationClips[0].name);
            animation.wrapMode = WrapMode.Loop;
            animation.Play();
        }
        return model;
    }
    
    private IEnumerator UpdatePrefabDictionary(ARTrackedImage trackedImage)
    {
        List<ApplicationModel.Overlay> overlayList = ApplicationModel.GetOverlayList(trackedImage.referenceImage.name); 

        string resourceUrl;
        List<GameObject> downloadedOverlaysList = new List<GameObject>();
        foreach (var overlay in overlayList)
        {
            var overlayType = overlay.type;
            switch (overlayType)
            {
                case "image":
                    resourceUrl = overlay.url;

                    if (File.Exists(resourceUrl))
                    {
                        Debug.Log("Loading image: " + resourceUrl.ToString());
                        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + resourceUrl);
                        yield return uwr.SendWebRequest();
                        var texture = DownloadHandlerTexture.GetContent(uwr);

                        if (texture)
                        {
                            Debug.Log("Image loaded: " + resourceUrl.ToString());
                            GameObject imageObject = GameObject.CreatePrimitive(PrimitiveType.Quad);

                            // calibration
                            var imageScale = new ApplicationModel.Scale();
                            imageScale.x = overlay.scale.x;
                            imageScale.y = overlay.scale.y * texture.height / texture.width;
                            imageScale.z = overlay.scale.z;

                            var imagePosition = new ApplicationModel.Position();
                            imagePosition.x = overlay.position.x;
                            imagePosition.y = overlay.position.y;
                            imagePosition.z = -overlay.position.z;

                            var imageRotation = new ApplicationModel.Rotation();
                            imageRotation.x = -overlay.rotation.x;
                            imageRotation.y = -overlay.rotation.z;
                            imageRotation.z = -overlay.rotation.y;

                            imageObject.GetComponent<Renderer>().material.mainTexture = texture;

                            imageObject = setTransformInfos(imageObject, imagePosition, imageRotation, imageScale);
                            imageObject.SetActive(false);

                            downloadedOverlaysList.Add(imageObject);
                        }
                        else
                        {
                            Debug.Log("Nao foi possivel carregar a imagem:  " + resourceUrl.ToString());
                        }
                    }
                    else
                    {
                        Debug.Log("Imagem nao encontrada:  " + resourceUrl.ToString());
                    }

                    break;

                case "video":
                    resourceUrl = overlay.url;

                    GameObject videoPlayerObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    videoPlayerObject.AddComponent<VideoPlayer>();
                    var player = videoPlayerObject.GetComponent<VideoPlayer>();
                    player.playOnAwake = true;
                    player.url = resourceUrl;
                    videoPlayerObject = setTransformInfos(videoPlayerObject, overlay.position, overlay.rotation, overlay.scale);
                    videoPlayerObject.SetActive(false);

                    downloadedOverlaysList.Add(videoPlayerObject);
                    break;

                case "model":
                    resourceUrl = overlay.url;

                    if (File.Exists(resourceUrl))
                    {
                        Debug.Log("Found file locally, loading...");
                        var model = LoadModel(resourceUrl);

                        // calibration
                        var modelPosition = new ApplicationModel.Position();
                        modelPosition.x = overlay.position.x;
                        modelPosition.y = overlay.position.y;
                        modelPosition.z = -overlay.position.z;
                        model.transform.Rotate(0, 180, 0, Space.World);

                        var modelScale = new ApplicationModel.Scale();
                        modelScale.x = overlay.scale.x;
                        modelScale.y = overlay.scale.z;
                        modelScale.z = overlay.scale.y;

                        model = setTransformInfos(model, modelPosition, overlay.rotation, modelScale);
                        model.SetActive(false);
                        downloadedOverlaysList.Add(model);
                    }
                    else
                    {
                        Debug.Log("Modelo nao encontrado:  " + resourceUrl.ToString());
                    }
                    break;

                default:
                    Debug.LogError("Overlay nao suportado");
                    break;
            }
        }


        // Update no dicionario
        m_PrefabsDictionary[trackedImage.referenceImage.name] = downloadedOverlaysList;
        // Limpa a cena
        DestroyGameObject(trackedImage);

        // Adiciona prefabs a cena
        AssignPrefab(trackedImage);
    }

    private GameObject setTransformInfos(
        GameObject gameObject, 
        ApplicationModel.Position position, 
        ApplicationModel.Rotation rotation, 
        ApplicationModel.Scale scale
    )
    {
        gameObject.transform.position = new Vector3(position.x, position.y, position.z);

        gameObject.transform.rotation = 
            gameObject.transform.rotation *
            Quaternion.AngleAxis(rotation.x, Vector3.right) *
            Quaternion.AngleAxis(rotation.y, Vector3.back) *
            Quaternion.AngleAxis(rotation.z, Vector3.up);

        gameObject.transform.localScale = new Vector3(scale.x, scale.y, scale.z);
        return gameObject;
    }
}
