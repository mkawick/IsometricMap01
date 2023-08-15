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

    [SerializeField]
    MapGenerator mapGenerator;
    // Start is called before the first frame update
    void Start()
    {
        
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
