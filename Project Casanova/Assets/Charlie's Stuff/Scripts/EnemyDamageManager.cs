using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDamageManager : CharacterDamageManager
{
    private EnemyAI m_enemyAI;
    private List<Material> m_materialList = new List<Material>();


    protected override void Start()
    {
        base.Start();
        m_enemyAI = GetComponent<EnemyAI>();

        int iteration = 0;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach ( Renderer child in renderers )
        {
            m_materialList.Add( renderers[ iteration ].material );
            m_materialList[ iteration ] = renderers[ iteration ].material;
            m_materialList[ iteration ].SetFloat( "_FadeStartTime", float.MaxValue );
            m_materialList[ iteration ].SetInt( "_ForceVisible", 0 );
            iteration++;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public override void TakeDamage(Transform othersTransform, float damage = 30f )
    {
        if ( !GetInvulnerable() )
        {
            m_enemyAI.ResetLastUsedAnimTrigger();
            m_enemyAI.StopNavMesh();
            m_enemyAI.DisableCollision();
            m_enemyAI.SetLastUsedAnimTrigger( an_getHitTrigger );

            //Check base stuff after as that is where it checks for death, where as above, overwrites with get hurt
            base.TakeDamage( othersTransform, damage );

        }
    }

    protected override void Die()
    {
        m_enemyAI.SetAIState( AIState.Dead );
        m_enemyAI.UnregisterAttacker();
        StartCoroutine( DissolveEnemy( 2f ) );

        base.Die();
    }

    private IEnumerator DissolveEnemy( float time )
	{
        yield return new WaitForSeconds( time );

        foreach ( Material mat in m_materialList )
        {
            mat.SetFloat( "_FadeStartTime", Time.time );
        }
    }
}
