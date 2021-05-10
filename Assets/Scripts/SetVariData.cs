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
    private GameObject arrayObj;

    [SerializeField]
    private GameObject dataObj;

    public void CreateData(string data)
    {
        if(dataObj != null)
		{
            var tmp = Instantiate(dataObj);
            tmp.GetComponent<Text>().text = data;
            if (arrayObj != null)
                tmp.transform.parent = arrayObj.transform;
        }
    }
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
