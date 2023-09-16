using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoCameraController : MonoBehaviour
{
    Vector3 offset;
    private bool isRegularGame;
    void Start()
    {
        GameModeManager.OnGameModeChanged += OnGameGameModeChanged;
    }


    void OnGameGameModeChanged(GameModeManager.Mode mode, bool regularGame)
    {
        isRegularGame = regularGame;
        if (mode == GameModeManager.Mode.StartSinglePlayerGame && isRegularGame)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, 0xFFF))
            {
                offset = hit.transform.position - transform.position;
            }
        }
    }

    public void SetTarget(GameObject target)
    {
        if (isRegularGame == false)
            return;

        Vector3 newPos = target.transform.position - offset;
        transform.LeanMoveLocal(newPos, 0.4f).setEaseOutExpo();
    }
}
