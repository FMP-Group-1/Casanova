using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum HandType
{
    Left,
    Right
}

public class EnemyAttackCollision : MonoBehaviour
{
    [SerializeField]
    private EnemyAI parentEnemy;
    private BoxCollider m_boxCollider;

    [SerializeField]
    private HandType hand;

    private void Awake()
    {
        m_boxCollider = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter( Collider other )
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Player Hit with " + hand + " hand");
            m_boxCollider.enabled = false;
        }
    }
}
