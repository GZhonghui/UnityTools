using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AD_WwiseBank : AD_WwiseMonoBase
{
    [SerializeField]
    public List<string> m_LoadBanks;

    // To Unload
    private Queue<string> m_LoadedBanks = new Queue<string>();

    protected override void WwiseInvoke()
    {
        base.WwiseInvoke();

        // Load Banks
        for (int i = 0; i < m_LoadBanks.Count; i++)
        {
            string BankName = m_LoadBanks[i];
            if (BankName.Length > 0)
            {
                AD_WwiseManager.Instance.LoadBank(BankName);
                m_LoadedBanks.Enqueue(BankName);
            }
        }
    }

    protected override void WwiseDestroy()
    {
        base.WwiseDestroy();

        // Unload Banks
        while (m_LoadedBanks.Count > 0)
        {
            string BankName = m_LoadedBanks.Peek();

            AD_WwiseManager.Instance.UnloadBank(BankName);
            m_LoadedBanks.Dequeue();
        }
    }
}
