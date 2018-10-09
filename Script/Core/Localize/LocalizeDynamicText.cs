using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizeDynamicText : MonoBehaviour {

    string id;
    Text text;
    public LocalizeDynamicText(Text _text)
    {
        text = _text;
        LocalizationManager.Instance.onChangeLocalizationData += OnChangeLocalizationData;
    }

    public void InitLocalizeText(string _id)
    {
        id = _id;
        text.text = "";

        if(id.Contains("@"))
        {
            string[] part = id.Split('@');
            
            for (int i = 0; i < part.Length; i++)
            {
                if(LocalizationManager.localizingData.ContainsKey(part[i]))
                {
                    text.text += LocalizationManager.GetText(part[i]);
                }
                else
                {
                    text.text += part[i];
                }
            }
        }
        else
        {
            text.text = LocalizationManager.GetText(id);
        }
    }

    public string GetLocalizeText(string _id)
    {
        string localizedText = LocalizationManager.GetText(_id);

        return localizedText;
    }

    void OnChangeLocalizationData()
    {
        if (text != null)
            text.text = LocalizationManager.GetText(id);
    }

    private void OnDestroy()
    {
        LocalizationManager.Instance.onChangeLocalizationData -= OnChangeLocalizationData;
    }
}

