using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Script for visualizing the available positions for AI attackers in the Editor
// Logic from https://www.youtube.com/watch?v=rQG9aUWarwE

[CustomEditor(typeof(AIManager))]
public class AttackZonesVisualizer : Editor
{
    void OnSceneGUI()
    {
        // Setting object refs
        AIManager aiManager = GameObject.Find("AIManager").GetComponent<AIManager>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (EditorApplication.isPlaying)
        {
            DrawSolidZones(aiManager, player);
        }

        // Drawing the initial zone arcs
        Handles.color = Color.red;
        Handles.DrawWireArc(player.transform.position, Vector3.up, Vector3.forward, 360.0f, aiManager.GetActiveAttackerMinDist());
        Handles.DrawWireArc(player.transform.position, Vector3.up, Vector3.forward, 360.0f, aiManager.GetActiveAttackerMaxDist());

        // Finding the section half angle to use as an offset
        float sectionHalf = (360.0f / aiManager.GetAttackZonesNum()) * 0.5f;

        // For loop to figure out how many zones/zone lines need to be drawn
        for (int i = 0; i < aiManager.GetAttackZonesNum(); i++)
        {
            Vector3 lineAngle = DirFromAngle(((360.0f / aiManager.GetAttackZonesNum()) * i) - sectionHalf, true, player);
            Handles.DrawLine(player.transform.position + lineAngle * aiManager.GetActiveAttackerMinDist(), player.transform.position + lineAngle * aiManager.GetPassiveAttackerMaxDist());
        }

        // Changing color to blue for passive zones
        Handles.color = Color.blue;
        Handles.DrawWireArc(player.transform.position, Vector3.up, Vector3.forward, 360.0f, aiManager.GetPassiveAttackerMaxDist());

        for (int i = 0; i < aiManager.GetAttackZonesNum(); i++)
        {
            Vector3 lineAngle = DirFromAngle(((360.0f / aiManager.GetAttackZonesNum()) * i) - sectionHalf, true, player);
            Handles.DrawLine(player.transform.position + lineAngle * aiManager.GetActiveAttackerMaxDist(), player.transform.position + lineAngle * aiManager.GetPassiveAttackerMaxDist());
        }
    }

    private void DrawSolidZones(AIManager aiManager, GameObject player)
    {
        Color transRed = new Color(255, 0, 0, 0.1f);
        Color transGreen = new Color(0, 255, 0, 0.1f);

        Handles.color = transGreen;

        float angle = aiManager.GetAttackZoneManager().GetAnglePerSection();

        for (int i = 0; i < aiManager.GetAttackZonesNum(); i++)
        {
            AttackZone passiveZone = aiManager.GetAttackZoneManager().GetAttackZoneByNum(i, ZoneType.Passive);
            AttackZone activeZone = aiManager.GetAttackZoneManager().GetAttackZoneByNum(i, ZoneType.Active);

            if (passiveZone.IsObstructed())
            {
                Handles.color = transRed;
            }
            else
            {
                Handles.color = transGreen;
            }

            Handles.DrawSolidArc(player.transform.position, Vector3.up, DirFromAngle(passiveZone.GetAngleStart(), true, player), angle,  passiveZone.GetEndDist());

            if (activeZone.IsObstructed())
            {
                Handles.color = transRed;
            }
            else
            {
                Handles.color = transGreen;
            }

            Handles.DrawSolidArc(player.transform.position, Vector3.up, DirFromAngle(activeZone.GetAngleStart(), true, player), angle, activeZone.GetEndDist());

        }
    }

    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal, GameObject gameObject )
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += gameObject.transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}