using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManagerMock : MonoBehaviour
{
    [SerializeField]
    GameObject loadingScreen;

    [SerializeField]
    Text loadingText;

    [SerializeField]
    Slider loadingSlider;

    AsyncOperation asyncLoad;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void StopEverythingElse()
	{
        loadingScreen.SetActive(true);
	}

    public void LoadScene(string scene )
    {
        StopEverythingElse();
        StartCoroutine( LoadYourAsyncScene( scene ) );
    }


    private IEnumerator LoadYourAsyncScene( string scene )
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        asyncLoad = SceneManager.LoadSceneAsync( scene );

        // Wait until the asynchronous scene fully loads
        while( !asyncLoad.isDone )
        {
            loadingSlider.value = asyncLoad.progress;
            //loadingText.text = "Loading...\n" + asyncLoad.progress * 100 + "%";
            yield return null;
        }
    }
}
