using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using TMPro;

public class SettingsMenu : MonoBehaviour
{

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    Resolution[] resolutions;

    private void Start()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        QualitySettings.SetQualityLevel(2); // Assumi che l'indice 2 sia per "Alta"
        if (qualityDropdown != null)
        {
            qualityDropdown.value = 2; // Imposta il dropdown alla qualità "Alta"
            qualityDropdown.RefreshShownValue(); // Aggiorna la visualizzazione del dropdown
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
    }

    
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
