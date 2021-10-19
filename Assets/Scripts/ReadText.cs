using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;
using System.Runtime.InteropServices;

public unsafe class ReadData
{
    public struct LOOP_TYPE
    {
        public int nest;
        public LOOP_TYPE_NAME type;

        // �R���X�g���N�^
        public LOOP_TYPE(int nestNum, LOOP_TYPE_NAME name)
        {
            nest = nestNum;
            type = name;
        }
    }
    public struct ARGUMENT_PASS_DATA
    {
        public object pass;             // �l
        public object mold;             // �^
    }


    public enum LOOP_TYPE_NAME
    {
        NONE,
        FOR,
        WHILE,
        DO_WHILE
    }



    public struct SCOOP_NUM
    {
        public int number;
        public int line;
    }


    public Queue<string> callFuncName =
    new Queue<string>();                        // �Ăяo���֐���

    public string mold = "";                        // �f�[�^�^
    public string funcName = "";                    // �֐���
    public string variableName = "";                // �ϐ���
    //public string leftValname = "";                 // ���ӕϐ�
    public VARIABLE_DATA leftValue = 
        new VARIABLE_DATA(); // ���ӕϐ�
    public VARIABLE_DATA tmpValue =
        new VARIABLE_DATA(); // ���ӕϐ�
    public string switchLeftName = "";              // switch�p�̔�r�f�[�^
    public string caseName = "";                    // case �̒l
    
    public IntPtr *parentValDataName;               // �e�f�[�^�̃|�C���^
    
    public bool argumentFlag = false;               // �����t���O
    public bool argumentCanmaFlag = true;           // �����J���}�t���O
    public bool substitutionFlag = false;           // ����t���O
    public bool ifFlag = false;                     // if���t���O
    public bool ifCheckFlag = false;                // if�������t���O
    public bool skipFlag = false;                   // �X�L�b�v�t���O
    public bool switchFlag = false;                 // �X�C�b�`�t���O
    public bool loopFlag = false;                   // ���[�v�p�t���O
    public bool prefixFlag = false;                 // �O�u�t���O
    public bool arrayFlag = false;                  // �z��p�t���O
    public bool structFlag = false;                 // �\���̗p�t���O
    public bool nextCaseFlag = false;               // case�p�̃t���O
    public bool breakFlag = false;                  // break�p�t���O
    public bool argumentpassFlag = false;           // �����J�n�t���O
    public bool searchFuncFlag = false;             // �֐������J�n�t���O
    public bool funcCheckFlag = false;              // �֐��쐬�����t���O
    public bool dotFlag = false;                    // �ǂ��ƃt���O
    
    
    public bool returnFlag = false;                 // �߂�l�t���O
    public bool callFuncEndFlag = false;            // ();����t���O
    public bool bracketsEndFlag = false;            // ���J�b�R�I���t���O
    public bool nextLoopFlag;                       // for���Ō�̏����t���O
    public bool loopEndFlag = false;                // for���I���t���O
    public bool newLoopFlag = false;                // �C���N�������g�f�N�������g

    public LOOP_TYPE_NAME loopType = new LOOP_TYPE_NAME();

    public int bracketsCount = 0;

    public int ifnestLevel = 0;
    public int allNestLevel = 0;
    public int skipNestLevel = -1;
    public int funcNestLevel = 0;
    public List<int> arrayCountList = new List<int>();                      // �z��
    public Stack<LOOP_TYPE> loopNestLevel = new Stack<LOOP_TYPE>();
    public Stack<int> switchNestLevel = new Stack<int>();
    public Queue<ARGUMENT_PASS_DATA> argumentPass = new Queue<ARGUMENT_PASS_DATA>();

    public textGui.LOOP_NUMBER loopStep = textGui.LOOP_NUMBER.NONE;

    public Stack<SCOOP_NUM> stack = new Stack<SCOOP_NUM>();
    public Stack<int> nestStack = new Stack<int>();

    public List<string> substList = new List<string>();
    

    // �Z�O�̃f�[�^���擾
    public string GetBackNumSubstListData(int number)
	{
        if(substList.Count - number < 0)
		{
            return substList[substList.Count - number];
        }
        return substList[0];
    }
}

public partial class ReadText : MonoBehaviour
{

    // �v���O�^�C�v�錾���e
    static List<DataTableList.FUNC_DATA> prottypeFuncDataList = new List<DataTableList.FUNC_DATA>();
    // �֐���`�ꎞ���e
    static DataTableList.FUNC_DATA funcData = new DataTableList.FUNC_DATA();

    static DataTableList.STRUCT_DATA structData = new DataTableList.STRUCT_DATA();

    // �֐��������n�����
    public static Queue<DataTableList.FUNC_DATA> sendFuncData = new Queue<DataTableList.FUNC_DATA>();

    // �e��f�[�^�i�[
    public static Queue<ReadData> datas = new Queue<ReadData>();

    public static ReadData data = new ReadData();

    public static bool sendFuncFlag = false;                // �Ăяo���֐������n���t���O
    public static bool returnFuncFlag = false;              // �֐��I���t���O
    public static object returnValue;                       // �߂�l

    public static bool structFlag = false;

    public static int structLevel = 0;
    static VARIABLE_DATA valData = new VARIABLE_DATA();

    static Stack<VARIABLE_DATA> varData = new Stack<VARIABLE_DATA>();

