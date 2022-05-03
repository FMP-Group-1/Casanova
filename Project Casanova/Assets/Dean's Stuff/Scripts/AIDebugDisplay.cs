using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum DebugType
{
    AI,
    Zone,
    None
}

// Todo: Script needs to either be renamed, or split into 2 to separate AI debug from Zone debug
public class AIDebugDisplay : MonoBehaviour
{
    private DebugType m_debugType = DebugType.None;
    private GameObject m_aiDebugHolder;
    private GameObject m_zoneDebugHolder;

    private List<GameObject> m_aiList = new List<GameObject>();
    private EnemyAI m_targetAI;

    private int m_currentAiNum = 0;

    private List<AttackZone> m_passiveAttackZones = new List<AttackZone>();
    private List<AttackZone> m_activeAttackZones = new List<AttackZone>();
    private AttackZone m_targetZone;
    private AttackingType m_targetZoneType = AttackingType.Passive;
    private AttackZoneManager m_attackZoneManager;

    private int m_currentAttackZoneNum = 0;

    private Text m_aiNameText;
    private Text m_aiStateText;
    private Text m_aiSubstateText;
    private Text m_aiHealth;
    private Text m_playerDetectedText;
    private Text m_strafeAtDistText;
    private Text m_attackZoneText;
    private Text m_occupiedZoneText;

    private Text m_zoneNameText;
    private Text m_zoneOccupantText;
    private Text m_zoneObstructedText;

    //Input Shenanigans
    [SerializeField]
    private InputActionReference m_f12Pressed;
    [SerializeField]
    private InputActionReference m_arrowKeys;

    private void OnEnable()
    {
        m_f12Pressed.action.Enable();
        m_arrowKeys.action.Enable();
    }
    void Start()
    {
        m_aiDebugHolder = GameObject.Find("AIDebugHolder");
        m_zoneDebugHolder = GameObject.Find("ZoneDebugHolder");
        SetupAIDebugDisplay();
        SetupZoneDebugDisplay();

        SetDebugType(DebugType.None);
    }

    void Update()
    {
        ToggleDebugType();

        switch (m_debugType)
        {
            case DebugType.AI:
            {
                ChangeAITarget();
                AIDebugTextUpdate();

                break;
            }
            case DebugType.Zone:
            {
                ChangeZoneTarget();
                ZoneDebugUpdate();
                break;
            }
        }
    }

