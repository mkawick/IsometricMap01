using System;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public enum GameMode 
    {
        SplashScreen, 
        LogoScreen, 
        StartScreen, 
        StartSinglePlayerGame, 
        PlaySinglePlayerGame, 
        PathingTest,
        EndGame
    }
    GameMode currentMode = GameMode.StartScreen;

    [SerializeField]
    MapGenerator _mapGenerator;
    public MapGenerator mapGenerator { get { return _mapGenerator; } }
    [SerializeField] 
    private bool isRegularGame;
    [SerializeField, Range(2,8)]
    private int numPlayers;
    [SerializeField]
    private bool pathingTest;
    [SerializeField]
    private GameObject pathingTestPanel;

    public static event Action<GameObject> OnGameObjectClicked;
    public static event Action<GameMode, bool> OnGameModeChanged;

    void Start()
    {
        GameUnitSelector.OnGameObjectClicked += Callback;
        if (numPlayers == 0) // someone forgot to set this
            numPlayers = 2;
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
            case GameMode.StartScreen:
                if (pathingTest == false)// this works for now but needs a refactor with any additional complexity
                {
                    currentMode = GameMode.StartSinglePlayerGame;
                    pathingTestPanel.SetActive(false);
                }
                else
                {
                    currentMode = GameMode.PathingTest;
                    pathingTestPanel.SetActive(true);
                }
                var startingPositions = mapGenerator.Generate(numPlayers);
                OnGameModeChanged?.Invoke(currentMode, isRegularGame);
                break;
            case GameMode.StartSinglePlayerGame:
                if (pathingTest == false)
                {
                    currentMode = GameMode.PlaySinglePlayerGame;
                    OnGameModeChanged?.Invoke(currentMode, isRegularGame);
                }
                break;
        }
    }
}
