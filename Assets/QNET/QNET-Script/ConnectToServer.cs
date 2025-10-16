using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QNetLib;

public class ConnectToServer : MonoBehaviour
{

    public enum HeaderType
    {
        JoinRoom,
        OutRoom,
        SendMesege,

    }

    public Text IPAddress;
    //Hierarchy 에서 넣어야함

    public Text ClientChatBox;
    public InputField CHAT;
    public InputField NICKNAME;


    public Client client;

    string Text_string;
    byte[] Text_byte;

    int id;
    int type;
    int Length;
    string Msg;

    PacketQueue.Packet recv;

    BufferManager buffer = new BufferManager();

    

    // Start is called before the first frame update
    void Start()
    {
        ClearChatBox();

        int port = 9998;

        client = new Client("127.0.0.1", port);
        



        client.OnConnetHandler += (e) =>
        {
            Debug.Log("connect");
        };
        client.OnCloseHandler += (e) =>
        {
            Debug.Log("closer");
        };
        client.OnReceiveHandler += (e) =>
        {
            Debug.Log("re");
        };
        client.OnErrorHandler += (e) =>
        {
            Debug.Log("Error" + e.AsyncSocketException);
        };

        Debug.Log("이벤트 등록!");

    }

    // Update is called once per frame
    void Update()
    {
        if (client.isConnect())
        {
            recv = client.packetQueue.Dequeue();

            if (recv.size > 0)
            {
                Debug.Log("데이터옴");
                recv.buff.Read(out  type);


                switch (type)
                {

                    case (int)HeaderType.JoinRoom:
                        recv.buff.Read(out id);
                        ClientChatBox.text += "JoinRoom : " + id + "\n";
                        break;

                    case (int)HeaderType.OutRoom:
                        recv.buff.Read(out id);
                        ClientChatBox.text += "OutRoom : " + id + "\n";
                        break;

                    case (int)HeaderType.SendMesege:

                        recv.buff.Read(out  Length);
                        recv.buff.Read(out  Msg, Length);

                        ClientChatBox.text += Msg + "\n";
                        break;




                }
            }
        }
    }
    public void Connect()
    {
        ClearChatBox();


        client.Start();

    }

    public void Send()
    {
        Text_string = NICKNAME.text + " : " + CHAT.text;


        buffer.Write((int)HeaderType.SendMesege);
        buffer.Write(Text_string.Length);
        buffer.Write(Text_string);
        client.SendToServer(buffer.buffer);

        buffer.ClearBuffer();

        buffer.ClearBuffer();

        Debug.Log(Text_string);
    }


    public void ClearChatBox()
    {
        ClientChatBox.text = "";
    }

    public void Text_Update(byte[] Text_byte)
    {

        Text_string = System.Text.Encoding.UTF8.GetString(Text_byte);

        ClientChatBox.text += "\n OnReceiveData \n" + Text_string;
        

    }
    
    public void Close()
    {
        ClientChatBox.text = "disConnect server \n";

        client.Close();
    }
    

    
}

