using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


public class ReadText : MonoBehaviour
{
    static string mold = "";
    static string valName = "";
    static string value = "";
    static string funcName = "";

    static string leftValname = "";                     // ���ӕϐ�

    static bool argumentFlag = false;               // �����t���O
    static bool argumentCanmaFlag = true;           // �����J���}�t���O
    static bool substitutionFlag = false;           // ����t���O
    static bool ifFlag = false;                     // if���t���O
    static bool ifCheckFlag = false;                // if�������t���O
    static bool skipFlag = false;                   // �X�L�b�v�t���O
    static bool forFlag = false;                    // for���t���O

    static bool bracketsEndFlag = false;            // ���J�b�R�I���t���O

    static int bracketsCount = 0;

    static int ifnestLevel = 0;
    static int allNestLevel = 0;
    static int skipNestLevel = -1;
    static int funcNestLevel = 0;

    struct SCOOP_NUM
	{
        public int number;
        public int line;
    }

	enum IF_TYPE{
        NOT,
        EQUAL,
        BIG,
        SMALL
	}

    static IF_TYPE ifType;

    static Stack<SCOOP_NUM> stack = new Stack<SCOOP_NUM>();
    static Stack<int> nestStack = new Stack<int>();

    static List<string> substList = new List<string>();
    static int nowIndex = 0;
    //----------�f�o�b�O�p--------------------------------------------------
    [SerializeField]
    GameObject variableTable;
    [SerializeField]
    GameObject funcTable;
    [SerializeField]
    GameObject variaObj;
    [SerializeField]
    GameObject funcObj;
    //------------------------------------------------------------

    //----------�f�o�b�O�X�^�b�N�p--------------------------------------------------
    static GameObject tmpvTable;
    static GameObject tmpvObj;

    static GameObject tmpfunTable;
    static GameObject tmpfunObj;
    //------------------------------------------------------------
    static DataTable.FUNC_DATA fncData;


    void Start()
    {
        tmpvObj = variaObj;
        tmpvTable = variableTable;

        tmpfunObj = funcObj;
        tmpfunTable = funcTable;
    }

    public static readonly string[] cName = new string[]
    {
        "int",
        "float",
        "double",
        "void"
    };

    public static readonly string[] cResWord = new string[]
    {
        "for",
        "while",
        "if",
        "else",
    };

    public static readonly string[] symbol = new string[]
    {
        "[","{","}","(",")","=",";",
        ",","+","-","*","/","<",">",
        "|","]","+",
    };

    static void ResetData()
	{
        fncData = new DataTable.FUNC_DATA();
        fncData.getVariable = new List<DataTable.VARIABLE_DATA>();
        argumentFlag = false;
         mold = "";
        valName = "";
        argumentCanmaFlag = true;
        leftValname = "";
        substitutionFlag = false;
        ifFlag = false;
        substList.Clear();
        value = "";
        nowIndex = 0;
        bracketsCount = 0;
    }

    static public void InitializeData()
	{
        bracketsEndFlag = false;
        ResetData();
	}

