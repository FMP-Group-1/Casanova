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
        AIManager aiManager = GameObject.Find("AIManager").GetComponent<AIManager>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Handles.color = Color.red;
        Handles.DrawWireArc(player.transform.position, Vector3.up, Vector3.forward, 360.0f, aiManager.GetActiveAttackerMinDist());
        Handles.DrawWireArc(player.transform.position, Vector3.up, Vector3.forward, 360.0f, aiManager.GetActiveAttackerMaxDist());

        float sectionHalf = (360.0f / aiManager.GetAttackZonesNum()) * 0.5f;

        for (int i = 0; i < aiManager.GetAttackZonesNum(); i++)
        {
            Vector3 lineAngle = DirFromAngle(((360.0f / aiManager.GetAttackZonesNum()) * i) - sectionHalf, true, player);
            Handles.DrawLine(player.transform.position + lineAngle * aiManager.GetActiveAttackerMinDist(), player.transform.position + lineAngle * aiManager.GetPassiveAttackerMaxDist());
        }

        Handles.color = Color.blue;
        Handles.DrawWireArc(player.transform.position, Vector3.up, Vector3.forward, 360.0f, aiManager.GetPassiveAttackerMaxDist());

        for (int i = 0; i < aiManager.GetAttackZonesNum(); i++)
        {
            Vector3 lineAngle = DirFromAngle(((360.0f / aiManager.GetAttackZonesNum()) * i) - sectionHalf, true, player);
            Handles.DrawLine(player.transform.position + lineAngle * aiManager.GetActiveAttackerMaxDist(), player.transform.position + lineAngle * aiManager.GetPassiveAttackerMaxDist());
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