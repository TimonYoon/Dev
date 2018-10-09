using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System.Linq;
using UnityEngine.UI;

namespace KingdomManagement
{
    public enum CitizenGenderType
    {
        Male,
        Female,
        Uni
    }

    [System.Serializable]
    public class CitizenPart
    {
        public string name;

        public float probability = 100f;

        public CitizenGenderType genderType = CitizenGenderType.Uni;

        public List<CitizenAttachmentInfo> attachmentInfoList = new List<CitizenAttachmentInfo>();
    }

    [System.Serializable]
    public class CitizenAttachmentInfo
    {
        //[SpineSlot]
        //public List<string> slots = new List<string>();

        [SpineSlot]
        public string slot;

        [SpineAttachment]
        public string attachment;
    }

    public class Citizen : MonoBehaviour
    {

        #region 리소스 설정
        [SpineAnimation]
        public string animationIdle;

        [SpineAnimation]
        public string animationWalk;

        [SpineAnimation]
        public string animationWalkSad;

        [SpineAnimation]
        public string animationHappy;

        [SpineAnimation]
        public string faceDefault;

        [SpineAnimation]
        public string faceAngry;

        [SpineAnimation]
        public string faceHappy;

        [SpineAnimation]
        public string faceSad;




        public List<CitizenPart> hairPartList = new List<CitizenPart>();

        public List<CitizenPart> beardPartList = new List<CitizenPart>();

        public List<CitizenPart> upperPartList = new List<CitizenPart>();

        public List<CitizenPart> lowerPartList = new List<CitizenPart>();

        public List<CitizenPart> accArmPartList = new List<CitizenPart>();

        public List<CitizenPart> accHeadPartList = new List<CitizenPart>();

        public List<CitizenPart> accFacePartList = new List<CitizenPart>();

        [Header("부위별 슬롯들 정의")]
        [SpineSlot]
        public List<string> slotHairList = new List<string>();

        [SpineSlot]
        public List<string> slotBeardList = new List<string>();

        [SpineSlot]
        public List<string> slotSkinList = new List<string>();

        [SpineSlot]
        public List<string> slotUpperList = new List<string>();

        [SpineSlot]
        public List<string> slotLowerList = new List<string>();

        [SpineSlot]
        public List<string> slotAccArmList = new List<string>();

        [SpineSlot]
        public List<string> slotAccHeadList = new List<string>();

        [SpineSlot]
        public List<string> slotAccFaceList = new List<string>();

        #endregion

        SkeletonGraphic skeletonGraphic;

        RectTransform rectTransform;

        float originalScale = 0.5f;
        //#######################################################
        void Awake()
        {
            skeletonGraphic = GetComponentInChildren<SkeletonGraphic>();
            rectTransform = GetComponent<RectTransform>();
            originalScale = transform.localScale.x;
        }



        void Start()
        {
            skeletonGraphic.AnimationState.SetAnimation(0, animationIdle, true);

            mood = Mood.Normal;
        }

        enum AnimState
        {
            Idle,
            Walk,
            Happy
        }

        enum Mood
        {
            NotDefined,
            Normal,
            Happy,
            Angry,
            Sad
        }

        Mood _mood = Mood.NotDefined;
        Mood mood
        {
            get { return _mood; }
            set
            {
                _mood = value;

                switch (value)
                {
                    case Mood.Normal:
                        skeletonGraphic.AnimationState.SetAnimation(1, faceDefault, true);
                        break;
                    case Mood.Happy:
                        skeletonGraphic.AnimationState.SetAnimation(1, faceHappy, true);
                        break;
                    case Mood.Angry:
                        skeletonGraphic.AnimationState.SetAnimation(1, faceAngry, true);
                        break;
                    case Mood.Sad:
                        skeletonGraphic.AnimationState.SetAnimation(1, faceSad, true);
                        break;
                }
            }
        }

