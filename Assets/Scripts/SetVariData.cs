using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetVariData : MonoBehaviour
{
    [SerializeField]
    Text moldText;

    [SerializeField]
    Text valNameText;

    [SerializeField]
    Text valueText;

    [SerializeField]
    Text arrayNum;


    public void SetArrayNumText(string tex)
    {
        arrayNum.text = tex;
    }
    public void SetMolText(string tex)
	{
        moldText.text = tex;
	}

    public void SetValNameText(string tex)
	{
        valNameText.text = tex;
	}

    public void SetValueText(string tex)
	{
        valueText.text = tex;
	}

}
