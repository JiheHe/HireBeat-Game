using System;
using UnityEngine;
using UnityEngine.Audio;

namespace FrostweepGames.VoicePro
{
    [Serializable]
    public class SpeakerConfig
    {
        [Range(0f, 1f)]
        [Tooltip("0 - 2D : 1 = 3D")]
        public float spatialBlend;

        public AudioMixerGroup outputAudioMixerGroup;

        [Range(1, 3600)]
        [Tooltip("if client not receives any data for {maxInactiveTime} seconds then its inactive")]
        public int maxInactiveTime = 120;
    }
}