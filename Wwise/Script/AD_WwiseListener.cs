using Chessia;
using UnityEngine;

public class AD_WwiseListener : AkGameObj
{
    private GameObject Player = null;

    public void RegPlayer(GameObject Player)
    {
        if (Player != null)
        {
            this.Player = Player;
            GS_Debug.Log("Wwise Update Local Player: " + Player.name);
        }
    }

    // TODO, Find Player, Return Player Position
    public GameObject GetPlayer()
    {
        if (Player != null) return Player; return this.gameObject;
    }

    public override Vector3 GetPosition()
    {
        if (Player != null)
        {
            return GetPlayer().transform.position;
        }
        else
        {
            return transform.position;
        }
    }

    public override Vector3 GetForward()
    {
        return transform.forward;
    }

    public override Vector3 GetUpward()
    {
        return transform.up;
    }
}
