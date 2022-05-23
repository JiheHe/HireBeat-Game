using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public string SceneThemeName;
    public bool hasSettings = false;
    public bool isLoading = false;

    public Sound[] sounds;
    // Start is called before the first frame update

    //public static AudioManager instance;

    void Awake() //awake is called before start; set it up so can play at start
    {
        /*if(instance == null) //avoid creating 2 audio managers on scene transition
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }*/

        //DontDestroyOnLoad(gameObject); //sounds carry on through scene transitions

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    void Start()
    {
        if (hasSettings) StartCoroutine(LoadInPlayerPrefSettings()); 
        else if (isLoading)
        {
            if (PlayerPrefs.HasKey("ALLSOUNDON"))
            {
                if (PlayerPrefs.GetInt("ALLSOUNDON") == 0) MuteAll(true);
                else SetAllSFXVolume(PlayerPrefs.GetFloat("SFXVOL"));
            }
            //else stays on and 1
        }
        else if (SceneThemeName.Length != 0) Play(SceneThemeName);
    }

    IEnumerator LoadInPlayerPrefSettings()
    {
        var playerHud = GameObject.FindGameObjectWithTag("PlayerHUD");
        if (playerHud == null)
        {
            yield return null;
            StartCoroutine(LoadInPlayerPrefSettings());
        }
        else
        {
            yield return null;

            var ss = playerHud.transform.Find("Settings").GetComponent<SettingsScript>();
            ss.SetCurrentAudioManager(this);
            ss.LoadInPlayerPrefSettings();
            Play(SceneThemeName);
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name); // cool shorthand ;D
        if(s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return; //just in case..
        }
        s.source.Play();
    }

    public void PlayAll()
    {
        foreach (Sound s in sounds)
        {
            s.source.Play();
        }
    }

    public void MuteAll(bool mute)
    {
        foreach (Sound s in sounds)
        {
            s.source.mute = mute;
        }
    }

    public void SetAllSFXVolume(float volume)
    {
        foreach(Sound s in sounds)
        {
            if(s.name != SceneThemeName) s.source.volume = s.volume * volume;
        }
    }

    public void SetVolume(string name, float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name); // cool shorthand ;D
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return; //just in case..
        }
        //s.volume = volume; //Sound class's volume should stay the same. Only need to change the applied so can rec og value.
        s.source.volume = s.volume * volume;
    }

    public void SetSceneThemeVolume(float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == SceneThemeName); // cool shorthand ;D
        if (s == null)
        {
            Debug.LogWarning("Sound: " + SceneThemeName + " not found!");
            return; //just in case..
        }
        //s.volume = volume;
        s.source.volume = s.volume * volume;
    }
}
