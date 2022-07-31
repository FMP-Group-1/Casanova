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
    private List<GameObject> m_guardPool;
    private List<Spawner> m_spawnerList;
    private int m_maxGrunts;
    private int m_maxGuards;


    void Start()
    {
        // Getting the AI Manager, and creating the lists
        m_aiManager = GameObject.Find("AIManager").GetComponent<AIManager>();
        m_spawnerList = new List<Spawner>();
        m_gruntPool = new List<GameObject>();
        m_guardPool = new List<GameObject>();

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
            GameObject newEnemy = Instantiate(m_gruntPrefab);
            m_gruntPool.Add(newEnemy);
        }
        for (int i = 0; i < m_maxGuards; i++)
        {
            GameObject newEnemy = Instantiate(m_guardPrefab);
            m_guardPool.Add(newEnemy);
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
                GameObject enemyToSpawn = GetInactiveEnemy(spawner.GetSpawnType());
                spawner.Spawn(enemyToSpawn);
            }
        }
    }

    private GameObject GetInactiveEnemy(EnemyType typeToGet)
    {
        // Finding an inactive enemy from the desired type for the purpose of spawning
        // Todo: Review this function, wrong enemy could be used if an inactive enemy isn't found
        // Also, may be cleaner to create a pool object/list
        GameObject enemyToReturn = m_gruntPool[0];

        switch(typeToGet)
        {
            case EnemyType.Grunt:
            {
                foreach(GameObject enemy in m_gruntPool)
                {
                    if (enemy.activeSelf == false)
                    {
                        enemyToReturn = enemy;
                        break;
                    }
                }

                break;
            }
            case EnemyType.Guard:
            {
                foreach (GameObject enemy in m_guardPool)
                {
                    if (enemy.activeSelf == false)
                    {
                        enemyToReturn = enemy;
                        break;
                    }
                }

                break;
            }
            default:
            {
                Debug.Log("ERROR: Enemy type not found");

                break;
            }
        }

        return enemyToReturn;
    }
}
