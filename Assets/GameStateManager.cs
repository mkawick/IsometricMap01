using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Scene scene = SceneManager.GetActiveScene();
        DontDestroyOnLoad(this.gameObject);
    }

    public void Play()
    {
        SceneManager.LoadScene("Start01", mode: LoadSceneMode.Single);
    }

    public void Exit() 
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
