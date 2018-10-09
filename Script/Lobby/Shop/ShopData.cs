using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;
/// <summary>
/// 상점 데이터. 자료형만 정적 동적 총합 들어간다.
/// </summary>
public class ShopData 
{
    /// <summary> 상품 - 이걸로 goodsName과 spirteName가져올 것 </summary>
    public ObscuredString id;

    /// <summary> 상품 Index - </summary>
    public string Index;
    
    /// <summary> 상품 카테고리 </summary>
    public ObscuredString category;
    /// <summary> 상품 지불 방식 </summary>
    public ObscuredString costType;
    /// <summary> 상품 가격 </summary>
    public ObscuredString price;
    /// <summary> 상품 가능 - 이남이형</summary>
    public ObscuredString enable;

    /// <summary> 상품 재화 타입 </summary>
    public ObscuredString productType;
    /// <summary> 상품 재화 값 </summary>
    public ObscuredString productAmount;

    /// <summary> 상품 구매 한계치 </summary>
    public ObscuredString maxCount;

    // TODO : 클라에서 가지고 있는 리소스 초기화해주자. 정적 데이터
    /// <summary> 상품 내용 </summary>
    public string goodsName;
    /// <summary> 상품 이미지 이름</summary>
    public string goodsSpriteName;
    /// <summary> 상품 설명</summary>
    public string goodsDescription;
    /// <summary> 상품 쿨타임 시간 - 현재는 분기준 </summary>
    public string paySpriteName;
    /// <summary> 1+1 상품 여부 </summary>
    public string doubleFlag;
    /// <summary> bonus태그에 들어갈 문구 및 수량 </summary>
    public string bonusAmount;


    /// <summary> 뽑기 결과 - Dummy </summary>
    public string darwResult;

}
