using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHp : MonoBehaviour
{
    private GameManager gameManager;
    private Image Hp;
    private Image Effect;
    private Transform trsPlayer;
    [SerializeField] private float curPlayerHp; 
    [SerializeField] private float maxPlayerHp; 

    private void Awake()
    {
        Hp = transform.Find("Hp").GetComponent<Image>();
        Effect = transform.Find("Effect").GetComponent<Image>();
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        trsPlayer = gameManager.GetPlayerTransform(); 
    }

    
    void Update()
    {
        checkPostion();
        checkPlayerHp();
        isDestorying();
    }

    private void checkPostion()
    {
        if (trsPlayer == null) return;

        Vector3 movePos = trsPlayer.position;//�÷��̾��� ��ġ
        movePos.y -= 0.7f;
        transform.position = movePos;
    }
    private void checkPlayerHp()
    {
        if (Hp.fillAmount < Effect.fillAmount)//�������� �Ծ� ü���� ����������
        {
            Effect.fillAmount -= Time.deltaTime * 0.5f; //����Ʈ�� ���� �κ��� ���ҵ�

            if (Effect.fillAmount <= Hp.fillAmount)//���� Hp������ �۾��� �����ٸ�
            {
                Effect.fillAmount = Hp.fillAmount;//����Ʈ�� Hp������ ����
            }
        }
        else if (Hp.fillAmount > Effect.fillAmount)//ü���� ȸ��������
        {
            Effect.fillAmount = Hp.fillAmount;//����Ʈ�� Hp������ ����
        }

        float value = curPlayerHp / maxPlayerHp;//���� hp�� ����
        if (Hp.fillAmount > value) //�������� ����
        {
            Hp.fillAmount -= Time.deltaTime;
            if (Hp.fillAmount <= value)
            {
                Hp.fillAmount = value;
            }
        }
        else if (Hp.fillAmount < value)//ȸ�� �Ǿ�����
        {
            Hp.fillAmount += Time.deltaTime;
            if (Hp.fillAmount >= value)
            {
                Hp.fillAmount = value;
            }
        }
    }

    private void isDestorying()
    {
        if (Effect.fillAmount <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerHp(int _hp, int _maxHp)
    {
        curPlayerHp = _hp;
        maxPlayerHp = _maxHp;
    }
}
