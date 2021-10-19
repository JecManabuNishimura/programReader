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

        // コンストラクタ
        public LOOP_TYPE(int nestNum, LOOP_TYPE_NAME name)
        {
            nest = nestNum;
            type = name;
        }
    }
    public struct ARGUMENT_PASS_DATA
    {
        public object pass;             // 値
        public object mold;             // 型
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
    new Queue<string>();                        // 呼び出し関数名

    public string mold = "";                        // データ型
    public string funcName = "";                    // 関数名
    public string variableName = "";                // 変数名
    //public string leftValname = "";                 // 左辺変数
    public VARIABLE_DATA leftValue = 
        new VARIABLE_DATA(); // 左辺変数
    public VARIABLE_DATA tmpValue =
        new VARIABLE_DATA(); // 左辺変数
    public string switchLeftName = "";              // switch用の比較データ
    public string caseName = "";                    // case の値
    
    public IntPtr *parentValDataName;               // 親データのポインタ
    
    public bool argumentFlag = false;               // 引数フラグ
    public bool argumentCanmaFlag = true;           // 引数カンマフラグ
    public bool substitutionFlag = false;           // 代入フラグ
    public bool ifFlag = false;                     // if文フラグ
    public bool ifCheckFlag = false;                // if文判定後フラグ
    public bool skipFlag = false;                   // スキップフラグ
    public bool switchFlag = false;                 // スイッチフラグ
    public bool loopFlag = false;                   // ループ用フラグ
    public bool prefixFlag = false;                 // 前置フラグ
    public bool arrayFlag = false;                  // 配列用フラグ
    public bool structFlag = false;                 // 構造体用フラグ
    public bool nextCaseFlag = false;               // case用のフラグ
    public bool breakFlag = false;                  // break用フラグ
    public bool argumentpassFlag = false;           // 引数開始フラグ
    public bool searchFuncFlag = false;             // 関数検索開始フラグ
    public bool funcCheckFlag = false;              // 関数作成完了フラグ
    public bool dotFlag = false;                    // どっとフラグ
    
    
    public bool returnFlag = false;                 // 戻り値フラグ
    public bool callFuncEndFlag = false;            // ();回避フラグ
    public bool bracketsEndFlag = false;            // 中カッコ終わりフラグ
    public bool nextLoopFlag;                       // for文最後の処理フラグ
    public bool loopEndFlag = false;                // for文終了フラグ
    public bool newLoopFlag = false;                // インクリメントデクリメント

    public LOOP_TYPE_NAME loopType = new LOOP_TYPE_NAME();

    public int bracketsCount = 0;

    public int ifnestLevel = 0;
    public int allNestLevel = 0;
    public int skipNestLevel = -1;
    public int funcNestLevel = 0;
    public List<int> arrayCountList = new List<int>();                      // 配列数
    public Stack<LOOP_TYPE> loopNestLevel = new Stack<LOOP_TYPE>();
    public Stack<int> switchNestLevel = new Stack<int>();
    public Queue<ARGUMENT_PASS_DATA> argumentPass = new Queue<ARGUMENT_PASS_DATA>();

    public textGui.LOOP_NUMBER loopStep = textGui.LOOP_NUMBER.NONE;

    public Stack<SCOOP_NUM> stack = new Stack<SCOOP_NUM>();
    public Stack<int> nestStack = new Stack<int>();

    public List<string> substList = new List<string>();
    

    // 〇個前のデータを取得
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

    // プログタイプ宣言内容
    static List<DataTableList.FUNC_DATA> prottypeFuncDataList = new List<DataTableList.FUNC_DATA>();
    // 関数定義一時内容
    static DataTableList.FUNC_DATA funcData = new DataTableList.FUNC_DATA();

    static DataTableList.STRUCT_DATA structData = new DataTableList.STRUCT_DATA();

    // 関数情報引き渡し情報
    public static Queue<DataTableList.FUNC_DATA> sendFuncData = new Queue<DataTableList.FUNC_DATA>();

    // 各種データ格納
    public static Queue<ReadData> datas = new Queue<ReadData>();

    public static ReadData data = new ReadData();

    public static bool sendFuncFlag = false;                // 呼び出し関数引き渡しフラグ
    public static bool returnFuncFlag = false;              // 関数終了フラグ
    public static object returnValue;                       // 戻り値

    public static bool structFlag = false;

    public static int structLevel = 0;
    static VARIABLE_DATA valData = new VARIABLE_DATA();

    static Stack<VARIABLE_DATA> varData = new Stack<VARIABLE_DATA>();

    //----------デバッグ用--------------------------------------------------
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

    //----------デバッグスタック用--------------------------------------------------
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
        * メモ：C＃でメモリーを扱いたい場合は、unsafeを使うとできる。
        * unsafeオプションをONにする必要がある。
        * Unity側のplayerSettingと新しくAssenblyDefinitionを追加する必要がある。
        * ただし、アドレスは数字のみの表記になる
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

    // 関数定義作成
    static public void CreateFuncData(string uiText,int line, int cursorIndex)
	{
        string newSyntax = uiText.TrimEnd(' ');
        if (newSyntax == "\n" || newSyntax == "")
        {
            return;
        }

        // 構造体はここでは読まない
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
                                // データの確定
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
                                    // プロトタイプ宣言
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
                            funcData.begin = cursorIndex;                  // 開始行目を記録
                            funcData.line = line;

                            data.allNestLevel++;
                            data.skipNestLevel = data.allNestLevel;
                            data.skipFlag = true;
                            break;
                    }
                }
                // スキップ対応
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
                // 引数フラグが無い場合
                if (!data.argumentFlag)
                {
                    // 型チェック
                    if (CheckMold(newSyntax,out bool structFlag))
                    {
                        // 戻り値の型
                        funcData.returnName = newSyntax;
                    }
                    else
                    {
                        // 関数名の設定
                        if (funcData.returnName != "" && funcData.name == null)
                        {
                            funcData.name = newSyntax;
                        }
                    }
                }
                // 引数判定
                else
                {
                    if (data.mold == "")
                    {
                        // 型チェック
                        if (CheckMold(newSyntax, out bool structFlag))
                        {
                            // 一旦データ型を保存
                            data.mold = newSyntax;
                        }
                    }
                    else
                    {
                        // 変数名
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
                                val.type = VARIABLE_DATA.DATA_TYPE.INT;         // 臨時対応
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
                                // 構造体の終わり
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
                    // 名前が指定されていない。
                    Debug.Log("構造体の名前が設定されていません。");
				}
			}
            else
			{
                if (structData.name == null)
                {
                    // 型チェック
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
                        // 型が指定されている場合
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
        // 記号の場合
        if(SymbolCheck(newSyntax))
		{
            // case の条件が合わなかったとき
            if(data.switchFlag)
                if(data.nextCaseFlag)
                    return;

            // 中カッコが終わっている
            if (data.bracketsEndFlag)
            {
                if (data.ifnestLevel > 0) data.ifnestLevel--;
            }
            // カッコ終了フラグを下す
            data.bracketsEndFlag = false;
            
            for (int i=0; i < newSyntax.Length;i++)
            {
                switch (newSyntax[i])
                {
                    case ';':
                        if(data.breakFlag)
						{
                            data.skipFlag = true;
                            // ループの場合
                            if (data.loopNestLevel.Count != 0)
                            {
                                data.skipNestLevel = data.loopNestLevel.Pop().nest;
                            }
                            // switchの場合
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
                                        // 戻り値の型が合っているかチェック
                                        if (mold == sendFuncData.Peek().returnName)
                                        {
                                            returnValue = val;
                                            returnFuncFlag = true;
                                            data = datas.Dequeue();
                                            
                                        }
                                    }
                                    else
									{
                                        // 戻り値がvoidの時
									}
                                }
                                data.returnFlag = false;
                                return;
                            }
                            else if(sendFuncFlag)
							{
                                // ();のパターンの時は一旦スキップ-
							}
                            else
							{

                                // 代入処理
                                Substitution(data.substList, ref data.leftValue);
                            }

                            // for文チェック
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
                                        // textGui.cs内で判断するように保存
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
                        // 関数定義
                        else if (fncData.name != "" && fncData.name != null)
                        {
                            // 行数
                            fncData.begin = line;
                            // 関数登録
                            DataTable.AddFuncData(fncData);
                        }*/

                        ResetData();

                        break;
                   
                    case '(':
                        if (data.loopFlag)
                        {
                            // textGui.cs内で判断するように保存
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
                                    // 引数あり
                                    argumentFlag = true;
                                }
                                // 関数になるので、変数名から変更
                                fncData.returnName = mold;
                                fncData.name = leftValname;
                                funcName = leftValname;
                                // 関数が始まった時のネストを代入
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
                                    Debug.Log("関数はない:" + data.substList[data.substList.Count - 1]);
                                }
                            }
                            
                            
                            
                        }

                        data.bracketsCount++;
                        break;
                    case ')':
                        data.argumentFlag = false;

                        data.bracketsCount--;
                        // ループ場合
                        if(data.loopFlag)
						{
                            switch (data.loopNestLevel.Peek().type)
							{
                                case ReadData.LOOP_TYPE_NAME.FOR:
                                    if (textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT)
                                    {
                                        data.nextLoopFlag = true;
                                        // 代入処理
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
                                            // textGui.cs内で判断するように保存
                                            data.loopType = data.loopNestLevel.Peek().type;
                                        }
                                    }
                                    else
									{
                                        data.nextLoopFlag = true;
									}
                                    break;
                            }
                            //なんでかわからないけど、これが無いとダメ
                            // もしかしたら、skipflagが問題化も。。。。
                            ResetData();    
                        }
                        else if (data.argumentpassFlag)
                        {
                            SetArgumentPass();

                            // 関数呼び出し
                            if (DataTable.GetFuncOneData(data.callFuncName.Peek(), out DataTableList.FUNC_DATA fd, data.argumentPass))
                            {
                                // 関数が見つかった場合
                                sendFuncFlag = true;
                                data.substList.RemoveAt(data.substList.Count - 1);
                                sendFuncData.Enqueue(fd);
                                datas.Enqueue(data);
                                data = new ReadData();
                            }
                            data.argumentpassFlag = false;
                            // ();のパターン回避
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
                                // if文のチェック
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
                        //ResetData();   // 一旦コメントアウト
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
                            // if文でtrueになった場合の処理を書く

                        }
                        ResetData();

                        break;
                    case '}':
                        data.allNestLevel--;

                        // スキップされる行数を越した場合
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

                        // ループネストが終了した場合
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
                        // 呼び出されている場合はその場に戻る
                        else if (sendFuncData.Count != 0)
                        {
                            returnFuncFlag = true;
                        }

                        // 関数から抜けた場合
                        else if(data.funcNestLevel == data.allNestLevel)
						{
                            data.funcName = "";
						}
                        ScoopPop();
                        break;
                    case '=':

                        // 左辺が変数宣言ではなかった場合
                        if(data.tmpValue.mold != null)
						{
                            data.leftValue = data.tmpValue;
                            
                            data.substList.RemoveAt(data.substList.Count - 1);
                            data.tmpValue = new VARIABLE_DATA();
                        }
                        
                        
                        if (data.skipFlag && data.skipNestLevel != -1)
                        {
                            // スキップされる行数を越した場合
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
                            // += -=　の場合
                            if (data.substList[data.substList.Count - 1] == "-" || data.substList[data.substList.Count - 1] == "+")
                            {
                                data.substList.RemoveAt(data.substList.Count - 2);
                                data.substList.Insert(0, data.leftValue.value.ToString());
                            }
                        }

                        break;
                    case ',':
                        // カンマあり
                        data.argumentCanmaFlag = true;
                        // 呼び出し時
                        if(data.argumentpassFlag)
						{
                            SetArgumentPass();
                        }
                        break;

                    case '+':
                    case '-':
                        // インクリメントや、計算はfor文の時は最後のステップの時のみ
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
                        // 配列の場合
                        // (宣言）データ型＆変数名が定義されている場合のみ
                        //if(mold != "" && leftValname  != "")
						{
                            data.arrayFlag = true;
                        }
                        break;
                    case ']':
                        // 配列の終わり
                        if(data.arrayFlag)
						{
                            // 数値の場合
                            if (int.TryParse(data.substList[data.substList.Count - 1], out int result))
							{
                                data.arrayCountList.Add(result);
                                data.substList.RemoveAt(data.substList.Count - 1);
                            }
                            else
							{
                                // 変数が定義されている場合
                                if(CheckVarialbleData(data.substList[data.substList.Count - 1]))
								{
                                    DataTable.GetVariableValueData(data.substList[data.substList.Count - 1],out VARIABLE_DATA vData);
                                    data.arrayCountList.Add(int.Parse((string)vData.value));
                                    data.substList.RemoveAt(data.substList.Count - 1);
                                }
							}
                            // 変数宣言の時は入らない
                            if(data.substList.Count > 0)
							{
                                // 代入の場合
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
                            // コロン入力時に値を比較する
                            if (int.TryParse(data.substList[data.substList.Count - 1], out int result))
                            {
                                data.caseName = result.ToString();
                            }
                            else
                            {
                                // 変数が定義されている場合
                                if (DataTable.GetVariableValueData(data.substList[data.substList.Count - 1], out VARIABLE_DATA vdata))
								{
                                    data.caseName = vdata.value.ToString();
                                }
                            }
                            // スイッチ条件
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
            // ネストスキップの場合
            if(data.skipFlag && data.skipNestLevel != -1)
			{
                return;
            }
            else if (data.nextCaseFlag && newSyntax != "case")
            {
                return;
            }
            // 予約語チェック
            if (CheckReservedWord(newSyntax))
			{
				#region 予約語処理
				switch (newSyntax)
				{
                    case "if":
                        data.ifFlag = true;
                        data.ifnestLevel++;

                        break;
                    case "else":
						#region else文の処理
						// if文が定義されている
						if (data.ifnestLevel > 0)
						{
                            // if文がfalseだったら
                            if(!data.ifCheckFlag)
							{
                                data.skipNestLevel = -1;
                                data.skipFlag = false;
                            }
                            else
							{
                                data.skipFlag = true;
                                // trueの場合は読み飛ばし
                                data.skipNestLevel = data.allNestLevel;
							}
						}
                        else
						{
                            // if文が定義されていないからエラー
                            Debug.Log("if文がありません");
						}
						#endregion
						break;
                    case "for":
                        data.loopFlag = true;
                        data.newLoopFlag = true;
                        // 自身のネストを保存する
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

			// 関数引数(宣言時)
			#region ゴミ
			/*
			else if (argumentFlag)
            {
                #region 関数の引数処理

                // カンマがありの場合のみ引数を入れる
                // 最初は絶対入れる
                if (argumentCanmaFlag)
                {
                    // 型
                    if (mold == "")
                    {
                        // 型チェック
                        foreach (var st in cName)
                        {
                            if (st == newSyntax)
                            {
                                mold = newSyntax;
                                break;
                            }
                        }
                    }
                    // 変数名
                    else
                    {
                       VARIABLE_DATA vd = newVARIABLE_DATA();
                        vd.name = newSyntax;
                        vd.mold = mold;
                        vd.value = "0";
                        vd.scoopNum = allNestLevel;

                        mold = "";
                        fncData.getVariable.Add(vd);
                        // カンマ無にする
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
                // インクリメント・デクリメント用前置型
                if (data.prefixFlag)
                {
                    // 変数の場合
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
                        // 変数ではないので、エラー
                        Debug.LogError("インクリメント・デクリメントエラー");
                    }
                    data.prefixFlag = false;
                }
                // ドットがある場合
                else if (data.dotFlag)
                {
                    // ドット前の値が変数だったら
                    if (DataTable.GetVariableChildData(data.GetBackNumSubstListData(2), newSyntax, out VARIABLE_DATA vData))
					{
                        varData.Push(vData);
                       
                    }
                }
                else if (data.switchFlag)
                {
                    if (data.substList[data.substList.Count - 2] == "(")
                    {
                        // 予約語じゃないとき
                        if ((!CheckReservedWord(newSyntax)))
                        {
                            string name = "";
                            // コロン入力時に値を比較する
                            if (int.TryParse(newSyntax, out int result))
                            {
                                name = result.ToString();
                            }
                            else
                            {
                                // 変数が定義されている場合
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
                        // カッコが必要だよ
                    }
                }
                else if (data.substitutionFlag || data.ifFlag)
                {
                    // 左辺が無い場合
                    if (!data.ifFlag && data.leftValue.name == "")
                    {
                        data.leftValue.name = data.substList[data.substList.Count - 2];
                        data.substList.RemoveAt(data.substList.Count - 2);
                    }
                    else
                    {
                        // 変数だった場合
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
                    // 型のチェック
                    if (CheckMold(newSyntax, out bool structFlag))
                    {
                        data.tmpValue.mold = newSyntax;
                        data.tmpValue.type = structFlag ? VARIABLE_DATA.DATA_TYPE.STRUCT : VARIABLE_DATA.DATA_TYPE.INT;
                        //data.structFlag = structFlag;
                        return;                 // 型指定の為、終了
                    }
                    else
					{
                        //data.substList.RemoveAt(data.substList.Count - 1);
                        if (!DataTable.GetVariableValueData(newSyntax, out data.tmpValue))
						{
                            Debug.LogError("変数が宣言されていません。:" + newSyntax);
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
                        // 変数宣言
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
            // 中カッコが終わっている
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
        // 変数があるかチェック
        if (CheckVarialbleData(data.substList[data.substList.Count - 1], out VARIABLE_DATA vd))
        {
            // 配列の場合
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

    // インクリメント対応
    static bool SetOpeData(string ope,out VARIABLE_DATA outData)
	{
        // 後置の場合のみtrue
        bool result = false;
        outData = new VARIABLE_DATA();
        // インクリメント対応
        if (ope == "+" || ope == "-")
        {
            if(data.substList[data.substList.Count - 1] == ope)
			{
                List<string> tmp = new List<string>();
                if (data.substList.Count >= 2)
                {
                    // 後置型
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
                    // 前置型
                    // インクリメントが初めの場合
                    data.prefixFlag = true;
                }
            }
            else
			{
                // 符号の可能性あり
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
    

    // 変数宣言
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

            // 配列ONの場合
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
        // こういった消し方もある（勉強用）
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
        // 代入チェック
        if (flag)
        {
            var val = arithmeticCheck.Check(list,out string mold);
            DataTable.SetVarialbleData(ref leftDataValue, val, data.arrayCountList);
            /*
            // 変数名チェック
            if (CheckVarialbleData(subName))
            {
                DataTable.SetVarialbleData(subName, val, data.arrayCountList);
            }
           
            // 引数のチェック
            else if (DataTable.SetFuncVarialbleData(data.funcName, subName, val))
            {
                // 定義されていない変数に代入しようとしている

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
		#region 変数作成
		// 変数
		if (tmpvTable != null && tmpvObj != null)
        {
            // 一旦削除
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

		#region 関数作成
		// 関数
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
        // 関数定義があるかチェック
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
                // 呼び出しより上にプロトタイプ宣言がある場合
                if(data.line <= line)
				{
                    // 関数定義があるかチェック
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
                // 記述時の配列数と宣言時の配列数が合っているか確認
                if(_arrayList.Count == data.array_size.Length)
				{
                    // 要素番号があるかチェック
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
        // 型チェック
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


