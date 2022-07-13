using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Grunt,
    Guard
}

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
        m_aiManager = GameObject.Find("AIManager").GetComponent<AIManager>();
        m_spawnerList = new List<Spawner>();
        m_gruntPool = new List<GameObject>();
        m_guardPool = new List<GameObject>();
        SetupSpawnerList();
        CalculateEnemiesNeeded();
        SetupEnemies();
        EventManager.SpawnEnemiesEvent += SpawnGroup;
    }

    void Update()
    {
        
    }

    private void CalculateEnemiesNeeded()
    {
        int totalGroups = 0;

        foreach(Spawner spawner in m_spawnerList)
        {
            if( totalGroups < spawner.GetSpawnGroup())
            {
                totalGroups = spawner.GetSpawnGroup();
            }
        }

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
        foreach(GameObject spawnerObj in GameObject.FindGameObjectsWithTag("Spawner"))
        {
            m_spawnerList.Add(spawnerObj.GetComponent<Spawner>());
        }
    }

    private void SetupEnemies()
    {
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

        m_aiManager.RegisterEnemies();

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
        // Todo: Review this function, needs to be more foolproof
        switch(typeToGet)
        {
            case EnemyType.Grunt:
            {
                foreach(GameObject enemy in m_gruntPool)
                {
                    if (enemy.activeSelf == false)
                    {
                        return enemy;
                    }
                }

                Debug.Log("Inactive enemy not found");
                return m_gruntPool[0];

                break;
            }
            case EnemyType.Guard:
            {
                foreach (GameObject enemy in m_guardPool)
                {
                    if (enemy.activeSelf == false)
                    {
                        return enemy;
                    }
                }

                Debug.Log("Inactive enemy not found");
                return m_guardPool[0];

                break;
            }
            default:
            {
                Debug.Log("Enemy type not found");
                return m_gruntPool[0];

                break;
            }
        }
    }
}
