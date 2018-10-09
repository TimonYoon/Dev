using UnityEngine;
using System.Collections;
using LitJson;

public class JsonTools : MonoBehaviour
{
	public static JsonData ParseCheckDodge(string wwwString)
	{
		if (string.IsNullOrEmpty(wwwString))
			return null;

		// 중요.. 유니코드를 한글로 변환 시켜주는 함수.. 
		//JsonParser jsonParser = new JsonParser();
		wwwString = JsonParser.Decode(wwwString);

		//DB에 지정된 필드 이름 참조할 것.
		JsonReader jReader = new JsonReader(wwwString);
		JsonData jData = JsonMapper.ToObject(jReader);
		return jData;
	}
}
