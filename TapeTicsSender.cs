using System.Collections;
using System.Net.Sockets;   //UDP通信のためのクラスを使用するために必要
using System.Text;  //文字列をバイト配列に変換するために必要
using UnityEngine;
using UnityEngine.InputSystem;

public class TapeTicsSender : MonoBehaviour
{
    public enum LedChannel  //LEDの色を指定するために使う列挙型。LedChannerlの中にR,G,Bの3つの値をいれておく
    {
        R,
        G,
        B
    }

    //インスペクター上で編集できるようにするための部分
    //UDP通信の設定
    [Header("UDP Settings")]
    [SerializeField] private string udpIp = "127.0.0.1";
    [SerializeField] private int udpPort = 12345;

    //振動パターンの設定
    [Header("TapeTics Pattern Settings")]
    [SerializeField] private LedChannel ledChannel = LedChannel.R;  //LEDの色を選ぶためのフィールド
    [SerializeField] private int nodeNumber = 1;    //振動させたいノードの番号を指定するためのフィールド。
   
    [Tooltip("振動の強さ。BLE.py内では intensity として扱われます")]    //intensityの説明
    [SerializeField] private int intensity = 30;    //振動の強さを指定するためのフィールド。

    [Tooltip("振動を開始する時刻（秒）")]
    [SerializeField] private float startTime = 0.0f;

    [Tooltip("振動を終了する時刻（秒）")]
    [SerializeField] private float endTime = 1.0f;

    [Header("ダミー値")]
    [Tooltip("BLE.py が先頭要素を捨てるため、その捨てられるダミー値を送る")]
    [SerializeField] private string dummyValue = "0";

    [Header("Input")]
    [SerializeField] private Key triggerKey = Key.Space;    //このキーを押すと振動パターンが送信されるようにする

    [Header("Send Timing")]
    [Tooltip("各UDPメッセージ送信の間に入れる待ち時間（秒）")]
    [SerializeField] private float messageInterval = 0.02f;

    private UdpClient udpClient;    //UDP通信を行うオブジェクトをいれておくためのフィールド
    private bool isSending = false; //現在振動パターンを送信中かどうかを管理するためのフラグ。これがtrueのときは、まだ前のパターンの送信が終わっていないので、新しいパターンの送信を開始しないようにするために使う。

    void Start()
    {
        udpClient = new UdpClient();    //UDP通信を行うためのオブジェクトnew UdpClient()で取り出してudpClientに代入する。UdpClientにあるメソッドを使用したい場合はudpClientを使えばよい。
        Debug.Log($"UDP sender ready: {udpIp}:{udpPort}");  //UDP通信のためのIDとポート番号を表示する
        StartCoroutine(SendPatternSequence());  //ゲーム開始時に振動パターンだけ送る
    }

    void Update()
    {
        //デバッグ用、キーボード入力で振動パターンを送るための部分
        if (Keyboard.current == null || isSending)  //Keyboard.currentがnullのときはキーボード入力を受け付けられないので、何もしないようにする。また、isSendingがtrueのときは、まだ前のパターンの送信が終わっていないので、新しいパターンの送信を開始しないようにする。
            return;

        if (Keyboard.current[triggerKey].wasPressedThisFrame)   //triggerKeyで指定されたキーが押されたフレームだけ以下の処理を実行する
        {
            StartCoroutine(SendPatternSequence());  //SendPatternSequenceコルーチンを開始する。(振動パターンを送る)
        }
    }

    //外部スクリプトから振動パターンを送るためのpublicなメソッド
    public void SendPattern()
    {
        if (isSending)
        {
            Debug.Log("TapeTicsSenderは既に送信中です。新しいリクエストは無視されます。");
            return;
        }

        StartCoroutine(SendRun());  //SendRunコルーチンを開始する。(すでに送っている振動パターンを実行する)
        StartCoroutine(SendPatternSequence());  //振動パターンを送る(振動パターンを待機させておく)
    }

    private IEnumerator SendPatternSequence()   //振動パターンを送るためのコルーチン
    {
        isSending = true;

        string color = ledChannel.ToString();   //ledCannelの値を文字列に変換してcolorに入れる。

        // BLE.py が解釈しやすいように、
        // set → 色 → node → ノード番号 → 強度/時間列 → end → send → run
        yield return SendUdpMessage("set"); //setという文字列を送るコルーチン(SendUdpMessage(下で解説))を実行する。yield returnはこの処理が終わるまで、ここで待つという意味
        yield return WaitInterval();    //messageIntervalで指定された時間だけ待つコルーチン(WaitInterval)を実行する。

        yield return SendUdpMessage(dummyValue);    //BLE.pyの仕様上、message配列の０番目を読み飛ばすため、０番目にはダミーの値を送る必要がある
        yield return WaitInterval();

        yield return SendUdpMessage(color); //変数colorの値を送るコルーチンを実行する
        yield return WaitInterval();

        yield return SendUdpMessage("node");
        yield return WaitInterval();

        yield return SendUdpMessage(nodeNumber.ToString()); //変数nodeNumberの値を.ToString()で文字列に変換して送るコルーチンを実行する
        yield return WaitInterval();

        // 2点でパターンを作る
        // 例:
        // intensity startTime intensity endTime
        // → 一定強度で startTime から endTime まで振動するイメージ
        yield return SendUdpMessage(intensity.ToString());
        yield return WaitInterval();

        yield return SendUdpMessage(startTime.ToString("F2"));  //変数startTimeの値をToString("F2")で小数点以下2桁の文字列に変換して送るコルーチンを実行する。
        yield return WaitInterval();

        yield return SendUdpMessage(intensity.ToString());
        yield return WaitInterval();

        yield return SendUdpMessage(endTime.ToString("F2"));
        yield return WaitInterval();

        yield return SendUdpMessage("end");
        yield return WaitInterval();

        yield return SendUdpMessage("send");
        yield return WaitInterval();

        Debug.Log(
            $"TapeTics pattern sent | Dummy: {dummyValue}, Color: {color}, Node: {nodeNumber}, Intensity: {intensity}, Time: {startTime:F2} -> {endTime:F2}"
        );

        isSending = false;
    }

    private IEnumerator SendRun()   //振動パターンを送るためのコルーチン
    {
        isSending = true;

        yield return SendUdpMessage("run");
        yield return WaitInterval();

        isSending = false;
        Debug.Log("SendRun が実行された");
    }

    private IEnumerator SendUdpMessage(string message)  //引数massageは送信したい文字列
    {
        byte[] data = Encoding.UTF8.GetBytes(message);  //UDP通信はバイト配列でデータを送るため、文字列をバイト配列に変換する必要がある。

        try //一区切りを送信する
        {
            udpClient.Send(data, data.Length, udpIp, udpPort);
            Debug.Log($"Sent UDP message: {message}");
        }
        catch (SocketException e)
        {
            Debug.LogError($"UDP send failed: {e.Message}");
        }

        yield return null;
    }

    private IEnumerator WaitInterval()  //messageIntervalで指定された時間だけ待つためのコルーチン
    {
        if (messageInterval > 0f)
        {
            yield return new WaitForSeconds(messageInterval);
        }
        else
        {
            yield return null;
        }
    }

    void OnApplicationQuit()    //アプリケーションが終了するときに呼び出されるメソッド。エラー防止のため入れておく
    {
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }
    }
}
