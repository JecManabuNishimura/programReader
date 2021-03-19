using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ifcheck : MonoBehaviour
{
    static int nowIndex = 0;
    static List<string> substList;

    static public bool CheckConditions(List<string> list)
    {
        nowIndex = 0;
        substList = list;
        
        return conc1();
    }

    //　条件判定式
    static bool conc1()
    {
        string left = conc2();
        if (nowIndex == substList.Count)
        {
            return left == "true" ? true : false;
        }

        bool res = left == "true" ? true : false;
        if (substList[nowIndex] == "!" && substList[nowIndex + 1] == "=")
        {
            nowIndex += 2;
            if (left != conc2())
            {
                res = true;
            }
            else
            {
                res = false;
            }
        }
        else if (substList[nowIndex] == "=" && substList[nowIndex + 1] == "=")
        {
            nowIndex += 2;
            if (left == conc2())
            {
                res = true;
            }
            else
            {
                res = false;
            }
        }

        else if (substList[nowIndex] == "<" && substList[nowIndex + 1] == "=")
        {
            nowIndex += 2;
            if (int.Parse(left) <= int.Parse(conc2()))
            {
                res = true;
            }
            else
            {
                res = false;
            }
        }
        else if (substList[nowIndex] == ">" && substList[nowIndex + 1] == "=")
        {
            nowIndex += 2;
            if (int.Parse(left) >= int.Parse(conc2()))
            {
                res = true;
            }
            else
            {
                res = false;
            }
        }

        return res;
    }

    static string conc2()
    {
        string res = conc3();
        if (nowIndex == substList.Count)
        {
            return res;
        }
        if (substList[nowIndex] == "|" && substList[nowIndex + 1] == "|")
        {
            nowIndex += 2;
            string right = conc3();
            if (res == "true" || right == "true")
            {
                res = "true";
            }
            else
            {
                res = "false";
            }
        }
        else if (substList[nowIndex] == "&" && substList[nowIndex + 1] == "&")
        {
            nowIndex += 2;
            string right = conc3();
            if (res == "true" && right == "true")
            {
                res = "true";
            }
            else
            {
                res = "false";
            }
        }
        return res;
    }

    static string conc3()
    {
        if (substList[nowIndex] == "(")
        {
            nowIndex++;
            bool text = conc1();
            nowIndex++;
            return text ? "true" : "false";
        }
        else
        {
            return NumCheck();
        }
    }

    static string NumCheck()
    {
        string num = "";
        if (int.TryParse(substList[nowIndex], out int n))
        {
            num = substList[nowIndex];
            nowIndex++;
        }
        else
        {
            // 定義済み変数かチェック
            string val = DataTable.GetVariableValueData(substList[nowIndex]);
            if (val != "")
            {
                nowIndex++;

                num = val;
            }
            else
            {
                // エラー
            }
        }

        return num;
    }
}
