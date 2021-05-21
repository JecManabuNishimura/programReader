using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;

public partial class ReadText : MonoBehaviour
{
    static string mold = "";
    static string funcName = "";
    static string leftValname = "";                     // ���ӕϐ�
    static string switchLeftName = "";              // switch�p�̔�r�f�[�^
    static string caseName = "";                    // case �̒l

    static bool argumentFlag = false;               // �����t���O
    static bool argumentCanmaFlag = true;           // �����J���}�t���O
    static bool substitutionFlag = false;           // ����t���O
    static bool ifFlag = false;                     // if���t���O
    static bool ifCheckFlag = false;                // if�������t���O
    static public bool skipFlag = false;                   // �X�L�b�v�t���O
    static bool switchFlag = false;                 // �X�C�b�`�t���O
    static bool loopFlag = false;                    // ���[�v�p�t���O
    static bool prefixFlag = false;                 // �O�u�t���O
    static bool arrayFlag = false;                  // �z��p�t���O
    static bool nextCaseFlag = false;               // case�p�̃t���O
    static bool breakFlag = false;                  // break�p�t���O
    static bool argumentpassFlag = false;           // �����J�n�t���O

    static bool bracketsEndFlag = false;            // ���J�b�R�I���t���O
    static public bool nextLoopFlag;                // for���Ō�̏����t���O
    static public bool loopEndFlag = false;         // for���I���t���O
    static public bool newLoopFlag = false;         // �C���N�������g�f�N�������g

    static public LOOP_TYPE_NAME loopType = new LOOP_TYPE_NAME();

    static int bracketsCount = 0;

    static int ifnestLevel = 0;
    static int allNestLevel = 0;
    static int skipNestLevel = -1;
    static int funcNestLevel = 0;
    static List<int> arrayCountList = new List<int>();                      // �z��
    static Stack<LOOP_TYPE> loopNestLevel = new Stack<LOOP_TYPE>();
    static Stack<int> switchNestLevel = new Stack<int>();
    static Queue<object> argumentPass = new Queue<object>();

    public enum LOOP_TYPE_NAME
    {
        NONE,
        FOR,
        WHILE,
        DO_WHILE
    }

    struct LOOP_TYPE
	{
        public int nest;
        public LOOP_TYPE_NAME type;

        // �R���X�g���N�^
        public LOOP_TYPE(int nestNum,LOOP_TYPE_NAME name)
		{
            nest = nestNum;
            type = name;
		}
	}

    struct SCOOP_NUM
	{
        public int number;
        public int line;
    }


    static Stack<SCOOP_NUM> stack = new Stack<SCOOP_NUM>();
    static Stack<int> nestStack = new Stack<int>();

    static List<string> substList = new List<string>();


    static public textGui.LOOP_NUMBER loopStep = textGui.LOOP_NUMBER.NONE;

    //----------�f�o�b�O�p--------------------------------------------------
    [SerializeField]
    GameObject variableTable;
    [SerializeField]
    GameObject funcTable;
    [SerializeField]
    GameObject variaObj;
    [SerializeField]
    GameObject funcObj;
    [SerializeField]
    GameObject ArrayDataObj;
    [SerializeField]
    GameObject ArrayListObj;
    [SerializeField]
    Text skipFLagObj;
    //------------------------------------------------------------

    //----------�f�o�b�O�X�^�b�N�p--------------------------------------------------
    static GameObject tmpvTable;
    static GameObject tmpvObj;

    static GameObject tmpfunTable;
    static GameObject tmpfunObj;
    static GameObject tmpskipflagObj;
    static GameObject tmpArrayDataObj;
    static GameObject tmpArrayListObj;
    //------------------------------------------------------------
    static DataTable.FUNC_DATA fncData;


