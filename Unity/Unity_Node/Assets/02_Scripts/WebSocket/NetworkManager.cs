using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;                              //WebSocket Ȱ��
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;

//�޼��� Ÿ�� ���� 
[Serializable]
public class NetworkMessage
{
    public string type;
    public string playerId;
    public string message;
    public Vector3Data position;
}

//Vecotr3 ����ȭ�� ���� Ŭ����
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
    [SerializeField] private string serverUrl = "ws://localhost:3000";

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button SendButton;
    [SerializeField] private TextMeshProUGUI chatLog;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;  // Inspector���� �÷��̾� ������ �Ҵ��
    private string myPlayerId;                         // �� �÷��̾� ID ����
    private GameObject myPlayer;                       // �� �÷��̾� ��ü
    private Dictionary<string, GameObject> otherPlayers = new Dictionary<string, GameObject>();  // �ٸ� �÷��̾�� ����

    private float syncInterval = 0.1f;    // ��ġ ����ȭ �ֱ�
    private float syncTimer = 0f;         // Ÿ�̸�

    // Start is called before the first frame update
    void Start()
    {
        // ���� ���� ����
        ConnectToServer();

        //// UI �̺�Ʈ ����
        //messageInput.onEndEdit.AddListener((message) => {
        //    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        //    {
        //        SendChatMessage();
        //    }
        //});

        //SendButton.onClick.AddListener(SendChatMessage);
    }

    // Update is called once per frame
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (webSocket != null)
        {
            webSocket.DispatchMessageQueue();
        }
#endif
        // ��ġ ����ȭ Ÿ�̸�
        if (myPlayer != null)
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
            Debug.Log("���� �޽���: " + json);  // �߰�: ���� �޽��� ���

            NetworkMessage message = JsonConvert.DeserializeObject<NetworkMessage>(json);
            Debug.Log("�޽��� Ÿ��: " + message.type);  // �߰�: �޽��� Ÿ�� ���




            switch (message.type)
            {
                case "connection":
                    HandleConnection(message);  // ���� �� �޼��带 ���� ������ ��
                    break;

                case "chat":
                    AddToChatLog(message.message);
                    break;

                // ���� �߰��ϴ� ���̽���
                case "playerPosition":
                    UpdatePlayerPosition(message);  // ���� ���� �޼���
                    break;

                case "playerDisconnect":
                    RemovePlayer(message.playerId); // ���� ���� �޼���
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
        if (string.IsNullOrEmpty(messageInput.text)) return;

        if (webSocket.State == WebSocketState.Open)
        {
            NetworkMessage message = new NetworkMessage
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

    // ���� ó�� �޼���
    private void HandleConnection(NetworkMessage message)
    {
        myPlayerId = message.playerId;
        AddToChatLog($"������ ����� (ID: {myPlayerId})");

        // �÷��̾� ����
        Vector3 spawnPosition = new Vector3(0, 1, 0);
        myPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        myPlayer.name = $"Player_{myPlayerId}";

        // �� �÷��̾� ����
        PlayerController controller = myPlayer.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetAsMyPlayer();
        }
    }

    // ��ġ ���� �޼���
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

    // �ٸ� �÷��̾� ��ġ ������Ʈ �޼���
    private void UpdatePlayerPosition(NetworkMessage message)
    {
        if (message.playerId == myPlayerId) return; // �ڽ��� ��ġ�� ����

        if (!otherPlayers.ContainsKey(message.playerId))
        {
            // ���ο� �÷��̾� ����
            GameObject newPlayer = Instantiate(playerPrefab);
            newPlayer.name = $"Player_{message.playerId}";
            otherPlayers.Add(message.playerId, newPlayer);
        }

        // ��ġ ������Ʈ
        otherPlayers[message.playerId].transform.position = message.position.ToVector3();
    }

    // �÷��̾� ���� �޼���
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