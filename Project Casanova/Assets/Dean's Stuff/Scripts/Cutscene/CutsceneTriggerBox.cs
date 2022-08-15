using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneTriggerBox : MonoBehaviour
{
    [SerializeField]
    private GameObject m_cutsceneObj;
    private UIManager m_uiManager;

    private void Start()
    {
        m_uiManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UIManager>();
    }

    private void OnTriggerEnter( Collider other )
    {
        m_cutsceneObj.SetActive(true);
        m_uiManager.CutsceneHandover(m_cutsceneObj.GetComponent<Cutscene>());
        gameObject.SetActive(false);
    }
}
