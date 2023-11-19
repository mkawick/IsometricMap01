using System;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public enum Mode 
    {
        SplashScreen, LogoScreen, StartScreen, StartSinglePlayerGame, PlaySinglePlayerGame, EndGame
    }
    Mode currentMode = Mode.StartScreen;

    [SerializeField]
    MapGenerator _mapGenerator;
    public MapGenerator mapGenerator { get { return _mapGenerator; } }
    [SerializeField] 
    private bool isRegularGame;
    [SerializeField, Range(2,8)]
    private int numPlayers;

    public static event Action<GameObject> OnGameObjectClicked;
    public static event Action<Mode, bool> OnGameModeChanged;

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
                mapGenerator.Generate(numPlayers);
                OnGameModeChanged?.Invoke(currentMode, isRegularGame);
                break;
            case Mode.StartSinglePlayerGame:
                currentMode = Mode.PlaySinglePlayerGame;
                OnGameModeChanged?.Invoke(currentMode, isRegularGame);
                break;
        }
    }
}
