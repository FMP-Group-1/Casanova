using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDamageManager : CharacterDamageManager
{
    private EnemyAI m_enemyAI;
    private List<Material> m_materialList = new List<Material>();

    private SpawnManager m_spawnManager;

    protected override void Start()
    {
        base.Start();

        m_spawnManager = GameObject.FindGameObjectWithTag( "GameController" ).GetComponent<SpawnManager>();

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
            m_enemyAI.PlayDamageSFX();
            SetStaggerable(true);

            //Check base stuff after as that is where it checks for death, where as above, overwrites with get hurt
            base.TakeDamage( othersTransform, damage );

        }
    }

    protected override void Die()
    {
        m_enemyAI.SetAIState( AIState.Dead );
        m_enemyAI.UnregisterAttacker();
        gameObject.GetComponent<Collider>().enabled = false;
        StartCoroutine( DissolveEnemy( 3f ) );

        base.Die();
    }

    private IEnumerator DissolveEnemy( float time )
	{
        yield return new WaitForSeconds( time );
        int iteration = 0;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach( Renderer child in renderers )
        {
            m_materialList.Add( renderers[ iteration ].material );
            m_materialList[ iteration ] = renderers[ iteration ].material;
            m_materialList[ iteration ].SetFloat( "_FadeStartTime", float.MaxValue );
            m_materialList[ iteration ].SetInt( "_ForceVisible", 0 );
            iteration++;
        }
        foreach ( Material mat in m_materialList )
        {
            mat.SetFloat( "_FadeStartTime", Time.time );
        }
        StartCoroutine(ResetEnemy());
    }

    private void ResetShader( )
    {
        int iteration = 0;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach( Renderer child in renderers )
        {
            m_materialList.Add( renderers[ iteration ].material );
            m_materialList[ iteration ] = renderers[ iteration ].material;
            m_materialList[ iteration ].SetFloat( "_FadeStartTime", float.MaxValue );
            m_materialList[ iteration ].SetInt( "_ForceVisible", 1 );
            iteration++;
        }
    }

    private IEnumerator ResetEnemy()
	{
        yield return new WaitForSeconds( 1f );

        gameObject.SetActive( false );
        SetHealth( 100 ); 
        ResetShader();
        m_spawnManager.AddToAvailable(gameObject.GetComponent<EnemyAI>());
        SetInvulnerable(false);
        SetAlive(true);
        gameObject.GetComponent<Collider>().enabled = true;

    }
}
