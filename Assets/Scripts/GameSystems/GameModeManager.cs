using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public enum Mode 
    {
        SplashScreen, LogoScreen, StartScreen, StartSinglePlayerGame, PlaySinglePlayerGame, EndGame
    }
    Mode currentMode = Mode.StartSinglePlayerGame;

    public static event Action<GameObject> OnGameObjectClicked;

    [SerializeField]
    MapGenerator mapGenerator;

    //public CameraMove cameraMover;
    // Start is called before the first frame update
    void Start()
    {
        CameraMove.OnGameObjectClicked += Callback;
    }

    void Callback(GameObject obj)
    {
        OnGameObjectClicked?.Invoke(obj);
        
    }
    // Update is called once per frame
    void Update()
    {
        switch (currentMode)
        {
            case Mode.StartSinglePlayerGame: 
                currentMode = Mode.PlaySinglePlayerGame;
                mapGenerator.Generate();
                break;
        }
    }
}
