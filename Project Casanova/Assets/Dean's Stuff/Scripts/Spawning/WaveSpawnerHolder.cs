using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawnerHolder : MonoBehaviour
{
    [SerializeField]
    private GameObject m_leftGroup;
    [SerializeField]
    private GameObject m_middleGroup;
    [SerializeField]
    private GameObject m_rightGroup;

    private int m_groupToReturn = 0;
    private int m_totalGroups = 3;
    private int m_lastReturnedL = 0;
    private int m_lastReturnedM = 0;
    private int m_lastReturnedR = 0;

    public WaveSpawner ReturnSpawnPoint()
    {
        WaveSpawner pointToReturn = null;

        switch (m_groupToReturn)
        {
            case 0:
            {
                pointToReturn = m_leftGroup.transform.GetChild(m_lastReturnedL).gameObject.GetComponent<WaveSpawner>();
                m_lastReturnedL = (m_lastReturnedL + 1) % m_leftGroup.transform.childCount;

                break;
            }
            case 1:
            {
                pointToReturn = m_middleGroup.transform.GetChild(m_lastReturnedM).gameObject.GetComponent<WaveSpawner>();
                m_lastReturnedM = (m_lastReturnedM + 1) % m_middleGroup.transform.childCount;

                break;
            }
            case 2:
            {
                pointToReturn = m_rightGroup.transform.GetChild(m_lastReturnedR).gameObject.GetComponent<WaveSpawner>();
                m_lastReturnedR = (m_lastReturnedR + 1) % m_rightGroup.transform.childCount;

                break;
            }
            default:
            {
                m_leftGroup.transform.GetChild(0);

                break;
            }
        }

        m_groupToReturn = (m_groupToReturn + 1) % m_totalGroups;

        return pointToReturn;
    }
}
