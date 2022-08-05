using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class RevertPrefab : EditorWindow
{
    [MenuItem( "Custom/Revert Prefab" )]
    private static void Init()
    {
        RevertPrefab window = ( RevertPrefab )EditorWindow.GetWindow( typeof( RevertPrefab ) );

    }

    private void OnGUI()
    {
        if ( GUILayout.Button( "Revert Prefabs" ) )
        {
            RevertPrefabs();
        }
    }

    private static void RevertPrefabs()
    {
        GameObject[] prefabs = GameObject.FindGameObjectsWithTag( "Revert" );
        foreach ( GameObject prefab in prefabs )
        {
            prefab.tag = null;
            PrefabUtility.RevertPrefabInstance( prefab, InteractionMode.AutomatedAction );
        }
    }

}