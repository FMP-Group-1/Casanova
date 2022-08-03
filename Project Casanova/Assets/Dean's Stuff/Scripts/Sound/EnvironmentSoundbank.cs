using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSoundbank : MonoBehaviour
{
    [SerializeField]
    private AudioClip m_gateOpenSFX;
    [SerializeField]
    private AudioClip m_waterDripSFX;

    public ref AudioClip GetGateOpenSFX()
    {
        return ref m_gateOpenSFX;
    }
}
