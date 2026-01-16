using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        filePath = Path.Combine(Application.persistentDataPath, "saveData.json");
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
        // データのロードを試みる
        GameDataList loadedData = LoadData();

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
        }

    }
    void TitleIsRankingShow()
    {
        // データのロードを試みる
        GameDataList loadedData = LoadData();
        if (loadedData != null) {
            loadedData.SortByScore();
        }
        
        for (int i = 0; i < 3; i++)
        {
            RankingShowText[i].text = "　　：" + loadedData.playerDataList[i].score + "M";
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
        }
    }
    void OnEnable()
    {
        // シーン読み込み時のイベントに登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // イベント解除（安全のため）
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameTitle")
        {
            // シーン遷移後にUIを再取得（ヒエラルキー上のオブジェクト名と一致している必要あり）
            RankingShowText = new Text[5];
            RankingShowText[0] = GameObject.Find("1king_text")?.GetComponent<Text>();
            RankingShowText[1] = GameObject.Find("2king_text")?.GetComponent<Text>();
            RankingShowText[2] = GameObject.Find("3king_text")?.GetComponent<Text>();
            RankingShowText[3] = GameObject.Find("Text4")?.GetComponent<Text>();
            RankingShowText[4] = GameObject.Find("Text5")?.GetComponent<Text>();

            // 念のため null チェック
            for (int i = 0; i < RankingShowText.Length; i++)
            {
                if (RankingShowText[i] == null)
                {
                    Debug.LogError($"RankingShowText[{i}] が null です。");
                }
            }
            TitleIsRankingShow();
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
    /// <summary>
    /// 順位を取得する
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    public int GetNowRanking(float score)
    {
        GameDataList dataList = LoadData();

        if (dataList == null || dataList.playerDataList.Count == 0)
        {
            Debug.LogWarning("ランキングデータが存在しません。");
            return 0; // 無効値
        }

        // スコアが高い順に並べる
        var sortedList = dataList.playerDataList
            .OrderByDescending(d => d.score)
            .ToList();

        // 指定スコアより高いスコアの数をカウント
        int rank = 1;
        foreach (var data in sortedList)
        {
            if (data.score > score)
            {
                rank++;
            }
            else
            {
                break;
            }
        }

        return rank;
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
        int newId = 0;
        try
        {
            // 新しいIDを取得する
            newId = dataList.playerDataList.Count > 0 ? dataList.playerDataList.Max(data => data.id) + 1 : 1;
        }
        catch (Exception e) { 
        }

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
        if (dataList == null)
        {
            dataList = new GameDataList(); // ★ ここで初期化！
        }
        // 新しいIDを取得する
        int newId = nextGameDataID();
        gameData.id = newId;
        gameData.date = System.DateTime.Now.ToString();
        gameData.score = score;
        dataList.playerDataList.Add(gameData);
        Debug.Log("プレイヤーデータを作成しました: ID:{gameData.id} date:{gameData.date} score:{gameData.score}");

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

        Debug.Log("プレイヤーのデータを保存しました: ");
    }



}
