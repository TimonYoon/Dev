using UnityEngine;
using System.Collections;

public class UIResourceLoader : MonoBehaviour {

	static public GameObject Load(Transform trParent, string path, string name )
	{
		GameObject loadObject = Resources.Load(path + "/" + name) as GameObject;
		if (null == loadObject)
		{
			Debug.LogError("Resources Load " + path + "/" + name + "error");
			return null;
		}
		GameObject instObject = Instantiate(loadObject) as GameObject;
		instObject.name = name;
		instObject.transform.SetParent(trParent.transform, false);
		return instObject;
	}

	static public GameObject Load(string tagParent, string path, string prefabName)
	{
		GameObject tagGameObject = GameObject.FindGameObjectWithTag("tagParent");
		if (null == tagGameObject)
		{
			Debug.LogError(tagParent + " Tag cant found");
			return null;
		}

		GameObject loadObject = Resources.Load(path + "/" + prefabName) as GameObject;
		if (null == loadObject)
		{
			Debug.LogError("Resources Load " + path + "/" + prefabName + "error");
			return null;
		}
		GameObject instObject = Instantiate(loadObject) as GameObject;
		instObject.name = prefabName;
		instObject.transform.SetParent(tagGameObject.transform, false);

		return instObject;
	}

	static public GameObject Load(string path, string name)
	{
		GameObject loadObject = Resources.Load(path + "/" + name) as GameObject;
		if (null == loadObject)
		{
			Debug.LogError("Resources Load " + path + "/" + name + "error");
			return null;
		}
		GameObject instObject = Instantiate(loadObject) as GameObject;
		instObject.name = name;
		return instObject;
	}
	
}