    static public void GetText(string uiText,int line)
	{
        string newSyntax = uiText.TrimEnd(' ');
        if(newSyntax == "\n" || newSyntax == "")
		{
            return;
		}
        // �L���̏ꍇ
        if(SymbolCheck(newSyntax))
		{
            // ���J�b�R���I����Ă���
            if (bracketsEndFlag)
            {
                if (ifnestLevel > 0) ifnestLevel--;
            }
            // �J�b�R�I���t���O������
            bracketsEndFlag = false;
            for (int i=0; i < newSyntax.Length;i++)
            {
                switch (newSyntax[i])
                {
                    case ';':
                        // ����`�F�b�N
                        if (substitutionFlag)
						{
                            value = arithmeticCheck.Check(substList);
                        }
                        // �ϐ��錾�̏ꍇ
                        if (((valName != "") || (leftValname != "")) && mold != "")
                        {
                            DataTable.VARIABLE_DATA ValData;
                            ValData.name = valName == "" ? leftValname : valName;
                            ValData.mold = mold;
                            if (value == "")
                            {
                                // �b��ݒ�
                                value = "0";
                            }
                            ValData.value = value;
                            DataTable.AddVariableData(ValData);
                        }
                        else
						{
                            if(((valName != "") || (leftValname != "")))
							{
                                DataTable.VARIABLE_DATA ValData;
                                ValData.name = valName == "" ? leftValname : valName;
                                ValData.mold = mold;
                                // �ϐ��̃`�F�b�N
                                if (CheckVarialbleData(ValData.name))
                                {
                                    DataTable.SetVarialbleData(ValData.name, value);
                                }
                                // �����̃`�F�b�N
                                else if (DataTable.SetFuncVarialbleData(funcName, ValData.name, value))
								{
                                    // ��`����Ă��Ȃ��ϐ��ɑ�����悤�Ƃ��Ă���
								}
                            }
						}
                        ResetData();
                        break;
                    case '{':
                        allNestLevel++;
                        ifFlag = false;
                        ScoopPush(line);

                        if(ifCheckFlag)
						{
                            // if����true�ɂȂ����ꍇ�̏���������
                            
                        }
                        // �֐���`
                        if (fncData.name != "" && fncData.name != null)
                        {
                            // �s��
                            fncData.begin = line;
                            // �֐��o�^
                            DataTable.AddFuncData(fncData);
                        }
                        ResetData();

                        break;
                    case '(':
                        if (mold != "" && valName != "")
                        {
                            if (newSyntax.IndexOf("(") >= 0)
                            {
                                // ��������
                                argumentFlag = true;
                            }
                            // �֐��ɂȂ�̂ŁA�ϐ�������ύX
                            fncData.returnName = mold;
                            fncData.name = valName;
                            funcName = valName;
                            // �֐����n�܂������̃l�X�g����
                            funcNestLevel = allNestLevel;
                            valName = "";
                            mold = "";
                        }
                        if (substitutionFlag || ifFlag)
                        {
                            substList.Add(newSyntax[i].ToString());
                        }
                        bracketsCount++;
                        break;
                    case ')':
                        argumentFlag = false;
                        bracketsCount--;

                        if (substitutionFlag || ifFlag)
                        {
                            substList.Add(newSyntax[i].ToString());

                            if (bracketsCount == 0)
							{
                                // if���̃`�F�b�N
                                ifCheckFlag = ifcheck.CheckConditions(substList);
                                if(!ifCheckFlag)
								{
                                    skipNestLevel = allNestLevel;
                                    skipFlag = true;
                                }
                                else
								{
                                    skipFlag = false;

                                }
                            }
                        }
                        break;
                    case '}':
                        allNestLevel--;
                        skipNestLevel = -1;
                        bracketsEndFlag = true;
                        // �֐����甲�����ꍇ
                        if(funcNestLevel == allNestLevel)
						{
                            funcName = "";
						}
                        ScoopPop();
                        break;
                    case '=':
                        substitutionFlag = true;
                        if (valName != "")
                        {
                            // ���ӂ̒l�Ƃ���
                            leftValname = valName;
                            valName = "";
                        }
                        if(ifFlag)
						{
                            substList.Add(newSyntax[i].ToString());
                        }
                        break;
                    case ',':
                        // �J���}����
                        argumentCanmaFlag = true;
                        break;

                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '!':
                    case '|':
                    case '&':
                        substList.Add(newSyntax[i].ToString());
                        break;
                }
            }
        }
        else
		{
            if(skipFlag && skipNestLevel != -1)
			{
                // �X�L�b�v�����s�����z�����ꍇ
                if (skipNestLevel >= allNestLevel)
                {
                    skipNestLevel = -1;
                }

                return;
            }
            // �\���`�F�b�N
            if(CheckReservedWord(newSyntax))
			{
				#region �\��ꏈ��
				switch (newSyntax)
				{
                    case "if":
                        ifFlag = true;
                        ifnestLevel++;

                        break;
                    case "else":
                        // if������`����Ă���
                        if(ifnestLevel > 0)
						{
                            // if����false��������
                            if(!ifCheckFlag)
							{
                                skipNestLevel = -1;
                                skipFlag = false;
                            }
                            else
							{
                                skipFlag = true;
                                // true�̏ꍇ�͓ǂݔ�΂�
                                skipNestLevel = allNestLevel;
							}
						}
                        else
						{
                            // if������`����Ă��Ȃ�����G���[
                            Debug.Log("if��������܂���");
						}
                        break;
                    case "for":
                        break;
                    case "while":
                        break;
                }
				#endregion
			}
			// �֐�����
			else if (argumentFlag)
            {
                #region �֐��̈�������

                // �J���}������̏ꍇ�݈̂���������
                // �ŏ��͐�Γ����
                if (argumentCanmaFlag)
                {
                    // �^
                    if (mold == "")
                    {
                        // �^�`�F�b�N
                        foreach (var st in cName)
                        {
                            if (st == newSyntax)
                            {
                                mold = newSyntax;
                                break;
                            }
                        }
                    }
                    // �ϐ���
                    else
                    {
                        DataTable.VARIABLE_DATA vd;
                        vd.name = newSyntax;
                        vd.mold = mold;
                        vd.value = value;

                        mold = "";
                        fncData.getVariable.Add(vd);
                        // �J���}���ɂ���
                        argumentCanmaFlag = false;
                    }
                }
                #endregion
            }
            // ���
            else if(substitutionFlag || ifFlag)
			{
                substList.Add(newSyntax);
            }
            else if (mold == "" && fncData.returnName == null)
            {
                // �^�̃`�F�b�N
                foreach (var st in cName)
                {
                    if (st == newSyntax)
                    {
                        mold = newSyntax;
                        return; // �^�w��ׁ̈A�I��
                    }
                }

                // �ϐ������ݒ肳��Ă��Ȃ��ꍇ
                if (valName == "")
                {
                    // ���łɕϐ��錾������Ă���̂�
                    if (CheckVarialbleData(newSyntax))
                    {
                        leftValname = newSyntax;
                    }
                    else
                    {
                        valName = newSyntax;
                    }
                }
            }
            else
			{
                // �ϐ������ݒ肳��Ă��Ȃ��ꍇ
                if (valName == "")
                {
                    // ���łɕϐ��錾������Ă���̂�
                    if (CheckVarialbleData(newSyntax))
                    {
                        leftValname = newSyntax;
                    }
                    else
                    {
                        valName = newSyntax;
                    }
                }
            }

            // ���J�b�R���I����Ă���
            if(bracketsEndFlag)
			{
                if(ifnestLevel > 0)   ifnestLevel--;
            }

            bracketsEndFlag = false;
        }
    }
    
