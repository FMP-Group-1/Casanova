using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class enemyHit : MonoBehaviour
{
    Color defaultColour;
    Color hitColour = Color.white;

    Renderer thisRenderer;

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
        StartCoroutine( Flash() );
    }

    private IEnumerator Flash()
    {
        thisRenderer.material.color = hitColour;
        yield return new WaitForSeconds( .1f );
        thisRenderer.material.color = defaultColour;

    }

    private void OnCollisionEnter( Collision collision )
    {
        //Debug.Log("Called from Enemy OnCollision");
    }
}
