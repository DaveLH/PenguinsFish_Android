using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Help set the mood by randomly playing seagull sounds in the background...
/// </summary>
/// 
public class RndGullSounds : MonoBehaviour
{
    [SerializeField] float _minimumTime;  // Set a minimum interval between gull sounds (in sec.)
    [SerializeField] float _maximumTime;  // Set a maximum interval between gull sounds (in sec.)

    AudioSource[] m_gullSounds;

    int m_indexSoundToPlay;   // Sound to play next

    float m_nextSoundTime;    // Time to play next sound

    /// <summary>
    /// Set when to play next gull sound, and which one
    /// </summary>
    /// 
    void SetNextGullSound()
    {
        m_indexSoundToPlay = Random.Range(-1, m_gullSounds.Length);

        m_nextSoundTime = Time.time + Random.Range(_minimumTime, _maximumTime);
    }
	
    /// <summary>
    /// Play gull sound
    /// </summary>
    /// 
    void PlayGullSound()
    {
        m_gullSounds[m_indexSoundToPlay].Play();  // Play sound

        m_indexSoundToPlay = -1;  // Reset
    }

	void Start ()
    {
        m_gullSounds = GetComponentsInChildren<AudioSource>();	
	}


    void Update()
    {
        if (m_indexSoundToPlay < 0)   // Set sound
        {
            SetNextGullSound();
        }
        else if (Time.time > m_nextSoundTime)   // Time to play sound
        {
            PlayGullSound();
        }
    }
}
