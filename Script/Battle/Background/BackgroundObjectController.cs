using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary> 카메라 위치에 따라 BackgroundMove 오브젝트들의 활성/비활성 캐싱처리 </summary>
public class BackgroundObjectController : MonoBehaviour {

    public BattleGroup battleGroup;

    public BattleMoveCamera battleCamera;
    
    public List<List<BackGroundMove>> pool = new List<List<BackGroundMove>>();

    public Transform backgroundRoot;

	void Start ()
    {
        BackGroundMove[] bs = backgroundRoot.GetComponentsInChildren<BackGroundMove>();

        for(int i = 0; i < bs.Length; i++)
        {
            List<BackGroundMove> a = new List<BackGroundMove>();
            a.Add(bs[i]);
            pool.Add(a);
        }

        //Debug.Log(backGroundMoves.Count);
	}

    BackGroundMove GetObjectFromPool(List<BackGroundMove> list)
    {
        if (list == null || list.Count == 0)
            return null;

        BackGroundMove b = list.Find(x => !x.gameObject.activeSelf);
        if (!b)
        {
            //Debug.Log(list[0]);
            GameObject go = Instantiate(list[0].gameObject, backgroundRoot);
            go.name = list[0].gameObject.name;
            //go.transform.SetParent(list[0].root);
            b = go.GetComponent<BackGroundMove>();// Instantiate(list[0]);
            
            list.Add(b);
        }
        
        return b;
    }

	void Update ()
    {
        
        for (int i = 0; i < pool.Count; i++)
        {
            List<BackGroundMove> list = pool[i];

            if (list.Count == 0)
                continue;

            BackGroundMove dummy = list.Find(x => x.gameObject.activeSelf);

            if (!dummy)
                continue;

            int lowestIndex = dummy.index;
            int highestIndex = dummy.index;

            BackGroundMove leftEndObj = dummy;// list.Find(x => x.gameObject.activeSelf && x.index == lowestIndex);
            BackGroundMove rightEndObj = dummy;// list.Find(x => x.gameObject.activeSelf && x.index == highestIndex);
            
            for (int a = 0; a < list.Count; a++)
            {
                if (!list[a] || !list[a].gameObject.activeSelf)
                    continue;

                if (list[a].index < lowestIndex)
                {
                    lowestIndex = list[a].index;
                    leftEndObj = list[a];
                }
                    

                if (list[a].index > highestIndex)
                {
                    highestIndex = list[a].index;
                    rightEndObj = list[a];
                }
            }

            float xMin = leftEndObj.transform.position.x - leftEndObj.width * 0.5f;
            float xMax = rightEndObj.transform.position.x + rightEndObj.width * 0.5f;

            if (xMax - 60f < battleCamera.transform.position.x)
            {
                BackGroundMove newObj = GetObjectFromPool(list);
                newObj.transform.position = rightEndObj.startPos + Vector3.right * rightEndObj.width;
                newObj.startPos = rightEndObj.startPos + Vector3.right * rightEndObj.width;
                newObj.index = highestIndex + 1;
                newObj.gameObject.SetActive(true);

                if (newObj.index > lowestIndex + newObj.cacheCount)
                    leftEndObj.gameObject.SetActive(false);
            }
            else if (xMin + 60f > battleCamera.transform.position.x)
            {
                BackGroundMove newObj = GetObjectFromPool(list);
                newObj.transform.position = leftEndObj.startPos + Vector3.left * leftEndObj.width;
                newObj.startPos = leftEndObj.startPos + Vector3.left * leftEndObj.width;
                newObj.index = lowestIndex - 1;
                newObj.gameObject.SetActive(true);

                if (newObj.index < highestIndex - newObj.cacheCount)
                    rightEndObj.gameObject.SetActive(false);
            }
        }
    }
    
}
