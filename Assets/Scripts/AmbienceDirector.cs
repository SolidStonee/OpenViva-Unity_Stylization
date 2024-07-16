using System.Collections;
using System.Collections.Generic;
using OccaSoftware.Altos.Runtime;
using UnityEngine;

namespace Viva
{
    public class AmbienceDirector : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        private List<AudioSource> windowSourcesA = new List<AudioSource>();
        private List<AudioSource> windowSourcesB = new List<AudioSource>();

        [HideInInspector]
        [SerializeField]
        private List<AudioSource> daytimeOnlySources = new List<AudioSource>();
        [HideInInspector]
        [SerializeField]
        private List<AudioSource> nighttimeOnlySources = new List<AudioSource>();
        private bool usingAmbienceSourcesA = true;

        [Header("Ambience")]
        [SerializeField]
        private Ambience defaultAmbience = null;
        [SerializeField]
        private List<AudioSource> globalAmbienceSourcesA = new List<AudioSource>();
        [SerializeField]
        private List<AudioSource> globalAmbienceSourcesB = new List<AudioSource>();

        private Coroutine ambienceChangeCoroutine = null;
        private Ambience currentAmbience = null;
        private int currentAmbienceEnterCount = 0;

        private Coroutine windowChangeCoroutine = null;
        private Coroutine randomSoundsCoroutine = null;
        private bool usingWindowSourcesA = true;

        private Dictionary<Ambience, int> activeAmbienceCounts = new Dictionary<Ambience, int>();


        public void InitializeAmbience()
        {
            foreach (var outdoorSource in windowSourcesA)
            {
                windowSourcesB.Add(CopyAudioSource(outdoorSource, outdoorSource.gameObject));
            }
            foreach (var globalSource in globalAmbienceSourcesA)
            {
                globalAmbienceSourcesB.Add(CopyAudioSource(globalSource, globalSource.gameObject));
            }
            if (currentAmbience == null)
            {
                EnterDefaultAmbience();
            }
        }

