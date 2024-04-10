﻿using UnityEngine;

namespace Viva
{


    [System.Serializable]
    [CreateAssetMenu(fileName = "Voice", menuName = "Voice", order = 1)]
    public class Voice : ScriptableObject
    {

        public enum VoiceType
        {
            SHINOBU,
            MERIDA,
            MIGI
        }

        public SoundSet[] voiceLines = new SoundSet[System.Enum.GetValues(typeof(Companion.VoiceLine)).Length];
        public VoiceType type;
    }

}