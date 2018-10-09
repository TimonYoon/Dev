using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerritoryTextPool : MonoBehaviour {

    public static TerritoryTextPool Instance;

    public GameObject territoryTextPrefab;

    private void Awake()
    {
        Instance = this;
    }
    List<Text> textList = new List<Text>();

    Text CreatTextObject()
    {
        Text text = null;
        for (int i = 0; i < textList.Count; i++)
        {
            if(textList[i].gameObject.activeSelf == false)
            {
                text = textList[i];
                break;
            }
        }

        if(text == null)
        {
            GameObject go = Instantiate(territoryTextPrefab, transform, false);
            go.transform.SetParent(this.transform, false);
            text = go.GetComponent<Text>();
            textList.Add(text);
        }

       
        return text;
    }


    /// <summary> 텍스트 보여주기 </summary>
    public void ShowText(string message, Vector3 showPoint)
    {
        Text text = CreatTextObject();
        text.text = message;
        text.rectTransform.position = showPoint;
        text.gameObject.SetActive(true);
    }
}
