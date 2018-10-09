using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

//public class EmbeddedMaterialData
//{
//    public EmbeddedMaterialData(string _materialID,int _materialProbability,int _embeddedMaterialAmonut)
//    {
//        materialID = _materialID;
//        materialProbability = _materialProbability;
//        embeddedMaterialAmonut = _embeddedMaterialAmonut;
//    }

//    public string materialID { get; private set; }

//    public int materialProbability { get; private set; }

//    public int embeddedMaterialAmonut { get; private set; }
//}

public enum PlaceElementalType
{
    
    Fire,
    Water,
    Earth,
    Dark,
    Light
}

/// <summary>영지 베이스 데이터</summary>
public class PlaceBaseData
{
    public PlaceBaseData(JsonData jsonData)
    {
        id = JsonParser.ToString(jsonData["id"]);
        name = JsonParser.ToString(jsonData["name"]);

        //buildingID = JsonParser.ToString(jsonData["buildingID"]);
        productID = JsonParser.ToString(jsonData["productID"]);

        placeBuffDescription = JsonParser.ToString(jsonData["placeBuffDescription"]);
        placeTier = JsonParser.ToInt(jsonData["placeTier"]);



        placeElementalTypeList = new List<PlaceElementalType>();
        for (int i = 0; i < 2; i++)
        {
            if (jsonData.ContainsKey("elemental_" + (i + 1)) == false)
                continue;

            string data = JsonParser.ToString(jsonData["elemental_" + (i + 1)]);
            if (string.IsNullOrEmpty(data) == false)
            {
                if (System.Enum.IsDefined(typeof(PlaceElementalType), data))
                {
                    PlaceElementalType type = (PlaceElementalType)System.Enum.Parse(typeof(PlaceElementalType), data);
                    placeElementalTypeList.Add(type);
                }
            }
        }

        string key = "type";
        if (jsonData.ContainsKey(key))
            type = jsonData[key].ToString();

        key = "fillter";
        if (jsonData.ContainsKey(key))
            fillter = jsonData[key].ToString();

        key = "formula";
        if (jsonData.ContainsKey(key))
            formula = jsonData[key].ToString();

        key = "buffID";
        if (jsonData.ContainsKey(key))
            buffID = jsonData[key].ToString();


        //getItemIDList = new List<string>();
        //for (int i = 0; i < 2; i++)
        //{
        //    if (jsonData.ContainsKey("getItemID_" + (i + 1)) == false)
        //        continue;

        //    string data = JsonParser.ToString(jsonData["getItemID_" + (i + 1)]);
        //    if (string.IsNullOrEmpty(data) == false)
        //        getItemIDList.Add(data);
        //}



    }

    /// <summary> 생산품 아이디 </summary>
    public string productID { get; private set; }

    /// <summary> 영지 특성 설명 </summary>
    public string placeBuffDescription { get; private set; }

    /// <summary> 영지 고유 티어 </summary>
    public int placeTier { get; private set; }
    public List<PlaceElementalType> placeElementalTypeList { get; private set; }


    public string buffID { get; private set; }

    /// <summary> 적용타입 </summary>
    public string type { get; private set; }


    public string fillter { get; private set; }
    
    public string formula { get; private set; }

    /// <summary> 영지 고유아이디 </summary>
    public string id { get; private set; }

    /// <summary> 영지 이름 </summary>
    public  string name { get; private set; }
}
