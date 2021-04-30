using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arithmeticCheck : MonoBehaviour
{

    static int nowIndex = 0;

    static List<string> substList;

    static public string Check(List<string> list)
    {
        substList = list;
        nowIndex = 0;
        return eval1().ToString();
    }

    static double eval1()
    {
        var value = eval2();
        while(true)
		{
            if (nowIndex == substList.Count)
            {
                return value;
            }
            if (substList[nowIndex] == "+")
            {
                nowIndex++;
                value += eval2();
            }
            else if (substList[nowIndex] == "-")
            {
                nowIndex++;
                value -= eval2();
            }
            else
                break;
        }
        return value;
    }

    static double eval2()
    {
        var value = eval3();
        if (nowIndex == substList.Count)
        {
            return value;
        }
        if (substList[nowIndex] == "*")
        {
            nowIndex++;
            value *= eval3();
        }
        else if (substList[nowIndex] == "/")
        {
            nowIndex++;
            value /= eval3();
        }
        else if (substList[nowIndex] == "%")
        {
            nowIndex++;
            value %= eval3();
        }
        return value;
    }

    static double eval3()
    {
        if (substList[nowIndex] == "(")
        {
            nowIndex++;
            var value = eval1();
            if (substList[nowIndex] != ")")
            {
                // 閉じがない
            }
            nowIndex++;
            return value;
        }
        else
            return number();
    }

    static double number()
    {
        var value = 0.0f;
        bool plasFlag = true;           // falseはマイナス
        bool decimalFlag = false;
        int decCount = 10;
        // 無限ループさせる
        while (true)
		{
            if(substList.Count > nowIndex)
			{
                // 数値だった場合
                if (int.TryParse(substList[nowIndex], out int result))
                {
                    // 小数点対応
                    if (decimalFlag)
                    {
                        value = value + (result / (float)decCount);
                        decCount *= 10;
                        nowIndex++;
                        continue;
                    }
                    else
                    {
                        value = result;
                    }
                    if(nowIndex < substList.Count - 1)
                        // 次の値が、小数点だったら
                        if (substList[nowIndex + 1] == ".")
                        {
                            decimalFlag = true;
                            nowIndex += 2;
                            continue;
                        }
                    break;
                }
                else
                {
                    if (decimalFlag)
                    {
                        return value;
                    }
                    // 数字ではないので、変数
                    // すでに定義されている変数なのか
                    string sv = DataTable.GetVariableValueData(substList[nowIndex]);
                    if (sv != "")
                    {
                        value = int.Parse(sv);
                        break;
                    }
                    else
                    {
                        // マイナス・プラスの符号の可能性があり
                        switch (substList[nowIndex])
                        {
                            case "+":
                                plasFlag = true;
                                break;
                            case "-":
                                plasFlag = false;
                                break;
                            default:
                                //エラー
                                break;
                        }
                        nowIndex++;
                    }
                }
            }
            else
			{
                return value;
			}
        }
        nowIndex++;
        if (!plasFlag)
		{
            value *= -1; 
		}
        return value;
    }


}
