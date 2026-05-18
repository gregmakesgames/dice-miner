using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace GrishaGuWorkshop
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource BaseLoopSource
        {
            get { return musicSources[0]; }
        }

        public static AudioManager Instance { get; private set; }

        public const float GBC_INTERIOR_BGM_LOWEREDVOLUME = 0.35f;
        public const float GBC_INTERIOR_BGM_FULLVOLUME = 0.55f;

        [SerializeField] private List<AudioSource> musicSources = default;

        private List<AudioClip> sfx = new List<AudioClip>();
        private List<AudioClip> musics = new List<AudioClip>();

        private List<AudioSource> ActiveSFXSources
        {
            get
            {
                activeSFX.RemoveAll(x => x == null || ReferenceEquals(x, null));
                return activeSFX;
            }
        }

        private List<AudioSource> activeSFX = new List<AudioSource>();

        public bool Fading { get; set; }

        private Dictionary<string, float> limitedFrequencySounds = new Dictionary<string, float>();
        private Dictionary<string, int> lastPlayedSounds = new Dictionary<string, int>();

        private List<AudioMixer> loadedMixers = new List<AudioMixer>();
        private AudioMixerGroup currentSFXMixer = default;

        private const string SOUNDID_REPEAT_DELIMITER = "#";
        private const float DEFAULT_SPATIAL_BLEND = 0.75f;

        private readonly int[] DEFAULT_MUSICSOURCE_INDICES = new int[] { 0 };

        void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);

            foreach (object o in Resources.LoadAll("Audio/SFX"))
            {
                sfx.Add((AudioClip)o);
            }

            foreach (object o in Resources.LoadAll("Audio/Music"))
            {
                musics.Add((AudioClip)o);
            }
        }

        public AudioSource GetMusicSource(int index)
        {
            return musicSources[index];
        }

        private float GetVolumeFromOptions(int volume, int maxVolume)
        {
            float normalizedValue = volume / (float)maxVolume;
            float adjustedValue = Mathf.Pow(normalizedValue, 0.2f);
            return (1f - adjustedValue) * -80f;
        }

        public AudioSource PlaySound2D(string soundId, float volume = 1f, float skipToTime = 0f,
            AudioParams.Pitch pitch = null,
            AudioParams.Repetition repetition = null, AudioParams.Randomization randomization = null,
            AudioParams.Distortion distortion = null, bool looping = false)
        {
            var source = PlaySound3D(soundId, Vector3.zero, volume, skipToTime, pitch, repetition, randomization,
                distortion, looping);

            if (source != null)
            {
                source.spatialBlend = 0f;
            }

            return source;
        }

        public AudioSource PlaySound3D(string soundId, Vector3 position, float volume = 1f, float skipToTime = 0f,
            AudioParams.Pitch pitch = null,
            AudioParams.Repetition repetition = null, AudioParams.Randomization randomization = null,
            AudioParams.Distortion distortion = null, bool looping = false)
        {
            if (repetition != null)
            {
                if (RepetitionIsTooFrequent(soundId, repetition.minRepetitionFrequency, repetition.entryId))
                {
                    return null;
                }
            }

            string randomVariationId = soundId;
            if (randomization != null)
            {
                randomVariationId = GetRandomVariationOfSound(soundId, randomization.noRepeating);
            }

            var source = CreateAudioSourceForSound(randomVariationId, position, looping);
            if (source != null)
            {
                source.volume = volume;
                source.time = source.clip.length * skipToTime;

                if (pitch != null)
                {
                    source.pitch = pitch.pitch;
                }

                if (distortion != null)
                {
                    if (distortion.muffled)
                    {
                        MuffleSource(source);
                    }
                }
            }

            activeSFX.Add(source);
            return source;
        }

        public void SetAllSoundsPaused(bool paused)
        {
            ActiveSFXSources.ForEach(x =>
            {
                if (paused)
                {
                    x.Pause();
                }
                else
                {
                    x.UnPause();
                }
            });
        }

        public void FadeSourceVolume(AudioSource source, float volume, float duration, bool obeyTimescale = true)
        {
            source.DOFade(volume, duration).SetUpdate(!obeyTimescale);
        }

        public AudioClip GetMusicClip(string loopId)
        {
            return musics.Find(x => x.name.ToLowerInvariant() == loopId.ToLowerInvariant());
        }

        public AudioClip GetAudioClip(string soundId)
        {
            return sfx.Find(x => x.name.ToLowerInvariant() == soundId.ToLowerInvariant());
        }

        private AudioSource CreateAudioSourceForSound(string soundId, Vector3 position, bool looping)
        {
            if (!string.IsNullOrEmpty(soundId))
            {
                AudioClip sound = GetAudioClip(soundId);

                if (sound != null)
                {
                    return InstantiateAudioObject(sound, position, looping);
                }
            }

            return null;
        }

        private AudioSource InstantiateAudioObject(AudioClip clip, Vector3 pos, bool looping)
        {
            GameObject tempGO = new GameObject("Audio_" + clip.name);
            tempGO.transform.position = pos;

            AudioSource aSource = tempGO.AddComponent<AudioSource>();
            aSource.clip = clip;
            aSource.outputAudioMixerGroup = currentSFXMixer;
            aSource.spatialBlend = DEFAULT_SPATIAL_BLEND;

            aSource.Play();
            if (looping)
            {
                aSource.loop = true;
            }
            else
            {
                Destroy(tempGO, clip.length * 3f);
            }

            return aSource;
        }

        private bool RepetitionIsTooFrequent(string soundId, float frequencyMin, string entrySuffix = "")
        {
            float time = Time.unscaledTime;
            string soundKey = soundId + entrySuffix;

            if (limitedFrequencySounds.ContainsKey(soundKey))
            {
                if (time - frequencyMin > limitedFrequencySounds[soundKey])
                {
                    limitedFrequencySounds[soundKey] = time;
                    return false;
                }
            }
            else
            {
                limitedFrequencySounds.Add(soundKey, time);
                return false;
            }

            return true;
        }

        private string GetRandomVariationOfSound(string soundPrefix, bool noRepeating)
        {
            string soundId = "";

            if (!string.IsNullOrEmpty(soundPrefix))
            {
                List<AudioClip> variations = sfx.FindAll(x =>
                    x != null && x.name.ToLowerInvariant()
                        .StartsWith(soundPrefix.ToLowerInvariant() + SOUNDID_REPEAT_DELIMITER));

                if (variations.Count > 0)
                {
                    int index = Random.Range(0, variations.Count) + 1;
                    if (noRepeating)
                    {
                        if (!lastPlayedSounds.ContainsKey(soundPrefix))
                        {
                            lastPlayedSounds.Add(soundPrefix, index);
                        }
                        else
                        {
                            int breakOutCounter = 0;
                            const int BREAK_OUT_THRESHOLD = 100;
                            while (lastPlayedSounds[soundPrefix] == index && breakOutCounter < BREAK_OUT_THRESHOLD)
                            {
                                index = Random.Range(0, variations.Count) + 1;
                                breakOutCounter++;
                            }

                            if (breakOutCounter >= BREAK_OUT_THRESHOLD - 1)
                            {
                                Debug.Log("Broke out of infinite loop! AudioController.PlayRandomSound.");
                            }

                            lastPlayedSounds[soundPrefix] = index;
                        }
                    }

                    soundId = soundPrefix + SOUNDID_REPEAT_DELIMITER + index;
                }
                else
                {
                    soundId = soundPrefix;
                }
            }

            return soundId;
        }

        private void MuffleSource(AudioSource source, float cutoff = 300f)
        {
            var filter = source.gameObject.AddComponent<AudioLowPassFilter>();
            filter.cutoffFrequency = cutoff;
        }

        private void UnMuffleSource(AudioSource source)
        {
            var lowPassFilter = source.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter != null)
            {
                Destroy(lowPassFilter);
            }
        }

        public void MuffleMusic(float cutoff, int musicIndex = 0)
        {
            MuffleSource(musicSources[musicIndex], cutoff);
        }

        public void UnMuffleMusic(int musicIndex = 0)
        {
            UnMuffleSource(musicSources[musicIndex]);
        }

        public void SetMusicTimeNormalized(float normalizedTime, int musicIndex = 0)
        {
            if (musicSources[musicIndex].clip != null)
            {
                musicSources[musicIndex].time = Mathf.Clamp(normalizedTime * musicSources[musicIndex].clip.length, 0f,
                    musicSources[musicIndex].clip.length - 0.1f);
            }
        }

        public void SetMusicPaused(bool paused)
        {
            foreach (AudioSource musicSource in musicSources)
            {
                if (paused)
                {
                    musicSource.Pause();
                }
                else
                {
                    musicSource.UnPause();
                }
            }
        }

        public void ResumeMusic(float fadeInSpeed = float.MaxValue)
        {
            foreach (AudioSource musicSource in musicSources)
            {
                musicSource.UnPause();

                if (!musicSource.isPlaying)
                {
                    musicSource.Play();
                }
            }
        }

        public void RestartMusic(int sourceIndex = 0)
        {
            musicSources[sourceIndex].Stop();
            musicSources[sourceIndex].time = 0f;
            musicSources[sourceIndex].volume = 1f;
            musicSources[sourceIndex].pitch = 1f;
            musicSources[sourceIndex].Play();
        }

        public void StopAllMusic()
        {
            CancelFades();
            foreach (AudioSource musicSource in musicSources)
            {
                musicSource.Stop();
            }
        }

        public void StopMusic(int sourceIndex = 0)
        {
            musicSources[sourceIndex].Stop();
        }

        public void SetMusicAndPlay(string musicName, int sourceIndex = 0, bool looping = true, bool cancelFades = true)
        {
            if (cancelFades)
            {
                CancelFades();
            }

            TrySetMusic(musicName, sourceIndex);
            RestartMusic(sourceIndex);

            musicSources[sourceIndex].loop = looping;
        }

        public void CrossFadeMusic(string musicName, float duration, float volume = 1f, float newLoopStartTime = 0f)
        {
            if ((musicSources[0].clip == null || musicSources[0].clip.name != musicName) &&
                musics.Exists(x => x.name == musicName))
            {
                CancelFades();
                StartCoroutine(CrossFade(musicName, volume, duration, newLoopStartTime));
            }
        }

        public void FadeOutMusic(float fadeDuration, params int[] sourceIndices)
        {
            CancelFades();

            if (sourceIndices == null || sourceIndices.Length == 0)
            {
                sourceIndices = DEFAULT_MUSICSOURCE_INDICES;
            }

            for (int i = 0; i < sourceIndices.Length; i++)
            {
                StartCoroutine(DoFadeToVolume(fadeDuration, 0f, sourceIndices[i]));
            }
        }

        public void FadeInMusic(float fadeDuration, float toVolume, params int[] sourceIndices)
        {
            CancelFades();

            if (sourceIndices == null || sourceIndices.Length == 0)
            {
                sourceIndices = DEFAULT_MUSICSOURCE_INDICES;
            }

            for (int i = 0; i < sourceIndices.Length; i++)
            {
                StartCoroutine(DoFadeToVolume(fadeDuration, toVolume, sourceIndices[i]));
            }
        }

        public void SetMusicVolumeImmediate(float volume, int sourceIndex = 0)
        {
            CancelFades();
            musicSources[sourceIndex].volume = volume;
        }

        public void SetMusicVolume(float volume, float duration, int sourceIndex = 0, bool cancelOtherFades = true)
        {
            if (cancelOtherFades)
            {
                CancelFades();
            }

            StartCoroutine(DoFadeToVolume(duration, volume, sourceIndex));
        }

        private void CancelFades()
        {
            StopAllCoroutines();
            foreach (AudioSource musicSource in musicSources)
            {
                musicSource.DOKill();
            }

            Fading = false;
        }

        private void TrySetMusic(string musicName, int sourceIndex = 0)
        {
            AudioClip music = GetMusic(musicName);

            if (music != null)
            {
                musicSources[sourceIndex].clip = music;
                musicSources[sourceIndex].pitch = 1f;
            }
        }

        private AudioClip GetMusic(string musicName)
        {
            return musics.Find(x => x.name == musicName);
        }

        private IEnumerator DoFadeToVolume(float duration, float volume, int sourceIndex = 0)
        {
            Fading = true;
            musicSources[sourceIndex].DOFade(volume, duration).SetEase(Ease.InOutBack);
            yield return new WaitForSeconds(duration);

            Fading = false;
        }

        // TODO: make this ACTUALLY crossfade...
        private IEnumerator CrossFade(string newMusic, float volume, float duration, float newMusicStartTimeNormalized,
            int sourceIndex = 0)
        {
            if (musicSources[0].clip != null && musicSources[0].isPlaying)
            {
                StartCoroutine(DoFadeToVolume(duration * 0.5f, 0f, 1)); // HACK: also fade out 2nd music source here
                yield return DoFadeToVolume(duration * 0.5f, 0f);
            }

            TrySetMusic(newMusic);
            musicSources[0].time = 0f;
            musicSources[0].Play();
            SetMusicTimeNormalized(newMusicStartTimeNormalized);

            yield return DoFadeToVolume(duration * 0.5f, volume);
        }
    }
}