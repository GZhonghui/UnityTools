using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AD_WwiseEvent : AD_WwiseMonoBase
{
    [SerializeField]
    private string[] Events;

    protected override void WwiseInvoke()
    {
        base.WwiseInvoke();

        for (int i = 0; i < Events.Length; i += 1)
        {
            string eventName = Events[i];

            AD_WwiseManager.Instance.PostEvent(eventName, this.gameObject, true);
        }
    }

    protected override void WwiseDestroy()
    {
        base.WwiseDestroy();
    }
}
