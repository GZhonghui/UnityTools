using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AD_WwiseEvent : AD_WwiseMonoBase
{
    [SerializeField]
    public List<WwiseEventSelection> Events;

    protected override void WwiseInvoke()
    {
        base.WwiseInvoke();

        for (int i = 0; i < Events.Count; i += 1)
        {
            string eventName = Events[i].eventName;

            AD_WwiseManager.Instance.PostEvent(eventName, this.gameObject);
        }
    }

    protected override void WwiseDestroy()
    {
        base.WwiseDestroy();
    }
}