        void Despawn()
        {
            //UITerritoryResident.citizenList.Remove(this);

            HideBubble();

            gameObject.SetActive(false);
        }

        bool isPayTax = false;

        public int index = 0;

        int moodRandom = 0;

        CitizenGenderType gender;

        public void Init(int index)
        {
            //int r = Random.Range(0, skinList.Count);
            //skeletonGraphic.initialSkinName = skinList[r];
            //skeletonGraphic.Skeleton.SetSkin(skinList[r]);
            //skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            //skeletonGraphic.AnimationState.Apply(skeletonGraphic.Skeleton);
            //skeletonGraphic.OverrideTexture = skeletonGraphic.SkeletonDataAsset.atlasAssets[r].materials[0].mainTexture;
            //skeletonGraphic.Initialize(true);

            //성별
            gender = Random.Range(0, 2) == 0 ? CitizenGenderType.Male : CitizenGenderType.Female;


            //피부색

            int r = Random.Range(0, 3);

            float skinColor = 0f;
            if (r == 0)
                skinColor = Random.Range(0f, 0.06f);
            if (r == 1)
                skinColor = Random.Range(0.06f, 0.3f);
            if (r == 2)
                skinColor = Random.Range(0.3f, 0.8f);


            //float skinColor = Random.Range(0f, 0.7f);
            Color colorSkin = Color.HSVToRGB(Random.Range(15f, 40f) / 360f, skinColor, 1f - skinColor);
            for (int i = 0; i < slotSkinList.Count; i++)
            {
                skeletonGraphic.Skeleton.FindSlot(slotSkinList[i]).SetColor(colorSkin);
            }

            InitPart(PartType.Hair);
            InitPart(PartType.Upper);
            InitPart(PartType.Lower);
            InitPart(PartType.AccArm);
            InitPart(PartType.AccHead);
            InitPart(PartType.AccFace);

            skeletonGraphic.AnimationState.SetAnimation(0, animationWalk, true);

            isPayTax = false;

            mood = Mood.Normal;

            this.index = index;

            int count = CitizenSpawnController.citizenList.Count(x => x.transform.position.y > transform.position.y);

            //뎁스
            rectTransform.SetSiblingIndex(count);

            //원근
            scale = CitizenSpawnController.GetCitizenScale(transform.position);

            //퇴장할 때 무드 랜덤 (임시코드)
            moodRandom = Random.Range(0, 3);

            //말풍선 숨기기
            HideBubble();

            //요구사항 초기화
            InitRequest();

            //일상 시작
            coroutineDo = StartCoroutine(Do());
        }

        string requestedProductID = "food_001";

        /// <summary> 물건 요구 수량 </summary>
        double requestAmount = 0d;


        struct RequestProduct
        {
            public RequestProduct(string productID, int weight)
            {
                this.productID = productID;
                this.weight = weight;
            }

            public string productID;
            public int weight;
        }

        /// <summary> 주민 요구 사항 (빵 같은 것들) 설정 </summary>
        void InitRequest()
        {            
            //requestedProductID = "food_001";
            //if (Random.Range(0, 100) < 5)
            //    requestedProductID = "food_002";

            List<RequestProduct> rpList = new List<RequestProduct>();
            RequestProduct rp = new RequestProduct("food_001", 10000);  //빵
            rpList.Add(rp);
            if (User.Instance.userLevel >= 2/*5*/)
            {
                int w = Mathf.Min(1000 * (User.Instance.userLevel - 1/*4*/), 10000);
                RequestProduct rp2 = new RequestProduct("food_002", w); //생선
                rpList.Add(rp2);
            }

            int totalWeight = rpList.Sum(x => x.weight);

            float r = Random.Range(0f, totalWeight);

            float sum = 0f;

            int a = 0;

            for (int i = 0; i < rpList.Count; i++)
            {
                if (r > sum && r <= sum + rpList[i].weight)
                {
                    a = i;
                    break;
                }
                sum += rpList[i].weight;
            }

            requestedProductID = rpList[a].productID;

            Item productionData = ProductManager.Instance.productList.Find(x => x.id == requestedProductID);


            requestAmount = 3d * System.Math.Pow(1.25d, User.Instance.userLevel - 1);

            if (requestAmount < 1000)
                requestAmount = (int)requestAmount;

            //말풍선에 아이콘 설정
            TerritoryManager.Instance.ChangeMaterialImage(imageRequestIcon, productionData.image);


        }

