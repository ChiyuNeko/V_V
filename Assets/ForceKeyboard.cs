using UnityEngine;
using UnityEngine.EventSystems;

public class ForceKeyboard : MonoBehaviour, IPointerClickHandler
{
    private TouchScreenKeyboard keyboard;

    // 當玩家點擊這個 UI 物件時觸發
    public void OnPointerClick(PointerEventData eventData)
    {
        // 強制呼叫手機原生鍵盤
        if (TouchScreenKeyboard.isSupported)
        {
            keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        }
    }
}