    //----------�f�o�b�O�p--------------------------------------------------
    [SerializeField]
    GameObject canvasObj;
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
    [SerializeField]
    GameObject cursorObj;

    //------------------------------------------------------------

    //----------�f�o�b�O�X�^�b�N�p--------------------------------------------------
    static GameObject tmpvTable;
    static GameObject tmpvObj;

    static GameObject tmpfunTable;
    static GameObject tmpfunObj;
    static GameObject tmpskipflagObj;
    static GameObject tmpArrayDataObj;
    static GameObject tmpArrayListObj;
    static GameObject tmpCursorObj;
    static GameObject tmpCanvasObj;
    //------------------------------------------------------------
    static DataTableList.FUNC_DATA fncData;


    void Start()
    {

        /*
        * �����FC���Ń������[�����������ꍇ�́Aunsafe���g���Ƃł���B
        * unsafe�I�v�V������ON�ɂ���K�v������B
        * Unity����playerSetting�ƐV����AssenblyDefinition��ǉ�����K�v������B
        * �������A�A�h���X�͐����݂̂̕\�L�ɂȂ�
        unsafe
        {
           int test2;
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
        tmpCursorObj = cursorObj;
        tmpCanvasObj = canvasObj;
    }
	private void Update()
	{
        skipFLagObj.text =  "skipFlag:"         + data.skipFlag.ToString() + "\n";
        skipFLagObj.text += "nextLoopFlag:"     + data.nextLoopFlag.ToString() + "\n";
        skipFLagObj.text += "loopStep:"         + data.loopStep.ToString() + "\n";
        skipFLagObj.text += "loopType:"         + data.loopType.ToString() + "\n";
        skipFLagObj.text += "leftValueName:"    + data.leftValue.name + "\n";
        skipFLagObj.text += "substList:" ;
        foreach (var str in data.substList)
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
        "return",
    };

    public static readonly string[] symbol = new string[]
    {
        "[","{","}","(",")","=",";",
        ",","+","-","*","/","<",">",
        "|","]","+",":","."
    };

	static void ResetData()
	{
        fncData = new DataTableList.FUNC_DATA();
        fncData.getVariable = new List<VARIABLE_DATA>();
        data.argumentFlag = false;
        data.mold = "";
        data.argumentCanmaFlag = true;
        data.leftValue = new VARIABLE_DATA();
        data.tmpValue = new VARIABLE_DATA();
        data.substitutionFlag = false;
        data.ifFlag = false;
        data.substList.Clear();
        data.bracketsCount = 0;
        data.arrayFlag = false;
        data.arrayCountList.Clear();
        data.switchFlag = false;
        data.breakFlag = false;
        data.nextCaseFlag = false;
        data.argumentPass.Clear();
        DataTable.SetResetItemFlag();
    }

    static public void InitializeData()
	{
        data.prefixFlag = false;
        data.allNestLevel = 0;
        data.bracketsEndFlag = false;
        data.loopFlag = false;
        data.skipFlag = false;
        data.arrayFlag = false;


        data.loopNestLevel.Clear();

        ResetData();
	}

    static public void ResetRink()
	{
        prottypeFuncDataList.Clear();
        data.callFuncName.Clear();
        data.argumentFlag = false;
        data.skipFlag = false;
        funcData = new DataTableList.FUNC_DATA();
        structData = new DataTableList.STRUCT_DATA();
    }

    // �֐���`�쐬
    static public void CreateFuncData(string uiText,int line, int cursorIndex)
	{
        string newSyntax = uiText.TrimEnd(' ');
        if (newSyntax == "\n" || newSyntax == "")
        {
            return;
        }

        // �\���̂͂����ł͓ǂ܂Ȃ�
        if(newSyntax == "struct")
		{
            structFlag = true;
            return;
		}

        if(structFlag)
		{
            /*
            if (SymbolCheck(newSyntax))
			{
                for (int i = 0; i < newSyntax.Length; i++)
                {
                    switch (newSyntax[i])
                    {
                        case '{':
                            data.allNestLevel++;
                            break;  
                        case '}':
                            data.allNestLevel--;
                            if (data.allNestLevel <= structLevel)
                            {
                                structFlag = false;
							}
                            break;
                    }
                }

            }*/
			return;
		}


