using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AD_WwiseBank : AD_WwiseMonoBase
{
    [SerializeField]
    private string[] LoadBanks;

    // To Unload
    private Queue<string> LoadedBanks = new Queue<string>();

    protected override void WwiseInvoke()
    {
        base.WwiseInvoke();

        // Load Banks
        for (int i = 0; i < LoadBanks.Length; i++)
        {
            string BankName = LoadBanks[i];
            if (BankName.Length > 0)
            {
                AD_WwiseManager.Instance.LoadBank(BankName);
                LoadedBanks.Enqueue(BankName);
            }
        }
    }

    protected override void WwiseDestroy()
    {
        base.WwiseDestroy();

        // Unload Banks
        while (LoadedBanks.Count > 0)
        {
            string BankName = LoadedBanks.Peek();

            AD_WwiseManager.Instance.UnloadBank(BankName);
            LoadedBanks.Dequeue();
        }
    }
}
