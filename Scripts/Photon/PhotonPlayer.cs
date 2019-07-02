using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class PhotonPlayer : MonoBehaviour
{
    private PhotonView pView;

    private void Start()
    {
        pView = GetComponent<PhotonView>();
        if (pView.IsMine)
        {
            pView.RPC("RPC_InitEnemyDuelist", RpcTarget.OthersBuffered);
            DuelMetaData.Instance.MyDuelist.transform.parent = transform;
            //Debug.Log("Num ways: " + NumWays(4, new List<int> { 1, 2 }));
        }

    }

    private int NumWays(int n, List<int> stepSizes)
    {
        int minStep = Mathf.Min(stepSizes.ToArray());
        return NumWays(0, n, stepSizes, minStep, 
            new Dictionary<int, int>());
    }

    private int NumWays(int n, int max, List<int> stepSizes, 
        int smallestStep, Dictionary<int, int> calculated)
    {
        if (n > max || n < 0) return 0;
        int remaining = max - n;
        int numWays = 0;
        stepSizes.ForEach(ss =>
        {
            if (calculated.ContainsKey(n - ss))
            {
                numWays += calculated[n - ss];
            }
        });
        return 0;
    }

    [PunRPC]
    private void RPC_InitEnemyDuelist()
    {
        DuelMetaData.Instance.EnemyDuelist.
            transform.parent = transform;
    }
}
