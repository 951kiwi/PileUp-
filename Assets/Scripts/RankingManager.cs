using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;




public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance { get; private set; }
    private string filePath;
    [SerializeField, Header("テキスト表示用オブジェクト選択")]
    public Text[] RankingShowText = new Text[5];

    // 各プレイヤーのセーブデータ用のクラス
    [System.Serializable]
    public class GameData
    {
        public int id;          // プレイヤーのID
        public string date;     // 年月日時分秒
        public float score;       // 得点
    }

    // 複数のプレイヤーのデータを管理するリスト
    [System.Serializable]
    public class GameDataList
    {
        public List<GameData> playerDataList = new List<GameData>();


        // dateで昇順にソートするメソッド
        public void SortByScore()
        {
            playerDataList.Sort((data1, data2) => data2.score.CompareTo(data1.score));
        }
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "saveData.json");

        // データのロードを試みる
        GameDataList loadedData = LoadData();
        loadedData.SortByScore();
        for (int i = 0; i < 3; i++)
        {
            RankingShowText[i].text = "　　："+loadedData.playerDataList[i].score +"M";
        }
        RankingShowText[3].text = "4位：" + loadedData.playerDataList[3].score + "M";
        RankingShowText[4].text = "5位：" + loadedData.playerDataList[4].score + "M";

        if (loadedData != null)//データが存在した場合
        {
            foreach (var data in loadedData.playerDataList)
            {
                Debug.Log("ID: " + data.id + " | " + data.date + " さんのスコア: " + data.score);
            }
        }
        else
        {
            Debug.LogError("データが存在しないためフォルダを新規作成します。");
            SaveDataAppend(0);
            SaveDataAppend(0);
            SaveDataAppend(0);
            SaveDataAppend(0);
            SaveDataAppend(0);
        }

    }
    public float LoadScoreData(int id)
    {
        // データのロードを試みる
        GameDataList loadedData = LoadData();
        foreach (var data in loadedData.playerDataList)
        {
            if (data.id == id)
            {
                return data.score;
            }
        }
        return 0;
    }
    public string LoadDateData(int id)
    {
        // データのロードを試みる
        GameDataList loadedData = LoadData();
        foreach (var data in loadedData.playerDataList)
        {
            if (data.id == id)
            {
                return data.date;
            }
        }
        return "";
    }

    // データをロード
    public GameDataList LoadData()
    {
        if (File.Exists(filePath))
        {
            // ファイルからJSONを読み込み
            string jsonData = File.ReadAllText(filePath);

            // JSONからリストオブジェクトに変換
            GameDataList dataList = JsonUtility.FromJson<GameDataList>(jsonData);
            return dataList;
        }
        else
        {
            Debug.LogWarning("セーブデータが存在しません。");
            return null;
        }
    }

    //データを追加してセーブする 
    //SaveData()関数を使って最終は保存する。

    public int nextGameDataID()
    {
        // 現在のデータを読み込み
        GameDataList dataList = LoadData();

        // 新しいIDを取得する
        int newId = dataList.playerDataList.Count > 0 ? dataList.playerDataList.Max(data => data.id) + 1 : 1;

        return newId;
    }

    /// <summary>
    /// セーブを増やすにはこっちを使う
    /// </summary>
    /// <param name="score">スコア</param>
    public void SaveDataAppend(float score)
    {
        //単一のデータを新規作成
        GameData gameData = new GameData();
        // 現在のデータを読み込み
        GameDataList dataList = LoadData();
        // 新しいIDを取得する
        int newId = nextGameDataID();
        gameData.id = newId;
        gameData.date = System.DateTime.Now.ToString();
        gameData.score = score;
        dataList.playerDataList.Add(gameData);
        Debug.Log("プレイヤーデータを作成しました: " + gameData);

        SaveData(dataList);
    }
    /// <summary>
    /// セーブデータをデータに格納する。
    /// </summary>
    /// <param name="dataList"></param>
    public void SaveData(GameDataList dataList)
    {
        // リストをJSONに変換
        string jsonData = JsonUtility.ToJson(dataList, true);

        // ファイルに書き込み
        File.WriteAllText(filePath, jsonData);

        Debug.Log("複数プレイヤーのデータを保存しました: " + jsonData);
    }

    

}
