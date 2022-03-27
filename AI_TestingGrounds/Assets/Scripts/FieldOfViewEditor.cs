using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Script for visualizing the AI's FOV in the Editor
// Logic from https://www.youtube.com/watch?v=rQG9aUWarwE

[CustomEditor (typeof (EnemyAI))]
public class FieldOfViewEditor : Editor
{
    void OnSceneGUI()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        EnemyAI fov = (EnemyAI)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360.0f, fov.GetViewRadius());
        Vector3 viewAngleA = fov.DirFromAngle(-fov.GetViewAngle() * 0.5f, false);
        Vector3 viewAngleB = fov.DirFromAngle(fov.GetViewAngle() * 0.5f, false);

        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.GetViewRadius());
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.GetViewRadius());

        Handles.color = Color.red;
        if (EditorApplication.isPlaying && fov.IsPlayerVisible())
        {
            Handles.DrawLine(fov.transform.position, player.transform.position);
        }
    }
}
