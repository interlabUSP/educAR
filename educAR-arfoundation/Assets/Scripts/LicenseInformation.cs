using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LicenseInformation : MonoBehaviour
{
    public Button license;

    private DownloadManager downloadManager;

    private PopupLicense popup;
    
    void Start()
    {
        license.onClick.AddListener(GetLicenseInformation);
    }

    public void GetLicenseInformation()
    {
        if (popup == null || popup.popupActivated == false)
        {
            popup = UIController.Instance.CreatePopup();
            string text = GetChannelLicenseInformation();
            popup.Init(UIController.Instance.mainCanvas, text);
            popup.popupActivated = true;
        }
    }

    public string GetChannelLicenseInformation()
    {
        List<ApplicationModel.Scene> scenes = ApplicationModel.cena.scenes;

        string channelLicenseInformation = "";
        foreach (var scene in scenes)
        {
            foreach (var overlay in scene.overlays)
            {
                string overlayType = overlay.type;
                string resourceUrl = overlay.url;

                switch (overlayType)
                {
                    case "image":
                        channelLicenseInformation += $"Imagem disponível em {resourceUrl}\n\n";
                        break;

                    case "video":
                        break;

                    case "model":
                        if (overlay.attribution.license != null)
                        {
                            var attribution = overlay.attribution;
                            ApplicationModel.License license = attribution.license;
                            ApplicationModel.User user = attribution.user;
                            ApplicationModel.Model model = attribution.model;
                            channelLicenseInformation += $"Modelo {model.name} ({model.url}) do autor {user.name} ({user.url}) distribuído sob a licença {license.name} ({license.url})\n\n";
                        }
                        break;
                }

            }
        }
        if (string.IsNullOrEmpty(channelLicenseInformation))
        {
            channelLicenseInformation = "O canal não possui informações de licença";
        }
        return channelLicenseInformation;
    }

}
