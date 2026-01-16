using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class ToggleSwitch : MonoBehaviour,IPointerClickHandler
{
    [SerializeField] Image backgroundImage;
    [SerializeField] Color32 onColor;
    [SerializeField] Color32 offColor;
    [SerializeField] RectTransform handleTransform;
    [SerializeField] float handlePos;
    [SerializeField] float MoveSpeed;

    [SerializeField] UnityEvent onCustomEvent;
    [SerializeField] UnityEvent offCustomEvent;
    [SerializeField] BoolReference targetBool; // インスペクターから対象スクリプト＋フィールド名を指定

    bool isOn;

    public void OnPointerClick(PointerEventData eventData)
    {
        isOn = !isOn;
        UPdateUI();  // UI更新を呼ぶ
        Onclick();
    }
    // Start is called before the first frame update
    void Start()
    {
        targetBool.Init();
        isOn = targetBool.GetValue();
        UPdateUI();  // UI更新を呼ぶ
    }

    void UPdateUI()
    {
        Vector2 endPos;
        if (isOn)
        {
            endPos = new Vector2(handlePos,handleTransform.anchoredPosition.y);
            backgroundImage.color = onColor;

        }
        else
        {
            endPos = new Vector2(-handlePos, handleTransform.anchoredPosition.y);
            backgroundImage.color = offColor;
        }
        StartCoroutine(SmoothMode(handleTransform.anchoredPosition, endPos));
    }
    public void TriggerEventON()
    {
        Debug.Log($"イベント呼び出し！ value=ON");
        onCustomEvent?.Invoke();
    }
    public void TriggerEventOFF()
    {
        Debug.Log($"イベント呼び出し！ value=OFF");
        offCustomEvent?.Invoke();
    }

    void Onclick()
    {
        if (isOn)
        {
            TriggerEventON();
            Debug.Log("トリガーボタンON");
        }
        else
        {
            TriggerEventOFF();
            Debug.Log("トリガーボタンOFF");
        }
    }
    IEnumerator SmoothMode(Vector2 startPos,Vector2 endPos)
    {
        float elapsedTime = 0;
        while (elapsedTime < 1)
        {
            handleTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsedTime);
            elapsedTime += Time.deltaTime * MoveSpeed;
            yield return null;
        }
        // 最終位置を保証
        handleTransform.anchoredPosition = endPos;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    
}