        void InitPart(PartType partType)
        {
            List<string> slotNames = null;
            List<CitizenPart> partList = null;

            switch (partType)
            {
                case PartType.Hair:
                    slotNames = slotHairList;
                    partList = hairPartList;
                    break;
                case PartType.Beard:
                    slotNames = slotBeardList;
                    partList = beardPartList;
                    break;
                case PartType.Upper:
                    slotNames = slotUpperList;
                    partList = upperPartList;
                    break;
                case PartType.Lower:
                    slotNames = slotLowerList;
                    partList = lowerPartList;
                    break;
                case PartType.AccArm:
                    slotNames = slotAccArmList;
                    partList = accArmPartList;
                    break;
                case PartType.AccFace:
                    slotNames = slotAccFaceList;
                    partList = accFacePartList;
                    break;
                case PartType.AccHead:
                    slotNames = slotAccHeadList;
                    partList = accHeadPartList;
                    break;
            }


            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);

            s = 1 - v;
            if (partType == PartType.Hair)
                s += Random.Range(0.15f, 0.25f);
            else
                s *= Random.Range(1f, 1.15f);

            if (partType == PartType.Hair)
            {
                float r = Random.Range(0, 100);
                if (r < 20)
                    v = Mathf.Lerp(0f, 0.25f, v);
                else if (r > 90)
                    v = Mathf.Lerp(0.75f, 1f, v);
                else
                    v = Mathf.Lerp(0.25f, 0.75f, v);
            }


            if (partType == PartType.Hair)
            {
                if (Random.Range(0, 100) < 70)
                    h = Random.Range(20f, 50f);
                else
                {
                    h = Random.Range(50f, 385f);
                    if (h > 360f)
                        h = h - 360f;
                }
                h = h / 360f;
            }



            color = Color.HSVToRGB(h, s, v);

            //원래 달려 있던거 다 땜 & 색상 지정
            if (slotNames != null)
            {
                for (int i = 0; i < slotNames.Count; i++)
                {

                    Spine.Slot slot = skeletonGraphic.Skeleton.FindSlot(slotNames[i]);

                    if (slot == null)
                        continue;

                    slot.attachment = null;
                    slot.SetColor(color);
                }
            }

            //랜덤한 파츠로 선택
            if (partList != null && partList.Count > 0)
            {
                float totalProb = partList.Sum(x => x.genderType == gender || x.genderType == CitizenGenderType.Uni ? x.probability : 0);

                float r = Random.Range(0f, totalProb);

                float sum = 0f;

                int a = 0;

                for (int i = 0; i < partList.Count; i++)
                {
                    if (partList[i].genderType != gender && partList[i].genderType != CitizenGenderType.Uni)
                        continue;

                    if (r > sum && r <= sum + partList[i].probability)
                    {
                        a = i;
                        break;
                    }
                    sum += partList[i].probability;
                }

                //랜덤한 파츠 선택
                //int a = Random.Range(0, partList.Count);
                CitizenPart part = partList[a];
                for (int i = 0; i < part.attachmentInfoList.Count; i++)
                {
                    CitizenAttachmentInfo attachmentInfo = part.attachmentInfoList[i];
                    skeletonGraphic.Skeleton.SetAttachment(attachmentInfo.slot, attachmentInfo.attachment);
                }
            }



        }

        enum PartType
        {
            Hair,
            Beard,
            Upper,
            Lower,
            AccArm,
            AccHead,
            AccFace
        }

