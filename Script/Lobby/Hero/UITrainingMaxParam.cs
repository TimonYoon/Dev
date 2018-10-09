using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITrainingMaxParam : MonoBehaviour {

    public string avilityID;

    public int avilityValue;

    [SerializeField]
    Text textName;

    [SerializeField]
    Text textValue;


    public void InitSlot(string name, int value, Color color)
    {
        gameObject.SetActive(true);
        avilityID = name;
        avilityValue = value;
        textName.text = name;
        textValue.text = value.ToString();
        textValue.color = color;
    }

    private void OnDisable()
    {
        avilityValue = 0;
    }

}
