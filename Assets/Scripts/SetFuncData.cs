
using UnityEngine;
using UnityEngine.UI;

public class SetFuncData : MonoBehaviour
{

    //[SerializeField]
    //GameObject variTable;

    //[SerializeField]
    //GameObject svd;

    //[SerializeField]
    //Text returnText;

    [SerializeField]
    Text FuncNameText;

    [SerializeField]
    GameObject moldData;

    [SerializeField]
    GameObject moldTable;
    [SerializeField]
    GameObject returnMoldTable;


    public void SetFuncNameText(string tex)
    {
        FuncNameText.text = tex;
    }

    public void CreateMold(string tex)
	{
        var obj = Instantiate(moldData);
        obj.GetComponent<funcMold>().SetText(tex);
        obj.transform.parent = moldTable.transform;
    }

    public void CreateReturnMold(string tex)
    {
        var obj = Instantiate(moldData);
        obj.GetComponent<funcMold>().SetText(tex);
        obj.transform.parent = returnMoldTable.transform;
    }
    /*
    public void SetValText(object mol, string valName, object value)
	{
        var obj = Instantiate(svd);
        obj.GetComponent<SetVariData>().SetMolText(mol);
        obj.GetComponent<SetVariData>().SetValNameText(valName);
        obj.GetComponent<SetVariData>().SetValueText(value);
        obj.transform.parent = variTable.transform;
    }*/
}
