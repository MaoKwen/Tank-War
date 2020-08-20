using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;
using WebSocketSharp;

public enum GameStatus
{
    Running,
    Waiting
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private WebSocket ws;
    public Transform ground;
    public GameObject tankPrefab;

    public List<Tank> tankList;
    public Tank playerTank;
    public Queue<byte[]> messageQueue;
    public byte[] newMessage;
    private Camera camera;

    private GameStatus Status;

    void Awake()
    {
        if (Instance == null) Instance = this;
        tankList = new List<Tank>();
        messageQueue = new Queue<byte[]>();
    }

    void Start()
    {
        ws = new WebSocket("ws://192.168.96.165:9001");
        ws.ConnectAsync();
        ws.OnOpen += (o, e) => Debug.Log("Connect Success!");
        ws.OnError += (o, e) => Debug.Log("Connect Failed!");
        ws.OnMessage += OnServerMessage;

        Status = GameStatus.Running;
    }

    void Update()
    {
        //if (tankList.isEmpty())
        //{
        //    ws.Ping();
        //    return;
        //}
        //if (messageQueue.Count > 0)
        //{
        //    StartCoroutine(UpdatePosition(messageQueue.Dequeue()));
        //}
    }

    private void FixedUpdate()
    {
        UpdatePosition(newMessage);
    }

    public void SentInput(byte[] moveMessage)
    {
        if (!ws.IsAlive)
        {
            Debug.Log("服务器关闭");
        }
        ws.SendAsync(moveMessage, (bool comp) => { });
    }

    void OnServerMessage(object sender, MessageEventArgs e)
    {
        if (e.IsBinary && e.RawData.Length > 4)
        {
            //messageQueue.Enqueue(e.RawData);
            newMessage = e.RawData;
        }
    }

    private void UpdatePosition(byte[] data)
    {
        //if (Status == GameStatus.Waiting) return;
        if (newMessage.Length <= 4) return;

        Status = GameStatus.Waiting;

        byte[] rowData = new byte[data.Length - 4];
        Array.Copy(data, 4, rowData, 0, data.Length - 4);

        var str = System.Text.Encoding.Default.GetString(rowData);

        MoveInfo info = JsonUtility.FromJson<MoveInfo>(str);
        if (info.users == null) return;
        foreach (var u in info.users)
        {
            var match = tankList.Where(x => x.Id == u.Id);
            Tank tank;
            if (match.Count() == 0)
            {
                tank = CreateTank(u);
            }
            else
            {
                tank = match.First();
            }
            UpdatePositon(tank, u);
        }

        //Status = GameStatus.Running;
    }

    private void UpdatePositon(Tank tank, User u)
    {
        Vector3 newPosition = new Vector3(u.X * 10, 0, u.Y * 10);
        Vector3 curPosition = tank.gameObject.transform.position;
        Vector2 direct = new Vector2(newPosition.x - curPosition.x, newPosition.z - curPosition.z);
        if (direct.magnitude > 0.1f)
        {
            float angle = Vector2.Angle(Vector2.up, direct);
            Debug.Log(angle);
            Quaternion q = new Quaternion();
            q.eulerAngles = new Vector3(0, angle, 0);
            tank.gameObject.transform.rotation = q;
        }
        tank.gameObject.transform.localPosition = new Vector3(u.X * 10, 0, u.Y * 10);
    }

    private Tank CreateTank(User u)
    {
        Tank tank = Instantiate(tankPrefab, ground).GetComponent<Tank>();
        tank.Id = u.Id;
        tankList.Add(tank);
        if (playerTank == null)
        {
            playerTank = tank;
            camera = playerTank.camera;
            camera.gameObject.SetActive(true);
        }
        return tank;
    }
}
