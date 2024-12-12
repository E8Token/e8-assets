using Energy8.Auth;
using Energy8.Models.Games;
using UnityEngine;

public class TestGameAuthController : GameAuthControllerBase<GameUserDataBase, GameServerData>
{
    new void Awake()
    {
        Game = "Slots";
        base.Awake();
    }   
}
