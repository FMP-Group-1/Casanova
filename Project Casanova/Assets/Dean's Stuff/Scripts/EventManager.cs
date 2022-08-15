using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    // Setting up events
    public static event Action<int> WakeEnemiesEvent;
    public static event Action<int> SpawnEnemiesEvent;
    public static event Action<int> AlertEnemiesEvent;

    // Event functions
    public static void StartWakeEnemiesEvent(int triggerGroup)
    {
        WakeEnemiesEvent?.Invoke(triggerGroup);
    }

    public static void StartSpawnEnemiesEvent(int triggerGroup)
    {
        SpawnEnemiesEvent?.Invoke(triggerGroup);
    }

    public static void StartAlertEnemiesEvent( int triggerGroup )
    {
        AlertEnemiesEvent?.Invoke(triggerGroup);
    }
}