        public GameObject bubble;

        public Image imageRequestIcon;

        void HideBubble()
        {
            if (coroutineShowBubble != null)
            {
                StopCoroutine(coroutineShowBubble);
                coroutineShowBubble = null;
            }

            bubble.transform.localScale = Vector3.zero;
        }

        void ShowBubble()
        {
            if (coroutineShowBubble != null)
                return;

            coroutineShowBubble = StartCoroutine(ShowBubbleA());
        }

        Coroutine coroutineShowBubble = null;
        IEnumerator ShowBubbleA()
        {
            float startTime = Time.time;
            while (Time.time - startTime < 0.7f)
            {
                bubble.transform.localScale = Vector3.Slerp(bubble.transform.localScale, Vector3.one, 5f * Time.deltaTime);
                yield return null;
            }

            coroutineShowBubble = null;
        }

        Coroutine coroutineDo = null;
        IEnumerator Do()
        {
            while (!isPayTax)
            {

                Vector3 destPos = new Vector3(CitizenSpawnController.waitPoint.position.x, transform.position.y, transform.position.z);

                //Citizen leftCitizen = UITerritoryResident.Instance.citizenList.Find(x => !x.isPayTax && x.index == index - 1);
                Citizen leftCitizen = CitizenSpawnController.citizenList.Find(x => !x.isPayTax && x.index == index - 1);
                if (leftCitizen)
                    destPos = new Vector3(leftCitizen.transform.position.x + 1f, transform.position.y, transform.position.z);
                else
                {
                    if (transform.position.x < CitizenSpawnController.waitPoint.position.x + 4f)
                    {
                        if (coroutineShowBubble == null)
                            ShowBubble();
                    }

                    if (transform.position.x < CitizenSpawnController.waitPoint.position.x + 1f)
                    {
                        break;
                    }
                    else
                    {
                        yield return StartCoroutine(MoveTo(destPos));
                    }
                }


                float distance = transform.position.x > destPos.x ? transform.position.x - destPos.x : destPos.x - transform.position.x;
                if (distance > 3f)
                {
                    yield return StartCoroutine(MoveTo(destPos));

                    //if (skeletonGraphic.AnimationState.GetCurrent(0).animation.name != animationWalk)
                    //    skeletonGraphic.AnimationState.SetAnimation(0, animationWalk, true);

                    //transform.localScale = new Vector3(originalScale, originalScale, 1f) * scale;
                    //transform.position = Vector3.MoveTowards(transform.position, destPos, 3f * Time.deltaTime);

                }
                else if (distance < 1f)
                {
                    if (skeletonGraphic.AnimationState.GetCurrent(0).animation.name != animationIdle)
                        skeletonGraphic.AnimationState.SetAnimation(0, animationIdle, true);
                }

                yield return null;
            }

            //물건 수령
            yield return StartCoroutine(TakeStuff());

            //퇴장
            while (transform.localPosition.x > -550f)
            {
                transform.position = Vector3.MoveTowards(transform.position, transform.position + Vector3.left * 5f, moveSpeed * Time.deltaTime);

                yield return null;
            }

            Despawn();




            yield break;
        }

        float moveSpeed
        {
            get
            {
                if (mood == Mood.Happy)
                    return 4f;
                else if (mood == Mood.Sad)
                    return 2f;

                return 3f;
            }
        }

        IEnumerator MoveTo(Vector3 destPos)
        {
            while (true)
            {
                float distance = transform.position.x > destPos.x ? transform.position.x - destPos.x : destPos.x - transform.position.x;

                if (skeletonGraphic.AnimationState.GetCurrent(0).animation.name != animationWalk)
                    skeletonGraphic.AnimationState.SetAnimation(0, animationWalk, true);

                transform.localScale = new Vector3(originalScale, originalScale, 1f) * scale;
                transform.position = Vector3.MoveTowards(transform.position, destPos, moveSpeed * Time.deltaTime);

                if (distance < 1f)
                {
                    if (skeletonGraphic.AnimationState.GetCurrent(0).animation.name != animationIdle)
                        skeletonGraphic.AnimationState.SetAnimation(0, animationIdle, true);

                    yield break;
                }

                yield return null;
            }
        }

