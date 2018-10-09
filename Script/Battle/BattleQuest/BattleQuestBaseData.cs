using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

public class BattleQuestBaseData {

    public BattleQuestBaseData()
    {
    }

    public BattleQuestBaseData(JsonData jsonData)
    {
        id = JsonParser.ToString(jsonData["id"]);
        name = JsonParser.ToString(jsonData["name"]);
        tier = JsonParser.ToInt(jsonData["tier"]);
        time = JsonParser.ToFloat(jsonData["time"]);
        image = JsonParser.ToString(jsonData["image"]);
    }

    public ObscuredString id { get; private set; }

    public string name { get; private set; }
    
    public ObscuredFloat time { get; private set; }

    public ObscuredInt tier { get; private set; }

    public string image { get; private set; }
}