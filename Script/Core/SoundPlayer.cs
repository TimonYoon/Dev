using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour {

    public SoundType soundType = SoundType.None;

    public void Play()
    {
        SoundManager.Play(soundType);
    }

    public void SoundPlayCardLight()
    {
        SoundManager.Play(SoundType.CardLight);
    }

    public void SoundPlayCardResult()
    {
        SoundManager.Play(SoundType.CardResult);
    }

    public void SoundPlayBookUnfoldLight()
    {
        SoundManager.Play(SoundType.BookUnfoldLight);
    }
}
