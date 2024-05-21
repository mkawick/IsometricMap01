using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PathingTestManager : MonoBehaviour
{
    public PlayerTurnTaker[] playerArchetypes;
    public GameObject playersInstantiatedRootNode;
    int currentGameTurn = 1;
    enum GameTurnState { Move, Collect, ShowEndOfTurn };
    GameTurnState gameTurnState;

    public static event Action<int> OnTurnChanged;

    void Start()
    {
        //if (isRegularGame)
        //{
        // to do create players
        GameModeManager.OnGameModeChanged += OnGameModeChanged;
        // }
        //else
        // {     
        //players.Add(Instantiate(playerArchetypes[0], playerCollectionNode.transform));
        // players.Add(Instantiate(playerArchetypes[1], playerCollectionNode.transform));
        // playerArchetypes[0].gameObject.SetActive(false);
        //  playerArchetypes[1].gameObject.SetActive(false);
        // }

    }
    void OnGameModeChanged(GameModeManager.GameMode mode, bool regularGame)
    {
        if (regularGame == true)// only test mode
            return;
        if (mode == GameModeManager.GameMode.PathingTest)
        {
            //CreateAllPlayers();
            gameTurnState = GameTurnState.Move;
            OnTurnChanged?.Invoke(currentGameTurn);
        }

    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    public void OnStartButtonPressed()
    {

    }
    public void OnEndButtonPressed()
    {

    }
    public void OnMouseClick()
    {

    }
}