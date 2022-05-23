using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SettingsScript : MonoBehaviour
{
    public GameObject playerObj;
    public cameraController playerCamera;
    public InGameUIController playerZoneTab;
    public PlayerMenuUIController UIController;
    public changeReceiver playerHud;

    public Slider gameThemeVolumeSlider;
    public Slider sceneSFXVolumeSlider;
    public GameObject muteAllThemeAndSFXFakeToggle;

    float gameMusicVolume;
    float sceneSFXVolume;
    int allSoundOn; //0 is false, 1 is true

    public AudioManager currAudioManager;

    public void Initialize() //awake is called before start, so it works ;D!!!!!!!!!!!!!!!!
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine) //can also use GetComponent<playerController>().view.IsMine
            {
                playerObj = player;
                break;
            }
        }

        GameObject cameraController = GameObject.FindGameObjectWithTag("PlayerCamera");
        playerCamera = cameraController.GetComponent<cameraController>();
        UIController = cameraController.GetComponent<PlayerMenuUIController>();
        playerZoneTab = cameraController.GetComponent<InGameUIController>();
        playerHud = GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<changeReceiver>();
    }

    // Start is called before the first frame update
    void Start()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        GameObject cameraController = GameObject.FindGameObjectWithTag("PlayerCamera");
        playerCamera = cameraController.GetComponent<cameraController>();
        UIController = cameraController.GetComponent<PlayerMenuUIController>();
        playerZoneTab = cameraController.GetComponent<InGameUIController>();
        playerHud = GameObject.FindGameObjectWithTag("PlayerHUD").transform.GetChild(0).GetComponent<changeReceiver>();
        if (!playerZoneTab.hasOneOn) //prevents zone + UI
        {
            playerObj.GetComponent<playerController>().enabled = false;
            playerCamera.enabled = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTabOpen()
    {
        if (!playerZoneTab.hasOneOn) //prevents zone + UI
        {
            playerObj.GetComponent<playerController>().enabled = false;
            playerCamera.enabled = false;
        }
    }

    public void SetCurrentAudioManager(AudioManager manager)
    {
        currAudioManager = manager;
    }

    public void SetGameThemeVolume(float volume)
    {
        currAudioManager.SetSceneThemeVolume(volume);
        gameMusicVolume = volume;
    }

    public void SetSceneSFXVolume(float volume)
    {
        currAudioManager.SetAllSFXVolume(volume);
        sceneSFXVolume = volume;
    }

    public void AllThemeAndSFXOn(bool areOn)
    {
        currAudioManager.MuteAll(!areOn);
        if(areOn) allSoundOn = 1;
        else allSoundOn = 0;
    }

    public void OnSaveButtonPressed()
    {
        PlayerPrefs.SetFloat("MUSICVOL", gameMusicVolume);
        PlayerPrefs.SetFloat("SFXVOL", sceneSFXVolume);
        PlayerPrefs.SetInt("ALLSOUNDON", allSoundOn);
        PlayerPrefs.Save();

        closeWindow(); 
    }

    public void LoadInPlayerPrefSettings() 
    {
        if(PlayerPrefs.HasKey("ALLSOUNDON"))
        {
            gameMusicVolume = PlayerPrefs.GetFloat("MUSICVOL");
            sceneSFXVolume = PlayerPrefs.GetFloat("SFXVOL");
            allSoundOn = PlayerPrefs.GetInt("ALLSOUNDON");
        }
        else //default values
        {
            gameMusicVolume = 1;
            sceneSFXVolume = 1;
            allSoundOn = 1;
        }

        SetGameThemeVolume(gameMusicVolume);
        gameThemeVolumeSlider.value = gameMusicVolume;
        SetSceneSFXVolume(sceneSFXVolume);
        sceneSFXVolumeSlider.value = sceneSFXVolume;
        if (allSoundOn == 1)
        {
            AllThemeAndSFXOn(true);
            muteAllThemeAndSFXFakeToggle.transform.GetChild(0).gameObject.SetActive(true);
            muteAllThemeAndSFXFakeToggle.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            AllThemeAndSFXOn(false);
            muteAllThemeAndSFXFakeToggle.transform.GetChild(0).gameObject.SetActive(false);
            muteAllThemeAndSFXFakeToggle.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void OnCancelOrExitButtonPressed()
    {
        LoadInPlayerPrefSettings();
        closeWindow();
    }

    public void closeWindow()
    {
        gameObject.SetActive(false);
        playerCamera.enabled = true;
        if (!PersistentData.isMovementRestricted)
        {
            playerObj.GetComponent<playerController>().enabled = true;
            playerObj.GetComponent<playerController>().actionParem = (int)playerController.CharActionCode.IDLE; //this line prevents the player from getitng stuck after
        }
        UIController.hasOneOn = false;
    }
}
