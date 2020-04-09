using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuControllerInGame : MonoBehaviour
{
    #region Default Values
    [Header("Default Menu Values")]
    [SerializeField] private float defaultBrightness;
    [SerializeField] private float defaultMasterVolume;
    [SerializeField] private float defaultVolumeMusic;
    [SerializeField] private float defaultVolumeEffects;
    [SerializeField] private int defaultSen;


    [Space(10)]
    [Header("Menu sounds")]
    public AudioSource menuSoundEffect;
    public AudioSource mainMenuMusic;

    [SerializeField] private int menuNumber;
    #endregion

    #region Menu Dialogs
    [Header("Main Menu Components")]
    [SerializeField] private GameObject menuDefaultCanvas;
    [SerializeField] private GameObject GeneralSettingsCanvas;
    [SerializeField] private GameObject graphicsMenu;
    [SerializeField] private GameObject soundMenu;
    [SerializeField] private GameObject gameplayMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject confirmationMenu;
    [Space(10)]
    [Header("Menu Popout Dialogs")]
    [SerializeField] private GameObject noSaveDialog;
    [SerializeField] private GameObject loadGameDialog;
    #endregion

    #region Slider Linking
    [Header("Menu Sliders")]
    [SerializeField] private Text mouseSensitivityTex;
    [SerializeField] private Slider mouseSensitivitySlider;
    public float mouseSensitivityFloat = 2f;
    [Space(10)]
    [SerializeField] private Brightness brightnessEffect;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Text brightnessText;
    [Space(10)]
    [SerializeField] private Text volumeMasterText;
    [SerializeField] private Slider volumeSliderMaster;
    [SerializeField] private Text volumeTextMusic;
    [SerializeField] private Slider volumeSliderMusic;
    [SerializeField] private Text volumeTextEffects;
    [SerializeField] private Slider volumeSliderEffects;
    #endregion

    #region Initialisation - Button Selection & Menu Order
    private void Start()
    {
        menuNumber = 1;
        set_options_on_start();

        if (PlayerPrefs.HasKey("effectsVolume"))
            set_effects_volume(PlayerPrefs.GetFloat("effectsVolume"));

        if (PlayerPrefs.HasKey("musicVolume"))
            set_music_volume(PlayerPrefs.GetFloat("musicVolume"));
    }
    #endregion

    private void set_options_on_start()
    {
        //graphics

        //sound
        //main
        if (PlayerPrefs.HasKey("masterVolume"))
        {
            volumeSliderMaster.value = PlayerPrefs.GetFloat("masterVolume");
        }
        else
        {
            volumeSliderMaster.value = defaultMasterVolume;
        }
        volumeMasterText.text = volumeSliderMaster.value.ToString("0.0");

        //music
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            volumeSliderMusic.value = PlayerPrefs.GetFloat("musicVolume");
        }
        else
        {
            volumeSliderMusic.value = defaultVolumeMusic;
        }
        volumeTextMusic.text = volumeSliderMusic.value.ToString("0.0");
        //soundEffects
        if (PlayerPrefs.HasKey("effectsVolume"))
        {
            volumeSliderEffects.value = PlayerPrefs.GetFloat("effectsVolume");
        }
        else
        {
            volumeSliderEffects.value = defaultVolumeEffects;
        }
        volumeTextEffects.text = volumeSliderEffects.value.ToString("0.0");

        //gamelpay

        //sensitivity
        if (PlayerPrefs.HasKey("masterSen"))
        {
            mouseSensitivityFloat = PlayerPrefs.GetFloat("masterSen");
        }
        else
        {
            mouseSensitivityFloat = defaultSen;
        }
        mouseSensitivitySlider.value = mouseSensitivityFloat;
        mouseSensitivityTex.text = mouseSensitivityFloat.ToString("0.0");
        Prefs.mouse_sensitivity = mouseSensitivityFloat;


    }



    //MAIN SECTION
    #region Confrimation Box
    public IEnumerator ConfirmationBox()
    {
        confirmationMenu.SetActive(true);
        yield return new WaitForSeconds(2);
        confirmationMenu.SetActive(false);
    }
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuNumber == 2 || menuNumber == 7 || menuNumber == 8)
            {
                GoBackToMainMenu();
                ClickSound();
            }

            else if (menuNumber == 3 || menuNumber == 4 || menuNumber == 5)
            {
                GoBackToOptionsMenu();
                ClickSound();
            }

            else if (menuNumber == 6) //CONTROLS MENU
            {
                GoBackToGameplayMenu();
                ClickSound();
            }
        }
    }

    private void ClickSound()
    {
        GetComponent<AudioSource>().Play();
    }

    #region Menu Mouse Clicks
    public void MouseClick(string buttonType)
    {
        if (buttonType == "Controls")
        {
            gameplayMenu.SetActive(false);
            controlsMenu.SetActive(true);
            menuNumber = 6;
        }

        if (buttonType == "Graphics")
        {
            GeneralSettingsCanvas.SetActive(false);
            graphicsMenu.SetActive(true);
            menuNumber = 3;
        }

        if (buttonType == "Sound")
        {
            GeneralSettingsCanvas.SetActive(false);
            soundMenu.SetActive(true);
            menuNumber = 4;
        }

        if (buttonType == "Gameplay")
        {
            GeneralSettingsCanvas.SetActive(false);
            gameplayMenu.SetActive(true);
            menuNumber = 5;
        }

        if (buttonType == "Exit")
        {
            Debug.Log("YES QUIT!");
            Application.Quit();
        }

        if (buttonType == "Options")
        {
            menuDefaultCanvas.SetActive(false);
            GeneralSettingsCanvas.SetActive(true);
            menuNumber = 2;
        }

        if (buttonType == "DirectConnect")
        {
            SceneManager.LoadScene("MultiplayerMenu");
        }

    }
    #endregion

    #region Volume Sliders Click
    public void SetMasterVolumeFromSlider()
    {
        set_master_volume();
        volumeMasterText.text = volumeSliderMaster.value.ToString("0.0");
    }
    public void SetMusicVolumeFromSlider()
    {
        float volume = volumeSliderMusic.value;
        set_music_volume(volume);
        volumeTextMusic.text = volume.ToString("0.0");
    }
    public void SetEffectsVolumeFromSlider()
    {
        float volume = volumeSliderEffects.value;
        set_effects_volume(volume);
        volumeTextEffects.text = volume.ToString("0.0");
    }

    void set_master_volume()
    {
        AudioListener.volume = volumeSliderMaster.value;
        Prefs.volumeMaster = volumeSliderMaster.value;
        PlayerPrefs.SetFloat("masterVolume", volumeSliderMaster.value);
    }

    void set_music_volume(float v)
    {
        Prefs.volumeMusic = v;

        PlayerPrefs.SetFloat("musicVolume", volumeSliderMusic.value);
        Exploration_and_Battle.Instance.OnVolumeSettingsChanged();
    }

    void set_effects_volume(float v)
    {
        Prefs.volume_effects = v;
        PlayerPrefs.SetFloat("effectsVolume", volumeSliderEffects.value);
        GetComponent<AudioSource>().volume = v;

    }
    public void VolumeApply()
    {
        PlayerPrefs.SetFloat("masterVolume", volumeSliderMaster.value);
        PlayerPrefs.SetFloat("musicVolume", volumeSliderMusic.value);
        PlayerPrefs.SetFloat("effectsVolume", volumeSliderEffects.value);


        StartCoroutine(ConfirmationBox());
    }
    #endregion

    #region Brightness Sliders Click
    public void BrightnessSlider()
    {
        brightnessEffect.brightness = brightnessSlider.value;
        brightnessText.text = brightnessSlider.value.ToString("0.0");
    }

    public void BrightnessApply()
    {
        PlayerPrefs.SetFloat("masterBrightness", brightnessEffect.brightness);
        Debug.Log(PlayerPrefs.GetFloat("masterBrightness"));
        StartCoroutine(ConfirmationBox());
    }
    #endregion

    #region Controller Sensitivity
    public void ControllerSen()
    {
        mouseSensitivityTex.text = mouseSensitivitySlider.value.ToString("0.0");
        mouseSensitivityFloat = mouseSensitivitySlider.value;
        Prefs.mouse_sensitivity = mouseSensitivityFloat;
    }
    #endregion

    public void GameplayApply()
    {
        #region Mouse Sensitivity
        PlayerPrefs.SetFloat("masterSen", mouseSensitivityFloat);
        Debug.Log("Sensitivity" + " " + PlayerPrefs.GetFloat("masterSen"));
        #endregion

        StartCoroutine(ConfirmationBox());
    }

    #region ResetButton
    public void ResetButton(string menu)
    {
        if (menu == "Brightness")
        {
            brightnessEffect.brightness = defaultBrightness;
            brightnessSlider.value = defaultBrightness;
            brightnessText.text = defaultBrightness.ToString("0.0");
            BrightnessApply();
        }

        if (menu == "Audio")
        {
            AudioListener.volume = defaultMasterVolume;
            volumeSliderMaster.value = defaultMasterVolume;
            volumeMasterText.text = defaultMasterVolume.ToString("0.0");

            volumeSliderMusic.value = defaultVolumeMusic;
            volumeTextMusic.text = defaultVolumeMusic.ToString("0.0");

            volumeSliderEffects.value = defaultVolumeEffects;
            volumeTextEffects.text = defaultVolumeEffects.ToString("0.0");
            VolumeApply();
        }

        if (menu == "Gameplay")
        {
            mouseSensitivityTex.text = defaultSen.ToString("0.0");
            mouseSensitivitySlider.value = defaultSen;
            mouseSensitivityFloat = defaultSen;
            GameplayApply();
        }
    }
    #endregion

   

    #region Back to Menus
    public void GoBackToOptionsMenu()
    {
        GeneralSettingsCanvas.SetActive(true);
        graphicsMenu.SetActive(false);
        soundMenu.SetActive(false);
        gameplayMenu.SetActive(false);

        GameplayApply();
        BrightnessApply();
        VolumeApply();

        menuNumber = 2;
    }

    public void GoBackToMainMenu()
    {
        menuDefaultCanvas.SetActive(true);
        loadGameDialog.SetActive(false);
        noSaveDialog.SetActive(false);
        GeneralSettingsCanvas.SetActive(false);
        graphicsMenu.SetActive(false);
        soundMenu.SetActive(false);
        gameplayMenu.SetActive(false);
        menuNumber = 1;
    }

    public void GoBackToGameplayMenu()
    {
        controlsMenu.SetActive(false);
        gameplayMenu.SetActive(true);
        menuNumber = 5;
    }

    public void ClickQuitOptions()
    {
        GoBackToMainMenu();
    }

    public void ClickNoSaveDialog()
    {
        GoBackToMainMenu();
    }

    public void HelpButtonClicked() {
        Debug.Log("clicked help");
    }

    public void CustomizationButtonClicked() {
        Debug.Log("Customization button clicked");
    }

    public void DisconnectButtonClicked() {
        Debug.Log("Disconnect Button Clicked. we will set disconnection method in NetworkPlayerStats because this script is not network aware.");
        UILogic.localPlayerGameObject.GetComponent<NetworkPlayerStats>().localDisconnectRequest();
    }
    #endregion
}

