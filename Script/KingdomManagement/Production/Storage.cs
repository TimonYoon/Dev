using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LitJson;
using KingdomManagement;
using CodeStage.AntiCheat.ObscuredTypes;

/// <summary> TerritoryStorage? 매핑용. 이름이 안 맞는것 같아서 이걸로 접근해서 씀. 쓰기 싫으면 안 써도 무방 </summary>
namespace KingdomManagement
{
    public class Storage : MonoBehaviour
    {
        /// <summary> Storage에 dic 또는 list로 들고 있는 보관중인 상품?아이템?물건? </summary>
        public class StoredItemInfo
        {
            /// <summary> 보관중인 상품?아이템?물건?의 id </summary>
            public string itemID;
            public int index;

            Item _item;
            /// <summary> 보관중인 상품?아이템?물건? 무튼.. </summary>
            public Item item
            {
                get
                {
                    if (_item == null && !string.IsNullOrEmpty(itemID) && GameDataManager.itemDic.ContainsKey(itemID))
                        _item = GameDataManager.itemDic[itemID];

                    return _item;
                }
            }

            double _amount;
            /// <summary> 보관중인 상품?아이템?물건?의 수량 </summary>
            public double amount
            {
                get { return _amount; }
                set
                {
                    _amount = value;
                    if (onChangedAmount != null)
                        onChangedAmount();
                }
            }
                        
            public SimpleDelegate onChangedAmount;
        }

        
        public delegate void StorageDelegate(Item item, double amount);

        static public bool Consume(string productID, double amount)
        {
            Item productData = ProductManager.Instance.productList.Find(x => x.id == productID);

            return Consume(productData, amount);
        }

        static public bool Consume(Item product, double amount)
        {
            if (product == null || !storedItemDic.ContainsKey(product.id))
                return false;
            

            if (storedItemDic[product.id].amount < amount)
                return false;

            else
            {
                OutItem(product, amount);
                return true;
            }
        }

        static public bool Consume(StoredItemInfo itemData, double amount)
        {
            if (itemData == null)
                return false;

            if (itemData.amount < amount)
                return false;

            itemData.amount -= amount;

            return true;
        }

        static public void AddToStorage(Item item, double amount)
        {
            storedItemDic[item.id].amount += amount;

        }
                
        /// <summary> 보관 중인 제품들. key: 제품, value : 수량 </summary>
        static public CustomDictionary<string, StoredItemInfo> storedItemDic = new CustomDictionary<string, StoredItemInfo>();

        static public void RegisterOnChangedStoredAmountCallback(string itemID, SimpleDelegate reciever)
        {
            if (!storedItemDic.ContainsKey(itemID))
                return;

            StoredItemInfo info = storedItemDic[itemID];

            info.onChangedAmount += reciever;
        }

        static public void UnregisterOnChangedStoredAmountCallback(string itemID, SimpleDelegate reciever)
        {
            if (!storedItemDic.ContainsKey(itemID))
                return;

            StoredItemInfo info = storedItemDic[itemID];

            info.onChangedAmount -= reciever;
        }


        static public Item GetItem(string id)
        {
            if (!storedItemDic.ContainsKey(id))
                return null;

            return storedItemDic[id].item;
        }

        static public double GetItemStoredAmount(string id)
        {
            return GetItemStoredAmount(GetItem(id));
        }

        static public double GetItemStoredAmount(Item item)
        {
            if (item == null)
                return 0d;

            if (!storedItemDic.ContainsKey(item.id))
                return 0d;

            StoredItemInfo info = storedItemDic[item.id];

            return info.amount;
        }
        

        public string saveKey { get; private set; }
                
        static Storage Instance;

        void Awake()
        {
            Instance = this;
        }

        public SimpleDelegate onChangedValue;


        static public bool isInitialized { get; private set; }

        IEnumerator Start()
        {
            while (TerritoryManager.Instance == false)
                yield return null;

            LocalSave.RegisterSaveCallBack(SaveStorage);
            saveKey = "Storage_" + User.Instance.userID;
            if (ObscuredPrefs.HasKey(saveKey))
            {
                string storageDicJsonData = ObscuredPrefs.GetString(saveKey);
                Dictionary<string, double> storageDic = JsonMapper.ToObject<Dictionary<string, double>>(new JsonReader(storageDicJsonData));
                List<string> keys = storageDic.Keys.ToList();
                List<double> valueList = storageDic.Values.ToList();

                for (int i = 0; i < keys.Count; i++)
                {
                    InItem(keys[i], valueList[i]);
                }
            }

            isInitialized = true;
        }
        
        
        // 입고
        static public void InItem(string materialID, double value)
        {
            if (string.IsNullOrEmpty(materialID))
                return;

            if (value == 0)
                return;

            StoredItemInfo info = null;
            if (storedItemDic.ContainsKey(materialID))
            {
                info = storedItemDic[materialID];
                info.amount += value;
            }
                
            else
            {
                info = new StoredItemInfo();
                info.itemID = materialID;
                info.index = GameDataManager.itemDic[materialID].index;
                info.amount = value;

                storedItemDic.Add(materialID, info);
            }
        }

        static public double OutItem(Item item, double value)
        {
            return OutItem(item.id, value);
        }

        // 출고 "return (int)"
        static public double OutItem(string materialID, double value)
        {
            double returnValue = 0;

            if (string.IsNullOrEmpty(materialID))
                return returnValue;
                        
            if (storedItemDic.ContainsKey(materialID))
            {
                double outAmount = System.Math.Min(value, storedItemDic[materialID].amount);
                storedItemDic[materialID].amount -= outAmount;
            }

            return returnValue;
        }

        /// <summary> 저장소 내용 저장 </summary>
        static public void SaveStorage()
        {
            //Debug.Log("저장소 시설물 저장");
            string storageJsonData = "";

            List<string> keys = storedItemDic.Keys.ToList();
            Dictionary<string, double> storageDic = new Dictionary<string, double>();
            for (int i = 0; i < keys.Count; i++)
            {
                storageDic.Add(keys[i], storedItemDic[keys[i]].amount);
            }
            storageJsonData = JsonMapper.ToJson(storageDic);

            ObscuredPrefs.SetString(Instance.saveKey, storageJsonData);
        }        
    }
}
