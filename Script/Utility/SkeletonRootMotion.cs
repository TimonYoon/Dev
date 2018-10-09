using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;

//[RequireComponent(typeof(SkeletonAnimation))]
public class SkeletonRootMotion : MonoBehaviour
{
    SkeletonAnimation skeletonAnimation;
    int rootBoneIndex = -1;
    TrackEntry track;
    TranslateTimeline transTimeline;
    float lastTime;
    Vector2 lastPos;
    Vector2 fullDelta;

    //void OnEnable()
    //{
    //    skeletonAnimation.UpdateComplete += ApplyRootMotion;
    //    skeletonAnimation.UpdateLocal += UpdateBones;
    //    skeletonAnimation.state.Start += HandleStart;
    //    //skeletonAnimation.state.End += HandleEnd;
    //}
    //void OnDisable()
    //{
    //    skeletonAnimation.UpdateComplete -= ApplyRootMotion;
    //    skeletonAnimation.UpdateLocal -= UpdateBones;
    //    skeletonAnimation.state.Start -= HandleStart;
    //    //skeletonAnimation.state.End -= HandleEnd;
    //}

    void Awake()
    {
        if (!skeletonAnimation)
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

        if (!skeletonAnimation)
            Debug.LogError("skeletonAnimation is null");

        if (!skeletonAnimation)
            return;
        
        rootBoneIndex = skeletonAnimation.Skeleton.FindBoneIndex(skeletonAnimation.skeleton.RootBone.Data.Name);
    }

    void Start()
    {
        if (!skeletonAnimation)
        {
            Debug.LogError("skeletonAnimation is null");
            return;
        }

        //콜백 등록
        skeletonAnimation.UpdateComplete += ApplyRootMotion;
        skeletonAnimation.UpdateLocal += UpdateBones;
        skeletonAnimation.state.Start += HandleStart;
    }
    
    void HandleStart(TrackEntry trackEntry)
    {
        track = trackEntry;

        //int trackIndex = trackEntry.TrackIndex;

        //0번 트랙 아니면 리턴
        if (track.TrackIndex != 0)
            return;
        
        Spine.Animation anim = trackEntry.Animation;

        
        //루트본애니 걸린거 찾기
        transTimeline = null;
        foreach (Timeline t in anim.Timelines)
        {
            if (t.GetType() != typeof(TranslateTimeline))
                continue;           

            TranslateTimeline tt = (TranslateTimeline)t;
            if (tt.boneIndex == rootBoneIndex)
            {
                transTimeline = tt;
                lastTime = 0;
                lastPos = GetXYAtTime(transTimeline, 0);
                fullDelta = GetXYAtTime(transTimeline, trackEntry.animation.Duration) - lastPos;
                break;
            }
        }
    }

    void HandleEnd(TrackEntry trackEntry)
    {
        int trackIndex = trackEntry.TrackIndex;

        if (trackIndex != 0)
            return;

        ApplyRootMotion(skeletonAnimation);

        track = null;
        transTimeline = null;
    }
    public bool inverseX = false;
    void ApplyRootMotion(ISkeletonAnimation skelAnim)
    {
        if (transTimeline == null)
            return;
        
        float duration = track.animation.Duration;
        int loopCount = Mathf.FloorToInt(track.TrackTime / duration);
        float time = track.TrackTime - loopCount * duration;
        Vector2 pos = GetXYAtTime(transTimeline, time);
        Vector2 delta = pos - lastPos;

        delta += fullDelta * (loopCount - Mathf.FloorToInt(lastTime / duration));

        if (skelAnim.Skeleton.FlipX)
            delta.x = -delta.x;

        if (inverseX)
            delta.x = -delta.x;

        transform.Translate(new Vector3(delta.x, 0f, 0f));
        skeletonAnimation.transform.Translate(new Vector3(0f, delta.y, 0f));
        //transform.Translate(delta.x, delta.y, 0);
        lastTime = track.TrackTime;
        lastPos = pos;
    }

    void UpdateBones(ISkeletonAnimation skelAnim)
    {
        //if (transTimeline == null)
            //return;
            
        skelAnim.Skeleton.RootBone.X = 0;
        skelAnim.Skeleton.RootBone.Y = 0;
    }
        
    Vector2 GetXYAtTime(TranslateTimeline timeline, float time)
    {
        const int ENTRIES = 3;
        const int PREV_TIME = -3, PREV_X = -2, PREV_Y = -1;
        const int X = 1, Y = 2;

        float x, y;
        float[] frames = timeline.Frames;
        if (time < frames[0])
            return new Vector2(0, 0);

        if (time >= frames[frames.Length - ENTRIES])
        {
            x = frames[frames.Length + PREV_X];
            y = frames[frames.Length + PREV_Y];
            return new Vector2(x, y);
        }
        
        //보간
        int frame = Spine.Animation.BinarySearch(frames, time, ENTRIES);
        float prevX = frames[frame + PREV_X];
        float prevY = frames[frame + PREV_Y];
        float frameTime = frames[frame];
        float percent = timeline.GetCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));
        x = prevX + (frames[frame + X] - prevX) * percent;
        y = prevY + (frames[frame + Y] - prevY) * percent;
        return new Vector2(x, y);
    }
}