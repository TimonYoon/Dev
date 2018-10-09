using UnityEngine;
using System.Collections;

public abstract class LootObjectBase : BattleGroupElement, ILootable
{

    /// <summary> 담겨있는 것(ex: 경험치)의 값 </summary>
    public double value;

    public float lifeTime = 3f;

    Vector3 spawnPos;
    public Vector2 moveDest { get; set; }

    

    public void Init(BattleGroup battleGroup, double value, Vector3 spawnPos)
    {
        SetBattleGroup(battleGroup);

        this.value = value;
        this.spawnPos = spawnPos;

        //해당 배틀 그룹에 콜백 등록
        battleGroup.onRestartBattle += OnRestartBattle;
        battleGroup.onChangedStage += OnChangedStage;
        battleGroup.onChangedBattlePhase += OnChangedBattlePhase;

        //스폰 연출 시작
        coroutineSpawn = StartCoroutine(Spawn());
    }

    void Start()
    {
        InitMoveDest();        
    }

    public abstract void InitMoveDest();

    void OnDisable()
    {
        if (!battleGroup)
            return;

        //디스폰 될 때 클백들 해제. 다른 배틀 그룹에서 쓸 수 있기 때문
        battleGroup.onRestartBattle -= OnRestartBattle;
        battleGroup.onChangedStage -= OnChangedStage;
        battleGroup.onChangedBattlePhase -= OnChangedBattlePhase;
    }

    void OnChangedBattlePhase(BattleGroup b)
    {
        //페이드 아웃 될 때 빨아들임
        if (b.battlePhase == BattleGroup.BattlePhase.FadeOut)
            Loot();
    }

    void OnChangedStage(BattleGroup b)
    {
        Loot();
    }

    void OnRestartBattle(BattleGroup b)
    {
        Despawn();
    }

    public void Loot()
    {
        //스폰 연출 중이면 중단
        if (coroutineSpawn != null)
        {
            StopCoroutine(coroutineSpawn);
            coroutineSpawn = null;
        }

        //목적지로 빨려 들어감
        if (coroutineMove == null)
            coroutineMove = StartCoroutine(Move());
    }

    Coroutine coroutineSpawn = null;
    IEnumerator Spawn()
    {
        transform.position = spawnPos;

        //좌우 이동
        float x = Random.Range(0, 2) == 0 ? Random.Range(-4f, -15f) : Random.Range(4f, 15f);
        float speedX = Random.Range(0.5f, 0.5f);
        float randomY = Random.Range(-1f, 1f);

        //처음 위로 튀겨올라가는 정도
        float height = Random.Range(5f, 8f);

        //바운스 주기
        float frequency = Random.Range(2f, 3f);

        float startTime = Time.time;
        int bounceCount = 0;
        while (Time.time - startTime < 2f + lifeTime)
        {
            float elapsedTime = Time.time - startTime;
            float y = Mathf.Sin(elapsedTime * frequency - bounceCount * Mathf.PI) * height;
            if (y < 0)
            {
                y *= -1f;
                bounceCount++;
            }

            //튕길 때 마다 속도 감소
            float bouncePower = Mathf.Pow(0.5f, bounceCount);
            if (bouncePower < 0.1f)
                bouncePower = 0f;

            y *= bouncePower;

            float posX = Mathf.Lerp(transform.position.x, spawnPos.x + x, speedX * Time.deltaTime);

            transform.position = Vector3.Lerp(spawnPos, spawnPos/* + Vector3.right * x*/ + Vector3.up * (y + randomY), elapsedTime);

            transform.position = new Vector3(posX, transform.position.y, transform.position.z);

            yield return null;
        }

        coroutineSpawn = null;

        if (coroutineMove == null && moveDest != Vector2.zero)
            coroutineMove = StartCoroutine(Move());
    }

    void Despawn()
    {
        if (coroutineSpawn != null)
        {
            StopCoroutine(coroutineSpawn);
            coroutineSpawn = null;
        }

        if (coroutineMove != null)
        {
            StopCoroutine(coroutineMove);
            coroutineMove = null;
        }

        gameObject.SetActive(false);
    }

    Coroutine coroutineMove = null;
    IEnumerator Move()
    {
        Transform dest = UIBattleLevelUp.pivotTotalExp;
        //Debug.Log(Camera.main.ViewportToWorldPoint(UIBattleLevelUp.pivotTotalExp.position) + ", " + Camera.main.WorldToScreenPoint(dest.position)
        //    + battleGroup.battleCamera.camera.ScreenToWorldPoint(posExpText));

        float speed = Random.Range(0.5f, 1.5f);

        float startTime = Time.time;
        while (Time.time < startTime + 4f)
        {
            float elapsedTime = Time.time - startTime;

            float t = elapsedTime / 4f;

            Vector2 destPos = battleGroup.battleCamera.camera.ScreenToWorldPoint(moveDest);

            transform.position = Vector2.Lerp(transform.position, destPos, t * speed);

            float distance = Vector2.Distance(transform.position, destPos);
            if (distance < 1f)
                break;

            //if (t > 0.9f)
            //    break;

            yield return null;
        }

        DoFinalJob();

        Despawn();
    }

    abstract public void DoFinalJob();
}