    void Start()
    {
		/*
         * �����FC���Ń������[�����������ꍇ�́Aunsafe���g���Ƃł���B
         * unsafe�I�v�V������ON�ɂ���K�v������B
         * Unity����playerSetting�ƐV����AssenblyDefinition��ǉ�����K�v������B
         * �������A�A�h���X�͐����݂̂̕\�L�ɂȂ�
        unsafe
		{
            
            int* test = &test2;
            Debug.Log((long)test);
        }
        
        
        unsafe
		{
            for(int i =0; i<10; i++)
			{
                fixed(int *p = &test[i])
				{
                    Debug.Log((int)p);
                }
			}
		}
        */

		tmpvObj = variaObj;
        tmpvTable = variableTable;

        tmpfunObj = funcObj;
        tmpfunTable = funcTable;
        tmpArrayDataObj = ArrayDataObj;     
        tmpArrayListObj = ArrayListObj;
    }
	private void Update()
	{
        skipFLagObj.text =  "skipFlag:"         + skipFlag.ToString() + "\n";
        skipFLagObj.text += "nextLoopFlag:"     + nextLoopFlag.ToString() + "\n";
        skipFLagObj.text += "loopStep:"         + loopStep.ToString() + "\n";
        skipFLagObj.text += "loopType:"         + loopType.ToString() + "\n";
        skipFLagObj.text += "leftValueName:"    + leftValname.ToString() + "\n";
        skipFLagObj.text += "substList:" ;
        foreach (var str in substList)
		{
            skipFLagObj.text += str.ToString() + "  ";
        }
    }

	public static readonly string[] cName = new string[]
    {
        "int",
        "float",
        "double",
        "bool",
        "void"
    };

    public static readonly string[] cResWord = new string[]
    {
        "for",
        "while",
        "if",
        "else",
        "switch",
        "case",
        "break",
    };

    public static readonly string[] symbol = new string[]
    {
        "[","{","}","(",")","=",";",
        ",","+","-","*","/","<",">",
        "|","]","+",":",
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
        arrayFlag = false;
        arrayCountList.Clear();
        switchFlag = false;
        breakFlag = false;
        nextCaseFlag = false;
        argumentPass.Clear();
    }

    static public void InitializeData()
	{
        prefixFlag = false;
        allNestLevel = 0;
        bracketsEndFlag = false;
        loopFlag = false;
        skipFlag = false;
        arrayFlag = false;
        loopNestLevel.Clear();
        ResetData();
	}

