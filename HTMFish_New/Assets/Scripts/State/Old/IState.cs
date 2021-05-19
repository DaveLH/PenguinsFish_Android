using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void OnStateEnter();
    void OnStateExit();
    void OnStateUpdate();
    void OnClickPenguin(GamePenguin penguin);
}


