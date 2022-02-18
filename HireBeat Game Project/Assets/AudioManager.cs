using UnityEngine.Audio;
using UnityEngine;
using System;


public class AudioManager : MonoBehaviour
{

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
        }

        DontDestroyOnLoad(gameObject); sounds carry on through scene transitions*/

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
        //Play("ThemeName");
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
}
