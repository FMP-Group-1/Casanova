using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour
{
    private UIManager m_uiManager;
    [SerializeField]
    private Camera m_cutsceneCam;
    private Camera m_mainCamera;
    [SerializeField]
    [Tooltip("The speed of the camera.")]
    private float m_flySpeed = 0.5f;
    private float m_interpolateAmount = 0f;
    private int m_currentTrackNum = 0;
    [SerializeField]
    private Transform m_camLookTarget;
    private Transform[] m_dollyPoint;
    private bool m_isPlaying = false;

    void Start()
    {
        m_uiManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UIManager>();
        m_cutsceneCam.enabled = false;
        m_dollyPoint = new Transform[transform.GetChild(0).childCount];

        for (int i = 0; i < m_dollyPoint.Length; i++)
        {
            m_dollyPoint[i] = transform.GetChild(0).GetChild(i);
        }

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (m_isPlaying)
        {
            MoveCamera();
        }
    }

    private void MoveCamera()
    {
        m_interpolateAmount += (m_flySpeed * Time.deltaTime);
        m_cutsceneCam.transform.position = TrackPositions(m_interpolateAmount, m_currentTrackNum);
        m_cutsceneCam.transform.LookAt(m_camLookTarget);

        if (m_interpolateAmount >= 1.0f && m_currentTrackNum < m_dollyPoint.Length - 1)
        {
            m_currentTrackNum = (m_currentTrackNum + 1) % m_dollyPoint.Length;
            m_interpolateAmount = 0f;
        }
        if (m_interpolateAmount >= 1.0f && m_currentTrackNum == m_dollyPoint.Length - 1)
        {
            m_uiManager.ReturnFromCutscene(this);
            m_isPlaying = false;
        }
    }

    public void StartCutscene()
    {
        m_mainCamera = Camera.main;
        m_mainCamera.enabled = false;
        m_cutsceneCam.enabled = true;
        m_isPlaying = true;
        m_currentTrackNum = 0;
        m_interpolateAmount = 0.0f;
        m_cutsceneCam.transform.position = TrackPositions(m_interpolateAmount, m_currentTrackNum);

        EventManager.StartWaveSetupEvent();
        EventManager.StartWakeEnemiesEvent(4);
    }

    public void EndCutscene()
    {
        m_cutsceneCam.enabled = false;
        m_mainCamera.enabled = true;
        m_isPlaying = false;
        EventManager.StartAlertEnemiesEvent(4);
        EventManager.StartSpawnWaveEvent();
    }

    private Vector3 TrackPositions( float interpolateValue, int trackNum )
    {
        Vector3 trackPos = new Vector3(0, 0, 0);

        if (m_dollyPoint[trackNum].transform.childCount == 4)
        {
            trackPos = CubicLerp(m_dollyPoint[trackNum].GetChild(0).position, 
                                 m_dollyPoint[trackNum].GetChild(1).position, 
                                 m_dollyPoint[trackNum].GetChild(2).position, 
                                 m_dollyPoint[trackNum].GetChild(3).position, interpolateValue);
        }
        else if (m_dollyPoint[trackNum].transform.childCount == 2)
        {
            trackPos = Vector3.Lerp(m_dollyPoint[trackNum].GetChild(0).position, 
                                    m_dollyPoint[trackNum].GetChild(1).position, interpolateValue);
        }

        return trackPos;
    }

    private Vector3 QuadraticLerp( Vector3 pointA, Vector3 pointB, Vector3 pointC, float interpolateValue )
    {
        Vector3 lerpedVector;
        Vector3 pointAB = Vector3.Lerp(pointA, pointB, interpolateValue);
        Vector3 pointBC = Vector3.Lerp(pointB, pointC, interpolateValue);
        lerpedVector = Vector3.Lerp(pointAB, pointBC, interpolateValue);

        return lerpedVector;
    }

    private Vector3 CubicLerp( Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, float interpolateValue )
    {
        Vector3 lerpedVector;
        Vector3 pointAB_BC = QuadraticLerp(pointA, pointB, pointC, interpolateValue);
        Vector3 pointBC_CD = QuadraticLerp(pointB, pointC, pointD, interpolateValue);
        lerpedVector = Vector3.Lerp(pointAB_BC, pointBC_CD, interpolateValue);

        return lerpedVector;
    }
}
