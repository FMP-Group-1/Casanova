using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackCollision : MonoBehaviour
{
    [SerializeField]
    private GameObject m_parent;
    private BoxCollider m_collider;

    private float m_damage;
    private void Awake()
    {
        m_collider = GetComponent<BoxCollider>();
        m_damage = m_parent.GetComponent<Player>().GetDamageNum();
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            EnemyAI enemy = collision.GetComponent<EnemyAI>();

            if (enemy.GetState() == AIState.Sleeping)
            {
                enemy.WakeUpAI(WakeTrigger.Attack);
            }

            enemy.TakeDamage(m_damage);

            m_collider.enabled = false;
        }
    }
}
