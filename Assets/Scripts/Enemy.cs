using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum enumEnemy
    {
        EnemyA,
        EnemyB,
        EnemyC,
        Boss
    }

    [SerializeField] enumEnemy enemyType;

    [Header("����")]
    [SerializeField] bool boss;//�������� �Ǻ��մϴ�.
    public bool Boss => boss;
    [SerializeField] Vector2 vecCamMinMax;//��ȹ�ڰ� �����ϴ� ��ġ��, ī�޶�κ���
    bool bossStartMove = false;//ù���� ������ �ߴ���, ����
    private float startPosY;//������ ������ y��ġ
    float ratioY;//2.5?
    bool swayRight = false;//�¿� �̵���ġ ǥ��
    Animator anim;

    [SerializeField] float hp;
    float hpMax;
    [SerializeField] float speed;

    SpriteRenderer spriteRenderer;
    Sprite spriteDefault;
    [SerializeField] Sprite spriteHit;

    [SerializeField] GameObject fabExplosion;

    GameManager gameManager;

    [Header("������")]
    [SerializeField] bool hasItem = false;//�������� ������ �ִ���
    bool dropItem = false;//�������� ����ߴ���


    [Header("���� �̻��� ����")]
    //�¿�� �̵��ϸ� ��ġ���� �������� �Ѿ��� �߻�
    [SerializeField] int pattern1Count = 8;
    [SerializeField] float pattern1Reload = 1f;
    [SerializeField] GameObject pattern1Missile;
    //����ó�� ������ ������ ������ �Ѿ��� �߻�
    [SerializeField] int pattern2Count = 5;
    [SerializeField] float pattern2Reload = 0.8f;
    [SerializeField] GameObject pattern2Missile;
    //�÷��̾ ��Ȯ�� �����ؼ� ������ �Ѿ��� �߻�
    [SerializeField] int pattern3Count = 40;
    [SerializeField] float pattern3Reload = 0.3f;
    [SerializeField] GameObject pattern3Missile;

    int curPattern = 1;//���� ������� ���������
    int curpatternShootCount = 0;//���� ��� ����ߴ���
    float patternTimer;//���� Ÿ�̸�
    bool patternChange = false;//������ �ٲپ�� �ϴ� ��Ȳ����
    [SerializeField] float patternChangeTime = 1f;//���� �ٲܶ� �����̵Ǵ� �ð�

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.RemoveSpawnEnemyList(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        if (boss == false)
        {
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        hpMax = hp;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteDefault = spriteRenderer.sprite;

        if (hasItem == true)
        {
            spriteRenderer.color = new Color(0f, 0.4f, 1f, 1f);
        }

        gameManager = GameManager.Instance;
        gameManager.AddSpawnEnemyList(gameObject);
        if (boss == true)
        {
            gameManager.HitBoss((int)hp, (int)hpMax);
        }

        startPosY = transform.position.y;
    }

    void Update()
    {
        moving();
        shoot();
    }

    private void moving()
    {
        if (boss == false)
        {
            transform.position += -transform.up * Time.deltaTime * speed;
        }
        else
        {
            if (bossStartMove == false)
            {
                bossStartMoving();
            }
            else
            {
                bossSwayMoving();
            }
        }
    }

    private void shoot()
    {
        if (boss == false || bossStartMove == false) return;

        patternTimer += Time.deltaTime;
        if (patternChange == true)//������ �ٲ� ��� �����ִ� �ð�, ������ ������ Ÿ�̹�
        {
            if (patternTimer >= patternChangeTime)
            {
                patternTimer = 0;
                patternChange = false;
            }
            return;
        }

        switch (curPattern)
        {
            case 1://�������� �Ѿ��� �߻�
                if (patternTimer >= pattern1Reload)
                {
                    patternTimer = 0.0f;
                    shootStraight();
                    if (curpatternShootCount >= pattern1Count)
                    {
                        curPattern++;
                        //curPattern = 2;

                        //int rand = Random.Range(1, 4);//1~3
                        //while (curPattern == rand)
                        //{
                        //    rand = Random.Range(1, 4);//1~3
                        //}
                        //curPattern = rand;
                        patternChange = true;
                        curpatternShootCount = 0;
                    }
                }
                break;
            case 2:
                if (patternTimer >= pattern2Reload)
                {
                    patternTimer = 0.0f;
                    shootShotgun();
                    if (curpatternShootCount >= pattern2Count)
                    {
                        curPattern++;
                        patternChange = true;
                        curpatternShootCount = 0;
                    }
                }
                break;
            case 3:
                if (patternTimer >= pattern3Reload)
                {
                    patternTimer = 0.0f;
                    shootGatling();
                    if (curpatternShootCount >= pattern3Count)
                    {
                        curPattern = 1;
                        patternChange = true;
                        curpatternShootCount = 0;
                    }
                }
                break;
        }
    }

    private void shootStraight()
    {
        curpatternShootCount++;
        createMissile(transform.position, new Vector3(0f, 0f, 180f), pattern1Missile);
        createMissile(transform.position + new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 180f), pattern1Missile);
        createMissile(transform.position + new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 180f), pattern1Missile);
    }

    private void shootShotgun()
    {
        curpatternShootCount++;
        createMissile(transform.position, new Vector3(0f, 0f, 180f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f - 15f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f + 15f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f - 30f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f + 30f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f -45f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f +45f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f -60f), pattern2Missile);
        createMissile(transform.position, new Vector3(0f, 0f, 180f +60f), pattern2Missile);
    }

    private void shootGatling()
    {
        curpatternShootCount++;
        Transform trsPlayer = gameManager.GetPlayerTransform();
        if (trsPlayer == null) return;
        Vector3 playerPos = trsPlayer.position;

        float angle = Quaternion.FromToRotation(Vector3.up, playerPos - transform.position).eulerAngles.z;
        createMissile(transform.position, new Vector3(0,0,angle), pattern3Missile);
    }

    private void createMissile(Vector3 _pos, Vector3 _rot, GameObject _instObj)
    {
        GameObject objMissile = Instantiate(_instObj, _pos, Quaternion.Euler(_rot));
    }

    private void bossStartMoving()
    {
        ratioY += Time.deltaTime * 0.5f;
        if (ratioY >= 1.0f)
        {
            bossStartMove = true;
        }

        Vector3 curPos = transform.position;
        //curPos = Vector3.Lerp(new Vector3(0f,startPosY,0f), new Vector3(0f, 2.5f, 0f), ratioY);
        curPos.y = Mathf.SmoothStep(startPosY, 2.5f, ratioY);
        transform.position = curPos;
    }

    private void bossSwayMoving()
    {
        if (swayRight)
        {
            transform.position += transform.right * Time.deltaTime * speed;
        }
        else
        {
            transform.position -= transform.right * Time.deltaTime * speed;
        }
        checkMoveLimit();
    }

    private void checkMoveLimit()
    {
        Vector3 curPos = Camera.main.WorldToViewportPoint(transform.position);
        if (swayRight == false && curPos.x < vecCamMinMax.x)
        {
            swayRight = true;
        }
        else if (swayRight == true && curPos.x > vecCamMinMax.y)
        {
            swayRight = false;
        }
    }

    public void Hit(float _damage)
    {
        if (boss == true && bossStartMove == false) return;

        hp -= _damage;

        if (hp <= 0)
        {
            destroyFunction();

            if ((hasItem == true || boss == true) && dropItem == false)//���� �������� �����ϰ� �ְ� ������� �ʾҴٸ�
            {
                dropItem = true;
                GameManager.Instance.DropItem(transform.position);
            }
            gameManager.AddKillCount();

            if (boss == true)
            {
                gameManager.BossKill();
            }

            gameManager.DestroyEnemy(enemyType);
        }
        else
        {
            if (boss == false)
            {
                spriteRenderer.sprite = spriteHit;
                Invoke("setSpriteDefault", 0.1f);
            }
            else//�����϶���
            {
                if (checkAnim() == true)
                {
                    anim.SetTrigger("Hit");
                }
                gameManager.HitBoss((int)hp, (int)hpMax);
            }
        }
    }

    private bool checkAnim()
    {
        bool isNameBossHit = anim.GetCurrentAnimatorStateInfo(0).IsName("BossHit");//�̸��� ������Ʈ��� true �ƴ϶�� false 
        //anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f // 0~1 1�̶�� �ִϸ��̼��� ������ �����Ǿ���
        return !isNameBossHit;
    }

    public void DestroyOnBodySlam()
    {
        if (boss == false)
        {
            destroyFunction();
        }
    }

    private void destroyFunction()
    {
        Destroy(gameObject);

        GameObject expObj = Instantiate(fabExplosion, transform.position, Quaternion.identity);
        Explosion expSc = expObj.GetComponent<Explosion>();
        expSc.SetSize(spriteDefault.rect.width);
    }

    private void setSpriteDefault()
    {
        spriteRenderer.sprite = spriteDefault;
    }

    public void SetHaveItem()
    {
        hasItem = true;
    }
}
