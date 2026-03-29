using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 使用 TextMeshPro
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class MobileClient : MonoBehaviour
{
    [Header("UI 參考")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField messageInput;
    public TextMeshProUGUI statusText;

    private FirebaseAuth auth;
    private DatabaseReference dbReference;
    private FirebaseUser currentUser;

    void Start()
    {
        // 初始化 Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            statusText.text = "Firebase 初始化成功。";
        });
    }

    // 註冊帳號綁定到按鈕
    public void RegisterUser()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "請輸入 Email 和密碼。";
            return;
        }

        statusText.text = "註冊中...";
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                statusText.text = "註冊失敗：" + task.Exception.InnerExceptions[0].Message;
                return;
            }

            AuthResult result = task.Result;
            currentUser = result.User;
            statusText.text = $"註冊成功！使用者：{currentUser.Email}";
        });
    }

    // 登入帳號綁定到按鈕
    public void LoginUser()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        statusText.text = "登入中...";
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                statusText.text = "登入失敗：" + task.Exception.InnerExceptions[0].Message;
                return;
            }

            AuthResult result = task.Result;
            currentUser = result.User;
            statusText.text = $"登入成功！歡迎：{currentUser.Email}";
        });
    }

    // 傳送訊息綁定到按鈕
    public void SendMessage()
    {
        // 1. 檢查 UI 元件是否有綁定 (防止 Inspector 忘記拖曳)
        if (messageInput == null || statusText == null)
        {
            Debug.LogError("【錯誤】UI 元件未綁定！請檢查 Inspector 面板。");
            return;
        }

        // 2. 檢查 Firebase 資料庫是否初始化完成
        if (dbReference == null)
        {
            statusText.text = "資料庫尚未連線，請稍候再試。";
            Debug.LogWarning("dbReference 為 null，Firebase 可能尚未初始化完成。");
            return;
        }

        // 3. 檢查使用者是否已登入
        if (currentUser == null)
        {
            statusText.text = "請先登入才能傳送訊息！";
            return;
        }

        string msg = messageInput.text;
        if (string.IsNullOrEmpty(msg)) return;

        // 建立訊息資料結構
        MessageData newMsg = new MessageData(currentUser.Email, msg);
        string json = JsonUtility.ToJson(newMsg);

        // 傳送至資料庫
        dbReference.Child("Messages").Push().SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                statusText.text = "傳送失敗。";
                Debug.LogError("Firebase 寫入失敗: " + task.Exception);
            }
            else
            {
                statusText.text = "訊息傳送成功！";
                messageInput.text = ""; // 清空輸入框
            }
        });
    }

}

// 用來序列化成 JSON 的資料結構
[System.Serializable]
public class MessageData
{
    public string senderEmail;
    public string content;

    public MessageData(string email, string msg)
    {
        this.senderEmail = email;
        this.content = msg;
    }
}