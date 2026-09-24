using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject[] Setact;

    public void setAcc(int num)
    {
        for (int i = 0; i < Setact.Length; i++)
        {
            Setact[i].SetActive(false);
        }

        Setact[num].SetActive(true);

    }
}
