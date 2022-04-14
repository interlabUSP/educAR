using System;
using System.Collections;
using UnityEngine.Video;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Siccity.GLTFUtility;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using UnityEngine.SceneManagement;
using glTFLoader;
using System.IO;
using UnityEngine.XR.ARFoundation;

public class DownloadManager : MonoBehaviour {

    private string filePathModel;
    private string filePathImage;
    private string filePathTrigger;

    private int cont = 0;
    private WWW _wwwRequest;

    [Serializable]
    public class DownloadApiResponse
    {
        public Gltf gltf;
    }
    
    [Serializable]
    public class Gltf
    {
        public string url;
    }

    void Awake()
    {
        string IDcanal = ApplicationModel.cena.id;
        filePathModel = $"{Application.temporaryCachePath}/Models/" + IDcanal;
        filePathImage = $"{Application.temporaryCachePath}/Images/" + IDcanal;
        filePathTrigger = $"{Application.temporaryCachePath}/Trigger/" + IDcanal;
    }

    public IEnumerator GetTriggers(Slider slider, GameObject loadingScreen)
    {
        var scenes = ApplicationModel.cena.scenes;
        loadingScreen.SetActive(true);
        slider.value = 0;
        int progress = 0;
        foreach (var scene in scenes)
        {
            string triggerFile = filePathTrigger + "/" + scene.name +  "/trigger.jpg";
            yield return (GetImageRequest(scene.trigger, triggerFile, (UnityWebRequest req) =>
            {
                if (((req.result == UnityWebRequest.Result.ConnectionError)) || (req.result == UnityWebRequest.Result.ProtocolError))
                {
                    Debug.Log($"{req.error} : {req.downloadHandler.text}");
                }
            }));
            ApplicationModel.cena.scenes[scenes.IndexOf(scene)].trigger = triggerFile;

            progress++;
            float process = progress / scenes.Count;
            slider.value = Mathf.Clamp01(process/ .9f);
            Debug.Log("Imagem baixada");
        }

    }

    public IEnumerator GetOverlays(ARTrackedImage trackedImage, ImageTrackingManager.CoroutineCallback callback)
    {
        // Reseta o contador que diferencia entre as imagens
        cont = 0; 
        List<ApplicationModel.Scene> scenes = ApplicationModel.cena.scenes;
        ApplicationModel.Scene scene = ApplicationModel.getScene(trackedImage.referenceImage.name);
        Debug.Log(scenes.ToString());

        foreach (ApplicationModel.Overlay overlay in scene.overlays)
        {
            Debug.Log(overlay.type);
            string overlayType = overlay.type;
            string resourceUrl = overlay.url;

            if (File.Exists(resourceUrl))
            {
                continue;
            }

            switch (overlayType)
            {
                case "image":
                    string pathimage = GetImagePath(scene.name);
                    yield return (GetImageRequest(resourceUrl, pathimage, (UnityWebRequest req) =>
                    {
                            if (((req.result == UnityWebRequest.Result.ConnectionError)) || (req.result == UnityWebRequest.Result.ProtocolError))
                            {
                                Debug.Log($"{req.error} : {req.downloadHandler.text}");
                            }
                    }));
                    ApplicationModel.cena.scenes[scenes.IndexOf(scene)].overlays[scene.overlays.IndexOf(overlay)].url = pathimage;
                    break;


                case "video":
                    break;

                case "model":
                    yield return StartCoroutine(DownloadSketchfabLink(resourceUrl, scene.name));
                    string pathmodel = $"{filePathModel}/{scene.name}/{resourceUrl}/model.glb";
                    ApplicationModel.cena.scenes[scenes.IndexOf(scene)].overlays[scene.overlays.IndexOf(overlay)].url = pathmodel;
                    break;
            }

        }

        yield return StartCoroutine(callback(trackedImage));
    }

    private IEnumerator GetImageRequest(string url, string path, Action<UnityWebRequest> callback)
    {
        
        using(UnityWebRequest req = UnityWebRequest.Get(url))
        {
            print("A url e:" + url);
            req.downloadHandler = new DownloadHandlerFile(path);
            yield return req.SendWebRequest();
            Debug.Log("Download completed");
            callback(req);
        }

    }

    private IEnumerator DownloadSketchfabLink(string UID, string nomeCena)
    {

        var headers = new Dictionary<string, string>();
        Debug.Log("UID:" + UID);
        headers["Authorization"] = "Bearer " + ApplicationModel._token;

        yield return _wwwRequest = new WWW("https://api.sketchfab.com/v3/models/" + UID + "/download", null, headers);

        var downloadLink = JsonUtility.FromJson<DownloadApiResponse> (_wwwRequest.text).gltf.url;

        Debug.Log("Link donwload sketchfab:" + downloadLink);

        yield return StartCoroutine ((DownloadGltfModel(downloadLink, UID, nomeCena, (UnityWebRequest req) =>
            {
                if (((req.result == UnityWebRequest.Result.ConnectionError)) || (req.result == UnityWebRequest.Result.ProtocolError))
                {
                    Debug.Log($"{req.error} : {req.downloadHandler.text}");
                }
            })));

    }

    private IEnumerator DownloadGltfModel(string url, string UID, string nomeCena, Action<UnityWebRequest> callback)
    {
        
        using(UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.downloadHandler = new DownloadHandlerFile(GetModelPath(UID, nomeCena));
            yield return req.SendWebRequest();

            Debug.Log("Download completed");
            Unzip(UID, nomeCena);
            Interface.Pack(filePathModel + "/" + nomeCena + "/" + UID + "/scene.gltf", filePathModel + "/" + nomeCena + "/" + UID + "/model.glb");
            File.Delete(GetModelPath(UID, nomeCena));
            callback(req);
        }

    }

    private void Unzip(String UID, string nomeCena)
    {
        var zipFileName = GetModelPath(UID, nomeCena);
        var targetDir = filePathModel + $"/{nomeCena}/{UID}";
        FastZip fastZip = new FastZip();
        string fileFilter = null;
        fastZip.ExtractZip(zipFileName, targetDir, fileFilter);
        Debug.Log("Unzip finalizado");
    }

    private string GetImagePath(string nomeCena)
    {
        int aux = cont;
        cont += 1;
        
        Debug.Log($"{filePathImage}/{nomeCena}/{aux.ToString() + ".jpg"}");

        return $"{filePathImage}/{nomeCena}/{aux.ToString() + ".jpg"}";


    }

    private string GetModelPath(string UID, string nomeCena)
    {
        string fileName = UID + ".zip";
        Debug.Log($"{filePathModel}/{nomeCena}/{fileName}");

        return $"{filePathModel}/{nomeCena}/{fileName}";
    }
}
