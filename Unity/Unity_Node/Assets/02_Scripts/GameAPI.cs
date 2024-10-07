using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;          //JSON ���̺귯�� �߰�
using System;
using UnityEngine.Rendering;
using UnityEditor.Compilation;
using Unity.VisualScripting;       //Action<> ����� ���� ���ӽ����̽� �߰�

public class GameAPI : MonoBehaviour
{
    private string baseUrl = "http://localhost:4000/api";   //Node.js ������ URL
    //�÷��̾� ��� �޼���
    public IEnumerator RegisterPlayer(string playerName, string password)
    {
        var requestData = new { name = playerName, password = password };
        string jsonData = JsonConvert.SerializeObject(requestData);
        Debug.Log($"Registering Player: {jsonData}");

        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/register", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error registering player: {request.result}");
            }
            else
            {
                Debug.Log("Player registered successfully");
            }
        }
    }

    //�÷��̾� �α��� �޼���
    public IEnumerator LoginPlayer(string playerName, string password, Action<PlayerModel> onSuccess)
    {
        var requestData = new { name = playerName, password = password };
        string jsonData = JsonConvert.SerializeObject(requestData);

        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)        //���� ����
            {
                Debug.LogError($"Error loging in : {request.error}");   //���� �α�
            }
            else
            {
                //������ ó���Ͽ� PlayerModel ����
                string responseBody = request.downloadHandler.text;

                try
                {
                    var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);

                    //���� ���信�� PlayerModel ����
                    PlayerModel playerModel = new PlayerModel(responseData["playerName"].ToString())
                    {
                        metal = Convert.ToInt32(responseData["metal"]),
                        crystal = Convert.ToInt32(responseData["crystal"]),
                        deuterium = Convert.ToInt32(responseData["deuterium"]),
                        Planets = new List<PlanetModel>()
                    };

                    onSuccess?.Invoke(playerModel); //PlayerModel ��ȯ
                    Debug.Log("Login successful");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing login response: {ex.Message}");
                }
            }
        }
    }

    //�÷��̾� ��� �޼���
    public IEnumerator CollectResources(string playerName, Action<PlayerModel> onSuccess)
    {
        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/collect/{playerName}", "POST"))
        {
            string jsonData = JsonConvert.SerializeObject(new { });         //�� JSON ��ü
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)        //���� ����
            {
                Debug.LogError($"Error loging in : {request.error}");   //���� �α�
            }
            else
            {
                //������ ó���Ͽ� PlayerModel ����
                string responseBody = request.downloadHandler.text;

                try
                {
                    var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);

                    //���� ���信�� PlayerModel ����
                    PlayerModel playerModel = new PlayerModel("")
                    {
                        metal = Convert.ToInt32(responseData["metal"]),
                        crystal = Convert.ToInt32(responseData["crystal"]),
                        deuterium = Convert.ToInt32(responseData["deuterium"])
                    };

                    onSuccess?.Invoke(playerModel); //PlayerModel ��ȯ
                    Debug.Log("Login successful");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing login response: {ex.Message}");
                }
            }
        }
    }
}