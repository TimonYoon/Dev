using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class MoneyBaseData
{

	public MoneyBaseData(JsonData json)
    {
        if (json.ContainsKey("id"))
            id = JsonParser.ToString(json["id"]);

        if (json.ContainsKey("name"))
            name = JsonParser.ToString(json["name"]);

        if (json.ContainsKey("spriteName"))
            spriteName = JsonParser.ToString(json["spriteName"]);
    }

    public string id { get; private set; }

    public string name { get; private set; }

    public string spriteName { get; private set; }
}
