using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static partial class DataTable
{
	// INT = 単体
	// PTR = アドレス
	// ARRAY = 配列
	public enum DATA_TYPE { INT, PTR, ARRAY };
	public struct VARIABLE_DATA
	{
		
		public DATA_TYPE type;
		public string name;
		public string mold;
		public string value;
		public int scoopNum;
		public int[] array_size;
		public object[] array_data;
	}
	public struct FUNC_DATA
	{
		public string name;
		public int begin;
		public int end;
		public string returnName;
		public List<VARIABLE_DATA> getVariable;
	}
	
	static List<FUNC_DATA> function = new List<FUNC_DATA>();
	static List<VARIABLE_DATA> variable = new List<VARIABLE_DATA>(); 
	public static void CrearData()
	{
		variable.Clear();
		function.Clear();
	}

	public static void DeleteVariableScoopData(int scoopIndex)
	{
		//データを削除する（解放された)
		IEnumerable<VARIABLE_DATA> data = variable.Where(n => n.scoopNum > scoopIndex);
		foreach(var d in data)
		{
			Debug.Log(d.name + "が、解放されました");
		}
		
		variable.RemoveAll(p => p.scoopNum > scoopIndex);
	}

	public static void DeleteVariableData(string name)
	{
		int i = 0;
		int index = 0;
		foreach(var data in variable)
		{
			if(data.name == name)
			{
				index = i;
				break;
			}
			i++;
		}
		variable.RemoveAt(index);
	}

	public static void DeleteVariableData(int index)
	{
		variable.RemoveAt(index);
	}

	public static void AddFuncData(FUNC_DATA fnc)
	{
		function.Add(fnc);
	}

	public static void AddVariableData(VARIABLE_DATA val, List<int> arraySize)
	{
		val.type = DATA_TYPE.ARRAY;
		int arraynum = 1;
		int count = 0;
		val.array_size = new int[arraySize.Count];
		// 動的に配列を確保
		foreach (var tmp in arraySize)
		{
			arraynum *= tmp;
			val.array_size[count] = tmp;
			count++;
		}
		
		val.array_data = new object[arraynum];
		for (int i = 0; i < val.array_data.Length; i++)
		{
			val.array_data[i] = "null";
		}
		variable.Add(val);
	}
	public static void AddVariableData(VARIABLE_DATA val)
	{
		
		val.type = DATA_TYPE.INT;
		variable.Add(val);
	}

	public static string GetVariableValueData(string name)
	{
		foreach(var data in variable)
		{
			if(data.name == name)
			{
				return data.value;
			}
		}
		return "";
	}

	public static List<VARIABLE_DATA> GetVarialbleDataList()
	{
		return variable;
	}

	public static List<FUNC_DATA> GetFunctionDataLIst()
	{
		return function;
	}


	public static bool SetVarialbleData(string valName,string value, List<int> _arrayCount)
	{
		for(int i =0; i < variable.Count; i++)
		{
			if (variable[i].name == valName)
			{
				if(variable[i].type == DATA_TYPE.ARRAY)
				{
					VARIABLE_DATA vd = variable[i];
					int number = _arrayCount.Count;
					vd.array_data[CheckArrayAddress(vd,_arrayCount,ref number)] = value;
					variable[i] = vd;
					return true;
				}
				else
				{
					VARIABLE_DATA vd = variable[i];
					vd.value = value;
					variable[i] = vd;
					return true;
				}
			}
		}
		return false;
	}

	public static string GetArrayAddress(VARIABLE_DATA vd, int eleNum)
	{
		int mod = eleNum;
		int number = 0;
		var str = ReturnArrayAddress(vd, ref mod, ref number);
		return str;
	}
	
	
	public static string ReturnArrayAddress(VARIABLE_DATA vd, ref int mod, ref int number)
	{
		if(number == vd.array_size.Length - 1)
		{
			return "[" + mod + "]";
		}
		
		int calc = 1;
		for (int i=number + 1; i< vd.array_size.Length;i++)
		{
			calc *= vd.array_size[i];
		}
		int div = mod / calc;
		mod = mod % calc;

		number++;
		return "[" + div + "]" + ReturnArrayAddress(vd,ref mod, ref number);

	}

	public static int CheckArrayAddress(VARIABLE_DATA vd, List<int> _arrayCount, ref int number)
	{
		int calc = 1;
		
		if(number != vd.array_size.Length)
		{
			for (int i = number; i < vd.array_size.Length; i++)
			{
				calc *= vd.array_size[i];
			}
			calc *= _arrayCount[number-1];
		}
		else
		{
			calc = _arrayCount[number-1];
		}
		number--;
		if (number == 0)
		{
			return calc;
		}
		else
		{
			calc += CheckArrayAddress(vd, _arrayCount, ref number);
			return calc;
		}
	}

	public static bool SetFuncVarialbleData(string funcName,string valName,string value)
	{
		for(int i = 0; i < function.Count; i++)
		{
			if(function[i].name == funcName)
			{
				for(int y = 0; y < function[i].getVariable.Count; y++)
				{
					if(function[i].getVariable[y].name == valName)
					{
						VARIABLE_DATA vd = function[i].getVariable[y];
						vd.value = value;
						function[i].getVariable[y] = vd;
						return true;
					}
				}
			}
		}
		return false;
	}

	public static int GetVariableNum()
	{
		return variable.Count;
	}
	public static int GetFunctionNum()
	{
		return function.Count;
	}
	static int CheckDataSize(string tex)
	{
		int num;
		switch (tex)
		{
			case "int": num = sizeof(int); break;
			case "float": num = sizeof(float); break;
			case "double": num = sizeof(double); break;
			case "bool": num = sizeof(int); break;
			case "char": num = sizeof(char); break;
			default:
				num = 0;
				break;
		}

		return num;
	}
}
public static partial class DataTable
{
	public static IEnumerable<(T item, int index)> Indexed<T>(this IEnumerable<T> source)
	{
		var i = 0;
		IEnumerable<(T item, int index)> impl()
		{
			foreach (var item in source)
			{

				yield return (item, i);
				i++;
			}
		}

		return impl();
	}
}