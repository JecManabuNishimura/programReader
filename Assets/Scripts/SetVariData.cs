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

    [SerializeField]
    Image cursorImg;


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
        valueText.text = tex.ToString();
	}

    public void SetVisibleImg(bool flag = true)
	{
        cursorImg.enabled = flag;
	}

}
