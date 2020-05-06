using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PollHandler : MonoBehaviour
{
    public static PollHandler instance;
    public Poll[] polelight;
    public Material greenLight, blackLight;
    int waveNumber;
    
    private void Start()
    {
        if (instance != null)
            Destroy(instance);

         instance = this;
    }
    

    public void WaveFinished()
    {
        polelight[waveNumber].PollDestroyable(waveNumber == polelight.Length - 1);
        waveNumber++;
    }


}
