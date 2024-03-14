using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;

public enum SceneNums
{
    MainScene,
    PlayScene,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;//�ƹ��̸��̳� �����ŵ� ��

    [Header("�������")]
    [SerializeField] bool isSpawn = false;
    [SerializeField] List<GameObject> listEnemy;//������ ����
    List<GameObject> listSpawnEnemy = new List<GameObject>();//������ �����

    [SerializeField, Range(0.1f, 2.0f)] float spawnTime = 1.0f;
    float sTimer = 0.0f;//����Ÿ�̸� 
    Transform trsSpawnPoint;

    [Header("������� ī�޶󸮹�")]
    [SerializeField] Vector2 vecCamMinMax;//��ȹ�ڰ� �����ϴ� ��ġ��, ī�޶�κ���
    Vector2 vecSpawnLimit;//���� ������ ���� ���� ���� ��ġ��, ����������

    [Header("�����۵��")]
    [SerializeField, Range(0.0f, 100.0f)] float itemDropRate = 0.0f;//0.0~100.0f
    [SerializeField] List<GameObject> listItem;

    Camera mainCam;

    [SerializeField] GameObject objPlayer;

    [Header("������")]
    [SerializeField] Slider slider;
    [SerializeField] Image sliderFill;
    [SerializeField] TMP_Text textTimer;

    float bossSpawnTime = 60f;
    [SerializeField] float gameTime = 0f;
    bool spawnBoss = false;
    float colorRatio = 0;//0�϶� �Ϲݻ���, 1�϶��� ����ü�»���

    [SerializeField] Color colorTimer;
    [SerializeField] Color colorBossHp;

    [SerializeField] int killCountBossSpawn = 60;
    [SerializeField] int killCount = 0;
    [Header("����")]
    [SerializeField] TMP_Text textScore;//���� �ؽ�Ʈ
    int score;//��������

    [Header("���ӿ����޴�")]
    [SerializeField] GameObject objGameoverMenu;
    [SerializeField] TMP_Text textGameover;
    [SerializeField] TMP_InputField iFGameover;
    [SerializeField] Button btnGameover;
    int rank = -1;
    string keyRankData = "rankData";

    public class cRank
    {
        public int score = 0;
        public string name = "";
    }

    List<cRank> listRank = new List<cRank>();//0~9

    private void Awake()
    {
        if(Tool.IsEnterFirstScene == false)
        {
            SceneManager.LoadScene((int)SceneNums.MainScene);
            return;
        }
        if (Instance == null)//�̱���, �̱���, ����������, ���ϳ��� �����ϴ� ��ũ��Ʈ ������ �Ҷ�
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        initGameoverMenu();
        initRank();
    }

    //�÷��̾ ��������� ���������� ��ũ�κ��� ������� �������� �Լ�
    private int GetRank()
    {
        int count = listRank.Count;
        for (int iNum = 0; iNum < count; iNum++)
        {
            cRank rank = listRank[iNum];
            if (rank.score < score)//��ũ�� ������ ���� �������� ���ٸ� ���ڸ��� ���� ������ ����
            {
                return iNum + 1;
            }
        }
        return -1;
    }

    private void initGameoverMenu()
    {
        if (objGameoverMenu.activeSelf == true)//������Ʈ�� active true�Ǿ��־��ٸ�
        {
            objGameoverMenu.SetActive(false);//�����Ե�
        }

        btnGameover.onClick.AddListener(saveAndNextScene);
    }

    private void initRank()
    {
        string rankValue = PlayerPrefs.GetString(keyRankData, string.Empty);//"";
        if (rankValue == string.Empty)
        {
            int count = 10;
            for (int iNum = 0; iNum < count; iNum++)
            {
                listRank.Add(new cRank());
            }

            rankValue = JsonConvert.SerializeObject(listRank);
            PlayerPrefs.SetString(keyRankData, rankValue);
        }
        else//""�� �ƴϾ��ٸ�
        {
            listRank = JsonConvert.DeserializeObject<List<cRank>>(rankValue);
        }
    }

