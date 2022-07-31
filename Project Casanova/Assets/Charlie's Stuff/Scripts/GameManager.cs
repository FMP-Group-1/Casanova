using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    private UIManager m_uiManager;

    void Start()
    {
        m_uiManager = GetComponent<UIManager>();
        m_uiManager.BeginScene();
    }


	private void Update()
	{

	}

}
