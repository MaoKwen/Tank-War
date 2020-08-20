using MiscUtil.Conversion;
using MiscUtil.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputController Instance;
    private float m_VerticalInputValue;
    private float m_HorizontalInputValue;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_VerticalInputValue = 0f;
        m_HorizontalInputValue = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        m_VerticalInputValue = Input.GetAxis("Vertical");
        m_HorizontalInputValue = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        handleMovement();
    }

    void handleMovement()
    {
        var moveDirect = new Vector2(m_HorizontalInputValue, m_VerticalInputValue);
        m_VerticalInputValue = m_VerticalInputValue = 0f;
        if (moveDirect.magnitude < 0.1f)
        {
            return;
        }

        int moveAngle = (int)Vector2.SignedAngle(Vector2.up, moveDirect);
        if (moveAngle < 0) moveAngle += 360;

        byte[] moveMessage;

        //using (MemoryStream ms = new MemoryStream())
        //{
        //    Debug.Log(BitConverter.IsLittleEndian);
        //    ms.Write(BitConverter.GetBytes(0), 0, 4);
        //    ms.Write(BitConverter.GetBytes((short)1), 0, 2);
        //    ms.Write(BitConverter.GetBytes((short)0), 0, 2);
        //    ms.Write(BitConverter.GetBytes(moveAngle), 0, 4);
        //    moveMessage = ms.ToArray();
        //}

        using (MemoryStream stream = new MemoryStream())
        {
            LittleEndianBitConverter converter = new LittleEndianBitConverter();
            //BigEndianBitConverter converter = new BigEndianBitConverter();
            EndianBinaryWriter writer = new EndianBinaryWriter(converter, stream);

            writer.Write(0);
            writer.Write((short)0);
            writer.Write((short)1);
            writer.Write(moveAngle);
            moveMessage = stream.ToArray();
        }

        if (moveMessage != null)
        {
            GameManager.Instance.SentInput(moveMessage);
        }
    }
}
