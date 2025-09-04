using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum FieldState {
    NONE,
    SEEDED,
    GROWING,
    CARROT
}

public class CarrotFarm_Field : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] stateSprites;
    public Sprite[] growingSprites;
    public Sprite[] moleSprites;
    public Sprite[] attacktedMoleSprites;
    public Sprite[] waterDropSprites;
    public Sprite[] boomSprites;

    public SpriteRenderer stateObj;
    public SpriteRenderer moleObj;
    public SpriteRenderer boomObj;
    public SpriteRenderer waterDropObj;

    private FieldState current;
    private Coroutine moleCoroutine; // (씨앗 심고 3~10초 뒤)
    private Coroutine moleAnimCoroutine; // 두더지 애니메이션 :loop
    private Coroutine waterAnimCoroutine; // (물아이템 사용) 물방울 애니메이션
    private Coroutine growAnimCoroutine; // (물아이템 사용 성공) 식물 자라는 애니메이션 lev1->2->3->carrot
    private Coroutine hammerAnimCoroutine; // (망치아이템 사용) boom 애니메이션
    private Coroutine attacktedMoleAnimCoroutine; // (망치아이템 사용 성공) 두더지가 맞는 애니메이션
    private float moleTimer_appear;
    private float moleTimer_eat;
    private float timer;
    private bool isUnderAttack;

    void Start()
    {
        current = FieldState.NONE;
        moleTimer_appear = Random.Range(3f, 10f);
        moleTimer_eat = 5;

        moleObj.enabled = false;
        waterDropObj.enabled = false;
        boomObj.enabled = false;
    }

    void OnMouseDown() { TryInteract(isClick: true); }
    void OnMouseEnter() { if (Input.GetMouseButton(0)) TryInteract(isClick: false); }

    private void SetFieldState(FieldState newState) {
        current = newState;
        gameObject.GetComponent<SpriteRenderer>().sprite = stateSprites[(int)newState];
    }

    void TryInteract(bool isClick)
    {
        if (isClick && current == FieldState.CARROT) {
            Harvest();
            return;
        }

        ItemState item = CarrotFarm_Manager.Instance.GetState();
        switch(item)
        {
            case(ItemState.SEED):
                if (current == FieldState.NONE) Plant();
                else CarrotFarm_Manager.Instance.SetState(ItemState.NONE);
                break;
            case(ItemState.WATER):
                waterDropObj.enabled = true;
                StartCoroutine(AnimCoroutine(waterDropObj, waterDropSprites, isRandomGap: false, isLoop:false, onComplete: () => {
                    waterDropObj.enabled = false;
                }));
                if (current == FieldState.SEEDED) Grow();
                break;
            case(ItemState.HAMMER):
                boomObj.enabled = true;
                StartCoroutine(AnimCoroutine(boomObj, boomSprites, isRandomGap: false, isLoop:false, onComplete: ()=>{
                    boomObj.enabled = false;
                }));
                if (isUnderAttack) HitMole();
                break;
        }
    }


    void Plant()
    {
        SetFieldState(FieldState.SEEDED);
        moleCoroutine = StartCoroutine(MoleCoroutine());
    }

    IEnumerator MoleCoroutine()
    {
        yield return new WaitForSeconds(moleTimer_appear);

        if (current != FieldState.NONE){
            isUnderAttack = true;
            moleObj.enabled = true;
            moleAnimCoroutine = StartCoroutine(AnimCoroutine(moleObj, moleSprites, isRandomGap: false, isLoop:true));
            yield return new WaitForSeconds(moleTimer_eat);
        }

        isUnderAttack = false;
        moleObj.enabled = false;
        if (moleAnimCoroutine != null) StopCoroutine(moleAnimCoroutine);
        SetFieldState(FieldState.NONE);
    }

    void Grow()
    {
        StartCoroutine(AnimCoroutine(stateObj, growingSprites, isRandomGap: true, isLoop:false, onComplete: () =>{
            SetFieldState(FieldState.CARROT);
        }));
    }

    void Harvest()
    {
        CarrotFarm_Manager.Instance.AddScore();
        SetFieldState(FieldState.NONE);
    }

    void HitMole()
    {
        isUnderAttack = false;
        if (moleCoroutine != null) StopCoroutine(moleCoroutine);
        if (moleAnimCoroutine != null) StopCoroutine(moleAnimCoroutine);
        StartCoroutine(AnimCoroutine(moleObj, attacktedMoleSprites, isRandomGap:false, isLoop:false, onComplete: ()=>{
            moleObj.enabled = false;
        }));
    }

    IEnumerator AnimCoroutine(SpriteRenderer target, Sprite[] sprites, bool isRandomGap, bool isLoop, System.Action onComplete=null)
    {
        int index = 0;
        while (true) {
            target.sprite = sprites[index % sprites.Length];
            index++; 
            if (isRandomGap)
                yield return new WaitForSeconds(Random.Range(1f, 5f));
            else
                yield return new WaitForSeconds(1f);
            if (index == sprites.Length && !isLoop) break;
        }
        onComplete?.Invoke();
    }
}
