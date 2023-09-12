using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTurnManager : MonoBehaviour
{
    public PlayerTurnTaker[] playerArchetypes;
    public GameObject playerCollectionNode;
    public bool isRegularGame;
    public List<PlayerTurnTaker> players;
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
            players.Add(Instantiate(playerArchetypes[0], playerCollectionNode.transform));
            players.Add(Instantiate(playerArchetypes[1], playerCollectionNode.transform));
        }
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
