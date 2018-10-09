using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

namespace KingdomManagement
{
    public enum ItemType
    {
        None,
        Collect,
        Production,
    }


    public class Item
    {        
        public SimpleDelegate onChangedLevel;

        public Item(JsonData jsonData)
        {
            id = jsonData["id"].ToString();
            index = jsonData["index"].ToInt();
            category = jsonData["category"].ToString();

            string _itemType = jsonData["type"].ToString();
            if (System.Enum.IsDefined(typeof(ItemType), _itemType))
                itemType = (ItemType)System.Enum.Parse(typeof(ItemType), _itemType);

            if(jsonData.ContainsKey("productionAmount"))
                productionAmountBase = jsonData["productionAmount"].ToDouble();

            if(jsonData.ContainsKey("price"))
                basePrice = jsonData["price"].ToDouble();

            name = jsonData["name"].ToString();
            image = jsonData["image"].ToString();
            description = jsonData["description"].ToString();
            productionTime = jsonData["productionTime"].ToInt();

            string ingredientIDHeader = "ingredientID_";
            string ingredientCountHeader = "ingredientCount_";


            for(int i = 1; i < 4; i++)
            {
                string ingredientId = string.Empty;
                if (string.IsNullOrEmpty(jsonData[ingredientIDHeader + i].ToString()))
                    continue;

                ingredientId = jsonData[ingredientIDHeader + i].ToString();
                
                double ingredientCount = jsonData[ingredientCountHeader + i].ToDouble();

                IngredientInfo info = new IngredientInfo(ingredientId, ingredientCount);
                ingredientList.Add(info);

            }            
        }

        public ObscuredInt index { get; set; }

        public string name { get; set; }

        public string image { get; set; }
        
        public string description { get; set; }

        /// <summary> 자원 카테고리</summary>
        public ObscuredString category { get; private set; }

        /// <summary> 자원 타입 </summary>
        public ItemType itemType { get; private set; }
                
        public ObscuredString id { get; private set; }

        ObscuredDouble basePrice = 10d;

        /// <summary> 물건 수령시 시민이 뱉어내는 최종 금화량. 왕국 레벨에 비례함 </summary>
        public ObscuredDouble price
        {
            get
            {
                //Todo : 상품마다 기준가가 달라야 함
                return basePrice * System.Math.Pow(1.2d, User.Instance.userLevel - 1);
            }
        }

        ObscuredInt _level = 1;
        /// <summary> 레벨? 숙련도? 골드로 올릴 수 있고, 레벨에 비례해서 생산량이 올라감 </summary>
        public ObscuredInt level
        {
            get { return _level; }
            set
            {
                bool isChanged = _level != value;

                _level = value;

                productionAmount = 5d * System.Math.Pow(1.2d, level - 1);
                //Debug.Log("들어옴 : " + productionAmount);
                if (isChanged && onChangedLevel != null)
                    onChangedLevel();
            }
        }

        public SimpleDelegate onChangedProductionAmount;

        ObscuredDouble productionAmountBase = 5d;

        ObscuredDouble _productionAmount = 0d;
        /// <summary> 기본 생산 량 </summary>
        public ObscuredDouble productionAmount
        {
            get
            {
                return _productionAmount = productionAmountBase * System.Math.Pow(1.2d, level - 1);
            }
            private set
            {
                bool isChanged = _productionAmount != value;

                _productionAmount = value;

                if (isChanged && onChangedProductionAmount != null)
                    onChangedProductionAmount();
            }
        }
        
        /// <summary> 생산 시간 </summary>
        public ObscuredFloat productionTime { get; private set; }


        /// <summary> 업그레이드 비용. 1레벨 올릴 때 비용 </summary>
        public double upgradeCost
        {
            get
            {                
                return GetUpgradeCost();
            }
        }

        /// <summary> 업그레이드 비용. 여러 레벨 올릴 때 </summary>
        public double GetUpgradeCost(int upgradeAmount = 1)
        {            
            double curLevelCost = 2000d * System.Math.Pow(1.3d, level - 1);
            double destLevelCost = 2000d * System.Math.Pow(1.3d, level + upgradeAmount - 1);

            return destLevelCost - curLevelCost;
        }

        /// <summary> 업그레이드 가능 여부 </summary>
        public bool canUpgrade
        {
            get { return MoneyManager.GetMoney(MoneyType.gold).value >= upgradeCost; }
        }

        public void Upgrade(int amount = 1)
        {
            //Todo : 보유한 골드량 체크
            if (MoneyManager.GetMoney(MoneyType.gold).value >= upgradeCost)
            {
                //MoneyManager.GetMoney(MoneyType.gold).value -= upgradeCost;
                //Todo : 보유한 골드 감소
                ProductManager.Instance.UpgradeProduct(id, amount, upgradeCost);
            }
        }

        bool _isProduction = false;
        private JsonData _jsonData1;

        /// <summary> 생산시설이 실행되고 있는가? </summary>
        public bool isProduction
        {
            get
            {
                return _isProduction;
            }
            set
            {
                if (_isProduction == value)
                    return;
                _isProduction = value;                
            }
        }

        public class IngredientInfo
        {
            public IngredientInfo(string itemID, double count)
            {
                this.itemID = itemID;
                this.count = count;
            }

            public ObscuredString itemID;

            Item _item = null;
            public Item item
            {
                get
                {
                    if (_item == null && !string.IsNullOrEmpty(itemID) && GameDataManager.itemDic.ContainsKey(itemID))
                        _item = GameDataManager.itemDic[itemID];

                    return _item;
                }
            }
            public double count;
        }
        ObscuredDouble _placeBuffValue = 0d;
        public ObscuredDouble placeBuffValue
        {
            get
            {
                return _placeBuffValue;
            }
            set
            {
                _placeBuffValue = value;

                if (onChangedProductionAmount != null)
                    onChangedProductionAmount();
            }
        }
        //void OnAddPlace()
        //{
            
        //    placeBuffValue = 0;

        //    for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        //    {
        //        PlaceData placData = TerritoryManager.Instance.myPlaceList[i];

        //        if (placData == null)
        //            return;
        //        //PlaceBaseData data = TerritoryManager.Instance.myPlaceList[i].placeBaseData;
        //        if (placData.placeBaseData.type == "Production")
        //        {
        //            if (placData.placeBaseData.fillter == product.id)
        //            {
        //                placeBuffValue += placData.power;
        //            }
        //            else if (placData.placeBaseData.fillter == "all")
        //            {
        //                placeBuffValue += placData.power;
        //            }
        //        }

        //        if (placData.placeBaseData.type == "CategoryProduction")
        //        {
        //            if (placData.placeBaseData.fillter == product.category)
        //            {
        //                placeBuffValue += placData.power;
        //            }
        //            else if (placData.placeBaseData.fillter == "all")
        //            {
        //                placeBuffValue += placData.power;
        //            }
        //            //CategoryCollect()
        //        }
        //    }
        //    placeBuffValue = finalProductionAmount * (placeBuffValue / 100);
        //    totalValue = finalProductionAmount + placeBuffValue;
        //    //CalculateProductinAmount();
        //}

        /// <summary> 생산에 필요한 자원 리스트 </summary>
        public List<IngredientInfo> ingredientList = new List<IngredientInfo>();
       
    }
}
