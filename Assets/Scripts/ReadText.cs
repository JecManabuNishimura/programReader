using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


public partial class ReadText : MonoBehaviour
{
    static string mold = "";
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
    static public bool nextLoopFlag;                // for���Ō�̏����t���O
    static public bool loopEndFlag = false;         // for���I���t���O

    static int bracketsCount = 0;

    static int ifnestLevel = 0;
    static int allNestLevel = 0;
    static int skipNestLevel = -1;
    static int funcNestLevel = 0;
    static Stack<int> loopNestLevel = new Stack<int>();

    
    static LOOP_TYPE loopType = LOOP_TYPE.NONE;

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

    public enum LOOP_TYPE
	{
        NONE,
        DEF,
        INIT,
        TERM,
        NEXT,
        PROCESSING,
        BEGIN_PROCESSING,
        END,
	}

    static IF_TYPE ifType;

    static Stack<SCOOP_NUM> stack = new Stack<SCOOP_NUM>();
    static Stack<int> nestStack = new Stack<int>();

    static List<string> substList = new List<string>();

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
        argumentCanmaFlag = true;
        leftValname = "";
        substitutionFlag = false;
        ifFlag = false;
        substList.Clear();
        bracketsCount = 0;
    }

    static public void InitializeData()
	{
        allNestLevel = 0;
        bracketsEndFlag = false;
        forFlag = false;
        skipFlag = false;
        ResetData();
	}

    static public void GetText(string uiText,int line,int cursorIndex)
	{
        string newSyntax = uiText.TrimEnd(' ');
        
        nextLoopFlag = false;

        if (newSyntax == "\n" || newSyntax == "")
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
                        // �ϐ��錾
                        if (!CheckVarialbleData(leftValname))
						{
                            VariableDeclaration(leftValname, mold);
                        }
                        // �������
                        Substitution(substList,leftValname);

                        // for���`�F�b�N
                        if (forFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.TERM)
                        {
                            if (ifcheck.CheckConditions(substList))
                            {
                                nextLoopFlag = true;
                            }
                            else
                            {
                                skipFlag = true;
                                skipNestLevel = allNestLevel;
                                forFlag = false;
                                nextLoopFlag = true;
                            }
                        }
                        else if (forFlag)
                        {
                            nextLoopFlag = true;
                        }

                        ResetData();

                        break;
                   
                    case '(':
                        if(forFlag)
						{
                            nextLoopFlag = true;
						}
                        if (mold != "" && leftValname != "")
                        {
                            if (newSyntax.IndexOf("(") >= 0)
                            {
                                // ��������
                                argumentFlag = true;
                            }
                            // �֐��ɂȂ�̂ŁA�ϐ�������ύX
                            fncData.returnName = mold;
                            fncData.name = leftValname;
                            funcName = leftValname;
                            // �֐����n�܂������̃l�X�g����
                            funcNestLevel = allNestLevel;
                            leftValname = "";
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

                        // for ���̏ꍇ
                        if(forFlag)
						{
                            if(textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT)
							{
                                nextLoopFlag = true;
                                // �������
                                Substitution(substList, leftValname);

                            }
                        }
                        else if (substitutionFlag || ifFlag)
                        {
                            substList.Add(newSyntax[i].ToString());

                            if (bracketsCount == 0 && ifFlag)
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
                            break;
                        }
                        // �֐���`
                        else if (fncData.name != "" && fncData.name != null)
                        {
                            // �s��
                            fncData.begin = line;
                            // �֐��o�^
                            DataTable.AddFuncData(fncData);
                        }
                        ResetData();
                        break;
                    case '{':
                        allNestLevel++;
                        ifFlag = false;

                        ScoopPush(line);

                        if (forFlag)
                        {
                            nextLoopFlag = true;
                            forFlag = false;
                        }
                        if (ifCheckFlag)
                        {
                            // if����true�ɂȂ����ꍇ�̏���������

                        }
                        ResetData();

                        break;
                    case '}':
                        allNestLevel--;
                        skipNestLevel = -1;
                        bracketsEndFlag = true;
                        DataTable.DeleteVariableScoopData(allNestLevel);

                        // ���[�v�l�X�g���I�������ꍇ
                        if (loopNestLevel.Count != 0)
						{
                            if (loopNestLevel.Peek() == allNestLevel)
                            {
                                nextLoopFlag = true;
                                forFlag = true;
                            }
                        }
                        // �֐����甲�����ꍇ
                        else if(funcNestLevel == allNestLevel)
						{
                            funcName = "";
						}
                        ScoopPop();
                        break;
                    case '=':
                        if (skipFlag && skipNestLevel != -1)
                        {
                            // �X�L�b�v�����s�����z�����ꍇ
                            if (skipNestLevel >= allNestLevel)
                            {
                                skipNestLevel = -1;
                            }

                            return;
                        }
                        substitutionFlag = true;

                        if((ifFlag) || (forFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.TERM))
                        {
                            substList.Add(newSyntax[i].ToString());
                        }
                        break;
                    case ',':
                        // �J���}����
                        argumentCanmaFlag = true;
                        break;

                    case '+':
                        // �C���N�������g��A�v�Z��for���̎��͍Ō�̃X�e�b�v�̎��̂�
                        if ((forFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT) ||
                            (!forFlag) && !skipFlag)
                        {
                            if (substList.Count >= 1)
                            {
                                // �C���N�������g�Ή�
                                if (substList[substList.Count - 1] == "+")
                                {
                                    List<string> tmp = new List<string>();
                                    var name = substList.Count >= 2 ? substList[substList.Count - 2] : leftValname;

                                    tmp.Add(name); tmp.Add("+"); tmp.Add("1");
                                    if (substitutionFlag == false)
                                    {
                                        substitutionFlag = true;
                                        Substitution(tmp, name);
                                        substitutionFlag = false;
                                    }
                                    else
                                    {
                                        Substitution(tmp, name);
                                    }
                                    if (substList.Count >= 2)
                                    {
                                        substList.RemoveAt(substList.Count - 1);
                                    }
                                    else
                                    {
                                        substList[substList.Count - 1] = name;
                                    }
                                    break;
                                }
                            }
                            substList.Add(newSyntax[i].ToString());
                        }
                        break;
                    case '-':
                        if ((forFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT) ||
                            (!forFlag) && !skipFlag)
                        {
                            if (substList.Count >= 1)
                            {
                                // �C���N�������g�Ή�
                                if (substList[substList.Count - 1] == "-")
                                {
                                    List<string> tmp = new List<string>();
                                    var name = substList.Count >= 2 ? substList[substList.Count - 2] : leftValname;

                                    tmp.Add(name); tmp.Add("-"); tmp.Add("1");
                                    if (substitutionFlag == false)
                                    {
                                        substitutionFlag = true;
                                        Substitution(tmp, name);
                                        substitutionFlag = false;
                                    }
                                    else
                                    {
                                        Substitution(tmp, name);
                                    }
                                    if (substList.Count >= 2)
                                    {
                                        substList.RemoveAt(substList.Count - 1);
                                    }
                                    else
                                    {
                                        substList[substList.Count - 1] = name;
                                    }
                                    break;
                                }
                            }
                            substList.Add(newSyntax[i].ToString());
                        }
                        break;
                    case '*':
                    case '/':
                    case '!':
                    case '|':
                    case '&':
                    case '<':
                    case '>':
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
                        forFlag = true;
                        // ���g�̃l�X�g��ۑ�����
                        loopNestLevel.Push(allNestLevel);
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
                        vd.value = "0";
                        vd.scoopNum = allNestLevel;

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
            // for�����̏I�������p
            else if(textGui.loopStepNumber == textGui.LOOP_NUMBER.TERM)
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
                if (leftValname == "")
                {
                    // ���łɕϐ��錾������Ă���̂�
                    //if (CheckVarialbleData(newSyntax))
                    {
                        leftValname = newSyntax;
                    }
                }
            }
            else
			{
                // �ϐ������ݒ肳��Ă��Ȃ��ꍇ
                if (leftValname == "")
                {
                    // ���łɕϐ��錾������Ă���̂�
                    //if (CheckVarialbleData(newSyntax))
                    {
                        leftValname = newSyntax;
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
    
    // �ϐ��錾
    static void VariableDeclaration(string name,string setMold)
	{
        if(name != "" && setMold != "")
		{
            DataTable.VARIABLE_DATA ValData;
            ValData.name = name;
            ValData.mold = setMold;
            ValData.value = "0";
            ValData.scoopNum = allNestLevel;
            DataTable.AddVariableData(ValData);
        }
        
    }

    static void CheckVariableIsScoop(string name,int scoop)
	{
        // ����������������������i�׋��p�j
        foreach (var (data,index) in DataTable.GetVarialbleDataList().Indexed())
		{
            if(data.scoopNum > scoop)
			{
                DataTable.DeleteVariableData(index);
            }
		}
    }
    static void Substitution(List<string> list,string subName)
	{
        // ����`�F�b�N
        if (substitutionFlag)
        {
            var val = arithmeticCheck.Check(list);
            // �ϐ����`�F�b�N
            if (CheckVarialbleData(subName))
            {
                DataTable.SetVarialbleData(subName, val);
            }
            // �����̃`�F�b�N
            else if (DataTable.SetFuncVarialbleData(funcName, subName, val))
            {
                // ��`����Ă��Ȃ��ϐ��ɑ�����悤�Ƃ��Ă���

            }
        }
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


