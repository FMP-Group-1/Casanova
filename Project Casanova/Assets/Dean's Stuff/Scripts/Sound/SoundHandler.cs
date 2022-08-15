using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    protected AudioSource m_audioSource;
    protected virtual void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        
    }
}
