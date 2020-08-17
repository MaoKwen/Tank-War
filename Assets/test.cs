using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class test : MonoBehaviour
{

    long timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TestServer());
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator TestServer()
    {
        using (var ws = new WebSocket("ws://192.168.96.165:9001"))
        {
            ws.OnMessage += (sender, e) => Debug.Log("Server says: " + e.Data);
            ws.ConnectAsync();

            while (ws.IsAlive)
            {
                yield return new WaitForSeconds(10);

                Debug.Log(ws.Ping());
                ws.Send("hello");
            }
        }


    }
}
