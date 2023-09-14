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
    Mode currentMode = Mode.StartScreen;

    public static event Action<GameObject> OnGameObjectClicked;
    public static event Action<Mode> OnGameGameModeChanged;

    [SerializeField]
    MapGenerator _mapGenerator;
    public MapGenerator mapGenerator { get { return _mapGenerator; } }

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
            case Mode.StartScreen: 
                currentMode = Mode.StartSinglePlayerGame;
                mapGenerator.Generate();
                OnGameGameModeChanged?.Invoke(currentMode);
                break;
            case Mode.StartSinglePlayerGame:
                currentMode = Mode.PlaySinglePlayerGame;
                OnGameGameModeChanged?.Invoke(currentMode);
                break;
        }
    }
}