        if (SymbolCheck(newSyntax))
		{
            for (int i = 0; i < newSyntax.Length; i++)
			{
                if (!data.skipFlag)
                {
                    switch (newSyntax[i])
                    {
                        case '(':
                            data.argumentFlag = true;
                            break;
                        case ')':
                        case ',':
                            if (data.mold != "" && data.variableName != "")
                            {                              
                                // �f�[�^�̊m��
                               VARIABLE_DATA vari = new VARIABLE_DATA();
                                vari.mold = data.mold;
                                vari.name = data.variableName;
                                vari.type = VARIABLE_DATA.DATA_TYPE.INT;
                                if(funcData.getVariable == null)
                                    funcData.getVariable = new List<VARIABLE_DATA>();
                                funcData.getVariable.Add(vari);
                            }

                            break;
                        case ';':
                            if (funcData.returnName != "" && funcData.name != "")
                            {
                                if (data.argumentFlag)
                                {
                                    // �v���g�^�C�v�錾
                                    funcData.begin = funcData.end = cursorIndex;
                                    if(funcData.getVariable == null)
									{
                                        funcData.getVariable = new List<VARIABLE_DATA>();
									}
                                    prottypeFuncDataList.Add(funcData);
                                    funcData = new DataTableList.FUNC_DATA();
                                    data.argumentFlag = false;
                                }
                            }
                            break;
                        case '{':
                            data.argumentFlag = false;
                            funcData.begin = cursorIndex;                  // �J�n�s�ڂ��L�^
                            funcData.line = line;

                            data.allNestLevel++;
                            data.skipNestLevel = data.allNestLevel;
                            data.skipFlag = true;
                            break;
                    }
                }
                // �X�L�b�v�Ή�
                else if (newSyntax[i] == '{')
				{
                    data.allNestLevel++;
				}
                else if(newSyntax[i] == '}')
				{
                    data.allNestLevel--;
                    if(data.skipNestLevel > data.allNestLevel)
					{
                        data.skipFlag = false;
                        data.skipNestLevel = -1;
                        funcData.end = cursorIndex;
                        if (funcData.getVariable == null)
                        {
                            funcData.getVariable = new List<VARIABLE_DATA>();
                        }
                        DataTable.AddFuncData(funcData);
                        funcData = new DataTableList.FUNC_DATA();
                    }
                    data.mold = "";
                    data.variableName = "";
				}
			}
        }
        else
		{
            if (!data.skipFlag)
            {
                // �����t���O�������ꍇ
                if (!data.argumentFlag)
                {
                    // �^�`�F�b�N
                    if (CheckMold(newSyntax,out bool structFlag))
                    {
                        // �߂�l�̌^
                        funcData.returnName = newSyntax;
                    }
                    else
                    {
                        // �֐����̐ݒ�
                        if (funcData.returnName != "" && funcData.name == null)
                        {
                            funcData.name = newSyntax;
                        }
                    }
                }
                // ��������
                else
                {
                    if (data.mold == "")
                    {
                        // �^�`�F�b�N
                        if (CheckMold(newSyntax, out bool structFlag))
                        {
                            // ��U�f�[�^�^��ۑ�
                            data.mold = newSyntax;
                        }
                    }
                    else
                    {
                        // �ϐ���
                        data.variableName = newSyntax;
                    }
                }
            }
        }
    }

    static public void CreateStructData(string uiText, int line, int cursorIndex)
	{
        string newSyntax = uiText.TrimEnd(' ');
        if (newSyntax == "\n" || newSyntax == "")
        {
            return;
        }

        if (newSyntax == "struct")
        {
            structFlag = true;
            return;
        }

        if(structFlag)
		{
            if (SymbolCheck(newSyntax))
			{
                if(structData.name != "")
				{
                    for (int i = 0; i < newSyntax.Length; i++)
                    {
                        switch (newSyntax[i])
                        {
                            case ';':
                               VARIABLE_DATA val = new VARIABLE_DATA();
                                val.mold = valData.mold;
                                val.name = valData.name;
                                val.type = VARIABLE_DATA.DATA_TYPE.INT;         // �Վ��Ή�
                                if(structData.variable_data == null)
								{
                                    structData.variable_data = new List<VARIABLE_DATA>();
								}
                                structData.variable_data.Add(val);
                                data = new ReadData();
                                break;
                            case '{':
                                data.allNestLevel++;
                                break;
                            case '}':
                                data.allNestLevel--;
                                // �\���̂̏I���
                                if(data.allNestLevel <= structLevel)
								{
                                    structFlag = false;
                                    DataTable.SetStructData(structData);
								}
                                break;
                        }
                    }
                }
                else
				{
                    // ���O���w�肳��Ă��Ȃ��B
                    Debug.Log("�\���̖̂��O���ݒ肳��Ă��܂���B");
				}
			}
            else
			{
                if (structData.name == null)
                {
                    // �^�`�F�b�N
                    if (!CheckMold(newSyntax, out bool structFlag))
                    {
                        structData.name = newSyntax;
                        structLevel = data.allNestLevel;
                    }
                }
                else
                {
                    if (CheckMold(newSyntax, out bool structFlag))
                    {
                        if(valData.mold == null)
						{
                            valData.mold = newSyntax;
                        }
                    }
                    else
					{
                        // �^���w�肳��Ă���ꍇ
                        if(valData.mold != null)
						{
                            valData.name = newSyntax;
                        }
					}
                }
			}
        }
    }

    static unsafe public void GetText(string uiText,int line,int cursorIndex)
	{
        string newSyntax = uiText.TrimEnd(' ');

        data.nextLoopFlag = false;
        data.loopEndFlag = false;
        
        data.callFuncEndFlag = false;
        returnFuncFlag = false;

        if(returnValue != null)
		{
            data.substList.Add((string)returnValue);
		}
        sendFuncFlag = false;

        if (newSyntax == "\n" || newSyntax == "")
		{
            return;
		}
        // �L���̏ꍇ
        if(SymbolCheck(newSyntax))
		{
            // case �̏���������Ȃ������Ƃ�
            if(data.switchFlag)
                if(data.nextCaseFlag)
                    return;

            // ���J�b�R���I����Ă���
            if (data.bracketsEndFlag)
            {
                if (data.ifnestLevel > 0) data.ifnestLevel--;
            }
            // �J�b�R�I���t���O������
            data.bracketsEndFlag = false;
            
            for (int i=0; i < newSyntax.Length;i++)
            {
                switch (newSyntax[i])
                {
                    case ';':
                        if(data.breakFlag)
						{
                            data.skipFlag = true;
                            // ���[�v�̏ꍇ
                            if (data.loopNestLevel.Count != 0)
                            {
                                data.skipNestLevel = data.loopNestLevel.Pop().nest;
                            }
                            // switch�̏ꍇ
                            else if (data.switchNestLevel.Count != 0)
                            {
                                data.skipNestLevel = data.switchNestLevel.Pop();
                            }
                        }
                        else
						{
                            if (data.returnFlag)
                            {
                                if (sendFuncData.Count != 0)
                                {
                                    if (sendFuncData.Peek().returnName != "void")
                                    {
                                        var val = arithmeticCheck.Check(data.substList, out string mold);
                                        // �߂�l�̌^�������Ă��邩�`�F�b�N
                                        if (mold == sendFuncData.Peek().returnName)
                                        {
                                            returnValue = val;
                                            returnFuncFlag = true;
                                            data = datas.Dequeue();
                                            
                                        }
                                    }
                                    else
									{
                                        // �߂�l��void�̎�
									}
                                }
                                data.returnFlag = false;
                                return;
                            }
                            else if(sendFuncFlag)
							{
                                // ();�̃p�^�[���̎��͈�U�X�L�b�v-
							}
                            else
							{

                                // �������
                                Substitution(data.substList, ref data.leftValue);
                            }

                            // for���`�F�b�N
                            if (data.loopFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.TERM)
                            {
                                if (ifcheck.CheckConditions(data.substList))
                                {
                                    data.nextLoopFlag = true;
                                }
                                else
                                {
                                    data.skipFlag = true;
                                    data.skipNestLevel = data.allNestLevel;
                                    data.loopNestLevel.Pop();
                                    data.loopFlag = false;
                                    //nextLoopFlag = true;
                                    data.loopEndFlag = true;
                                    if (data.loopNestLevel.Count != 0)
                                    {
                                        // textGui.cs���Ŕ��f����悤�ɕۑ�
                                        data.loopType = data.loopNestLevel.Peek().type;
                                    }
                                }
                            }
                            else if (data.loopFlag)
                            {
                                data.nextLoopFlag = true;
                            }
                        }
                        
                        /*
                        // �֐���`
                        else if (fncData.name != "" && fncData.name != null)
                        {
                            // �s��
                            fncData.begin = line;
                            // �֐��o�^
                            DataTable.AddFuncData(fncData);
                        }*/

                        ResetData();

                        break;
                   
                    case '(':
                        if (data.loopFlag)
                        {
                            // textGui.cs���Ŕ��f����悤�ɕۑ�
                            data.loopType = data.loopNestLevel.Peek().type;

                            switch (data.loopType)
                            {
                                case ReadData.LOOP_TYPE_NAME.FOR:

                                    data.nextLoopFlag = true;
                                    break;
                                case ReadData.LOOP_TYPE_NAME.WHILE:
                                    data.ifFlag = true;
                                    data.nextLoopFlag = true;
                                    break;
                            }
                            data.newLoopFlag = false;

                        }
                        else
						{
                            /*
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
                            
                            else*/
                            if (data.substitutionFlag || data.ifFlag || data.switchFlag)
                            {
                                data.substList.Add(newSyntax[i].ToString());
                            }
                            else if (!int.TryParse(data.substList[data.substList.Count - 1], out int result))
                            {
                                if (CheckFunctionData(data.substList[data.substList.Count - 1], line, out DataTableList.FUNC_DATA func))
                                {
                                    data.callFuncName.Enqueue(func.name);
                                    data.argumentpassFlag = true;
                                    
                                }
                                else
                                {
                                    Debug.Log("�֐��͂Ȃ�:" + data.substList[data.substList.Count - 1]);
                                }
                            }
                            
                            
                            
                        }

                        data.bracketsCount++;
                        break;
                    case ')':
                        data.argumentFlag = false;

                        data.bracketsCount--;
                        // ���[�v�ꍇ
                        if(data.loopFlag)
						{
                            switch (data.loopNestLevel.Peek().type)
							{
                                case ReadData.LOOP_TYPE_NAME.FOR:
                                    if (textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT)
                                    {
                                        data.nextLoopFlag = true;
                                        // �������
                                        Substitution(data.substList,ref data.leftValue);
                                    }
                                    break;
                                case ReadData.LOOP_TYPE_NAME.WHILE:
                                    if (!ifcheck.CheckConditions(data.substList))
									{
                                        data.skipFlag = true;
                                        data.skipNestLevel = data.allNestLevel;
                                        data.loopNestLevel.Pop();
                                        data.loopFlag = false;
                                        data.loopEndFlag = true;
                                        if(data.loopNestLevel.Count != 0)
										{
                                            // textGui.cs���Ŕ��f����悤�ɕۑ�
                                            data.loopType = data.loopNestLevel.Peek().type;
                                        }
                                    }
                                    else
									{
                                        data.nextLoopFlag = true;
									}
                                    break;
                            }
                            //�Ȃ�ł��킩��Ȃ����ǁA���ꂪ�����ƃ_��
                            // ������������Askipflag����艻���B�B�B�B
                            ResetData();    
                        }
                        else if (data.argumentpassFlag)
                        {
                            SetArgumentPass();

                            // �֐��Ăяo��
                            if (DataTable.GetFuncOneData(data.callFuncName.Peek(), out DataTableList.FUNC_DATA fd, data.argumentPass))
                            {
                                // �֐������������ꍇ
                                sendFuncFlag = true;
                                data.substList.RemoveAt(data.substList.Count - 1);
                                sendFuncData.Enqueue(fd);
                                datas.Enqueue(data);
                                data = new ReadData();
                            }
                            data.argumentpassFlag = false;
                            // ();�̃p�^�[�����
                            if(newSyntax[i + 1] == ';')
							{
                                data.callFuncEndFlag = true;
                                return;
                            }
                        }
                        else if (data.substitutionFlag || data.ifFlag)
                        {
                            data.substList.Add(newSyntax[i].ToString());

                            if (data.bracketsCount == 0 && data.ifFlag)
							{
                                // if���̃`�F�b�N
                                data.ifCheckFlag = ifcheck.CheckConditions(data.substList);
                                if(!data.ifCheckFlag)
								{
                                    data.skipNestLevel = data.allNestLevel;
                                    data.skipFlag = true;
                                }
                                else
								{
                                    data.skipFlag = false;

                                }
                            }
                            break;
                        }
                        //ResetData();   // ��U�R�����g�A�E�g
                        break;
                    case '{':
                        data.allNestLevel++;
                        data.ifFlag = false;

                        ScoopPush(line);

                        if (data.loopFlag)
                        {
                            switch (data.loopNestLevel.Peek().type)
                            {
                                case ReadData.LOOP_TYPE_NAME.FOR:
                                    data.nextLoopFlag = true;
                                    break;
                                case ReadData.LOOP_TYPE_NAME.WHILE:
                                    break;
                            }
                            data.nextLoopFlag = true;
                            data.loopFlag = false;
                        }
                        if (data.ifCheckFlag)
                        {
                            // if����true�ɂȂ����ꍇ�̏���������

                        }
                        ResetData();

                        break;
                    case '}':
                        data.allNestLevel--;

                        // �X�L�b�v�����s�����z�����ꍇ
                        if (data.skipNestLevel >= data.allNestLevel)
                        {
                            if(data.skipFlag)
							{
                                data.skipFlag = false;
                                data.skipNestLevel = -1;
                            }
                        }
                        //skipNestLevel = -1;
                        data.bracketsEndFlag = true;

                        DataTable.DeleteVariableScoopData(data.allNestLevel);

                        // ���[�v�l�X�g���I�������ꍇ
                        if (data.loopNestLevel.Count != 0)
						{
                            if (data.loopNestLevel.Peek().nest == data.allNestLevel)
                            {
								switch (data.loopNestLevel.Peek().type)
								{
                                    case ReadData.LOOP_TYPE_NAME.FOR:
                                        data.nextLoopFlag = true;
                                        data.loopFlag = true;
                                        break;
                                    case ReadData.LOOP_TYPE_NAME.WHILE:
                                        data.nextLoopFlag = true;
                                        data.loopFlag = true;
                                        data.ifFlag = true;
                                        break;
                                }
                            }
                        }
                        // �Ăяo����Ă���ꍇ�͂��̏�ɖ߂�
                        else if (sendFuncData.Count != 0)
                        {
                            returnFuncFlag = true;
                        }

                        // �֐����甲�����ꍇ
                        else if(data.funcNestLevel == data.allNestLevel)
						{
                            data.funcName = "";
						}
                        ScoopPop();
                        break;
                    case '=':

                        // ���ӂ��ϐ��錾�ł͂Ȃ������ꍇ
                        if(data.tmpValue.mold != null)
						{
                            data.leftValue = data.tmpValue;
                            
                            data.substList.RemoveAt(data.substList.Count - 1);
                            data.tmpValue = new VARIABLE_DATA();
                        }
                        
                        
                        if (data.skipFlag && data.skipNestLevel != -1)
                        {
                            // �X�L�b�v�����s�����z�����ꍇ
                            if (data.skipNestLevel >= data.allNestLevel)
                            {
                                data.skipNestLevel = -1;
                            }

                            return;
                        }
                        data.substitutionFlag = true;

                        if((data.ifFlag) || (data.loopFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.TERM))
                        {
                            data.substList.Add(newSyntax[i].ToString());
                        }
                        else if(data.substList.Count >= 1)
						{
                            // += -=�@�̏ꍇ
                            if (data.substList[data.substList.Count - 1] == "-" || data.substList[data.substList.Count - 1] == "+")
                            {
                                data.substList.RemoveAt(data.substList.Count - 2);
                                data.substList.Insert(0, data.leftValue.value.ToString());
                            }
                        }

                        break;
                    case ',':
                        // �J���}����
                        data.argumentCanmaFlag = true;
                        // �Ăяo����
                        if(data.argumentpassFlag)
						{
                            SetArgumentPass();
                        }
                        break;

                    case '+':
                    case '-':
                        // �C���N�������g��A�v�Z��for���̎��͍Ō�̃X�e�b�v�̎��̂�
                        if ((data.loopFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT) ||
                            (!data.loopFlag) && !data.skipFlag)
                        {
                            if (data.substList.Count >= 1)
                            {
                                if(SetOpeData(newSyntax[i].ToString(),out VARIABLE_DATA vARIABLE_DATA))
								{
                                    data.leftValue = vARIABLE_DATA;
                                    data.substitutionFlag = true;
                                    continue;
								}
                            }
                            data.substList.Add(newSyntax[i].ToString());
                        }
                        break;
                   /* case '-':
                        if ((data.loopFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT) ||
                            (!data.loopFlag) && !data.skipFlag)
                        {
                            if (data.substList.Count >= 1)
                            {
                                if (SetOpeData(newSyntax[i].ToString()))
                                {
                                    break;
                                }
                            }
                            data.substList.Add(newSyntax[i].ToString());
                        }
                        break;}*/
                    case '[':
                        // �z��̏ꍇ
                        // (�錾�j�f�[�^�^���ϐ�������`����Ă���ꍇ�̂�
                        //if(mold != "" && leftValname  != "")
						{
                            data.arrayFlag = true;
                        }
                        break;
                    case ']':
                        // �z��̏I���
                        if(data.arrayFlag)
						{
                            // ���l�̏ꍇ
                            if (int.TryParse(data.substList[data.substList.Count - 1], out int result))
							{
                                data.arrayCountList.Add(result);
                                data.substList.RemoveAt(data.substList.Count - 1);
                            }
                            else
							{
                                // �ϐ�����`����Ă���ꍇ
                                if(CheckVarialbleData(data.substList[data.substList.Count - 1]))
								{
                                    DataTable.GetVariableValueData(data.substList[data.substList.Count - 1],out VARIABLE_DATA vData);
                                    data.arrayCountList.Add(int.Parse((string)vData.value));
                                    data.substList.RemoveAt(data.substList.Count - 1);
                                }
							}
                            // �ϐ��錾�̎��͓���Ȃ�
                            if(data.substList.Count > 0)
							{
                                // ����̏ꍇ
                                if(data.substitutionFlag)
								{
                                   VARIABLE_DATA vd;
                                    if (CheckVarialbleData(data.substList[data.substList.Count - 1], data.arrayCountList, out vd))
                                    {
                                        data.substList.RemoveAt(data.substList.Count - 1);
                                        data.substList.Add(GetArrayData(vd, data.arrayCountList));
                                    }
                                }
                            }
                        }
                        break;
                    case ':':
                        if(data.switchNestLevel.Count != 0)
						{
                            // �R�������͎��ɒl���r����
                            if (int.TryParse(data.substList[data.substList.Count - 1], out int result))
                            {
                                data.caseName = result.ToString();
                            }
                            else
                            {
                                // �ϐ�����`����Ă���ꍇ
                                if (DataTable.GetVariableValueData(data.substList[data.substList.Count - 1], out VARIABLE_DATA vdata))
								{
                                    data.caseName = vdata.value.ToString();
                                }
                            }
                            // �X�C�b�`����
                            if(data.switchLeftName != data.caseName)
							{
                                data.nextCaseFlag = true;
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
                        data.substList.Add(newSyntax[i].ToString());
                        break;
                    case '.':
                        data.substList.Add(newSyntax[i].ToString());
                        data.dotFlag = true;
                        break;
                }
            }
        }
        else
		{
            // �l�X�g�X�L�b�v�̏ꍇ
            if(data.skipFlag && data.skipNestLevel != -1)
			{
                return;
            }
            else if (data.nextCaseFlag && newSyntax != "case")
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
                        data.ifFlag = true;
                        data.ifnestLevel++;

                        break;
                    case "else":
						#region else���̏���
						// if������`����Ă���
						if (data.ifnestLevel > 0)
						{
                            // if����false��������
                            if(!data.ifCheckFlag)
							{
                                data.skipNestLevel = -1;
                                data.skipFlag = false;
                            }
                            else
							{
                                data.skipFlag = true;
                                // true�̏ꍇ�͓ǂݔ�΂�
                                data.skipNestLevel = data.allNestLevel;
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
                        data.loopFlag = true;
                        data.newLoopFlag = true;
                        // ���g�̃l�X�g��ۑ�����
                        data.loopNestLevel.Push(new ReadData.LOOP_TYPE(data.allNestLevel, ReadData.LOOP_TYPE_NAME.FOR));
                        data.loopType = data.loopNestLevel.Peek().type;
                        break;
                    case "while":
                        data.loopFlag = true;
                        data.newLoopFlag = true;
                        data.nextLoopFlag = true;
                        data.loopNestLevel.Push(new ReadData.LOOP_TYPE(data.allNestLevel, ReadData.LOOP_TYPE_NAME.WHILE));
                        data.loopType = data.loopNestLevel.Peek().type;
                        break;
                    case "switch":
                        data.switchFlag = true;
                        data.switchNestLevel.Push(data.allNestLevel);
                        break;
                    case "case":
                        data.nextCaseFlag = false;
                        break;
                    case "break":
                        data.breakFlag = true;
                        break;
                    case "return":
                        data.returnFlag = true;
                        break;
                }
				#endregion
			}

			// �֐�����(�錾��)
			#region �S�~
			/*
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
                       VARIABLE_DATA vd = newVARIABLE_DATA();
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
            */
			#endregion
			else
			{
                data.substList.Add(newSyntax);
                // �C���N�������g�E�f�N�������g�p�O�u�^
                if (data.prefixFlag)
                {
                    // �ϐ��̏ꍇ
                    if (CheckVarialbleData(newSyntax))
                    {
                        List<string> tmp = new List<string>();
                        tmp.Add(newSyntax); tmp.Add(data.substList[data.substList.Count - 2]); tmp.Add("1");
                        Substitution(tmp, ref data.leftValue, true);
                        data.substList.RemoveAt(data.substList.Count - 3);
                        data.substList.RemoveAt(data.substList.Count - 2);
                        data.substList[data.substList.Count - 1] = data.leftValue.value.ToString();
                    }
                    else
                    {
                        // �ϐ��ł͂Ȃ��̂ŁA�G���[
                        Debug.LogError("�C���N�������g�E�f�N�������g�G���[");
                    }
                    data.prefixFlag = false;
                }
                // �h�b�g������ꍇ
                else if (data.dotFlag)
                {
                    // �h�b�g�O�̒l���ϐ���������
                    if (DataTable.GetVariableChildData(data.GetBackNumSubstListData(2), newSyntax, out VARIABLE_DATA vData))
					{
                        varData.Push(vData);
                       
                    }
                }
                else if (data.switchFlag)
                {
                    if (data.substList[data.substList.Count - 2] == "(")
                    {
                        // �\��ꂶ��Ȃ��Ƃ�
                        if ((!CheckReservedWord(newSyntax)))
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
                                    DataTable.GetVariableValueData(data.substList[data.substList.Count - 1], out VARIABLE_DATA vARIABLE_DATA);
                                    name = vARIABLE_DATA.value.ToString();
                                }
                            }
                            data.switchLeftName = name;
                        }
                    }
                    else
                    {
                        // �J�b�R���K�v����
                    }
                }
                else if (data.substitutionFlag || data.ifFlag)
                {
                    // ���ӂ������ꍇ
                    if (!data.ifFlag && data.leftValue.name == "")
                    {
                        data.leftValue.name = data.substList[data.substList.Count - 2];
                        data.substList.RemoveAt(data.substList.Count - 2);
                    }
                    else
                    {
                        // �ϐ��������ꍇ
                        if (CheckVarialbleData(newSyntax))
                        {
                            DataTable.SetVariableItemFlag(newSyntax);
                            data.substList.RemoveAt(data.substList.Count - 1);
                            DataTable.GetVariableValueData(newSyntax, out VARIABLE_DATA vARIABLE_DATA);

                            data.substList.Add(vARIABLE_DATA.value.ToString());
                            
                        }
                    }
                }
                //else if (mold == "" && fncData.returnName == null)
                else if (data.tmpValue.mold == null)
                {
                    // �^�̃`�F�b�N
                    if (CheckMold(newSyntax, out bool structFlag))
                    {
                        data.tmpValue.mold = newSyntax;
                        data.tmpValue.type = structFlag ? VARIABLE_DATA.DATA_TYPE.STRUCT : VARIABLE_DATA.DATA_TYPE.INT;
                        //data.structFlag = structFlag;
                        return;                 // �^�w��ׁ̈A�I��
                    }
                    else
					{
                        //data.substList.RemoveAt(data.substList.Count - 1);
                        if (!DataTable.GetVariableValueData(newSyntax, out data.tmpValue))
						{
                            Debug.LogError("�ϐ����錾����Ă��܂���B:" + newSyntax);
						}
                        else
						{
                            data.tmpValue.scoopNum = data.allNestLevel;
                            //data.leftValue = data.tmpValue;
                            //data.tmpValue = new VARIABLE_DATA();
                        }
                    }
                }
                else
                {
                    if (!data.returnFlag)
                    {
                        // �ϐ��錾
                        if (data.tmpValue.mold != null)
                        {
                            data.tmpValue.name = newSyntax;
                            VariableDeclaration(data.tmpValue, ref data.leftValue);
                            data.leftValue.scoopNum = data.allNestLevel;
                            data.tmpValue = new VARIABLE_DATA();

                            data.substList.RemoveAt(data.substList.Count - 1);
                            data.substList.RemoveAt(data.substList.Count - 1);
                        }
                    }
                }
            }
            // ���J�b�R���I����Ă���
            if(data.bracketsEndFlag)
			{
                if(data.ifnestLevel > 0) data.ifnestLevel--;
            }

            data.bracketsEndFlag = false;
        }
        
    }

    static void SetArgumentPass()
	{
        ReadData.ARGUMENT_PASS_DATA apd = new ReadData.ARGUMENT_PASS_DATA();
        // �ϐ������邩�`�F�b�N
        if (CheckVarialbleData(data.substList[data.substList.Count - 1], out VARIABLE_DATA vd))
        {
            // �z��̏ꍇ
            if (vd.type == VARIABLE_DATA.DATA_TYPE.ARRAY)
            {
                if (CheckVarialbleData(vd.name, data.arrayCountList, out vd))
                {
                    data.substList.RemoveAt(data.substList.Count - 1);
                    apd.pass = GetArrayData(vd, data.arrayCountList);
                    apd.mold = vd.mold;
                    data.argumentPass.Enqueue(apd);
                }
            }
            else
            {
                apd.pass = vd.value;
                apd.mold = vd.mold;
                data.argumentPass.Enqueue(apd);
            }
        }
        else
        {
            apd.pass = data.substList[data.substList.Count - 1];
            apd.mold = "int";
            data.argumentPass.Enqueue(apd);
        }
    }

    // �C���N�������g�Ή�
    static bool SetOpeData(string ope,out VARIABLE_DATA outData)
	{
        // ��u�̏ꍇ�̂�true
        bool result = false;
        outData = new VARIABLE_DATA();
        // �C���N�������g�Ή�
        if (ope == "+" || ope == "-")
        {
            if(data.substList[data.substList.Count - 1] == ope)
			{
                List<string> tmp = new List<string>();
                if (data.substList.Count >= 2)
                {
                    // ��u�^
                    if (DataTable.GetVariableValueData(data.substList[data.substList.Count - 2], out VARIABLE_DATA vdata))
                    {
                        outData = vdata;
                        data.substList[data.substList.Count - 2] = vdata.value.ToString();
                        data.substList.Add("1");
                    }
                    result = true;
                }
                else
                {
                    // �O�u�^
                    // �C���N�������g�����߂̏ꍇ
                    data.prefixFlag = true;
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
    static void VariableDeclaration(VARIABLE_DATA vData,ref VARIABLE_DATA returnData)
	{
        /*
        if(name != "" && setMold != "")
		{
           VARIABLE_DATA ValData = newVARIABLE_DATA() ;
            ValData.name = name;
            ValData.mold = setMold;
            ValData.value = 0;
            ValData.scoopNum = data.allNestLevel;

            // �z��ON�̏ꍇ
            if (data.arrayFlag)         DataTable.AddVariableData(vData, data.arrayCountList);
            else if(data.structFlag)    DataTable.AddVariableData(vData, data.structFlag);
            else                        DataTable.AddVariableData(vData);
        }
        */
        if (data.arrayFlag) DataTable.AddVariableData(vData, data.arrayCountList,ref returnData);
        else DataTable.AddVariableData(vData, out returnData);
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
    static void Substitution(List<string> list,ref VARIABLE_DATA leftDataValue, bool flag = false)
	{
        if(!flag)
		{
            flag = data.substitutionFlag;
        }
        // ����`�F�b�N
        if (flag)
        {
            var val = arithmeticCheck.Check(list,out string mold);
            DataTable.SetVarialbleData(ref leftDataValue, val, data.arrayCountList);
            /*
            // �ϐ����`�F�b�N
            if (CheckVarialbleData(subName))
            {
                DataTable.SetVarialbleData(subName, val, data.arrayCountList);
            }
           
            // �����̃`�F�b�N
            else if (DataTable.SetFuncVarialbleData(data.funcName, subName, val))
            {
                // ��`����Ă��Ȃ��ϐ��ɑ�����悤�Ƃ��Ă���

            } */
        }
    }

    static void ScoopPush(int line)
	{
        ReadData.SCOOP_NUM sn;
        sn.number = data.allNestLevel;
        sn.line = line;
        data.stack.Push(sn);
    }

    static void ScoopPop()
    {
        if(data.stack.Count > 0) data.stack.Pop();
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
                    if (data.type == VARIABLE_DATA.DATA_TYPE.ARRAY)
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
                        if(data.selectItemFlag)
						{
                            objVar.SetVisibleImg();
                        }
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
                    if (data.returnName != "void")
					{
                        
                        sfd.CreateReturnMold(data.returnName);
                        
                    }
                    sfd.SetFuncNameText(data.name);

                    if (data.getVariable != null)
                    {
                        foreach (var n in data.getVariable)
                        {
                            sfd.CreateMold((string)n.mold);
                            //sfd.SetValText(n.mold, n.name,n.value);
                        }
                    }
                    obj.transform.parent = tmpfunTable.transform;
                }
			}
        }
		#endregion
	}

    static void SetAllVarData(SetVariData obj, string varName,object moldName, object value)
	{
        obj.SetMolText(moldName);
        obj.SetValNameText(varName);
        obj.SetValueText(value);
    }

    static bool CheckFunctionData(string funcName, int line, out DataTableList.FUNC_DATA fData)
	{
        // �֐���`�����邩�`�F�b�N
        foreach (var fucData in DataTable.GetFunctionDataLIst())
        {
            if (fucData.name == funcName)
            {
                if (funcData.line <= line)
                {
                    fData = fucData;
                    return true;
                }
            }
        }
        foreach (var data in prottypeFuncDataList)
		{
            if(data.name == funcName)
			{
                // �Ăяo������Ƀv���g�^�C�v�錾������ꍇ
                if(data.line <= line)
				{
                    // �֐���`�����邩�`�F�b�N
                    foreach(var fucData in DataTable.GetFunctionDataLIst())
					{
                        if(data == fucData)
						{
                            fData = fucData;
                            return true;
						}
					}
				}
			}
		}
        fData = new DataTableList.FUNC_DATA();
        return false;
	}

    static bool CheckVarialbleData(string val,out VARIABLE_DATA vd)
	{
        foreach(var data in  DataTable.GetVarialbleDataList())
		{
            if(data.name == val)
			{
                vd = data;
                return true;
			}
		}
        vd = new VARIABLE_DATA();
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

    static bool CheckVarialbleData(string val,List<int> _arrayList,out VARIABLE_DATA vd)
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
        vd = new VARIABLE_DATA();
        return false;
    }

    static string GetArrayData(VARIABLE_DATA data,List<int>_arrayList)
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

    static bool CheckMold(string tex,out bool structFlag)
	{
        // �^�`�F�b�N
        foreach (var st in cName)
        {
            if (st == tex)
            {
                structFlag = false;
                return true;
            }
        }

        foreach(var st in DataTable.GetStructDataLIst())
		{
            if(st.name == tex)
			{
                structFlag = true;
                return true;
			}
		}
        structFlag = false;
        return false;
    }
}


