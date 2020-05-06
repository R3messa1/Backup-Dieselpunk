using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poll : MonoBehaviour
{
    public int NumOfHitsRequired = 5;
    bool canBeDestroyed = false;
    bool isLast = false;

    public void PollDestroyable(bool last)
    {
        isLast = last;
        canBeDestroyed = true;
        ChangeLight(PollHandler.instance.greenLight);
    }
    
    public void destroyPoll()
    {
        ChangeLight(PollHandler.instance.blackLight);

        if (isLast)
            Portal.instance.OpenPOrtal();
    }

    public void ChangeLight(Material material)
    {
        GetComponent<MeshRenderer>().material = material;
    }

    public void Hit()
    {
        NumOfHitsRequired--;
        if (NumOfHitsRequired <= 0)
            destroyPoll();
    }
}
