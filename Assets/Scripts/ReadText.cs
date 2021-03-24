using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


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
    static bool forFlag = false;                    // for文フラグ


    static bool bracketsEndFlag = false;            // 中カッコ終わりフラグ
    static public bool nextLoopFlag;                // for文最後の処理フラグ
    static public bool loopEndFlag = false;         // for文終了フラグ

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

    //----------デバッグ用--------------------------------------------------
    [SerializeField]
    GameObject variableTable;
    [SerializeField]
    GameObject funcTable;
    [SerializeField]
    GameObject variaObj;
    [SerializeField]
    GameObject funcObj;
    //------------------------------------------------------------

    //----------デバッグスタック用--------------------------------------------------
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
                        bracketsCount++;
                        break;
                    case ')':
                        argumentFlag = false;
                        bracketsCount--;

                        // for 文の場合
                        if(forFlag)
						{
                            if(textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT)
							{
                                nextLoopFlag = true;
                                // 代入処理
                                Substitution(substList, leftValname);

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

                        if (forFlag)
                        {
                            nextLoopFlag = true;
                            forFlag = false;
                        }
                        if (ifCheckFlag)
                        {
                            // if文でtrueになった場合の処理を書く

                        }
                        ResetData();

                        break;
                    case '}':
                        allNestLevel--;
                        skipNestLevel = -1;
                        bracketsEndFlag = true;
                        DataTable.DeleteVariableScoopData(allNestLevel);

                        // ループネストが終了した場合
                        if (loopNestLevel.Count != 0)
						{
                            if (loopNestLevel.Peek() == allNestLevel)
                            {
                                nextLoopFlag = true;
                                forFlag = true;
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

                        if((ifFlag) || (forFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.TERM))
                        {
                            substList.Add(newSyntax[i].ToString());
                        }
                        break;
                    case ',':
                        // カンマあり
                        argumentCanmaFlag = true;
                        break;

                    case '+':
                        // インクリメントや、計算はfor文の時は最後のステップの時のみ
                        if ((forFlag && textGui.loopStepNumber == textGui.LOOP_NUMBER.NEXT) ||
                            (!forFlag) && !skipFlag)
                        {
                            if (substList.Count >= 1)
                            {
                                // インクリメント対応
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
                                // インクリメント対応
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
                // スキップされる行数を越した場合
                if (skipNestLevel >= allNestLevel)
                {
                    skipNestLevel = -1;
                }

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
                        // if文が定義されている
                        if(ifnestLevel > 0)
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
                        break;
                    case "for":
                        forFlag = true;
                        // 自身のネストを保存する
                        loopNestLevel.Push(allNestLevel);
                        break;
                    case "while":
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
                        DataTable.VARIABLE_DATA vd;
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
            else if(substitutionFlag || ifFlag)
			{
                substList.Add(newSyntax);
            }
            // for文時の終了条件用
            else if(textGui.loopStepNumber == textGui.LOOP_NUMBER.TERM)
			{
                substList.Add(newSyntax);
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

                // 変数名が設定されていない場合
                if (leftValname == "")
                {
                    // すでに変数宣言がされているのか
                    //if (CheckVarialbleData(newSyntax))
                    {
                        leftValname = newSyntax;
                    }
                }
            }
            else
			{
                // 変数名が設定されていない場合
                if (leftValname == "")
                {
                    // すでに変数宣言がされているのか
                    //if (CheckVarialbleData(newSyntax))
                    {
                        leftValname = newSyntax;
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
    
    // 変数宣言
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
        // こういった消し方もある（勉強用）
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
        // 代入チェック
        if (substitutionFlag)
        {
            var val = arithmeticCheck.Check(list);
            // 変数名チェック
            if (CheckVarialbleData(subName))
            {
                DataTable.SetVarialbleData(subName, val);
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
                    var obj = Instantiate(tmpvObj);
                    obj.GetComponent<SetVariData>().SetMolText(data.mold);
                    obj.GetComponent<SetVariData>().SetValNameText(data.name);
                    obj.GetComponent<SetVariData>().SetValueText(data.value);
                    obj.transform.parent = tmpvTable.transform;
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


