using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class GameTurnManager : MonoBehaviour
{
    public PlayerTurnTaker[] playerArchetypes;
    public GameObject playersInstantiatedRootNode;
    private bool isRegularGame;
    public List<PlayerTurnTaker> players;

    public GameObject mainGameStatsPanel;
    public PlayerTurnPanel playerPanel;
    public UI_PlayerResources playerResourcesUi;
    public UI_GameTurnPanel turnPanelUi;
    
    int currentPlayer = 0;
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
    void CreateAllPlayers()
    {
        MapGenerator mapGenerator = GetComponent<GameModeManager>().mapGenerator;
        var listOfSpots = mapGenerator.ChoosenStartingPositions;
        int index = 0;
        foreach (var archetype in playerArchetypes)// problem... does not match number of players
        {
            var spot = listOfSpots[index++];
            var root = playersInstantiatedRootNode.transform;
            var pos = mapGenerator.TranslateMapToWorld(spot);
            players.Add(Instantiate(archetype, root.position + pos, root.rotation));
            archetype.gameObject.SetActive(false);
        }

        SetupPlayerTurnsPanelUI();

        PlayerTurnTaker localPlayer = null;
        foreach (var player in players)
        {
            player.gameObject.SetActive(true);
            var playerTurnTaker = player.GetComponent<PlayerTurnTaker>();
            playerTurnTaker.mapGenerator = mapGenerator;
            playerTurnTaker.IsRegularGame = isRegularGame;

            if (playerTurnTaker.IsHuman)
                localPlayer = playerTurnTaker;
        }
        
        if (localPlayer)
        {
            localPlayer.PlayerResourcesUi = playerResourcesUi;
            //playerResourcesUi.SetupPlayer(localPlayer.GetComponent<ResourceCollector>());
        }
        turnPanelUi.GameStart();
    }

    private void SetupPlayerTurnsPanelUI()
    {
        playerPanel.SetNumPlayers(players.Count);
        playerPanel.gameObject.SetActive(isRegularGame);
        int index = 0;
        foreach (var player in players)
        {
            playerPanel.SetPlayerName(player.PlayerName, index++);
        }
        playerPanel.SetActivePlayer(currentPlayer);
    }

    void CreateFirstHumanPlayer()
    {
        MapGenerator mapGenerator = GetComponent<GameModeManager>().mapGenerator;
        foreach (var archetype in playerArchetypes)
        {
            var player = archetype.GetComponent<PlayerTurnTaker>();
            if (player == null || player.IsHuman == false)
                continue;
            players.Add(Instantiate(archetype, playersInstantiatedRootNode.transform));
            archetype.gameObject.SetActive(false);
        }
        foreach (var player in players)
        {
            player.gameObject.SetActive(true);
            player.GetComponent<PlayerTurnTaker>().mapGenerator = mapGenerator;
        }
        playerPanel.SetActivePlayer(currentPlayer);
    }

    ResourceCollector FindNextCollector ()
    {
        do
        {
            ResourceCollector collector = players[currentPlayer].GetComponent<ResourceCollector>();
            if (collector != null)
                return collector;
        } while (++currentPlayer < players.Count);

        return null;
    }
    // Update is called once per frame
    void Update()
    {
        if(isRegularGame)
        {
            switch (gameTurnState)
            {
                case GameTurnState.Move:
                    {    // go round robin
                        if (players[currentPlayer].AmIDoneWithMyTurn() == true)
                        {
                            // we need to manage game state here with popups
                            int lastPlayerId = currentPlayer++;
                            if (currentPlayer >= players.Count)
                            {
                                currentPlayer = 0;
                                gameTurnState = GameTurnState.Collect;
                            }
                            playerPanel.SetActivePlayer(currentPlayer);

                            players[lastPlayerId].PlayEndTurnTransition(currentPlayer);
                            // lots of variants of game state happen here.. 
                            players[currentPlayer].YourTurn();
                        }
                        players[currentPlayer].ControlledUpdate();                        
                    }
                    break;
                case GameTurnState.Collect:
                    {
                        ResourceCollector collector = FindNextCollector();
                        if (collector == null)
                        {
                            currentPlayer = 0;
                            gameTurnState = GameTurnState.ShowEndOfTurn;
                            break;
                        }
                        if(collector.AmIDoneCollecting())
                        {
                            currentPlayer++;
                            if(currentPlayer >= players.Count)
                            {
                                currentPlayer = 0;
                                gameTurnState = GameTurnState.ShowEndOfTurn;                                
                                break;
                            }
                        }
                        else
                        {
                            collector.ControlledUpdate();
                        }
                    }
                    break;

                case GameTurnState.ShowEndOfTurn:
                        gameTurnState = GameTurnState.Move;
                    OnTurnChanged?.Invoke(++currentGameTurn);
                    break;
            }

        }
        else
        {
            foreach (PlayerTurnTaker player in players)
            {
                player.YourTurn();
                player.ControlledUpdate();
            }
        }
    }

    void LetEachPlayerRunCollections()
    {

    }

    void OnGameModeChanged(GameModeManager.GameMode mode, bool regularGame)
    {
        isRegularGame = regularGame;
        if (mode == GameModeManager.GameMode.StartSinglePlayerGame)// && isRegularGame)
        {
            CreateAllPlayers();
            gameTurnState = GameTurnState.Move;
            OnTurnChanged?.Invoke(currentGameTurn);
        }
        else if (isRegularGame == false)
        {
            mainGameStatsPanel?.SetActive(false);
        }
    }
}