        private AudioSource CopyAudioSource(AudioSource source, GameObject parent)
        {
            AudioSource copy = parent.AddComponent<AudioSource>();
            copy.priority = source.priority;
            copy.pitch = source.pitch;
            copy.panStereo = source.panStereo;
            copy.spatialBlend = source.spatialBlend;
            copy.reverbZoneMix = source.reverbZoneMix;
            copy.spread = source.spread;
            copy.dopplerLevel = source.dopplerLevel;
            copy.volume = 0.0f;
            copy.rolloffMode = source.rolloffMode;
            copy.minDistance = source.minDistance;
            copy.maxDistance = source.maxDistance;
            copy.SetCustomCurve(AudioSourceCurveType.CustomRolloff, source.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
            return copy;
        }

        public void EnterAmbience(Ambience ambience)
        {
            if (!activeAmbienceCounts.ContainsKey(ambience))
            {
                activeAmbienceCounts[ambience] = 0;
            }

            activeAmbienceCounts[ambience]++;
            UpdateCurrentAmbience();
        }

        public void ExitAmbience(Ambience ambience)
        {
            if (activeAmbienceCounts.ContainsKey(ambience))
            {
                activeAmbienceCounts[ambience]--;
                if (activeAmbienceCounts[ambience] <= 0)
                {
                    activeAmbienceCounts.Remove(ambience);
                }
                UpdateCurrentAmbience();
            }
        }

        private void UpdateCurrentAmbience()
        {
            if (activeAmbienceCounts.Count == 0)
            {
                EnterDefaultAmbience();
                return;
            }

            Ambience highestPriorityAmbience = null;
            foreach (var entry in activeAmbienceCounts)
            {
                if (highestPriorityAmbience == null || entry.Key.priority > highestPriorityAmbience.priority)
                {
                    highestPriorityAmbience = entry.Key;
                }
            }

            EnterAmbienceByPriority(highestPriorityAmbience);
        }
        
        private void EnterDefaultAmbience()
        {
            EnterAmbienceByPriority(defaultAmbience);
        }

        public void EnterAmbienceByPriority(Ambience ambience)
        {
            if (ambience == null)
            {
                ambience = defaultAmbience;
            }
            else
            {
                currentAmbienceEnterCount++;
            }
            Debug.Log("[Ambience] " + ambience.name);

            currentAmbience = ambience;
            FadeAmbience();

            if (randomSoundsCoroutine != null)
            {
                GameDirector.instance.StopCoroutine(randomSoundsCoroutine);
                randomSoundsCoroutine = null;
            }
            if (currentAmbience.randomSounds != null)
            {
                randomSoundsCoroutine = GameDirector.instance.StartCoroutine(RandomSoundPlayer());
            }
        }

        private IEnumerator RandomSoundPlayer()
        {
            while (true)
            {
                if (globalAmbienceSourcesA.Count > 0 && globalAmbienceSourcesB.Count > 0)
                {
                    if (usingAmbienceSourcesA)
                    {
                        globalAmbienceSourcesA[0].PlayOneShot(currentAmbience.randomSounds.GetRandomAudioClip());
                    }
                    else
                    {
                        globalAmbienceSourcesB[0].PlayOneShot(currentAmbience.randomSounds.GetRandomAudioClip());
                    }
                }
                yield return new WaitForSeconds(5.0f + Random.value * 10.0f);
            }
        }

        public void FadeAmbience()
        {
            FadeGlobalAmbience();
            FadeLocaleAmbience();
        }

        private void FadeGlobalAmbience()
        {
            if (ambienceChangeCoroutine != null)
            {
                GameDirector.instance.StopCoroutine(ambienceChangeCoroutine);
            }
            if (currentAmbience == null)
            {
                SetAudioSourcesVolume(globalAmbienceSourcesA, 0.0f);
                SetAudioSourcesVolume(globalAmbienceSourcesB, 0.0f);
                ambienceChangeCoroutine = null;
                return;
            }

            List<AudioSource> fadeInSources;
            List<AudioSource> fadeOutSources;
            if (usingAmbienceSourcesA)
            {
                fadeInSources = globalAmbienceSourcesB;
                fadeOutSources = globalAmbienceSourcesA;
                SetAndPlayLoopAudioSources(globalAmbienceSourcesB, currentAmbience.GetAudio(GameDirector.newSkyDirector.skyDefinition.currentSegment, GameDirector.instance.userIsIndoors));
            }
            else
            {
                fadeInSources = globalAmbienceSourcesA;
                fadeOutSources = globalAmbienceSourcesB;
                SetAndPlayLoopAudioSources(globalAmbienceSourcesA, currentAmbience.GetAudio(GameDirector.newSkyDirector.skyDefinition.currentSegment, GameDirector.instance.userIsIndoors));
            }
            usingAmbienceSourcesA = !usingAmbienceSourcesA;

            ambienceChangeCoroutine = GameDirector.instance.StartCoroutine(CrossFadeSound(fadeInSources, fadeOutSources, false, 2.0f));
        }

        private void SetDaySegmentSounds(DaySegment daySegment, float fadeVal)
        {
            if (daySegment == DaySegment.NIGHT)
            {
                SetAudioSourcesVolume(daytimeOnlySources, 1.0f - fadeVal);
                SetAudioSourcesVolume(nighttimeOnlySources, fadeVal);
            }
            else if (daySegment == DaySegment.MORNING)
            {
                SetAudioSourcesVolume(daytimeOnlySources, fadeVal);
                SetAudioSourcesVolume(nighttimeOnlySources, 1.0f - fadeVal);
            }
            else
            {
                SetAudioSourcesVolume(daytimeOnlySources, 1.0f);
                SetAudioSourcesVolume(nighttimeOnlySources, 0.0f);
            }
        }

        private void FadeLocaleAmbience()
        {
            if (currentAmbience == null)
            {
                return;
            }
            List<AudioSource> fadeInSources;
            List<AudioSource> fadeOutSources;
            if (usingWindowSourcesA)
            {
                fadeInSources = windowSourcesB;
                fadeOutSources = windowSourcesA;
            }
            else
            {
                fadeInSources = windowSourcesA;
                fadeOutSources = windowSourcesB;
            }
            usingWindowSourcesA = !usingWindowSourcesA;

            if (GameDirector.instance.userIsIndoors)
            {
                SetAndPlayLoopAudioSources(fadeInSources, currentAmbience.GetAudio(GameDirector.newSkyDirector.skyDefinition.currentSegment, false));
            }
            else
            {
                SetAndPlayLoopAudioSources(fadeInSources, null);
            }
            if (windowChangeCoroutine != null)
            {
                GameDirector.instance.StopCoroutine(windowChangeCoroutine);
            }

            windowChangeCoroutine = GameDirector.instance.StartCoroutine(CrossFadeSound(fadeInSources, fadeOutSources, true, 2.0f));
        }

        private float? GetFirstSourceVolume(List<AudioSource> sources)
        {
            if (sources == null)
            {
                return null;
            }
            foreach (AudioSource source in sources)
            {
                if (source)
                {
                    return source.volume;
                }
            }
            return null;
        }

        private IEnumerator CrossFadeSound(List<AudioSource> fadeInSources, List<AudioSource> fadeOutSources, bool windowSources, float fadeDuration)
        {
            float? fadeInStartSound = GetFirstSourceVolume(fadeInSources);
            float? fadeOutStartSound = GetFirstSourceVolume(fadeOutSources);

            if (fadeInStartSound.HasValue && fadeOutStartSound.HasValue)
            {
                float interval = 0.2f;
                while (fadeInStartSound < 1.0f)
                {
                    fadeInStartSound = Mathf.Min(1.0f, fadeInStartSound.Value + interval / fadeDuration);

                    SetAudioSourcesVolume(fadeInSources, fadeInStartSound.Value);
                    SetAudioSourcesVolume(fadeOutSources, 1.0f - fadeInStartSound.Value);
                    SetDaySegmentSounds(GameDirector.newSkyDirector.skyDefinition.currentSegment, fadeInStartSound.Value);
                    yield return new WaitForSeconds(interval);
                }
            }

            if (windowSources)
            {
                windowChangeCoroutine = null;
            }
            else
            {
                ambienceChangeCoroutine = null;
            }
        }

        private void SetAndPlayLoopAudioSources(List<AudioSource> sources, AudioClip clip)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                AudioSource source = sources[i];
                source.clip = clip;
                source.loop = true;
                source.Play();
            }
        }

        private void SetAudioSourcesVolume(List<AudioSource> sources, float volume)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                sources[i].volume = volume;
            }
        }
    }
}
