using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class MoveHero : MonoBehaviour
{
    public bool isStart = false;
    public GameObject heroSpawner;
    public GameObject heroDestination;

    const int timeScale = 1;
    //public SkeletonRootMotion rootMotion;
    public SkeletonAnimation skeletonAnimation;
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string idleAnimation = "Idle";

    [SpineAnimation(dataField: "skeletonAnimation")]
    public string runAnimation = "Run";

    private void Start()
    {
        OptionManager.onChangedBoostSpeed += OnChangedBoostSpeed;
        skeletonAnimation.timeScale = timeScale;
        transform.position = new Vector3(heroSpawner.transform.position.x, transform.position.y);
    }

    private void OnDestroy()
    {
        OptionManager.onChangedBoostSpeed -= OnChangedBoostSpeed;
    }

    void OnChangedBoostSpeed()
    {
        skeletonAnimation.timeScale /= OptionManager.boostSpeed;
    }

    public void Run()
    {
        
        skeletonAnimation.state.SetAnimation(0, runAnimation, true);
    }

    private void Update()
    {
        if (this.transform.position.x > heroDestination.transform.position.x)
        {
            isStart = false;
            skeletonAnimation.state.SetAnimation(0, idleAnimation, true);
            this.transform.localPosition = new Vector2(0f, transform.localPosition.y);
        }
    }
}
