using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for detecting attack collision from zombie placeholder
// Will likely be removed once a preferable method is implemented
public class EnemyAttackCollision : MonoBehaviour
{
    [SerializeField]
    private EnemyAI parentEnemy;
    private BoxCollider m_boxCollider;

    private void Awake()
    {
        m_boxCollider = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter( Collider other )
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Player Hit");
            m_boxCollider.enabled = false;
        }
    }
}
