#if (UNITY_EDITOR) 
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
        
        if ( prefabs.Length == 0 )
		{
            Debug.Log( "Nothing Tagged with \"Revert\"" );
		}
        
        foreach ( GameObject go in prefabs )
        {
            //Debug.Log( go.name );
            
            if (PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab)
            {
                PrefabUtility.RevertPrefabInstance( go, InteractionMode.AutomatedAction );
                Debug.Log( go.name + " Reverted" );
            }
            else
			{

                Debug.Log( go.name + " is not a prefab" );
                go.tag = "Untagged";
            }
        }
    }

}
#endif