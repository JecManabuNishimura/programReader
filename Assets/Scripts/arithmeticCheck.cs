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
                // �����Ȃ�
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
        bool plasFlag = true;           // false�̓}�C�i�X
        bool decimalFlag = false;
        int decCount = 10;
        // �������[�v������
        while (true)
		{
            if(substList.Count > nowIndex)
			{
                // ���l�������ꍇ
                if (int.TryParse(substList[nowIndex], out int result))
                {
                    // �����_�Ή�
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
                    // ���̒l���A�����_��������
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
                    // �����ł͂Ȃ��̂ŁA�ϐ�
                    // ���łɒ�`����Ă���ϐ��Ȃ̂�
                    string sv = DataTable.GetVariableValueData(substList[nowIndex]);
                    if (sv != "")
                    {
                        value = int.Parse(sv);
                        break;
                    }
                    else
                    {
                        // �}�C�i�X�E�v���X�̕����̉\��������
                        switch (substList[nowIndex])
                        {
                            case "+":
                                plasFlag = true;
                                break;
                            case "-":
                                plasFlag = false;
                                break;
                            default:
                                //�G���[
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
