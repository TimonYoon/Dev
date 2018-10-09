using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class UIMoney : MonoBehaviour {

    public static UIMoney Instance;
    
    public Text shopMoneyGoldText;
    
    void Awake()
    {
        Instance = this;        
    }
    
    
}
