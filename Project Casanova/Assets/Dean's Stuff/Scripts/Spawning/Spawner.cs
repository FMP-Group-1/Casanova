using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private EnemyType m_typeToSpawn;
    [SerializeField]
    private AIState m_stateToSpawn;
    [SerializeField]
    private int m_spawnGroup;


    public void Spawn(GameObject enemyToSpawn)
    {
        EnemyAI enemyScript = enemyToSpawn.GetComponent<EnemyAI>();

        enemyToSpawn.transform.position = transform.position;
        enemyToSpawn.SetActive(true);
        enemyScript.ResetToSpawn();
        enemyScript.SetAIState(m_stateToSpawn);
    }

    public EnemyType GetSpawnType()
    {
        return m_typeToSpawn;
    }

    public int GetSpawnGroup()
    {
        return m_spawnGroup;
    }
}
