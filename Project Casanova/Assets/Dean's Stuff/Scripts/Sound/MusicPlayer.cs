using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private AIManager m_aiManager;
    [SerializeField]
    private AudioClip m_bgMusic;
    [SerializeField]
    private AudioClip m_combatMusic;
    private AudioSource m_audioSource;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_bgMusicVolume = 1.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_combatMusicVolume = 1.0f;

    private bool m_prevInCombat = false;

    void Start()
    {
        m_aiManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<AIManager>();
        m_audioSource = GetComponent<AudioSource>();

        m_audioSource.clip = m_bgMusic;
        m_audioSource.volume = m_bgMusicVolume;
        m_audioSource.Play();
    }

    void Update()
    {
        MusicUpdate();
    }

    private void MusicUpdate()
    {
        bool currentlyInCombat = m_aiManager.IsCombatActive();

        if (m_prevInCombat != currentlyInCombat)
        {
            if (currentlyInCombat == true)
            {
                // Entered combat
                m_audioSource.clip = m_combatMusic;
                m_audioSource.volume = m_combatMusicVolume;
                m_audioSource.Play();
            }
            else
            {
                // Exited combat
                m_audioSource.clip = m_bgMusic;
                m_audioSource.volume = m_bgMusicVolume;
                m_audioSource.Play();
            }
        }

        m_prevInCombat = currentlyInCombat;
    }
}
