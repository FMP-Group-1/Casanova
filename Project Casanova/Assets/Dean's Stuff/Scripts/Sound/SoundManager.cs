using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip m_gruntDamage;

    public ref AudioClip GetGruntDamageSFX()
    {
        return ref m_gruntDamage;
    }
}