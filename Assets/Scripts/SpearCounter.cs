using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpearCounter : MonoBehaviour
{
    public TextMeshProUGUI spearCount;

    public void SetSpears(int count)
    {
        spearCount.text = count.ToString();
    }
}
