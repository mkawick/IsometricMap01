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
    public static event Action<Mode, bool> OnGameModeChanged;

    [SerializeField]
    MapGenerator _mapGenerator;
    public MapGenerator mapGenerator { get { return _mapGenerator; } }
    [SerializeField] 
    public bool isRegularGame;

    //public CameraMove cameraMover;
    // Start is called before the first frame update
    void Start()
    {
        GameUnitSelector.OnGameObjectClicked += Callback;
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
                OnGameModeChanged?.Invoke(currentMode, isRegularGame);
                break;
            case Mode.StartSinglePlayerGame:
                currentMode = Mode.PlaySinglePlayerGame;
                OnGameModeChanged?.Invoke(currentMode, isRegularGame);
                break;
        }
    }
}
