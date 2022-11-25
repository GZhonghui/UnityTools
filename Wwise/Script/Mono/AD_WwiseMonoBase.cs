using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AD_WwiseMonoBase : MonoBehaviour
{
    private enum LifeCycleType
    {
        None,
        ComponentStartDestroy,
        TriggerEnterExit,
    }

    [SerializeField]
    private LifeCycleType LifeCycle = LifeCycleType.None;

    private void Start()
    {
        if (LifeCycle == LifeCycleType.ComponentStartDestroy)
        {
            this.WwiseInvoke();
        }
    }

    private void OnDestroy()
    {
        if (LifeCycle == LifeCycleType.ComponentStartDestroy)
        {
            this.WwiseDestroy();
        }
    }

    private void OnTriggerEnter(Collider Other)
    {
        if (LifeCycle == LifeCycleType.TriggerEnterExit)
        {
            // Not Safe
            if (Other.GetComponent<CP_PlayerCollider>() != null)
            {
                this.WwiseInvoke();
            }
        }
    }

    private void OnTriggerExit(Collider Other)
    {
        if (LifeCycle == LifeCycleType.TriggerEnterExit)
        {
            // Not Safe
            if (Other.GetComponent<CP_PlayerCollider>() != null)
            {
                this.WwiseDestroy();
            }
        }
    }

    protected virtual void WwiseInvoke()
    {

    }

    protected virtual void WwiseDestroy()
    {

    }
}