        float scale = 1f;


        /// <summary> 물건 갯하기 </summary>
        IEnumerator TakeStuff()
        {
            bool takeStuff = false;

            Item product = ProductManager.Instance.productList.Find(x => x.id == requestedProductID);

            //최대 n초 대기
            float startTime = Time.time;
            while (Time.time - startTime < CitizenSpawnController.minWaitTime)
            {
                if (KingdomManagement.Storage.Consume(product, requestAmount))
                {
                    takeStuff = true;
                    break;
                }

                yield return null;
            }

            //물건 수령 여부에 따라 연출 달리함
            if (takeStuff)
            {
                mood = Mood.Happy;
                skeletonGraphic.AnimationState.SetAnimation(0, animationHappy, false);
                skeletonGraphic.AnimationState.AddAnimation(0, animationWalk, true, 0f);

                double price = product.price;

                double taxIncreaseValue = ProductManager.ReturnHeroSkillCitizenTaxValue + ProductManager.ReturnHeroSkillCitizenTaxValueByFillter(product);
                taxIncreaseValue += TerritoryManager.ReturnHeroSkillCitizenTaxValue + TerritoryManager.ReturnHeroSkillCitizenTaxValueByFillter(product);
                double taxPercent = 1 + (taxIncreaseValue * 0.01);
                //Debug.Log("세금 획득 증가량 : " + taxPercent);
                double tax = price * taxPercent;
                //세금 냄
                AutoGoldGeneration.Instance.TakeTax(transform, tax * Provavility());

                double expIncreaseValue = ProductManager.ReturnHeroSkillCitizenExpValue + ProductManager.ReturnHeroSkillCitizenExpValueByFillter(product);
                expIncreaseValue += TerritoryManager.ReturnHeroSkillCitizenExpValue + TerritoryManager.ReturnHeroSkillCitizenExpValueByFillter(product);
                double expPercent = 1 + (expIncreaseValue * 0.01);
                //Debug.Log("경험치 획득 증가량 : " + expPercent);
                double exp = price * expPercent;
            
                User.currentExp += exp;
            }
            else
            {
                mood = Mood.Sad;
                skeletonGraphic.AnimationState.SetAnimation(0, animationWalkSad, true);
            }

            isPayTax = true;
        }

        float normalProbability = 100f;
        float doubleProbability = 10f;
        float tripleProbability = 5f;
        int Provavility()
        {
            int result = 1;

            float total = 0;
            float n = normalProbability;
            float probabilityTaxDouble = TerritoryManager.ReturnHeroSkillCitizenDoubleTaxProbability + ProductManager.ReturnHeroSkillCitizenDoubleTaxProbability;

            float d = n + doubleProbability * (1 + (probabilityTaxDouble * 0.01f));
            //Debug.Log("두배 확률" + doubleProvavility * (1 + (probabilityTaxDouble * 0.01f)));
            //float t = d + tripleProvavility * (1 + (probabilityTaxTriple * 0.01f));
            //Debug.Log("세배 확률" + tripleProvavility * (1 + (probabilityTaxTriple * 0.01f)));
            total = d;


            double randomValue = UnityEngine.Random.Range(0, total);

            if (0 <= randomValue && randomValue <= n)
            {
                result = 1;
                //Debug.Log("일반 세금");
            }
            else if (n < randomValue && randomValue <= d)
            {
                result = 2;
                //Debug.Log("두배 세금");
            }
            //else if (d < randomValue && randomValue <= t)
            //{
            //    result = 3;
            //    Debug.Log("세배 세금");
            //}

            return result;
        }
    }
}
