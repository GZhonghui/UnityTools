using UnityEngine;

public class AD_WwiseMonoBase : MonoBehaviour
{
    public enum LifeCycleType
    {
        None = 0,
        ComponentStartDestroy = 100,
        ComponentStart = 101,
        ComponentDestroy = 102,
        TriggerEnterExit = 200,
        TriggerEnter = 201,
        TriggerExit = 202,
    }

    public static bool m_bDrawGizmos = false;

    public float m_fGizmosRadius = 1.8f;

    [SerializeField]
    public LifeCycleType m_LifeCycle = LifeCycleType.ComponentStart;

    private void Start()
    {
        if (m_LifeCycle == LifeCycleType.ComponentStartDestroy || m_LifeCycle == LifeCycleType.ComponentStart)
        {
            this.WwiseInvoke();
        }
    }

    private void OnDestroy()
    {
        if (m_LifeCycle == LifeCycleType.ComponentStartDestroy)
        {
            this.WwiseDestroy();
        }
        else if (m_LifeCycle == LifeCycleType.ComponentDestroy)
        {
            this.WwiseInvoke();
        }
    }

    private void OnTriggerEnter(Collider Other)
    {
        if (Other.gameObject == AD_WwiseManager.Instance.GetLocalPlayer())
        {
            if (m_LifeCycle == LifeCycleType.TriggerEnterExit || m_LifeCycle == LifeCycleType.TriggerEnter)
            {
                this.WwiseInvoke();
            }
        }
    }

    private void OnTriggerExit(Collider Other)
    {
        if (Other.gameObject == AD_WwiseManager.Instance.GetLocalPlayer())
        {
            if (m_LifeCycle == LifeCycleType.TriggerEnterExit)
            {
                this.WwiseDestroy();
            }
            else if (m_LifeCycle == LifeCycleType.TriggerExit)
            {
                this.WwiseInvoke();
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
