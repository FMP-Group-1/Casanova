using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Script for visualizing the AI's FOV in the Editor
// Logic from https://www.youtube.com/watch?v=rQG9aUWarwE

[CustomEditor (typeof (EnemyAI))]
public class ConeDetectionVisualizer : Editor
{
    void OnSceneGUI()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        EnemyAI targetEnemy = (EnemyAI)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(targetEnemy.transform.position, Vector3.up, Vector3.forward, 360.0f, targetEnemy.GetViewRadius());
        Vector3 viewAngleA = targetEnemy.DirFromAngle(-targetEnemy.GetViewAngle() * 0.5f, false);
        Vector3 viewAngleB = targetEnemy.DirFromAngle(targetEnemy.GetViewAngle() * 0.5f, false);

        Handles.DrawLine(targetEnemy.transform.position, targetEnemy.transform.position + viewAngleA * targetEnemy.GetViewRadius());
        Handles.DrawLine(targetEnemy.transform.position, targetEnemy.transform.position + viewAngleB * targetEnemy.GetViewRadius());

        Handles.color = Color.red;
        if (EditorApplication.isPlaying && targetEnemy.IsPlayerVisible())
        {
            Handles.DrawLine(targetEnemy.transform.position, player.transform.position);
        }

        Handles.color = Color.blue;
        if (EditorApplication.isPlaying)
        {
            Vector3 dir = targetEnemy.transform.forward;
            Vector3 castFrom = targetEnemy.transform.position;
            castFrom.y += targetEnemy.GetAgentHeight() * 0.5f;

            if (targetEnemy.GetCombatState() == CombatState.StrafingToZone)
            {
                if (targetEnemy.GetStrafeDir() == StrafeDir.Left)
                {
                    dir = -targetEnemy.transform.right;
                }
                else
                {
                    dir = targetEnemy.transform.right;
                }
            }
            Handles.DrawLine(castFrom, castFrom + (dir * targetEnemy.GetAICheckDist()));
            Handles.DrawLine(castFrom, castFrom + (Vector3.Normalize((dir + targetEnemy.DirFromAngle(-targetEnemy.GetAIAngleCheck(), false))) * targetEnemy.GetAICheckDist()));
            Handles.DrawLine(castFrom, castFrom + (Vector3.Normalize((dir + targetEnemy.DirFromAngle(targetEnemy.GetAIAngleCheck(), false))) * targetEnemy.GetAICheckDist()));
        }
    }
}