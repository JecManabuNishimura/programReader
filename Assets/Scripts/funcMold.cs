using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class funcMold : MonoBehaviour
{
	[SerializeField]
    private Text text;

    public void SetText(string tex)
	{
		text.text = tex;
	}
}