    static public void GetText(string uiText,int line,int cursorIndex)
	{
        string newSyntax = uiText.TrimEnd(' ');
        
        nextLoopFlag = false;
        loopEndFlag = false;

        if (newSyntax == "\n" || newSyntax == "")
		{
            return;
		}
        // �L���̏ꍇ
        if(SymbolCheck(newSyntax))
		{
            // case �̏���������Ȃ������Ƃ�
            if(switchFlag)
                if(nextCaseFlag)
                {
                    return;
                }
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
                        if(breakFlag)
						{
                            skipFlag = true;
                            // ���[�v�̏ꍇ
                            if (loopNestLevel.Count != 0)
                            {
                                skipNestLevel = loopNestLevel.Pop().nest;
                            }
                            // switch�̏ꍇ
                            else if (switchNestLevel.Count != 0)
                            {
                                skipNestLevel = switchNestLevel.Pop();
                            }

                        }
                        // �ϐ��錾
                        if (!CheckVarialbleData(leftValname))
						{
                            VariableDeclaration(leftValname, mold);
                        }
                        // �������
                        Substitution(substList,leftValname);

                        // for���`�F�b�N
                        if (loopFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.TERM)
                        {
                            if (ifcheck.CheckConditions(substList))
                            {
                                nextLoopFlag = true;
                            }
                            else
                            {
                                skipFlag = true;
                                skipNestLevel = allNestLevel;
                                loopNestLevel.Pop();
                                loopFlag = false;
                                //nextLoopFlag = true;
                                loopEndFlag = true;
                                if (loopNestLevel.Count != 0)
                                {
                                    // textGui.cs���Ŕ��f����悤�ɕۑ�
                                    loopType = loopNestLevel.Peek().type;
                                }
                            }
                        }
                        else if (loopFlag)
                        {
                            nextLoopFlag = true;
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
                   
                    case '(':
                        if (loopFlag)
                        {
                            // textGui.cs���Ŕ��f����悤�ɕۑ�
                            loopType = loopNestLevel.Peek().type;

                            switch (loopType)
                            {
                                case LOOP_TYPE_NAME.FOR:

                                    nextLoopFlag = true;
                                    break;
                                case LOOP_TYPE_NAME.WHILE:
                                    ifFlag = true;
                                    nextLoopFlag = true;
                                    break;
                            }
                            newLoopFlag = false;

                        }
                        else
						{
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
                            // �֐��Ăяo��
                            else
							{
                                if(CheckFunctionData(substList[substList.Count - 1],out string name))
								{
                                    argumentpassFlag = true;
                                }
							}
                            if (substitutionFlag || ifFlag || switchFlag)
                            {
                                substList.Add(newSyntax[i].ToString());
                            }
                        }

                        bracketsCount++;
                        break;
                    case ')':
                        argumentFlag = false;
                        if(argumentpassFlag)
						{
                            SetArgumentPass();
                        }
                        argumentpassFlag = false;
                        bracketsCount--;
                        // ���[�v�ꍇ
                        if(loopFlag)
						{
                            switch (loopNestLevel.Peek().type)
							{
                                case LOOP_TYPE_NAME.FOR:
                                    if (textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT)
                                    {
                                        nextLoopFlag = true;
                                        // �������
                                        Substitution(substList, leftValname);
                                    }
                                    break;
                                case LOOP_TYPE_NAME.WHILE:
                                    if (!ifcheck.CheckConditions(substList))
									{
                                        skipFlag = true;
                                        skipNestLevel = allNestLevel;
                                        loopNestLevel.Pop();
                                        loopFlag = false;
                                        loopEndFlag = true;
                                        if(loopNestLevel.Count != 0)
										{
                                            // textGui.cs���Ŕ��f����悤�ɕۑ�
                                            loopType = loopNestLevel.Peek().type;
                                        }
                                    }
                                    else
									{
                                        nextLoopFlag = true;
									}
                                    break;
                            }
                            //�Ȃ�ł��킩��Ȃ����ǁA���ꂪ�����ƃ_��
                            // ������������Askipflag����艻���B�B�B�B
                            ResetData();    
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
                        //ResetData();   // ��U�R�����g�A�E�g
                        break;
                    case '{':
                        allNestLevel++;
                        ifFlag = false;

                        ScoopPush(line);

                        if (loopFlag)
                        {
                            switch (loopNestLevel.Peek().type)
                            {
                                case LOOP_TYPE_NAME.FOR:
                                    nextLoopFlag = true;
                                    break;
                                case LOOP_TYPE_NAME.WHILE:
                                    break;
                            }
                            nextLoopFlag = true;
                            loopFlag = false;
                        }
                        if (ifCheckFlag)
                        {
                            // if����true�ɂȂ����ꍇ�̏���������

                        }
                        ResetData();

                        break;
                    case '}':
                        allNestLevel--;

                        // �X�L�b�v�����s�����z�����ꍇ
                        if (skipNestLevel >= allNestLevel)
                        {
                            if(skipFlag)
							{
                                skipFlag = false;
                                skipNestLevel = -1;
                            }
                            
                        }
                        //skipNestLevel = -1;
                        bracketsEndFlag = true;

                        DataTable.DeleteVariableScoopData(allNestLevel);

                        // ���[�v�l�X�g���I�������ꍇ
                        if (loopNestLevel.Count != 0)
						{
                            if (loopNestLevel.Peek().nest == allNestLevel)
                            {
								switch (loopNestLevel.Peek().type)
								{
                                    case LOOP_TYPE_NAME.FOR:
                                        nextLoopFlag = true;
                                        loopFlag = true;
                                        break;
                                    case LOOP_TYPE_NAME.WHILE:
                                        nextLoopFlag = true;
                                        loopFlag = true;
                                        ifFlag = true;
                                        break;
                                }
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

                        if((ifFlag) || (loopFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.TERM))
                        {
                            substList.Add(newSyntax[i].ToString());
                        }
                        else if(substList.Count >= 1)
						{
                            // += -=�@�̏ꍇ
                            if (substList[substList.Count - 1] == "-" || substList[substList.Count - 1] == "+")
                            {
                                leftValname = substList[substList.Count - 2];
                            }
                        }
                        else if(mold == "")
						{
                            
                            leftValname = substList[substList.Count - 1];

                            substList.RemoveAt(substList.Count - 1);

						}
                        // �ϐ��錾�̏ꍇ
                        else if(mold != "")
						{
                            // �ϐ��錾
                            if (!CheckVarialbleData(leftValname))
                            {
                                VariableDeclaration(leftValname, mold);
                            }
                        }
                        
                        break;
                    case ',':
                        // �J���}����
                        argumentCanmaFlag = true;
                        // �Ăяo����
                        if(argumentpassFlag)
						{
                            SetArgumentPass();
                        }
                        break;

                    case '+':
                        // �C���N�������g��A�v�Z��for���̎��͍Ō�̃X�e�b�v�̎��̂�
                        if ((loopFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT) ||
                            (!loopFlag) && !skipFlag)
                        {
                            if (substList.Count >= 1)
                            {
                                if(SetOpeData(newSyntax[i].ToString()))
								{
                                    break;
								}
                            }
                            substList.Add(newSyntax[i].ToString());
                        }
                        break;
                    case '-':
                        if ((loopFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT) ||
                            (!loopFlag) && !skipFlag)
                        {
                            if (substList.Count >= 1)
                            {
                                if (SetOpeData(newSyntax[i].ToString()))
                                {
                                    break;
                                }
                            }
                            substList.Add(newSyntax[i].ToString());
                        }
                        break;
                    case '[':
                        // �z��̏ꍇ
                        // (�錾�j�f�[�^�^���ϐ�������`����Ă���ꍇ�̂�
                        //if(mold != "" && leftValname  != "")
						{
                            arrayFlag = true;
                        }
                        break;
                    case ']':
                        // �z��̏I���
                        if(arrayFlag)
						{
                            // ���l�̏ꍇ
                            if (int.TryParse(substList[substList.Count - 1], out int result))
							{
                                arrayCountList.Add(result);
                                substList.RemoveAt(substList.Count - 1);
                            }
                            else
							{
                                // �ϐ�����`����Ă���ꍇ
                                if(CheckVarialbleData(substList[substList.Count - 1]))
								{
                                    arrayCountList.Add(int.Parse( DataTable.GetVariableValueData(substList[substList.Count - 1])));
                                    substList.RemoveAt(substList.Count - 1);
                                }
							}
                            // �ϐ��錾�̎��͓���Ȃ�
                            if(substList.Count > 0)
							{
                                // ����̏ꍇ
                                if(substitutionFlag)
								{
                                    DataTable.VARIABLE_DATA vd;
                                    if (CheckVarialbleData(substList[substList.Count - 1], arrayCountList, out vd))
                                    {
                                        substList.RemoveAt(substList.Count - 1);
                                        substList.Add(GetArrayData(vd, arrayCountList));
                                    }
                                }
                            }
                        }
                        break;
                    case ':':
                        if(switchNestLevel.Count != 0)
						{
                            // �R�������͎��ɒl���r����
                            if (int.TryParse(substList[substList.Count - 1], out int result))
                            {
                                caseName = result.ToString();
                            }
                            else
                            {
                                // �ϐ�����`����Ă���ꍇ
                                if (CheckVarialbleData(substList[substList.Count - 1]))
                                {
                                    caseName = DataTable.GetVariableValueData(substList[substList.Count - 1]);
                                }
                            }
                            // �X�C�b�`����
                            if( switchLeftName != caseName)
							{
                                nextCaseFlag = true;
                            }
                        }
                        break;

                    case '*':
                    case '/':
                    case '!':
                    case '|':
                    case '&':
                    case '<':
                    case '>':
                    case '%':
                    case '.':
                        substList.Add(newSyntax[i].ToString());
                        
                        break;
                }
            }
        }
        else
		{
            // �l�X�g�X�L�b�v�̏ꍇ
            if(skipFlag && skipNestLevel != -1)
			{
                return;
            }
            else if (nextCaseFlag && newSyntax != "case")
            {
                return;
            }
            // �\���`�F�b�N
            if (CheckReservedWord(newSyntax))
			{
				#region �\��ꏈ��
				switch (newSyntax)
				{
                    case "if":
                        ifFlag = true;
                        ifnestLevel++;

                        break;
                    case "else":
						#region else���̏���
						// if������`����Ă���
						if (ifnestLevel > 0)
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
						#endregion
						break;
                    case "for":
                        loopFlag = true;
                        newLoopFlag = true;
                        // ���g�̃l�X�g��ۑ�����
                        loopNestLevel.Push(new LOOP_TYPE(allNestLevel,LOOP_TYPE_NAME.FOR));
                        loopType = loopNestLevel.Peek().type;
                        break;
                    case "while":
                        loopFlag = true;
                        newLoopFlag = true;
                        nextLoopFlag = true;
                        loopNestLevel.Push(new LOOP_TYPE(allNestLevel, LOOP_TYPE_NAME.WHILE));
                        loopType = loopNestLevel.Peek().type;
                        break;
                    case "switch":
                        switchFlag = true;
                        switchNestLevel.Push(allNestLevel);
                        break;
                    case "case":
                        nextCaseFlag = false;
                        break;
                    case "break":
                        breakFlag = true;
                        break;
                }
				#endregion
			}

			// �֐�����(�錾��)
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
                        DataTable.VARIABLE_DATA vd = new DataTable.VARIABLE_DATA();
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
            else 
			{
                substList.Add(newSyntax);
                // �C���N�������g�E�f�N�������g�p�O�u�^
                if(prefixFlag)
                {
                    // �ϐ��̏ꍇ
                    if (CheckVarialbleData(newSyntax))
                    {
                        List<string> tmp = new List<string>();
                        tmp.Add(newSyntax); tmp.Add(substList[substList.Count - 2]); tmp.Add("1");
                        Substitution(tmp, newSyntax,true);
                        substList.RemoveAt(substList.Count - 3);
                        substList.RemoveAt(substList.Count - 2);
                        substList[substList.Count - 1] = DataTable.GetVariableValueData(newSyntax);
                    }
                    else
                    {
                        // �ϐ��ł͂Ȃ��̂ŁA�G���[
                        Debug.LogError("�C���N�������g�E�f�N�������g�G���[");
                    }
                    prefixFlag = false;
                }
                else if(switchFlag)
				{
                    if(substList[substList.Count -2] == "(")
					{
                        // �\��ꂶ��Ȃ��Ƃ�
                        if((!CheckReservedWord(newSyntax)))
						{
                            string name = "";
                            // �R�������͎��ɒl���r����
                            if (int.TryParse(newSyntax, out int result))
                            {
                                name = result.ToString();
                            }
                            else
                            {
                                // �ϐ�����`����Ă���ꍇ
                                if (CheckVarialbleData(newSyntax))
                                {
                                    name = DataTable.GetVariableValueData(substList[substList.Count - 1]);
                                }
                            }
                            switchLeftName = name;
                        }
                    }
                    else
					{
                        // �J�b�R���K�v����
					}
				}
                else if (substitutionFlag || ifFlag)
				{
                    // ���ӂ������ꍇ
                    if(!ifFlag && leftValname == "")
					{
                        leftValname = substList[substList.Count - 2];
                        substList.RemoveAt(substList.Count - 2);
                    }
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
                }
                else
                {
					// �ϐ������ݒ肳��Ă��Ȃ��ꍇ
					if (leftValname == "")
					{
						leftValname = newSyntax;
                        substList.RemoveAt(substList.Count - 2);
                        substList.RemoveAt(substList.Count - 1);
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

    static void SetArgumentPass()
	{
        // �ϐ������邩�`�F�b�N
        if (CheckVarialbleData(substList[substList.Count - 1], out DataTable.VARIABLE_DATA vd))
        {
            // �z��̏ꍇ
            if (vd.type == DataTable.DATA_TYPE.ARRAY)
            {
                if (CheckVarialbleData(vd.name, arrayCountList, out vd))
                {
                    substList.RemoveAt(substList.Count - 1);
                    argumentPass.Enqueue(GetArrayData(vd, arrayCountList));
                }
            }
            else
            {
                argumentPass.Enqueue(vd.value);
            }
        }
        else
        {
            argumentPass.Enqueue(substList[substList.Count - 1]);
        }
    }

    // �C���N�������g�Ή�
    static bool SetOpeData(string ope)
	{
        // ��u�̏ꍇ�̂�true
        bool result = false;
        // �C���N�������g�Ή�
        if (ope == "+" || ope == "-")
        {
            if(substList[substList.Count - 1] == ope)
			{
                List<string> tmp = new List<string>();
                if (substList.Count >= 2)
                {
                    // ��u�^
                    if (CheckVarialbleData(substList[substList.Count - 2]))
                    {
                        string tmpName = substList[substList.Count - 2];
                        substList[substList.Count - 2] = DataTable.GetVariableValueData(tmpName);
                        substList.RemoveAt(substList.Count - 1);
                        tmp.Add(tmpName); tmp.Add(ope); tmp.Add("1");
                        Substitution(tmp, tmpName, true);
                    }
                    result = true;
                }
                else
                {
                    // �O�u�^
                    // �C���N�������g�����߂̏ꍇ
                    prefixFlag = true;
                }
            }
            else
			{
                // �����̉\������
                result = false;
			}
        }
        return result;
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
    static bool CheckReservedWordType(string tex)
    {
        foreach (var wd in cName)
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
            DataTable.VARIABLE_DATA ValData = new DataTable.VARIABLE_DATA() ;
            ValData.name = name;
            ValData.mold = setMold;
            ValData.value = "0";
            ValData.scoopNum = allNestLevel;
            // �z��ON�̏ꍇ
            if(arrayFlag)   DataTable.AddVariableData(ValData, arrayCountList);
            else            DataTable.AddVariableData(ValData);
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
    static void Substitution(List<string> list, string subName, bool flag = false)
	{
        if(!flag)
		{
            flag = substitutionFlag;
        }
        // ����`�F�b�N
        if (flag)
        {
            var val = arithmeticCheck.Check(list);
            // �ϐ����`�F�b�N
            if (CheckVarialbleData(subName))
            {
                DataTable.SetVarialbleData(subName, val,arrayCountList);
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
                    if (data.type == DataTable.DATA_TYPE.ARRAY)
					{
                        var tmpListObj = Instantiate(tmpArrayListObj);
                        tmpListObj.transform.parent = tmpvTable.transform;
                        int counter = 0;
                        foreach (var arrayData in data.array_data)
                        {
                            var arrayObj = Instantiate(tmpArrayDataObj);
                            var objVar = arrayObj.GetComponent<SetVariData>();
                            objVar.SetArrayNumText(DataTable.GetArrayAddress(data,counter));
                            SetAllVarData(objVar, data.name, data.mold, arrayData.ToString());
                            counter++;
                            arrayObj.transform.parent = tmpListObj.transform;
                        }
                    }
                    else
					{
                        var obj = Instantiate(tmpvObj);
                        var objVar = obj.GetComponent<SetVariData>();
                        SetAllVarData(objVar, data.name, data.mold, data.value);
                        obj.transform.parent = tmpvTable.transform;
                    }
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

    static void SetAllVarData(SetVariData obj, string varName,string moldName, string value)
	{
        obj.SetMolText(moldName);
        obj.SetValNameText(varName);
        obj.SetValueText(value);
    }

    static bool CheckFunctionData(string val,out string name)
	{
        foreach(var data in DataTable.GetFunctionDataLIst())
		{
            if(data.name == val)
			{
                name = val;
                return true;
			}
		}
        name = "";
        return false;
	}

    static bool CheckVarialbleData(string val,out DataTable.VARIABLE_DATA vd)
	{
        foreach(var data in  DataTable.GetVarialbleDataList())
		{
            if(data.name == val)
			{
                vd = data;
                return true;
			}
		}
        vd = new DataTable.VARIABLE_DATA();
        return false;
	}
    static bool CheckVarialbleData(string val)
    {
        foreach (var data in DataTable.GetVarialbleDataList())
        {
            if (data.name == val)
            {
                return true;
            }
        }
        return false;
    }
    static bool CheckVarialbleData(string val,List<int> _arrayList,out DataTable.VARIABLE_DATA vd)
    {
        foreach (var data in DataTable.GetVarialbleDataList())
        {
            if (data.name == val)
            {
                // �L�q���̔z�񐔂Ɛ錾���̔z�񐔂������Ă��邩�m�F
                if(_arrayList.Count == data.array_size.Length)
				{
                    // �v�f�ԍ������邩�`�F�b�N
                    if (DataTable.CheckArrayNumber(data, _arrayList))
					{
                        vd = data;
                        return true;
                    }
				}
            }
        }
        vd = new DataTable.VARIABLE_DATA();
        return false;
    }

    static string GetArrayData(DataTable.VARIABLE_DATA data,List<int>_arrayList)
	{
        return (string)DataTable.GetOneArrayNumberData(data, _arrayList);
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


