using UnityEngine.Audio;
using UnityEngine;

[System.Serializable] //allows custom class to be shown in inspector
public class Sound
{

    public string name;

    public AudioClip clip;

    [Range(0f, 1f)] //adds a slider
    public float volume;

    [Range(0.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector] //accessible but wont show
    public AudioSource source;
}
