using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public void Spawn( GameObject enemyToSpawn )
    {
        EnemyAI enemyScript = enemyToSpawn.GetComponent<EnemyAI>();

        // Setting enemy position and rotation to match spawner
        enemyToSpawn.transform.position = transform.position;
        enemyToSpawn.transform.rotation = transform.rotation;

        // Reset the necessary values, then set the state
        enemyScript.ResetToSpawn();

        // Enable enemy
        enemyToSpawn.SetActive(true);
        enemyScript.SetAIState(AIState.InCombat);
        enemyScript.SetWaveEnemy(true);
    }
}
