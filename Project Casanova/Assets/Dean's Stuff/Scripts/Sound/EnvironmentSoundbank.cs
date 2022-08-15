using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSoundbank : MonoBehaviour
{
    [SerializeField]
    private AudioClip m_cellDoorOpenSFX;
    [SerializeField]
    private AudioClip m_gateOpenSFX;
    [SerializeField]
    private AudioClip m_gateCloseSFX;
    [SerializeField]
    private AudioClip m_waterDripSFX;

    public ref AudioClip GetCellDoorOpenSFX()
    {
        return ref m_gateOpenSFX;
    }

    public ref AudioClip GetGateOpenSFX()
    {
        return ref m_gateOpenSFX;
    }

    public ref AudioClip GetGateCloseSFX()
    {
        return ref m_gateCloseSFX;
    }

    public ref AudioClip GetWaterDripSFX()
    {
        return ref m_waterDripSFX;
    }
}
