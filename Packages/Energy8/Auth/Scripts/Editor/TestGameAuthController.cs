using Energy8.Auth;
using Energy8.Models.Games;
using UnityEngine;

public class TestGameAuthController : GameAuthControllerBase<GameUserData, GameServerData>
{
    new void Awake()
    {
        Game = "Slots";
        base.Awake();
    }   
}
