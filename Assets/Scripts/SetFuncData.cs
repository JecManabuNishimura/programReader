
using UnityEngine;
using UnityEngine.UI;

public class SetFuncData : MonoBehaviour
{

    [SerializeField]
    GameObject variTable;

    [SerializeField]
    GameObject svd;

    [SerializeField]
    Text returnText;

    [SerializeField]
    Text FuncNameText;


    public void SetReturnText(string tex)
	{
        returnText.text = tex;
    }
    public void SetFuncNameText(string tex)
    {
        FuncNameText.text = tex;
    }

    public void SetValText(string mol,string valName,string value)
	{
        var obj = Instantiate(svd);
        obj.GetComponent<SetVariData>().SetMolText(mol);
        obj.GetComponent<SetVariData>().SetValNameText(valName);
        obj.GetComponent<SetVariData>().SetValueText(value);
        obj.transform.parent = variTable.transform;
    }
}
