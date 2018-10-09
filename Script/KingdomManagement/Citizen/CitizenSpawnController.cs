using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace KingdomManagement
{
    /// <summary> 주기적으로 시민을 출현 시키는 일을 함 </summary>
    public class CitizenSpawnController : MonoBehaviour
    {
        static CitizenSpawnController Instance;

        public Transform pivot;

        public Transform genPoint;

        public Transform endPoint;

        static public Transform waitPoint { get { return Instance.endPoint; } }

        public GameObject citizenPrefab;

        float spawnInterval = 3f;

        void UpdateSpawnInterval()
        {
            spawnInterval = Random.Range(1f, 5f);
        }

        void Awake()
        {
            Instance = this;
        }

        int maxCitizenCount
        {
            get
            {
                return 18;

                return Mathf.Clamp(5 + User.Instance.userLevel - 1, 5, 18);
            }
        }

        IEnumerator Start()
        {
            while (!ProductManager.Instance.isInitialized)
                yield return null;

            while (true)
            {
                if (Time.time - lastSpawnTime > spawnInterval && citizenList.Count(x => x.gameObject.activeSelf) < maxCitizenCount)
                {
                    SpawnCitizen();

                    UpdateSpawnInterval();
                }
                    

                yield return null;
            }
        }

        static public float GetCitizenScale(Vector3 citizenPos)
        {
            return Mathf.Lerp(0.9f, 1.1f, (Instance.genPoint.position.y + 1f - citizenPos.y) / 2f);
        }

        List<Citizen> citizenPool = new List<Citizen>();

        static public List<Citizen> citizenList
        {
            get { return Instance.citizenPool; }
        }

        public float waitTime = 20f;

        static public float minWaitTime { get { return Instance.waitTime; } }


        float lastSpawnTime = float.MinValue;
        void SpawnCitizen()
        {
            //생성
            Citizen citizen = citizenPool.Find(x => !x.gameObject.activeSelf);
            if (!citizen)
            {
                GameObject go = Instantiate(citizenPrefab, pivot);
                citizen = go.GetComponent<Citizen>();
                citizenPool.Add(citizen);
            }

            citizen.transform.position = genPoint.position + Vector3.up * Random.Range(-1f, 1f);
            //citizenList.Add(citizen);

            citizen.gameObject.SetActive(true);
            citizen.Init(index);

            index++;

            lastSpawnTime = Time.time;
        }

        int index = 0;
    }
}

