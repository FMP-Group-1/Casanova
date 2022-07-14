using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType
{
    Spawn,
    Wake,
    Alert
}

public class TriggerBox : MonoBehaviour
{
    [SerializeField]
    private TriggerType m_triggerType = TriggerType.Spawn;
    [SerializeField]
    private int m_triggerGroup;

    private void OnTriggerEnter( Collider other )
    {
        // Using OnTriggerEnter to tell the event manager to send the relevant message to subscribers
        if (other.gameObject.tag == "Player")
        {
            switch (m_triggerType)
            {
                case TriggerType.Spawn:
                {
                    EventManager.StartSpawnEnemiesEvent(m_triggerGroup);
                    break;
                }
                case TriggerType.Wake:
                {
                    EventManager.StartWakeEnemiesEvent(m_triggerGroup);
                    break;
                }
                case TriggerType.Alert:
                {
                    EventManager.StartAlertEnemiesEvent(m_triggerGroup);
                    break;
                }
            }
        }
    }
}
