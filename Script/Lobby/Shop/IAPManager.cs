using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using System;
//#if RECEIPT_VALIDATION
using UnityEngine.Purchasing.Security;
//#endif
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;


public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance;

    public static IStoreController storeController { get; private set; } // The Unity Purchasing system.
    public static IExtensionProvider extensionProvider { get; private set; } // The store-specific Purchasing subsystems.

    private bool isGooglePlayStoreSelected; //구글스토어인지
    private bool isAppStoreSelected; //앱스토어인지

    private const string googlePublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmpZIPAm274xieiBOY34hUoNqbwPVdWLLzuWBcG8sitdlenSEJXsMLSphFD5Wo57gVVS7v1MCqObnVhO4JnLUUdulcIDpiZPM85NM3w8yXiJ9iyF6v5z8mf6ULp/d+/akB13dl+0TVeDNyABJ7tFYYn1tYu0ivh0lSQ/t9jMRVnXcMCNInvkGACRmgyNUH0Kc0XVxoxcRpV1xDvaR4u2HxN5duUdkYE0y9lFozhXBjJTz2Okaz/vKkN5t2gtxqvr2Zza0sOqyTJHVaT5JksSgfP1Qo1zRqQepz4zxmkKDFfjO5keH72uh2/dT0SYg6DHQMJeilatq3/s48W0fOx7gAQIDAQAB";

    //구입가능한 모든 상품의 식별자 : 결제를 편하게 하기위한 식별자 그리고 외부 결제에 대한 store-specific 식별자
    //대쉬보드에서 보기 위한 고유 식별자 코드
    private static string productID100ruby = "com.funmagic.projectl.goods_diamond_001"; //계속적인 구매가 가능한 상품(일반적)
    private static string productID300ruby = "com.funmagic.projectl.goods_diamond_002"; //한번만 구매가능한 아이템
    private static string productID500ruby = "com.funmagic.projectl.goods_diamond_003"; //구독(한달간 매일 얼마씩 주는 등의 상품)
    private static string productID1000ruby = "com.funmagic.projectl.goods_diamond_004";
    private static string productID3000ruby = "com.funmagic.projectl.goods_diamond_005";
    private static string productID5000ruby = "com.funmagic.projectl.goods_diamond_006";
    private static string productID10000ruby = "com.funmagic.projectl.goods_diamond_007";
    private static string productID100rubyDouble = "com.funmagic.projectl.goods_diamond_008"; 
    private static string productID300rubyDouble = "com.funmagic.projectl.goods_diamond_009"; 
    private static string productID500rubyDouble = "com.funmagic.projectl.goods_diamond_010"; 
    private static string productID1000rubyDouble = "com.funmagic.projectl.goods_diamond_011";
    private static string productID3000rubyDouble = "com.funmagic.projectl.goods_diamond_012";
    private static string productID5000rubyDouble = "com.funmagic.projectl.goods_diamond_013";
    private static string productID10000rubyDouble = "com.funmagic.projectl.goods_diamond_014";
    private static string productIDPackage1 = "com.funmagic.projectl.goods_package_001";//차후 변경
    private static string productIDFixedCharge1 = "com.funmagic.projectl.goods_package_002";

    //애플 구독 식별자
    //앱스토어 대쉬보드에서 설정한 각각의 고유 코드 입력
    private static string productApple100ruby = "com.funmagic.projectl.goods_diamond_001";
    private static string productApple300ruby = "com.funmagic.projectl.goods_diamond_002";
    private static string productApple500ruby = "com.funmagic.projectl.goods_diamond_003";
    private static string productApple1000ruby = "com.funmagic.projectl.goods_diamond_004";
    private static string productApple3000ruby = "com.funmagic.projectl.goods_diamond_005";
    private static string productApple5000ruby = "com.funmagic.projectl.goods_diamond_006";
    private static string productApple10000ruby = "com.funmagic.projectl.goods_diamond_007";
    private static string productApple100rubyDouble = "com.funmagic.projectl.goods_diamond_008";
    private static string productApple300rubyDouble = "com.funmagic.projectl.goods_diamond_009";
    private static string productApple500rubyDouble = "com.funmagic.projectl.goods_diamond_010";
    private static string productApple1000rubyDouble = "com.funmagic.projectl.goods_diamond_011";
    private static string productApple3000rubyDouble = "com.funmagic.projectl.goods_diamond_012";
    private static string productApple5000rubyDouble = "com.funmagic.projectl.goods_diamond_013";
    private static string productApple10000rubyDouble = "com.funmagic.projectl.goods_diamond_014";
    private static string productApplePackage1 = "com.funmagic.projectl.goods_package_001";//차후 변경
    private static string productAppleFixedCharge1 = "com.funmagic.projectl.goods_package_002";

    //private static string kProductNameAppleNonConsumable = "";
    //private static string kProductNameAppleSubscription = "";

    //구글 스토어 상품 고유 식별자
    //productType에 맞춰 사용
    //구글플레이에는 ProductType.NonConsumable 타입의 상품을 제공하지 않음 - 스크립트에서 내부적으로 지정가능
    private static string productGooglePlay100ruby = "com.funmagic.projectl.goods_diamond_001";
    private static string productGooglePlay300ruby = "com.funmagic.projectl.goods_diamond_002";
    private static string productGooglePlay500ruby = "com.funmagic.projectl.goods_diamond_003";
    private static string productGooglePlay1000ruby = "com.funmagic.projectl.goods_diamond_004";
    private static string productGooglePlay3000ruby = "com.funmagic.projectl.goods_diamond_005";
    private static string productGooglePlay5000ruby = "com.funmagic.projectl.goods_diamond_006";
    private static string productGooglePlay10000ruby = "com.funmagic.projectl.goods_diamond_007";
    private static string productGooglePlay100rubyDouble = "com.funmagic.projectl.goods_diamond_008";
    private static string productGooglePlay300rubyDouble = "com.funmagic.projectl.goods_diamond_009";
    private static string productGooglePlay500rubyDouble = "com.funmagic.projectl.goods_diamond_010";
    private static string productGooglePlay1000rubyDouble = "com.funmagic.projectl.goods_diamond_011";
    private static string productGooglePlay3000rubyDouble = "com.funmagic.projectl.goods_diamond_012";
    private static string productGooglePlay5000rubyDouble = "com.funmagic.projectl.goods_diamond_013";
    private static string productGooglePlay10000rubyDouble = "com.funmagic.projectl.goods_diamond_014";
    private static string productGooglePlayPackage1 = "com.funmagic.projectl.goods_package_001";//차후 변경
    private static string productGooglePlayFixedCharge1 = "com.funmagic.projectl.goods_package_002";
    //private static string kProductNameGooglePlayNonConsumable = "";
    //private static string kProductNameGooglePlaySubscription = "";
    public bool isSuccess { get; private set; }
    public bool isFailed { get; private set; }
    //#if RECEIPT_VALIDATION
    private CrossPlatformValidator validator;
    //#endif

    private void Awake()
    {
        Instance = this;

    }
    

    void Start()
    {
        if (storeController == null)
            InitializePurchasing();
    }    

    /// <summary> 초기화 부분 </summary>
    public void InitializePurchasing()
    {
        if (IsInitialized())
            return;

        var module = StandardPurchasingModule.Instance();

        isGooglePlayStoreSelected = Application.platform == RuntimePlatform.Android && module.appStore == AppStore.GooglePlay;
        isAppStoreSelected = Application.platform == RuntimePlatform.IPhonePlayer && module.appStore == AppStore.AppleAppStore;

        //구글 라이센싱 키는 windows>UnityIAP>Receipt Valiudation Obfuscator에 저장하면됨
        var builder = ConfigurationBuilder.Instance(module);

        //스크립트로 저장하는 방법
        //builder.Configure<IGooglePlayConfiguration>().SetPublicKey("MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmpZIPAm274xieiBOY34hUoNqbwPVdWLLzuWBcG8sitdlenSEJXsMLSphFD5Wo57gVVS7v1MCqObnVhO4JnLUUdulcIDpiZPM85NM3w8yXiJ9iyF6v5z8mf6ULp/d+/akB13dl+0TVeDNyABJ7tFYYn1tYu0ivh0lSQ/t9jMRVnXcMCNInvkGACRmgyNUH0Kc0XVxoxcRpV1xDvaR4u2HxN5duUdkYE0y9lFozhXBjJTz2Okaz/vKkN5t2gtxqvr2Zza0sOqyTJHVaT5JksSgfP1Qo1zRqQepz4zxmkKDFfjO5keH72uh2/dT0SYg6DHQMJeilatq3/s48W0fOx7gAQIDAQAB");

        //아이템들 등록
        builder.AddProduct(productID100ruby, ProductType.Consumable, new IDs()
        {
            { productApple100ruby, AppleAppStore.Name },
            { productGooglePlay100ruby, GooglePlay.Name }
        });
        builder.AddProduct(productID300ruby, ProductType.Consumable, new IDs()
        {
            { productApple300ruby, AppleAppStore.Name },
            { productGooglePlay300ruby, GooglePlay.Name }
        });
        builder.AddProduct(productID500ruby, ProductType.Consumable, new IDs()
        {
            { productApple500ruby, AppleAppStore.Name },
            { productGooglePlay500ruby, GooglePlay.Name }
        });
        builder.AddProduct(productID1000ruby, ProductType.Consumable, new IDs()
        {
            { productApple1000ruby, AppleAppStore.Name },
            { productGooglePlay1000ruby, GooglePlay.Name }
        });
        builder.AddProduct(productID3000ruby, ProductType.Consumable, new IDs()
        {
            { productApple3000ruby, AppleAppStore.Name },
            { productGooglePlay3000ruby, GooglePlay.Name }
        });
        builder.AddProduct(productID5000ruby, ProductType.Consumable, new IDs()
        {
            { productApple5000ruby, AppleAppStore.Name },
            { productGooglePlay5000ruby, GooglePlay.Name }
        });
        builder.AddProduct(productID10000ruby, ProductType.Consumable, new IDs()
        {
            { productApple10000ruby, AppleAppStore.Name },
            { productGooglePlay10000ruby, GooglePlay.Name }
        });
        builder.AddProduct(productApple100rubyDouble, ProductType.Consumable, new IDs()
        {
            { productApple100rubyDouble, AppleAppStore.Name },
            { productGooglePlay100rubyDouble, GooglePlay.Name }
        });
        builder.AddProduct(productID300rubyDouble, ProductType.Consumable, new IDs()
        {
            { productApple300rubyDouble, AppleAppStore.Name },
            { productGooglePlay300rubyDouble, GooglePlay.Name }
        });
        builder.AddProduct(productID500rubyDouble, ProductType.Consumable, new IDs()
        {
            { productApple500rubyDouble, AppleAppStore.Name },
            { productGooglePlay500rubyDouble, GooglePlay.Name }
        });
        builder.AddProduct(productID1000rubyDouble, ProductType.Consumable, new IDs()
        {
            { productApple1000rubyDouble, AppleAppStore.Name },
            { productGooglePlay1000rubyDouble, GooglePlay.Name }
        });
        builder.AddProduct(productID3000rubyDouble, ProductType.Consumable, new IDs()
        {
            { productApple3000rubyDouble, AppleAppStore.Name },
            { productGooglePlay3000rubyDouble, GooglePlay.Name }
        });
        builder.AddProduct(productID5000rubyDouble, ProductType.Consumable, new IDs()
        {
            { productApple5000rubyDouble, AppleAppStore.Name },
            { productGooglePlay5000rubyDouble, GooglePlay.Name }
        });
        builder.AddProduct(productID10000rubyDouble, ProductType.Consumable, new IDs()
        {
            { productApple10000rubyDouble, AppleAppStore.Name },
            { productGooglePlay10000rubyDouble, GooglePlay.Name }
        });
        builder.AddProduct(productIDPackage1, ProductType.Consumable, new IDs()
        {
            {productApplePackage1, AppleAppStore.Name },
            {productGooglePlayPackage1, GooglePlay.Name }
        });
        builder.AddProduct(productIDFixedCharge1, ProductType.Consumable, new IDs()
        {
            {productAppleFixedCharge1, AppleAppStore.Name },
            {productGooglePlayFixedCharge1, GooglePlay.Name }
        });

        UnityPurchasing.Initialize(this, builder);

    }

    private bool IsInitialized()
    {
        return storeController != null && extensionProvider != null;
    }

    public void BuyInAppProduct(string id)
    {
            BuyProductID(id);
    }

    void BuyProductID(string productID)
    {
        isFailed = false;
        isSuccess = false;
        try
        {
            if (IsInitialized())
            {
                Product product = storeController.products.WithID("com.funmagic.projectl." + productID);
                if (product != null && product.availableToPurchase)
                {
                    //비동기로 진행됨
                    storeController.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    //productID로 해당하는 product를 찾지 못했을 때
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                //초기화 실패했을 때
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }
        catch (Exception e)
        {
            Debug.Log("BuyProductID : Fail. Exception during purchase : " + e);
        }
    }

    //넣어야될지 빼야될지 모르겠음 - 아이폰을 새 아이폰으로 바꿨을때 구매한 상품 복구하는 기능 - IOS 구현때 고민
    /// <summary> 앱스토어에만 있는 구입한 상품 복구 기능 </summary>
    //public void RestorePurchases()
    //{
    //    if (!IsInitialized())
    //    {
    //        Debug.Log("RestorePurchases FAIL. Not initialized.");
    //        return;
    //    }

    //    if (Application.platform == RuntimePlatform.IPhonePlayer ||
    //        Application.platform == RuntimePlatform.OSXPlayer)
    //    {
    //        Debug.Log("RestorePurchases started ...");

    //        var apple = extensionProvider.GetExtension<IAppleExtensions>();
    //        //비동기로 과거에 구입한 상품에 대한 복구가 시작됨
    //        apple.RestoreTransactions((result) => {
    //            //복구에 대한 결과를 표시하는 부분
    //            Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
    //        });
    //    }
    //    else
    //    {
    //        // 다른 플랫폼 디바이스에서는 기능 안함
    //        Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
    //    }
    //}



    /// <summary> 초기화가 잘 이루어졌을 때 호출됨 </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");

        storeController = controller;
        extensionProvider = extensions;
    }

    /// <summary> 초기화가 실패했을 때 호출됨 </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);

        InitializePurchasing();
    }

    public string transactionID { get; private set; }

    /// <summary> 구매에 대한 처리 부분 </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log(args.purchasedProduct.receipt);
        string signedData = "";
        string signature = "";
        Dictionary<string, object> jsonReceiptDic = (Dictionary<string, object>)MiniJson.JsonDecode(args.purchasedProduct.receipt);
        var payload = jsonReceiptDic["Payload"];
        Dictionary<string, object> jsonDic = (Dictionary<string, object>)MiniJson.JsonDecode(payload.ToString());
       

        foreach (KeyValuePair<string, object> pair in jsonDic)
        {
            if (pair.Key == "json")
            {
                signedData = pair.Value.ToString();
                Debug.Log(pair.Value);
            }
            else if(pair.Key == "signature")
            {
                signature = pair.Value.ToString();
                Debug.Log(pair.Value);
            }
        }
        bool validPurchase = true;
        
        string purchaseToken = "";
        string purchaseDate = "";
        transactionID = null;

        //#if RECEIPT_VALIDATION
        if (isGooglePlayStoreSelected || isAppStoreSelected)
        {
            
            validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            try
            {
                var result = validator.Validate(args.purchasedProduct.receipt);
                Debug.Log("Receipt is valid. Contents:");
               
                foreach (IPurchaseReceipt productReceipt in result)
                {
                    Debug.Log(productReceipt.productID);
                    Debug.Log(productReceipt.purchaseDate);
                    Debug.Log(productReceipt.transactionID);
                    
                    GooglePlayReceipt google = productReceipt as GooglePlayReceipt;
                    if (null != google)
                    {
                        //Debug.Log(google.transactionID);
                        //Debug.Log(google.purchaseState);
                        //Debug.Log(google.purchaseToken);
                        Debug.Log(args.purchasedProduct.definition.id);
                        Debug.Log(productReceipt.productID);
                        if (args.purchasedProduct.transactionID != productReceipt.transactionID)
                            throw new IAPSecurityException("not matched transactionID");

                        if (args.purchasedProduct.definition.id != productReceipt.productID)
                            throw new IAPSecurityException("not matched productID");



                        transactionID = google.transactionID;
                        purchaseToken = google.purchaseToken;
                        purchaseDate = google.purchaseDate.ToString();
                        
                    }

                    AppleInAppPurchaseReceipt apple = productReceipt as AppleInAppPurchaseReceipt;
                    if (null != apple)
                    {
                        Debug.Log(apple.originalTransactionIdentifier);
                        Debug.Log(apple.subscriptionExpirationDate);
                        Debug.Log(apple.cancellationDate);
                        Debug.Log(apple.quantity);
                        
                    }

                    // For improved security, consider comparing the signed
                    // IPurchaseReceipt.productId, IPurchaseReceipt.transactionID, and other data
                    // embedded in the signed receipt objects to the data which the game is using
                    // to make this purchase.
                }
            }
            catch (IAPSecurityException ex)
            {
                isFailed = true;
                Debug.Log("Invalid receipt, not unlocking content. " + ex);
                validPurchase = false;
            }
        }
        //#endif
        
        
        if (validPurchase)
        {
#if UNITY_EDITOR
            isSuccess = true;
            if (AdController.Instance != null)
                AdController.Instance.DeleteBanner();
#endif
#if !UNITY_EDITOR
            if (isGooglePlayStoreSelected)
            {
                //StartCoroutine(PurchaseGoogleReceiptSave(args.purchasedProduct.definition.id, args.purchasedProduct.receipt, transactionID, purchaseToken, purchaseDate, signedData, signature));
                //
                StartVerifyGoogleReceipt(args.purchasedProduct.definition.id, args.purchasedProduct.receipt, transactionID, purchaseToken, purchaseDate, signedData, signature);
            }
            else if(isAppStoreSelected)
            {
                //애플 검증
            }
#endif

        }
        return PurchaseProcessingResult.Complete;
    }
       

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        switch (failureReason)
        {
            case PurchaseFailureReason.PurchasingUnavailable:
            case PurchaseFailureReason.ExistingPurchasePending:
                UIPopupManager.ShowOKPopup("구매실패", "다시 시도해 주세요", null);
                break;
            case PurchaseFailureReason.ProductUnavailable:
            case PurchaseFailureReason.SignatureInvalid:
                UIPopupManager.ShowOKPopup("구매실패", "존재하지 않는 상품입니다\n관리자에 문의해 주세요", null);
                break;
            case PurchaseFailureReason.UserCancelled:
            case PurchaseFailureReason.PaymentDeclined:
                UIPopupManager.ShowOKPopup("구매취소", "구매를 취소하셨습니다", null);
                break;
            case PurchaseFailureReason.DuplicateTransaction:
                UIPopupManager.ShowOKPopup("중복요청", "구매를 중복요청하여 취소되었습니다", null);
                break;
            case PurchaseFailureReason.Unknown:
                UIPopupManager.ShowOKPopup("구매오류", "알수 없는 이유로\n구매가 취소되었습니다", null);
                break;
        }
    }
    void StartVerifyGoogleReceipt(string productID, string receipt, string transactionID, string purchaseToken, string purchaseTime, string signedData, string signature)
    {
        StartCoroutine(PurchaseGoogleReceiptSave(productID, receipt, transactionID, purchaseToken, purchaseTime, signedData, signature));
    }

    IEnumerator PurchaseGoogleReceiptSave(string productID, string receipt, string transactionID, string purchaseToken, string purchaseTime, string signedData, string signature)
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID, System.Text.Encoding.UTF8);
        form.AddField("platformID", Social.localUser.id, System.Text.Encoding.UTF8);
        form.AddField("productID", productID);
        form.AddField("receipt", receipt.Trim());
        form.AddField("transactionID", transactionID);
        form.AddField("purchaseToken", purchaseToken);
        form.AddField("purchaseTime", purchaseTime);
        form.AddField("signedData", signedData);
        form.AddField("signature", signature);
        form.AddField("type", 1);
        string php = "Receipt.php";
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        if(!string.IsNullOrEmpty(result) && result == "1")
        {
            
            isSuccess = true;
            if (!AdController.Instance.isPayedUser)
                AdController.Instance.DeleteBanner();
            
        }
        else
        {
            UIPopupManager.ShowOKPopup("결제 오류", "결제가 정상적으로 진행되지 않았습니다", null);
        }
    }
}

