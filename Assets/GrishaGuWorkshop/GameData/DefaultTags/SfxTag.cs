using System;
using UnityEngine;

namespace GrishaGuWorkshop
{
    [Serializable]
    public class SfxTag : DataEntityTag
    {
        public string soundId;
    }

    public static class SfxTagAdapter
    {
        public static AudioSource PlaySound2D(this AudioManager audioManager, SfxTag sfxTag, 
            float volume = 1f,
            float skipToTime = 0f,
            AudioParams.Pitch pitch = null,
            AudioParams.Repetition repetition = null, 
            AudioParams.Randomization randomization = null,
            AudioParams.Distortion distortion = null, 
            bool looping = false)
        {
            return audioManager.PlaySound2D(sfxTag.soundId, volume, skipToTime, pitch, repetition, randomization, distortion, looping);
        }

        public static AudioSource PlaySound3D(this AudioManager audioManager, SfxTag sfxTag, Vector3 position,
            float volume = 1f, 
            float skipToTime = 0f,
            AudioParams.Pitch pitch = null,
            AudioParams.Repetition repetition = null, 
            AudioParams.Randomization randomization = null,
            AudioParams.Distortion distortion = null, 
            bool looping = false)
        {
            return audioManager.PlaySound3D(sfxTag.soundId, position, volume, skipToTime, pitch, repetition, randomization, distortion, looping);
        }
    }
}