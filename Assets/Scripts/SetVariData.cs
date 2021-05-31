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
    public void SetMolText(object tex)
	{
        moldText.text = tex.ToString();
	}

    public void SetValNameText(string tex)
	{
        valNameText.text = tex;
	}

    public void SetValueText(object tex)
	{
        //valueText.text = tex.ToString();
	}

}
