using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumbWalker : MonoBehaviour  /// needs different interface
{
    enum BehaviorState { Waiting, Moving };
    float nextMoveTime;
    BehaviorState state = BehaviorState.Waiting;

    void Start()
    {
        Invoke("GameIsReady", 2f);
        //this.DelayedInvoke(2.0f, "GameIsReady");
    }

    public void GameIsReady() // change to running???
    {
        state = BehaviorState.Moving;
        HandleMove();
    }

    void Update()
    {
        if (state == BehaviorState.Waiting)
            return;
        HandleMove();
    }

    void HandleMove()
    {
        if (nextMoveTime < Time.time)
        {
            nextMoveTime = Time.time + Random.Range(1, 3);
            var test2dPos = MapUtils.ConvertScreenPositionToMap(this.transform.position);

            int dir = Random.Range(0, 3);
            for (int i = 0; i < 4; i++, dir++)
            {// 4 should be part of the unit config

                var offset = MapUtils.Dir4Lookup(dir);
                if (MapUtils.IsDirValid(test2dPos + offset))
                {
                    this.transform.position += new Vector3Int(offset.x, 0, offset.y);
                    break;
                }
            }
        }
    }
}