    private void saveAndNextScene()
    {
        //��ũ�� ������ٸ� name�� score�� rankList�� �������ְ� 11���� ���� 
        if (rank != -1)
        {
            cRank data = new cRank();
            data.score = score;
            data.name = iFGameover.text;

            listRank.Insert(rank - 1, data);
            listRank.RemoveAt(listRank.Count - 1);

            string saveValue = JsonConvert.SerializeObject(listRank);
            PlayerPrefs.SetString(keyRankData, saveValue);//����Ƽ�� �����
        }

        SceneManager.LoadSceneAsync((int)SceneNums.MainScene);//���ξ� �̵�
    }

    private void Start()
    {
        mainCam = Camera.main;
        trsSpawnPoint = transform.Find("SpawnPoint");

        vecSpawnLimit.x = mainCam.ViewportToWorldPoint(
            new Vector3(vecCamMinMax.x, 0f)
            ).x;

        vecSpawnLimit.y = mainCam.ViewportToWorldPoint(
            new Vector3(vecCamMinMax.y, 0f)
            ).x;

        initSlider();
    }

    void Update()
    {
        checkSpawn();
        checkTime();
    }

    private void checkSpawn()//���⸦ ��ȯ�ص� �Ǵ��� üũ
    {
        if (isSpawn == false || spawnBoss == true) return;

        sTimer += Time.deltaTime;
        if (sTimer >= spawnTime && gameTime < bossSpawnTime - 1)
        {
            sTimer = 0.0f;
            createEnemy();//�������
        }
    }

    private void checkTime()
    {
        if (spawnBoss == true)
        {
            if (sliderFill.color != colorBossHp)
            {
                if (colorRatio != 1.0f)
                {
                    colorRatio += Time.deltaTime;
                    if (colorRatio > 1.0f)
                    {
                        colorRatio = 1.0f;
                    }
                }

                sliderFill.color = Color.Lerp(colorTimer, colorBossHp, colorRatio);
            }
        }
        else
        {
            //�� ����
            if (sliderFill.color != colorTimer)
            {
                if (colorRatio != 0.0f)
                {
                    colorRatio -= Time.deltaTime;
                    if (colorRatio < 0.0f)
                    {
                        colorRatio = 0.0f;
                    }
                }

                sliderFill.color = Color.Lerp(colorTimer, colorBossHp, colorRatio);
            }

            gameTime += Time.deltaTime;

            if (gameTime > bossSpawnTime)
            {
                gameTime = bossSpawnTime;
                spawnBoss = true;
                isSpawn = false;

                //��� ������� ����
                clearAllEnemy();
                //������ ����
                createBoss();
            }
            modifySlider();
        }
    }

    public void BossKill()
    {
        spawnBoss = false;
        isSpawn = true;
        initTimer();
    }

    public void HitBoss(int _curHp, int _maxHp)
    {
        textTimer.text = $"{_curHp.ToString("D4")} / {_maxHp.ToString("D4")}";
        slider.maxValue = _maxHp;
        slider.value = _curHp;
    }

    private void initTimer()
    {
        gameTime = 0;
        bossSpawnTime += 60;

        if (spawnTime != 0.1f)
        {
            spawnTime -= 0.1f;
        }

        initSlider();
    }

    private void checkKillCount()
    {
        if (spawnBoss == false && killCount == killCountBossSpawn)
        {
            spawnBoss = true;

            clearAllEnemy();
            createBoss();
        }
        modifySlider();
    }

    private void initSlider()
    {
        slider.maxValue = bossSpawnTime;
        //slider.maxValue = killCountBossSpawn;
        slider.value = 0;
        textTimer.text = $"{((int)slider.value).ToString("D2")} / {((int)slider.maxValue).ToString("D2")}";
        //textTimer.text = $"{0.ToString("D2")} / {((int)slider.maxValue).ToString("D2")}";
    }

