using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameTurnPanel : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text turnText;
    [SerializeField]
    TMPro.TMP_Text turnShadowText;
    void Start()
    {
        turnText.text = "0";
        turnShadowText.text = "0";
    }

    public void GameStart()
    {
        GameTurnManager.OnTurnChanged += OnTurn;
    }

    public void GameEnd()
    {
        GameTurnManager.OnTurnChanged -= OnTurn;
    }

    void OnTurn(int turn)
    {
        turnText.text = turn.ToString();
        turnShadowText.text = turn.ToString();
    }
}
