using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class enemyHit : MonoBehaviour
{
    Color defaultColour;
    Color hitColour = Color.white;

    Renderer thisRenderer;

    private float invulnerableTime = 0.1f;

    public bool canBeHit = true;


    // Start is called before the first frame update
    void Start()
    {
        thisRenderer = GetComponent<Renderer>();
        defaultColour = thisRenderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GetHit()
    {
        if ( canBeHit )
        {
            canBeHit = false;

            Debug.Log( "Can't be hit again" );
            StartCoroutine( Flash() );
            StartCoroutine( ResetTimer() );
        }
    }

    private IEnumerator Flash()
    {
        thisRenderer.material.color = hitColour;
        yield return new WaitForSeconds( .1f );
        thisRenderer.material.color = defaultColour;

    }

    private IEnumerator ResetTimer()
    {
        yield return new WaitForSeconds( invulnerableTime );
        Debug.Log( "Can be hit again" );
        canBeHit = true;

    }

    private void OnCollisionEnter( Collision collision )
    {
        //Debug.Log("Called from Enemy OnCollision");
    }
}
