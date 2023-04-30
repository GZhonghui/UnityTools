using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[DisallowMultipleComponent]
public class AD_WwiseEvent : AD_WwiseMonoBase
{
    [System.Serializable]
    public class AD_WwiseEventData
    {
        [SerializeField] public LifeCycleType m_lifeCycle = LifeCycleType.ComponentStart;
        [SerializeField] public string m_eventName = "";
        [SerializeField] public bool m_bIs3DSound = true;
    }

    [SerializeField]
    public List<AD_WwiseEventData> m_Events;

    private void Start()
    {
        for (int i = 0; i < m_Events.Count; i++)
        {
            if (m_Events[i].m_lifeCycle == LifeCycleType.ComponentStartDestroy
                || m_Events[i].m_lifeCycle == LifeCycleType.ComponentStart)
            {
                AD_WwiseManager.Instance.PostEvent(m_Events[i].m_eventName, m_Events[i].m_bIs3DSound ? this.gameObject : null);
            }
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < m_Events.Count; i += 1)
        {
            if (m_Events[i].m_lifeCycle == LifeCycleType.ComponentStartDestroy)
            {
                AD_WwiseManager.Instance.StopEvent(m_Events[i].m_eventName, m_Events[i].m_bIs3DSound ? this.gameObject : null);
            }
            else if (m_Events[i].m_lifeCycle == LifeCycleType.ComponentDestroy)
            {
                AD_WwiseManager.Instance.PostEvent(m_Events[i].m_eventName, m_Events[i].m_bIs3DSound ? this.gameObject : null);
            }
        }
    }

    private void OnTriggerEnter(Collider Other)
    {
        if (Other.gameObject == AD_WwiseManager.Instance.GetLocalPlayer())
        {
            for (int i = 0; i < m_Events.Count; i++)
            {
                if (m_Events[i].m_lifeCycle == LifeCycleType.TriggerEnterExit
                    || m_Events[i].m_lifeCycle == LifeCycleType.TriggerEnter)
                {
                    AD_WwiseManager.Instance.PostEvent(m_Events[i].m_eventName, m_Events[i].m_bIs3DSound ? this.gameObject : null);
                }
            }
        }
    }

    private void OnTriggerExit(Collider Other)
    {
        if (Other.gameObject == AD_WwiseManager.Instance.GetLocalPlayer())
        {
            for (int i = 0; i < m_Events.Count; i += 1)
            {
                if (m_Events[i].m_lifeCycle == LifeCycleType.TriggerEnterExit)
                {
                    AD_WwiseManager.Instance.StopEvent(m_Events[i].m_eventName, m_Events[i].m_bIs3DSound ? this.gameObject : null);
                }
                else if (m_Events[i].m_lifeCycle == LifeCycleType.TriggerExit)
                {
                    AD_WwiseManager.Instance.PostEvent(m_Events[i].m_eventName, m_Events[i].m_bIs3DSound ? this.gameObject : null);
                }
            }
        }
    }

    protected override void WwiseInvoke()
    {
        base.WwiseInvoke();
    }

    protected override void WwiseDestroy()
    {
        base.WwiseDestroy();
    }

    private void OnDrawGizmos()
    {
        if (m_bDrawGizmos)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, m_fGizmosRadius);
#if UNITY_EDITOR
            Handles.Label(transform.position + new Vector3(0, m_fGizmosRadius + 0.1f, 0), "AD_WwiseEvent");
#endif
        }
    }
}