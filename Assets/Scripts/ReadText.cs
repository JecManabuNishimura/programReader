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
    static string leftValname = "";                     // 左辺変数

    static bool argumentFlag = false;               // 引数フラグ
    static bool argumentCanmaFlag = true;           // 引数カンマフラグ
    static bool substitutionFlag = false;           // 代入フラグ
    static bool ifFlag = false;                     // if文フラグ
    static bool ifCheckFlag = false;                // if文判定後フラグ
    static bool skipFlag = false;                   // スキップフラグ
    static bool loopFlag = false;                    // ループ用フラグ
    static bool prefixFlag = false;                 // 前置フラグ
    static bool arrayFlag = false;                  // 配列用フラグ

    static bool bracketsEndFlag = false;            // 中カッコ終わりフラグ
    static public bool nextLoopFlag;                // for文最後の処理フラグ
    static public bool loopEndFlag = false;         // for文終了フラグ
    static public bool newLoopFlag = false;         // インクリメントデクリメント

    static public LOOP_TYPE_NAME loopType = new LOOP_TYPE_NAME();

    static int bracketsCount = 0;

    static int ifnestLevel = 0;
    static int allNestLevel = 0;
    static int skipNestLevel = -1;
    static int funcNestLevel = 0;
    static List<int> arrayCountList = new List<int>();                      // 配列数
    static Stack<LOOP_TYPE> loopNestLevel = new Stack<LOOP_TYPE>();

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

        // コンストラクタ
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


    static public textGui.LOOP_NUMBER loopStep = textGui.LOOP_NUMBER.NONE;

    //----------デバッグ用--------------------------------------------------
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

    //----------デバッグスタック用--------------------------------------------------
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
         * メモ：C＃でメモリーを扱いたい場合は、unsafeを使うとできる。
         * unsafeオプションをONにする必要がある。
         * Unity側のplayerSettingと新しくAssenblyDefinitionを追加する必要がある。
         * ただし、アドレスは数字のみの表記になる
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
        arrayFlag = false;
        arrayCountList.Clear();
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
        // 記号の場合
        if(SymbolCheck(newSyntax))
		{
            // 中カッコが終わっている
            if (bracketsEndFlag)
            {
                if (ifnestLevel > 0) ifnestLevel--;
            }
            // カッコ終了フラグを下す
            bracketsEndFlag = false;
            for (int i=0; i < newSyntax.Length;i++)
            {
                switch (newSyntax[i])
                {
                    case ';':
                        // 変数宣言
                        if (!CheckVarialbleData(leftValname))
						{
                            VariableDeclaration(leftValname, mold);
                        }
                        // 代入処理
                        Substitution(substList,leftValname);

                        // for文チェック
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
                                    // textGui.cs内で判断するように保存
                                    loopType = loopNestLevel.Peek().type;
                                }
                            }
                        }
                        else if (loopFlag)
                        {
                            nextLoopFlag = true;
                        }

                        ResetData();

                        break;
                   
                    case '(':
                        
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
                        if (substitutionFlag || ifFlag)
                        {
                            substList.Add(newSyntax[i].ToString());
                        }

                        if (loopFlag)
                        {
                            // textGui.cs内で判断するように保存
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

                        bracketsCount++;
                        break;
                    case ')':
                        argumentFlag = false;
                        bracketsCount--;
                        // ループ場合
                        if(loopFlag)
						{
                            switch (loopNestLevel.Peek().type)
							{
                                case LOOP_TYPE_NAME.FOR:
                                    if (textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT)
                                    {
                                        nextLoopFlag = true;
                                        // 代入処理
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
                                            // textGui.cs内で判断するように保存
                                            loopType = loopNestLevel.Peek().type;
                                        }
                                    }
                                    else
									{
                                        nextLoopFlag = true;
									}
                                    break;
                            }
                            
                        }

                        else if (substitutionFlag || ifFlag)
                        {
                            substList.Add(newSyntax[i].ToString());

                            if (bracketsCount == 0 && ifFlag)
							{
                                // if文のチェック
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
                        // 関数定義
                        else if (fncData.name != "" && fncData.name != null)
                        {
                            // 行数
                            fncData.begin = line;
                            // 関数登録
                            DataTable.AddFuncData(fncData);
                        }
                        ResetData();
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
                            // if文でtrueになった場合の処理を書く

                        }
                        ResetData();

                        break;
                    case '}':
                        allNestLevel--;

                        // スキップされる行数を越した場合
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

                        // ループネストが終了した場合
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
                        // 関数から抜けた場合
                        else if(funcNestLevel == allNestLevel)
						{
                            funcName = "";
						}
                        ScoopPop();
                        break;
                    case '=':
                        if (skipFlag && skipNestLevel != -1)
                        {
                            // スキップされる行数を越した場合
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
                            // += -=　の場合
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
                        
                        break;
                    case ',':
                        // カンマあり
                        argumentCanmaFlag = true;
                        break;

                    case '+':
                        // インクリメントや、計算はfor文の時は最後のステップの時のみ
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
                        // 配列の場合
                        // (宣言）データ型＆変数名が定義されている場合のみ
                        //if(mold != "" && leftValname  != "")
						{
                            arrayFlag = true;
                        }
                        break;
                    case ']':
                        // 配列の終わり
                        if(arrayFlag)
						{
                            // 数値の場合
                            if (int.TryParse(substList[substList.Count - 1], out int result))
							{
                                arrayCountList.Add(result);
                                substList.RemoveAt(substList.Count - 1);
                            }
                            else
							{
                                // 変数が定義されている場合
                                if(CheckVarialbleData(substList[substList.Count - 1]))
								{
                                    arrayCountList.Add(int.Parse( DataTable.GetVariableValueData(substList[substList.Count - 1])));
                                    substList.RemoveAt(substList.Count - 1);
                                }
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
            // ネストスキップの場合
            if(skipFlag && skipNestLevel != -1)
			{
                return;
            }
            // 予約語チェック
            if(CheckReservedWord(newSyntax))
			{
				#region 予約語処理
				switch (newSyntax)
				{
                    case "if":
                        ifFlag = true;
                        ifnestLevel++;

                        break;
                    case "else":
						#region else文の処理
						// if文が定義されている
						if (ifnestLevel > 0)
						{
                            // if文がfalseだったら
                            if(!ifCheckFlag)
							{
                                skipNestLevel = -1;
                                skipFlag = false;
                            }
                            else
							{
                                skipFlag = true;
                                // trueの場合は読み飛ばし
                                skipNestLevel = allNestLevel;
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
                        loopFlag = true;
                        newLoopFlag = true;
                        // 自身のネストを保存する
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
                }
				#endregion
			}
			// 関数引数
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
                        DataTable.VARIABLE_DATA vd = new DataTable.VARIABLE_DATA();
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
            // 代入
            else 
			{
                substList.Add(newSyntax);
                // インクリメント・デクリメント用前置型
                if(prefixFlag)
                {
                    // 変数の場合
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
                        // 変数ではないので、エラー
                        Debug.LogError("インクリメント・デクリメントエラー");
                    }
                    prefixFlag = false;
                }
                else if (substitutionFlag || ifFlag)
				{
                    // 左辺が無い場合
                    if(!ifFlag && leftValname == "")
					{
                        leftValname = substList[substList.Count - 2];
                        substList.RemoveAt(substList.Count - 2);
                    }
				}
                else if (mold == "" && fncData.returnName == null)
                {
                    // 型のチェック
                    foreach (var st in cName)
                    {
                        if (st == newSyntax)
                        {
                            mold = newSyntax;
                            return; // 型指定の為、終了
                        }
                    }

                }
                else
                {
					// 変数名が設定されていない場合
					if (leftValname == "")
					{
						leftValname = newSyntax;
                        substList.RemoveAt(substList.Count - 2);
                        substList.RemoveAt(substList.Count - 1);
					}
				}
            }
            // 中カッコが終わっている
            if(bracketsEndFlag)
			{
                if(ifnestLevel > 0)   ifnestLevel--;
            }

            bracketsEndFlag = false;
        }
        
    }

    // インクリメント対応
    static bool SetOpeData(string ope)
	{
        // 後置の場合のみtrue
        bool result = false;
        // インクリメント対応
        if (ope == "+" || ope == "-")
        {
            if(substList[substList.Count - 1] == ope)
			{
                List<string> tmp = new List<string>();
                if (substList.Count >= 2)
                {
                    // 後置型
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
                    // 前置型
                    // インクリメントが初めの場合
                    prefixFlag = true;
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
    static void VariableDeclaration(string name,string setMold)
	{
        if(name != "" && setMold != "")
		{
            DataTable.VARIABLE_DATA ValData = new DataTable.VARIABLE_DATA() ;
            ValData.name = name;
            ValData.mold = setMold;
            ValData.value = "0";
            ValData.scoopNum = allNestLevel;
            // 配列ONの場合
            if(arrayFlag)   DataTable.AddVariableData(ValData, arrayCountList);
            else            DataTable.AddVariableData(ValData);
        }
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
    static void Substitution(List<string> list, string subName, bool flag = false)
	{
        if(!flag)
		{
            flag = substitutionFlag;
        }
        // 代入チェック
        if (flag)
        {
            var val = arithmeticCheck.Check(list);
            // 変数名チェック
            if (CheckVarialbleData(subName))
            {
                DataTable.SetVarialbleData(subName, val,arrayCountList);
            }
            // 引数のチェック
            else if (DataTable.SetFuncVarialbleData(funcName, subName, val))
            {
                // 定義されていない変数に代入しようとしている

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


