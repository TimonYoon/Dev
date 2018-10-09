using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDeployedHeroCount : MonoBehaviour {

    [SerializeField]
    GameObject deployedHeroAmountPanel;

    [SerializeField]
    Text textTotalDeployedHeroAmount;

    [SerializeField]
    Text textMaxDeployedHeroAmount;

	
    IEnumerator Start ()
    {
        deployedHeroAmountPanel.SetActive(false);

        while (SceneLobby.Instance == false)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        
        while (TerritoryManager.Instance == false)
            yield return null;

        //TerritoryManager.Instance.onChangedMaxDeployedHeroAmount += ChangeMaxDeployedHeroAmount;

    }

    void ChangeMaxDeployedHeroAmount()
    {
        //textMaxDeployedHeroAmount.text = TerritoryManager.Instance.maxDeployedHeroAmount.ToString();
    }

    void OnChangedMenu(LobbyState state)
    {
        if (state == LobbyState.WorldMap)
            Show();
        else
            Close();
    }

    void Show()
    {
        deployedHeroAmountPanel.SetActive(true);
        //textTotalDeployedHeroAmount.text = TerritoryManager.Instance.totalDeployedHeroAmount.ToString();
        //textMaxDeployedHeroAmount.text = TerritoryManager.Instance.maxDeployedHeroAmount.ToString();
    }
    int deployableHeroes;
    void OnChangedDeployedHeroCount()
    {
        //if (UIBuildingInfo.Instance.numberOfDeployableHeroes == -1)
        //    Show();
        //else
        //{
        //    deployableHeroes = TerritoryManager.Instance.maxDeployedHeroAmount - TerritoryManager.Instance.totalDeployedHeroAmount;
        //    textTotalDeployedHeroAmount.text = (TerritoryManager.Instance.totalDeployedHeroAmount + (deployableHeroes - UIBuildingInfo.Instance.numberOfDeployableHeroes)).ToString();
        //}
            
    }

    //가까 더하기 표현하다가 영웅 창 끄면 취소 


    void Close()
    {
        deployedHeroAmountPanel.SetActive(false);
    }



}