    private void SetupAIDebugDisplay()
    {
        m_aiNameText = GameObject.Find("AINameText").GetComponent<Text>();
        m_aiStateText = GameObject.Find("AIStateText").GetComponent<Text>();
        m_aiSubstateText = GameObject.Find("AISubstateText").GetComponent<Text>();
        m_aiHealth = GameObject.Find("AIHealthText").GetComponent<Text>();
        m_playerDetectedText = GameObject.Find("PlayerDetectText").GetComponent<Text>();
        m_strafeAtDistText = GameObject.Find("StrafeDistText").GetComponent<Text>();
        m_attackZoneText = GameObject.Find("AttackZoneText").GetComponent<Text>();
        m_occupiedZoneText = GameObject.Find("OccupiedZoneText").GetComponent<Text>();

        m_aiList.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));

        m_targetAI = m_aiList[m_currentAiNum].GetComponent<EnemyAI>();

        SetAIDebugTarget(m_currentAiNum);
    }

    private void SetupZoneDebugDisplay()
    {
        m_zoneNameText = GameObject.Find("ZoneNameText").GetComponent<Text>();
        m_zoneOccupantText = GameObject.Find("OccupiedByText").GetComponent<Text>();
        m_zoneObstructedText = GameObject.Find("ObstructedText").GetComponent<Text>();

        m_attackZoneManager = GameObject.Find("AIManager").GetComponent<AIManager>().GetAttackZoneManager();
        m_passiveAttackZones.AddRange(m_attackZoneManager.GetPassiveAttackZones());
        m_activeAttackZones.AddRange(m_attackZoneManager.GetActiveAttackZones());

        SetZoneTarget(m_currentAttackZoneNum);
    }

    private void AIDebugTextUpdate()
    {
        AIState currentAIState = m_targetAI.GetState();

        m_aiStateText.text = "AI State: " + currentAIState;
        m_aiHealth.text = "AI Health: " + m_targetAI.GetHealth();
        m_playerDetectedText.text = "Player Detected: " + m_targetAI.IsPlayerVisible();
        m_strafeAtDistText.text = "Strafe Distance: " + m_targetAI.GetStrafeDist();

        if (m_targetAI.GetAttackZone() != null)
        {
            AttackZone attackZone = m_targetAI.GetAttackZone();
            m_attackZoneText.text = "Attack Zone: " + attackZone.GetZoneType() + " " + attackZone.GetZoneNum();
        }
        else
        {
            m_attackZoneText.text = "Attack Zone: None";
        }

        if (m_targetAI.GetOccupiedAttackZone() != null)
        {
            AttackZone attackZone = m_targetAI.GetOccupiedAttackZone();
            m_occupiedZoneText.text = "Occupied Zone: " + attackZone.GetZoneType() + " " + attackZone.GetZoneNum();
        }
        else
        {
            m_occupiedZoneText.text = "Occupied Zone: None";
        }

        if (currentAIState == AIState.InCombat || currentAIState == AIState.Patrolling)
        {
            if (currentAIState == AIState.InCombat)
            {
                m_aiSubstateText.text = "Combat State: " + m_targetAI.GetCombatState();
            }
            else if (currentAIState == AIState.Patrolling)
            {
                m_aiSubstateText.text = "Patrol State: " + m_targetAI.GetPatrolState();
            }
        }
        else
        {
            m_aiSubstateText.text = "Substate: None Active";
        }
    }

    private void ZoneDebugUpdate()
    {
        m_zoneNameText.text = "Zone: " + m_targetZoneType + " " + m_targetZone.GetZoneNum();
        m_zoneObstructedText.text = "Is Obstructed: " + m_targetZone.IsObstructed();
        if (m_targetZone.IsOccupied())
        {
            m_zoneOccupantText.text = "Occupied By: " + m_targetZone.GetOccupant().gameObject.name;
        }
        else
        {
            m_zoneOccupantText.text = "No Occupant";
        }
    }

    private void ChangeZoneTarget()
    {
        // Inputs for cycling debug target, could use a refactor
        if (m_arrowKeys.action.ReadValue<Vector2>().x < 0)
        {
            if (m_currentAttackZoneNum != 0)
            {
                m_currentAttackZoneNum--;
                SetZoneTarget(m_currentAttackZoneNum);
            }
        }
        if (m_arrowKeys.action.ReadValue<Vector2>().x > 0)
        {
            if (m_currentAttackZoneNum != m_passiveAttackZones.Count - 1)
            {
                m_currentAttackZoneNum++;
                SetZoneTarget(m_currentAttackZoneNum);
            }
        }
        if (m_arrowKeys.action.ReadValue<Vector2>().y > 0 || m_arrowKeys.action.ReadValue<Vector2>().y < 0)
        {
            if (m_targetZoneType == AttackingType.Passive)
            {
                m_targetZoneType = AttackingType.Active;
                SetZoneTarget(m_currentAttackZoneNum);
            }
            else
            {
                m_targetZoneType = AttackingType.Passive;
                SetZoneTarget(m_currentAttackZoneNum);
            }
        }
    }

    private void SetZoneTarget(int targetNum)
    {
        if (m_targetZoneType == AttackingType.Passive)
        {
            m_targetZone = m_passiveAttackZones[targetNum];
        }
        else
        {
            m_targetZone = m_activeAttackZones[targetNum];
        }

        m_zoneNameText.text = "Zone: " + m_targetZoneType + " " + m_targetZone.GetZoneNum();
    }

    private void ChangeAITarget()
    {
        // Inputs for cycling debug target, could use a refactor
        if (m_arrowKeys.action.ReadValue<Vector2>().x < 0)
        {
            if (m_currentAiNum != 0)
            {
                m_currentAiNum--;
                SetAIDebugTarget(m_currentAiNum);
            }
        }
        if (m_arrowKeys.action.ReadValue<Vector2>().x > 0)
            {
            if (m_currentAiNum != m_aiList.Count - 1)
            {
                m_currentAiNum++;
                SetAIDebugTarget(m_currentAiNum);
            }
        }
    }

    private void SetAIDebugTarget(int targetNum)
    {
        m_targetAI = m_aiList[targetNum].GetComponent<EnemyAI>();
        m_aiNameText.text = "AI: " + m_aiList[targetNum].name;
    }

    private void SetDebugType(DebugType typeToSet)
    {
        switch (typeToSet)
        {
            case DebugType.AI:
            {
                m_zoneDebugHolder.SetActive(false);
                m_aiDebugHolder.SetActive(true);
                m_debugType = DebugType.AI;
                break;
            }
            case DebugType.Zone:
            {
                m_aiDebugHolder.SetActive(false);
                m_zoneDebugHolder.SetActive(true);
                m_debugType = DebugType.Zone;
                break;
            }
            case DebugType.None:
            {
                m_aiDebugHolder.SetActive(false);
                m_zoneDebugHolder.SetActive(false);
                m_debugType = DebugType.None;
                break;
            }
        }
    }

    private void ToggleDebugType()
    {
        if (m_f12Pressed.action.triggered)
        {
            switch (m_debugType)
            {
                case DebugType.AI:
                {
                    SetDebugType(DebugType.Zone);
                    break;
                }
                case DebugType.Zone:
                {
                    SetDebugType(DebugType.None);
                    break;
                }
                case DebugType.None:
                {
                    SetDebugType(DebugType.AI);
                    break;
                }
            }
        }
    }
}
