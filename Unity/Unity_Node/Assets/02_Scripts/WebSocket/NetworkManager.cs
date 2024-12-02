using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;          // WebSocket Ȱ��
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;


// �޼��� Ÿ�� ����
[Serializable]
public class NetworkMessage
{
    public string type;
    public string playerId;
    public string message;
    public Vector3Data position;
}

[Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3Data(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

}

public class NetworkManager : MonoBehaviour
{
    private WebSocket webSocket;

    [SerializeField]
    private string serverUrl = "ws://localhost:3000";

    [Header("UI ��ҵ�")]
    [Tooltip("�޽��� �Է�")]
    [SerializeField]
    private TMP_InputField messageInput;

    [Tooltip("���ϱ� ��ư")]
    [SerializeField]
    private Button SendButton;

    [Tooltip("ê �α�")]
    [SerializeField]
    private TextMeshProUGUI chatLog;

    [Tooltip("���� �ؽ�Ʈ")]
    [SerializeField]
    private TextMeshProUGUI statusText;


    [Header("Player")]
    [Tooltip("�÷��̾� ������")]
    [SerializeField]
    private GameObject playerPrefabs;       // Inspector ���� �÷��̾� ������ �Ҵ�

    private string myPlayerId;              // �� �÷��̾� ID ����
    private GameObject myPlayer;            // �� �÷��̾� ��ü
    private Dictionary<string, GameObject> otherPlayers = new Dictionary<string, GameObject>(); // �ٸ� �÷��̾�� ����

    private float syncInterval = 0.1f;      // ��ġ ����ȭ �ֱ�
    private float syncTimer = 0f;           // Ÿ�̸�


    private void Start()
    {
        ConnectToServer();
    }

    private void Update()
    {
#if UNITY_WEBGL || UNITY_EDITOR
        if (webSocket != null)
        {
            webSocket.DispatchMessageQueue();
        }
#endif
        // ��ġ ����ȭ Ÿ�̸�
        if (messageInput != null)
        {
            syncTimer += Time.deltaTime;
            if (syncTimer >= syncInterval)
            {
                SendPositionUpdate();
                syncTimer = 0f;
            }
        }
    }


    private async void ConnectToServer()
    {
        webSocket = new WebSocket(serverUrl);

        webSocket.OnOpen += () =>
        {
            Debug.Log("���� ����!");
            UpdateStatusText("�����", Color.green);
        };

        webSocket.OnError += (e) =>
        {
            Debug.Log($"���� : {e}");
            UpdateStatusText("���� �߻�", Color.red);
        };

        webSocket.OnClose += (e) =>
        {
            Debug.Log("���� ����");
            UpdateStatusText("���� ����", Color.red);
        };

        webSocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            HandleMessage(message);
        };

        await webSocket.Connect();
    }
    private void HandleMessage(string json)
    {
        try
        {
            NetworkMessage message = JsonConvert.DeserializeObject<NetworkMessage>(json);

            switch (message.type)
            {
                case "connection":
                    HandleConnection(message);
                    break;
                case "chat":
                    AddToChatLog(message.message);
                    break;
                case "playerPosition":
                    UpdatePlayerPosition(message);
                    break;
                case "playerDisconnect":
                    RemovePlayer(message.playerId);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"�޼��� ó�� �� ���� : {e.Message}");
        }
    }

    private async void SendChatMessage()
    {
        if (string.IsNullOrEmpty(messageInput.text))
        {
            return;
        }

        if (webSocket.State == WebSocketState.Open)
        {
            NetworkMessage message = new NetworkMessage()
            {
                type = "chat",
                message = messageInput.text
            };

            await webSocket.SendText(JsonConvert.SerializeObject(message));
            messageInput.text = "";
        }
    }

    private void UpdateStatusText(string status, Color color)
    {
        if (statusText != null)
        {
            statusText.text = status;
            statusText.color = color;
        }
    }

    private void AddToChatLog(string message)
    {
        if (chatLog != null)
        {
            chatLog.text += $"\n{message}";
        }
    }

    private async void OnApplicationQuit()
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await webSocket.Close();
        }
    }

    /// <summary>
    /// ���� ó�� �޼���
    /// </summary>
    /// <param name="message"></param>
    private void HandleConnection(NetworkMessage message)
    {
        myPlayerId = message.playerId;
        AddToChatLog($"������ ����� (ID : {myPlayerId}");

        // �÷��̾� ����
        Vector3 spawnPosition = new Vector3(0, 1, 0);
        myPlayer = Instantiate(playerPrefabs, spawnPosition, Quaternion.identity);
        myPlayer.name = $"Player_{myPlayerId}";

        // �� �÷��̾� ����
        PlayerController controller = myPlayer.GetComponent<PlayerController>();

        if (controller != null)
        {
            controller.SetAsMyPlayer();
        }
    }


    /// <summary>
    /// ��ġ ���� �Լ� (��ġ ���� �Լ�)
    /// </summary>
    private async void SendPositionUpdate()
    {
        if (webSocket.State == WebSocketState.Open && myPlayer != null)
        {
            NetworkMessage message = new NetworkMessage
            {
                type = "playerPosition",
                playerId = myPlayerId,
                position = new Vector3Data(myPlayer.transform.position)
            };

            await webSocket.SendText(JsonConvert.SerializeObject(message));
        }
    }

    /// <summary>
    /// �ٸ� �÷��̾� ��ġ ������Ʈ �Լ�
    /// </summary>
    /// <param name="message"></param>
    private void UpdatePlayerPosition(NetworkMessage message)
    {
        if (message.playerId == myPlayerId)
        {
            return; // �ڽ��� ��ġ�� ����
        }

        if (!otherPlayers.ContainsKey(message.playerId))
        {
            // ���ο� �÷��̾� ����
            GameObject newPlayer = Instantiate(playerPrefabs);
            newPlayer.name = $"Player_{message.playerId}";
            otherPlayers.Add(message.playerId, newPlayer);
        }

        // ��ġ ������Ʈ
        otherPlayers[message.playerId].transform.position = message.position.ToVector3();
    }

    // �÷��̾� ���� �Լ�
    private void RemovePlayer(string playerId)
    {
        if (otherPlayers.ContainsKey(playerId))
        {
            Destroy(otherPlayers[playerId]);
            otherPlayers.Remove(playerId);
            AddToChatLog($"�÷��̾� {playerId} ����");
        }
    }
}
