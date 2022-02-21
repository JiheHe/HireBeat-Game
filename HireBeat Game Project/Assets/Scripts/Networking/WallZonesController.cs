using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WallZonesController : MonoBehaviour
{
    public AlphaReducer[] zones; //this is for name tracking
    public int[] counts; //this is for counting

    public PhotonView view;

    // Start is called before the first frame update
    void Start()
    {
        zones = FindObjectsOfType<AlphaReducer>(); //it works, a deep search
        counts = new int[zones.Length]; //all starts at 0!
        view = GetComponent<PhotonView>();
    }

    public void OnZoneEntry(string name)
    {
        view.RPC("OnZoneEntryRPC", RpcTarget.AllBuffered, name);
    }

    public void OnZoneExit(string name)
    {
        view.RPC("OnZoneExitRPC", RpcTarget.AllBuffered, name);
    }

    //thank god I gave all unique names to objs
    [PunRPC]
    public void OnZoneEntryRPC(string name) //name of the zone that's touched
    {
        int zoneIndex = FindIndex(name);
        counts[zoneIndex]++;
        zones[zoneIndex].changeAlpha(0.4f);
    }

    [PunRPC]
    public void OnZoneExitRPC(string name)
    {
        int zoneIndex = FindIndex(name);
        counts[zoneIndex]--;
        if(counts[zoneIndex] == 0) zones[zoneIndex].changeAlpha(1.0f);
    }

    private int FindIndex(string name) //Indexof takes same amount of run time... O(n)
    {
        for(int i = 0; i < zones.Length; i++)
        {
            if(zones[i].name == name)
            {
                return i;
            }
        }
        return -1; //this is error
    }
}
