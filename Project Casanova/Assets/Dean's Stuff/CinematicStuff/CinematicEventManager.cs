using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CinematicEventManager : MonoBehaviour
{
    public static event Action<int> CameraTrackEvent;
    public static event Action CameraTrackEndEvent;

    public static void StartCameraTrackEvent( int nextTrack )
    {
        CameraTrackEvent?.Invoke(nextTrack);
    }

    public static void StartCameraTrackEndEvent()
    {
        CameraTrackEndEvent?.Invoke();
    }
}