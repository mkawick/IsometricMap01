using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTurnManager : MonoBehaviour
{
    public PlayerTurnTaker[] playerArchetypes;
    public GameObject playerCollectionNode;
    public bool isRegularGame;
    public List<PlayerTurnTaker> players;
    public PlayerTurnPanel playerPanel;
    int currentPlayer = 0;
    int numberOfTurnsTaken = 0;

    void Start()
    {
        if (isRegularGame)
        {
            // to do create players
            GameModeManager.OnGameGameModeChanged += OnGameGameModeChanged;
        }
        else
        {     
            //players.Add(Instantiate(playerArchetypes[0], playerCollectionNode.transform));
            // players.Add(Instantiate(playerArchetypes[1], playerCollectionNode.transform));
            // playerArchetypes[0].gameObject.SetActive(false);
            //  playerArchetypes[1].gameObject.SetActive(false);
        }
        CreateAllPlayers();
    }
    void CreateAllPlayers()
    {
        MapGenerator mapGenerator = GetComponent<GameModeManager>().mapGenerator;
        foreach (var archetype in playerArchetypes)
        {
            players.Add(Instantiate(archetype, playerCollectionNode.transform));
            archetype.gameObject.SetActive(false);
        }

        playerPanel.SetNumPlayers(players.Count);
        playerPanel.gameObject.SetActive(isRegularGame);

        int index = 0;
        foreach (var player in players)
        {
            player.gameObject.SetActive(true);
            player.GetComponent<PlayerTurnTaker>().mapGenerator = mapGenerator;
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
            players.Add(Instantiate(archetype, playerCollectionNode.transform));
            archetype.gameObject.SetActive(false);
        }
        foreach (var player in players)
        {
            player.gameObject.SetActive(true);
            player.GetComponent<PlayerTurnTaker>().mapGenerator = mapGenerator;
        }
        playerPanel.SetActivePlayer(currentPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        if(isRegularGame)
        {
            // go round robin
            if (players[currentPlayer].AmIDoneWithMyTurn() == true)
            {               
                // we need to manage game state here with popups
                int lastPlayerId = currentPlayer++;
                if (currentPlayer >= players.Count)
                {
                    currentPlayer = 0;
                    numberOfTurnsTaken++;
                }
                playerPanel.SetActivePlayer(currentPlayer);

                players[lastPlayerId].PlayEndTurnTransition(currentPlayer);
                // lots of variants of game state happen here.. 
                players[currentPlayer].YourTurn();
            }
            players[currentPlayer].ControlledUpdate();
        }
        else
        {
            foreach (PlayerTurnTaker player in players)
            {
                player.YourTurn();
                player.ControlledUpdate();
            }
            //players[currentPlayer].YourTurn();
        }
    }

    void OnGameGameModeChanged(GameModeManager.Mode mode)
    {
        if (mode == GameModeManager.Mode.StartSinglePlayerGame && isRegularGame)
        {
            //UpdateUnitOnTile();
            var player = Instantiate(playerArchetypes[0]);
            players.Add(player);
            var aiPlayer = Instantiate(playerArchetypes[1]);
            players.Add(aiPlayer);
            players[0].YourTurn();
        }
    }
}