    static bool CheckReservedWord(string tex)
    {
        foreach (var wd in cResWord)
        {
            if (wd == tex)
            {
                return true;
            }
        }
        return false;
    }
    


    static void ScoopPush(int line)
	{
        SCOOP_NUM sn;
        sn.number = allNestLevel;
        sn.line = line;
        stack.Push(sn);
    }

    static void ScoopPop()
    {
        if(stack.Count > 0)     stack.Pop();
    }

    static public void CreateData()
	{
		#region �ϐ��쐬
		// �ϐ�
		if (tmpvTable != null && tmpvObj != null)
        {
            // ��U�폜
            foreach (Transform n in tmpvTable.transform)
            {
                GameObject.Destroy(n.gameObject);
            }
            if (DataTable.GetVariableNum() != 0)
            {
                foreach(var data in DataTable.GetVarialbleDataList())
				{
                    var obj = Instantiate(tmpvObj);
                    obj.GetComponent<SetVariData>().SetMolText(data.mold);
                    obj.GetComponent<SetVariData>().SetValNameText(data.name);
                    obj.GetComponent<SetVariData>().SetValueText(data.value);
                    obj.transform.parent = tmpvTable.transform;
                }
            }
        }
		#endregion

		#region �֐��쐬
		// �֐�
		if (tmpfunTable != null && tmpfunObj != null )
		{
            foreach (Transform n in tmpfunTable.transform)
            {
                GameObject.Destroy(n.gameObject);
            }

            if(DataTable.GetFunctionNum() != 0)
			{
                foreach(var data in DataTable.GetFunctionDataLIst())
				{
                    var obj = Instantiate(tmpfunObj);
                    var sfd = obj.GetComponent<SetFuncData>();
                    sfd.SetReturnText(data.returnName);
                    sfd.SetFuncNameText(data.name);

                    if (data.getVariable != null)
                    {
                        foreach (var n in data.getVariable)
                        {
                            sfd.SetValText(n.mold, n.name,n.value);
                        }
                    }
                    obj.transform.parent = tmpfunTable.transform;
                }
			}
        }
		#endregion
	}
	static public void NextWord()
	{
        mold = "";
        valName = "";
	}

    static bool CheckVarialbleData(string val)
	{
        foreach(var data in  DataTable.GetVarialbleDataList())
		{
            if(data.name == val)
			{
                return true;
			}
		}
        return false;
	}
    static bool CheckFunctionVarialbleData(string funcName,string val)
    {
        foreach (var data in DataTable.GetFunctionDataLIst())
        {
            if (data.name == funcName)
            {
                foreach(var valData in data.getVariable)
				{
                    if(valData.name == val)
					{
                        return true;
					}
				}
            }
        }
        return false;
    }

    static bool SymbolCheck(string tex)
    {
        foreach (var ch in symbol)
        {
            if (tex.IndexOf(ch) >= 0)
                return true;
        }
        return false;
    }

}
