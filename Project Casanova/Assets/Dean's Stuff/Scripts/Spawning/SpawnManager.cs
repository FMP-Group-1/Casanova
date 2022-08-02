using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private AIManager m_aiManager;
    [SerializeField]
    private GameObject m_gruntPrefab;
    [SerializeField]
    private GameObject m_guardPrefab;
    private List<GameObject> m_gruntPool;
    private List<GameObject> m_availableGrunts;
    private List<GameObject> m_guardPool;
    private List<GameObject> m_availableGuards;
    private List<Spawner> m_spawnerList;
    private int m_maxGrunts;
    private int m_maxGuards;
    private GameObject m_initialSpawnPoint;


    void Start()
    {
        // Getting the AI Manager, and creating the lists
        m_aiManager = gameObject.GetComponent<AIManager>();
        m_spawnerList = new List<Spawner>();
        m_gruntPool = new List<GameObject>();
        m_availableGrunts = new List<GameObject>();
        m_guardPool = new List<GameObject>();
        m_availableGuards = new List<GameObject>();
        m_initialSpawnPoint = GameObject.FindGameObjectWithTag("InitialSpawnPoint");

        // Enemy setup
        SetupSpawnerList();
        CalculateEnemiesNeeded();
        SetupEnemies();

        // Subscribing to event
        EventManager.SpawnEnemiesEvent += SpawnGroup;
    }

    private void CalculateEnemiesNeeded()
    {
        int totalGroups = 0;

        // Figuring out how many groups exist
        foreach(Spawner spawner in m_spawnerList)
        {
            if( totalGroups < spawner.GetSpawnGroup())
            {
                totalGroups = spawner.GetSpawnGroup();
            }
        }

        // Looping through each group to find out which one uses the most enemies and how many
        for (int i = 0; i <= totalGroups; i++)
        {
            int totalGruntsNeeded = 0;
            int totalGuardsNeeded = 0;

            foreach(Spawner spawner in m_spawnerList)
            {
                if (spawner.GetSpawnType() == EnemyType.Grunt && spawner.GetSpawnGroup() == i)
                {
                    totalGruntsNeeded++;
                }
                if (spawner.GetSpawnType() == EnemyType.Guard && spawner.GetSpawnGroup() == i)
                {
                    totalGuardsNeeded++;
                }
            }

            // Setting the max enemies num if there is more
            if (totalGruntsNeeded > m_maxGrunts)
            {
                m_maxGrunts = totalGruntsNeeded;
            }
            if (totalGuardsNeeded > m_maxGuards)
            {
                m_maxGuards = totalGuardsNeeded;
            }
        }
    }

    private void SetupSpawnerList()
    {
        // Adding all the spawner objects to a list
        foreach(GameObject spawnerObj in GameObject.FindGameObjectsWithTag("Spawner"))
        {
            m_spawnerList.Add(spawnerObj.GetComponent<Spawner>());
        }
    }

    private void SetupEnemies()
    {
        // Instantiating enemies based on the max amount needed
        for(int i = 0; i < m_maxGrunts; i++)
        {
            GameObject newEnemy = Instantiate(m_gruntPrefab, m_initialSpawnPoint.transform.position, m_initialSpawnPoint.transform.rotation);
            m_gruntPool.Add(newEnemy);
            m_availableGrunts.Add(newEnemy);
        }
        for (int i = 0; i < m_maxGuards; i++)
        {
            GameObject newEnemy = Instantiate(m_guardPrefab, m_initialSpawnPoint.transform.position, m_initialSpawnPoint.transform.rotation);
            m_guardPool.Add(newEnemy);
            m_availableGuards.Add(newEnemy);
        }

        // Make AI manager register the enemies in scene now that they've been instantiated
        m_aiManager.RegisterEnemies();

        // Set each enemy to inactive
        foreach(GameObject enemy in m_gruntPool)
        {
            enemy.SetActive(false);
        }
        foreach (GameObject enemy in m_guardPool)
        {
            enemy.SetActive(false);
        }
    }

    private void SpawnGroup(int groupNum)
    {
        // Spawn enemies in matching group number
        foreach(Spawner spawner in m_spawnerList)
        {
            if (groupNum == spawner.GetSpawnGroup())
            {
                GameObject enemyToSpawn = GetAvailableEnemy(spawner.GetSpawnType());
                spawner.Spawn(enemyToSpawn);
                RemoveFromAvailable(enemyToSpawn.GetComponent<EnemyAI>());
            }
        }
    }

    private GameObject GetAvailableEnemy(EnemyType typeToGet)
    {
        GameObject enemyToReturn = m_gruntPool[0];

        if (typeToGet == EnemyType.Grunt)
        {
            enemyToReturn = m_availableGrunts[0];
        }
        else if (typeToGet == EnemyType.Guard)
        {
            enemyToReturn = m_availableGuards[0];
        }
        else
        {
            Debug.Log("ERROR: No available enemy found in pool.");
        }

        return enemyToReturn;
    }

    public void AddToAvailable( EnemyAI enemyToAdd )
    {
        if (enemyToAdd.GetEnemyType() == EnemyType.Grunt)
        {
            if (!m_availableGrunts.Contains(enemyToAdd.gameObject))
            {
                m_availableGrunts.Add(enemyToAdd.gameObject);
            }
        }
        else if (enemyToAdd.GetEnemyType() == EnemyType.Guard)
        {
            if (!m_availableGuards.Contains(enemyToAdd.gameObject))
            {
                m_availableGuards.Add(enemyToAdd.gameObject);
            }
        }
    }

    public void RemoveFromAvailable(EnemyAI enemyToRemove)
    {
        if (enemyToRemove.GetEnemyType() == EnemyType.Grunt)
        {
            if (m_availableGrunts.Contains(enemyToRemove.gameObject))
            {
                m_availableGrunts.Remove(enemyToRemove.gameObject);
            }
        }
        else if (enemyToRemove.GetEnemyType() == EnemyType.Guard)
        {
            if (m_availableGuards.Contains(enemyToRemove.gameObject))
            {
                m_availableGuards.Remove(enemyToRemove.gameObject);
            }
        }
    }
}
