using UnityEngine;
using System.Reflection;

[System.Serializable]
public class BoolReference
{
    [SerializeField] MonoBehaviour targetScript;   // 対象のスクリプト
    [SerializeField] string fieldName;             // bool フィールド or プロパティ名

    FieldInfo fieldInfo;
    PropertyInfo propertyInfo;

    // 初期化
    public void Init()
    {
        if (targetScript == null || string.IsNullOrEmpty(fieldName)) return;

        var type = targetScript.GetType();

        fieldInfo = type.GetField(fieldName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo == null)
        {
            propertyInfo = type.GetProperty(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }

    // 値を取得
    public bool GetValue()
    {
        if (fieldInfo != null) return (bool)fieldInfo.GetValue(targetScript);
        if (propertyInfo != null) return (bool)propertyInfo.GetValue(targetScript);
        return false;
    }

    // 値を設定
    public void SetValue(bool value)
    {
        if (fieldInfo != null) fieldInfo.SetValue(targetScript, value);
        if (propertyInfo != null) propertyInfo.SetValue(targetScript, value);
    }
}
