using UnityEngine;
using System.Collections;
using LitJson;
using System;

public class PhpCaller : MonoBehaviour
{
	string _php;
	WWWForm _form;
	string result;
	Action<JsonData> _callBack;
	public void Call(string php, WWWForm form, Action<JsonData> callBack )
	{
		if (null == php || php.Length < 1)
			return;
		if (null == form)
			return;

		_php = php;
		_form = form;
		_callBack = callBack;
		StartCoroutine("CallCoroutine");
	}

	IEnumerator CallCoroutine()
	{
		yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(_php, _form, x => result = x));
		JsonData jData = JsonTools.ParseCheckDodge(result);
		if (_callBack!=null)
			_callBack(jData);

		yield return null;
	}
}
