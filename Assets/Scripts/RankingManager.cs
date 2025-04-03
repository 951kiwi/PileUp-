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
    [SerializeField, Header("�e�L�X�g�\���p�I�u�W�F�N�g�I��")]
    public Text[] RankingShowText = new Text[5];

    // �e�v���C���[�̃Z�[�u�f�[�^�p�̃N���X
    [System.Serializable]
    public class GameData
    {
        public int id;          // �v���C���[��ID
        public string date;     // �N���������b
        public float score;       // ���_
    }

    // �����̃v���C���[�̃f�[�^���Ǘ����郊�X�g
    [System.Serializable]
    public class GameDataList
    {
        public List<GameData> playerDataList = new List<GameData>();


        // date�ŏ����Ƀ\�[�g���郁�\�b�h
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

        // �f�[�^�̃��[�h�����݂�
        GameDataList loadedData = LoadData();
        loadedData.SortByScore();
        for (int i = 0; i < 3; i++)
        {
            RankingShowText[i].text = "�@�@�F"+loadedData.playerDataList[i].score +"M";
        }
        RankingShowText[3].text = "4�ʁF" + loadedData.playerDataList[3].score + "M";
        RankingShowText[4].text = "5�ʁF" + loadedData.playerDataList[4].score + "M";

        if (loadedData != null)//�f�[�^�����݂����ꍇ
        {
            foreach (var data in loadedData.playerDataList)
            {
                Debug.Log("ID: " + data.id + " | " + data.date + " ����̃X�R�A: " + data.score);
            }
        }
        else
        {
            Debug.LogError("�f�[�^�����݂��Ȃ����߃t�H���_��V�K�쐬���܂��B");
            SaveDataAppend(0);
            SaveDataAppend(0);
            SaveDataAppend(0);
            SaveDataAppend(0);
            SaveDataAppend(0);
        }

    }
    public float LoadScoreData(int id)
    {
        // �f�[�^�̃��[�h�����݂�
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
        // �f�[�^�̃��[�h�����݂�
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

    // �f�[�^�����[�h
    public GameDataList LoadData()
    {
        if (File.Exists(filePath))
        {
            // �t�@�C������JSON��ǂݍ���
            string jsonData = File.ReadAllText(filePath);

            // JSON���烊�X�g�I�u�W�F�N�g�ɕϊ�
            GameDataList dataList = JsonUtility.FromJson<GameDataList>(jsonData);
            return dataList;
        }
        else
        {
            Debug.LogWarning("�Z�[�u�f�[�^�����݂��܂���B");
            return null;
        }
    }

    //�f�[�^��ǉ����ăZ�[�u���� 
    //SaveData()�֐����g���čŏI�͕ۑ�����B

    public int nextGameDataID()
    {
        // ���݂̃f�[�^��ǂݍ���
        GameDataList dataList = LoadData();

        // �V����ID���擾����
        int newId = dataList.playerDataList.Count > 0 ? dataList.playerDataList.Max(data => data.id) + 1 : 1;

        return newId;
    }

    /// <summary>
    /// �Z�[�u�𑝂₷�ɂ͂��������g��
    /// </summary>
    /// <param name="score">�X�R�A</param>
    public void SaveDataAppend(float score)
    {
        //�P��̃f�[�^��V�K�쐬
        GameData gameData = new GameData();
        // ���݂̃f�[�^��ǂݍ���
        GameDataList dataList = LoadData();
        // �V����ID���擾����
        int newId = nextGameDataID();
        gameData.id = newId;
        gameData.date = System.DateTime.Now.ToString();
        gameData.score = score;
        dataList.playerDataList.Add(gameData);
        Debug.Log("�v���C���[�f�[�^���쐬���܂���: " + gameData);

        SaveData(dataList);
    }
    /// <summary>
    /// �Z�[�u�f�[�^���f�[�^�Ɋi�[����B
    /// </summary>
    /// <param name="dataList"></param>
    public void SaveData(GameDataList dataList)
    {
        // ���X�g��JSON�ɕϊ�
        string jsonData = JsonUtility.ToJson(dataList, true);

        // �t�@�C���ɏ�������
        File.WriteAllText(filePath, jsonData);

        Debug.Log("�����v���C���[�̃f�[�^��ۑ����܂���: " + jsonData);
    }

    

}
