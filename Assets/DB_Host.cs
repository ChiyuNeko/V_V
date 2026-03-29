using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class HostClient : MonoBehaviour
{
    [Header("UI 參考")]
    public TextMeshProUGUI chatDisplay;
    public TextMeshProUGUI statusText;

    private DatabaseReference dbReference;
    
    // 用來存放需要回到主執行緒執行的動作
    private readonly Queue<System.Action> mainThreadActions = new Queue<System.Action>();

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            statusText.text = "主機端：Firebase 初始化成功，正在監聽訊息...";
            
            StartListeningForMessages();
        });
    }

    void Update()
    {
        // 每一幀檢查佇列中是否有需要執行的 UI 更新動作
        // 使用 lock 確保多執行緒寫入與讀取時的安全
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                System.Action action = mainThreadActions.Dequeue();
                action?.Invoke();
            }
        }
    }

    void StartListeningForMessages()
    {
        dbReference.Child("Messages").ChildAdded += HandleMessageAdded;
    }

    void HandleMessageAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        string json = args.Snapshot.GetRawJsonValue();
        MessageData receivedMsg = JsonUtility.FromJson<MessageData>(json);

        string formattedMessage = $"[{receivedMsg.senderEmail}]: {receivedMsg.content}\n";
        
        // 將 UI 更新的動作打包，放進佇列中，讓 Update() 去執行
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(() => {
                if (chatDisplay != null)
                {
                    chatDisplay.text += formattedMessage;
                }
            });
        }
    }

    void OnDestroy()
    {
        if (dbReference != null)
        {
            dbReference.Child("Messages").ChildAdded -= HandleMessageAdded;
        }
    }
}
