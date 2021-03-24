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

    static int eval1()
    {
        int value = eval2();
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

    static int eval2()
    {
        int value = eval3();
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
        return value;
    }

    static int eval3()
    {
        if (substList[nowIndex] == "(")
        {
            nowIndex++;
            int value = eval1();
            if (substList[nowIndex] != ")")
            {
                // �����Ȃ�
            }
            nowIndex++;
            return value;
        }
        else
            return number();
    }

    static int number()
    {
        int value = 0;

        if (int.TryParse(substList[nowIndex], out int result))
        {
            value = result;
        }
        else
        {
            // �����ł͂Ȃ��̂ŁA�ϐ�
            // ���łɒ�`����Ă���ϐ��Ȃ̂�
            // ��`����Ă��Ȃ��ϐ��̏ꍇ�̓G���[
            string sv = DataTable.GetVariableValueData(substList[nowIndex]);
            if (sv != "")
            {
                value = int.Parse(sv);
            }
            else
            {
                // �G���[
            }
        }
        nowIndex++;
        return value;
    }


}
