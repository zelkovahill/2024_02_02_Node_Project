using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;




public class GameManager : MonoBehaviour
{
    public GameView gameView;
    public GameController gameController;

    private void Start()
    {
        gameController = gameController.gameObject.AddComponent<GameController>();
        gameController.gameView = gameView;
    }
}
