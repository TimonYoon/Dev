using UnityEngine;
using System.Collections;

public class UnscaledTimeParticle : MonoBehaviour
{
    ParticleSystem particle;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
    }
        
    void Update()
    {        
        particle.Simulate(Time.unscaledDeltaTime, true, false); //꼭 false로 설정해야 한다고 함        
    }    
}