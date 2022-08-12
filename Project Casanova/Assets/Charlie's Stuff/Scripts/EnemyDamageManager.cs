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


            m_enemyAI.DisableCollision();
            //Check base stuff after as that is where it checks for death, where as above, overwrites with get hurt
            base.TakeDamage( othersTransform, damage );

            if ( GetAlive() )
			{
                if ( GetStaggerable() )
                {
                    m_enemyAI.ResetLastUsedAnimTrigger();
                    SetStaggerable( false );
                    ResetStagerable( 6f );
                    m_enemyAI.SetLastUsedAnimTrigger( an_getHitTrigger );
                }
                m_enemyAI.StopNavMesh();
            }
        }
    }


    IEnumerator ResetStagerable(float time )
	{
        yield return new WaitForSeconds( time );
        SetStaggerable( true );

    }


    protected override void PlayDamageSFX()
    {
        m_enemyAI.GetSoundHandler().PlayDamageSFX();
    }

    protected override void PlayDeathSFX()
    {
        m_enemyAI.GetSoundHandler().PlayDeathSFX();
    }



    void ResetAllColliders(bool reset)
    {
        GetComponent<Collider>().enabled = false;

        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach ( Collider collider in allColliders )
        {
            collider.enabled = false;
        }
    }



    protected override void Die()
    {
        m_enemyAI.SetAIState( AIState.Dead );
        m_enemyAI.UnregisterAttacker();

        ResetAllColliders( false );

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

    public IEnumerator ResetEnemy()
	{
        yield return new WaitForSeconds( 1f );

        gameObject.SetActive( false );
        ResetAllColliders( true );
        ResetHealth(); 
        ResetShader();
        m_spawnManager.AddToAvailable(gameObject.GetComponent<EnemyAI>());
        SetInvulnerable(false);
        SetAlive(true);
        gameObject.GetComponent<Collider>().enabled = true;

        UpdateHealthBar();

    }
}