    private void modifySlider()
    {
        slider.value = gameTime;
        textTimer.text = $"{((int)gameTime).ToString("D2")} / {((int)bossSpawnTime).ToString("D2")}";

        //slider.value = killCount;
        //textTimer.text = $"{killCount.ToString("D2")} / {killCountBossSpawn.ToString("D2")}";
    }

    public void AddKillCount()
    {
        return;//ųī��Ʈ�� ����ϽǶ��� �������ּ���

        killCount++;
        if (killCount > killCountBossSpawn)
        {
            killCount = killCountBossSpawn;
        }

        checkKillCount();
    }

    private void clearAllEnemy()
    {
        int count = listSpawnEnemy.Count;
        for (int iNum = count - 1; iNum > -1; --iNum)
        {
            Destroy(listSpawnEnemy[iNum]);
        }
        listSpawnEnemy.Clear();
    }

    private void createBoss()
    {
        GameObject go = listEnemy[listEnemy.Count - 1];
        Instantiate(go, trsSpawnPoint.position, Quaternion.identity);
    }

    private void createEnemy()//���⸦ �����մϴ�.
    {
        float rand = Random.Range(0.0f, 100.0f);
        GameObject objEnemy = listEnemy[0];
        if (rand < 50.0f)
        {
            objEnemy = listEnemy[0];
        }
        else if (rand < 75.0f)
        {
            objEnemy = listEnemy[1];
        }
        else
        {
            objEnemy = listEnemy[2];
        }

        Vector3 newPos = trsSpawnPoint.position;
        newPos.x = Random.Range(vecSpawnLimit.x, vecSpawnLimit.y);
        GameObject go = Instantiate(objEnemy, newPos, Quaternion.identity);

        //listSpawnEnemy.Add(go);//���� ����Ʈ�� ����ϴ� ���

        float rate = Random.Range(0.0f, 100.0f);
        if (rate <= itemDropRate)
        {
            Enemy goSc = go.GetComponent<Enemy>();
            goSc.SetHaveItem();
        }
    }

    public void DropItem(Vector3 _pos)
    {
        int raniNum = Random.Range(0, listItem.Count);//0~1
        GameObject obj = listItem[raniNum];
        Instantiate(obj, _pos, Quaternion.identity);
    }

    public Transform GetPlayerTransform()
    {
        if (objPlayer == null)
        {
            return null;
        }
        else
        {
            return objPlayer.transform;
        }
    }

    public GameObject GetPlayerObject()
    {
        return objPlayer;
    }

    public void AddSpawnEnemyList(GameObject _value)
    {
        //if (listSpawnEnemy.Exists((x) => x == _value) == false)
        //{ 
        //    listSpawnEnemy.Add(_value);
        //}
        listSpawnEnemy.Add(_value);
    }

    public void RemoveSpawnEnemyList(GameObject _value)
    {
        listSpawnEnemy.Remove(_value);
    }

    public void DestroyEnemy(Enemy.enumEnemy _value)
    {
        switch (_value)
        {
            case Enemy.enumEnemy.EnemyA:
                score += 1;
                break;
            case Enemy.enumEnemy.EnemyB:
                score += 2;
                break;
            case Enemy.enumEnemy.EnemyC:
                score += 3;
                break;
            case Enemy.enumEnemy.Boss:
                score += 4;
                break;
        }

        textScore.text = score.ToString("D8");
    }

    public void GameOver()//�÷��̾ ��������� ����
    {
        rank = GetRank();//��ũ�� ���� �ʾҴٸ� -1;
        //��ũ�� ����ٸ� �̸��� ������ ����Ҽ� �ִ� �޴���
        if (rank != -1)
        {
            textGameover.text = $"{rank} �� �޼�!";
            iFGameover.gameObject.SetActive(true);
        }
        //��ũ�� ���� �ʾҴٸ� Ȯ�θ޴���
        else//-1�϶�
        {
            textGameover.text = "������ ���� ���� ���߽��ϴ�.";
            iFGameover.gameObject.SetActive(false);
        }
        objGameoverMenu.gameObject.SetActive(true);
        iFGameover.Select();
    }
}
