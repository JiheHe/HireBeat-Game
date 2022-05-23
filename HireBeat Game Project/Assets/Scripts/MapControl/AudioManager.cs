using UnityEngine.Audio;
using UnityEngine;
using System;


public class AudioManager : MonoBehaviour
{
    public string SceneThemeName;

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
        if (SceneThemeName.Length != 0) Play(SceneThemeName);
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

    public void SetVolume(string name, float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name); // cool shorthand ;D
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return; //just in case..
        }
        s.volume = volume;
        s.source.volume = volume;
    }

    public void SetSceneThemeVolume(float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == SceneThemeName); // cool shorthand ;D
        if (s == null)
        {
            Debug.LogWarning("Sound: " + SceneThemeName + " not found!");
            return; //just in case..
        }
        s.volume = volume;
        s.source.volume = volume;
    }
}
