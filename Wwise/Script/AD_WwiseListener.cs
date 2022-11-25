using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AD_WwiseListener : AkGameObj
{
    private GameObject Player = null;

    public void RegPlayer(GameObject Player)
    {
        if (Player != null) this.Player = Player;
    }

    // TODO, Find Player, Return Player Position
    private GameObject GetPlayer()
    {
        if (Player != null) return Player; return this.gameObject;
    }

    public override Vector3 GetPosition()
    {
        return GetPlayer().transform.position;
    }

    public override Vector3 GetForward()
    {
        return GetPlayer().transform.forward;
    }

    public override Vector3 GetUpward()
    {
        return GetPlayer().transform.up;
    }
}
