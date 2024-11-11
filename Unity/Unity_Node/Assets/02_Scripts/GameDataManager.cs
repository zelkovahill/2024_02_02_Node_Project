using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;


[System.Serializable]
public class Player
{
    public int player_id;
    public string username;
    public int level;
}

[System.Serializable]
public class InvetoryItem
{
    public int item_id;
    public string name;
    public string description;
    public int level;
    public int quantity;
}

[System.Serializable]
public class Quest
{
    public int quest_id;
    public string title;
    public string description;
    public int reward_exp;
    public int reward_item_id;
    public string status;
}

public class GameDataManager : MonoBehaviour
{
    private string serverUrl = "http://localhost:3000";
    private Player currentPlayer;

    // ������ ����Ʈ
    public List<InvetoryItem> inventoryItems = new List<InvetoryItem>();
    public List<Quest> playerQuests = new List<Quest>();

    // �α��� ���� �� ����� �̺�Ʈ
    // OnLoginSuccessHandler

    // ������ ������Ʈ�� ����� �̺�Ʈ
    // OnInventoryUpdateHandler
    // OnQuestsUpdatehandler

    private void Start()
    {
       
    }

}
