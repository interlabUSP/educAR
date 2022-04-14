using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System;

public class ApplicationModel : MonoBehaviour
{
    static public string _token = "";

    [Serializable]
    public class SceneList
    {
        public string id;
        public List<Scene> scenes;
    }

    [Serializable]
    public class Scene
    {
        public string trigger;
        public string name;
        public List<Overlay> overlays;
    }

    [Serializable]
    public class Overlay
    {
        public Position position;
        public Rotation rotation;
        public Scale scale;
        public string type;
        public string url;
        public Attribution attribution;
    }

    [Serializable]
    public class Attribution
    {
        public License license;
        public User user;
        public Model model;
    }

    [Serializable]
    public class License
    {
        public string name;
        public string url;
    }

    [Serializable]
    public class User
    {
        public string name;
        public string url;
    }

    [Serializable]
    public class Model
    {
        public string name;
        public string url;
    }

    [Serializable]
    public class Position
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class Rotation
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class Scale
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class Dados
    {
        public string password;
    }

    static public SceneList cena;

    static public Scene getScene(string name) {
        if (cena == null)
            throw new Exception("Cena not loaded");
        Scene scene = cena.scenes.Find(scene => scene.name == name);
        if (scene.name == name) {
            return scene;
        } else {
            throw new Exception("Scene not found");
        }
    }
    static public List<Overlay> GetOverlayList(string name) {
        return getScene(name).overlays;
    }
}
