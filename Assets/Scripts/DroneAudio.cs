using UnityEngine;

public class DroneAudio : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip startClip;
    [SerializeField] AudioClip flyingClip;
    [SerializeField] AudioClip crashClip;

    private bool hasCrashed = false;
    private bool wasPlayingFlying = false;
    private bool isInFlyingState = false; 

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        audioSource.volume = volume;
    }

    private void OnEnable()
    {
        if (isInFlyingState && !hasCrashed)
        {
            float currentVolume = PlayerPrefs.GetFloat("Volume", 1f);
            if (currentVolume > 0f)
            {
                audioSource.clip = flyingClip;
                audioSource.loop = true;
                audioSource.Play();
                wasPlayingFlying = true;
            }
            else
            {
                audioSource.clip = flyingClip;
                audioSource.loop = true;
                wasPlayingFlying = false;
            }
        }
    }

    private void OnDisable()
    {
        if (audioSource.clip == flyingClip)
        {
            isInFlyingState = true;
            wasPlayingFlying = audioSource.isPlaying;
        }
    }

    public void PlayStartThenFlying()
    {
        float currentVolume = PlayerPrefs.GetFloat("Volume", 1f);
        if (currentVolume > 0f)
        {
            audioSource.loop = false;
            audioSource.clip = startClip;
            audioSource.Play();
        }
        hasCrashed = false;
        wasPlayingFlying = false;
        isInFlyingState = false;
        float blendTime = 1f;
        StartCoroutine(PlayFlyingAfterStart(startClip.length - blendTime));
    }

    System.Collections.IEnumerator PlayFlyingAfterStart(float len)
    {
        yield return new WaitForSeconds(len);

        if (!hasCrashed) 
        {
            audioSource.clip = flyingClip;
            audioSource.loop = true;
            isInFlyingState = true;

            if (audioSource != null)
            {
                float currentVolume = PlayerPrefs.GetFloat("Volume", 1f);
                if (currentVolume > 0f && !audioSource.isPlaying && audioSource.clip != null)
                {
                    audioSource.Play();
                    wasPlayingFlying = true;
                }
                else
                {
                    wasPlayingFlying = false;
                }
            }
            else
            {
                Debug.LogWarning("Target Audio Source is not assigned!");
                wasPlayingFlying = false;
            }
        }
    }

    public void PLayCrash()
    {
        if (hasCrashed) return;

        hasCrashed = true;
        wasPlayingFlying = false;
        isInFlyingState = false;
        audioSource.loop = false;
        audioSource.clip = crashClip;
        
        float currentVolume = PlayerPrefs.GetFloat("Volume", 1f);
        if (currentVolume > 0f)
        {
            audioSource.Play();
        }
    }
}
