//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

////*******************************************
//// Author: Dean Pearce
//// Class: ConeDetectionVisualizer
//// Description: Script for visualizing the AI's FOV in the Editor
//// Logic from https://www.youtube.com/watch?v=rQG9aUWarwE
////*******************************************

//[CustomEditor (typeof (EnemyAI))]
//public class ConeDetectionVisualizer : Editor
//{
//    void OnSceneGUI()
//    {
//        // Getting object refs
//        GameObject player = GameObject.FindGameObjectWithTag("Player");
//        EnemyAI targetEnemy = (EnemyAI)target;

//        // Setting line color, drawing the initial arc, then getting the angles for the fov lines with DirFromAngle
//        Handles.color = Color.white;
//        Handles.DrawWireArc(targetEnemy.transform.position, Vector3.up, Vector3.forward, 360.0f, targetEnemy.GetViewRadius());
//        Vector3 viewAngleA = targetEnemy.DirFromAngle(-targetEnemy.GetViewAngle() * 0.5f, false);
//        Vector3 viewAngleB = targetEnemy.DirFromAngle(targetEnemy.GetViewAngle() * 0.5f, false);

//        // Drawing the fov lines
//        Handles.DrawLine(targetEnemy.transform.position, targetEnemy.transform.position + viewAngleA * targetEnemy.GetViewRadius());
//        Handles.DrawLine(targetEnemy.transform.position, targetEnemy.transform.position + viewAngleB * targetEnemy.GetViewRadius());

//        // When the player is detected, draw a line to the player to display that no obstacles are blocking vision
//        Handles.color = Color.red;
//        if (EditorApplication.isPlaying && targetEnemy.IsPlayerVisible())
//        {
//            Handles.DrawLine(targetEnemy.transform.position, player.transform.position);
//        }

//        // Strafe debug lines, drawing in the direction that the AI is checking for other AI
//        Handles.color = Color.blue;
//        if (EditorApplication.isPlaying)
//        {
//            // Getting the direction and position to draw from, setting the y at half the AI's height
//            Vector3 dir = targetEnemy.transform.forward;
//            Vector3 castFrom = targetEnemy.transform.position;
//            castFrom.y += targetEnemy.GetAgentHeight() * 0.5f;

//            // Checking if the enemy is strafing
//            if (targetEnemy.GetCombatState() == CombatState.StrafingToZone)
//            {
//                // Checking what direction the enemy is strafing to determine the direction to draw the lines
//                if (targetEnemy.GetStrafeDir() == StrafeDir.Left)
//                {
//                    dir = -targetEnemy.transform.right;
//                }
//                else
//                {
//                    dir = targetEnemy.transform.right;
//                }
//            }
//            // Drawing the lines
//            Handles.DrawLine(castFrom, castFrom + (dir * targetEnemy.GetAICheckDist()));
//            Handles.DrawLine(castFrom, castFrom + (Vector3.Normalize((dir + targetEnemy.DirFromAngle(-targetEnemy.GetAIAngleCheck(), false))) * targetEnemy.GetAICheckDist()));
//            Handles.DrawLine(castFrom, castFrom + (Vector3.Normalize((dir + targetEnemy.DirFromAngle(targetEnemy.GetAIAngleCheck(), false))) * targetEnemy.GetAICheckDist()));
//        }
//    }
//}