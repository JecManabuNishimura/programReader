using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class DataTableList
{
	public enum DATA_TYPE { INT, PTR, ARRAY };
	public struct VARIABLE_DATA
	{

		public DATA_TYPE type;
		public string name;
		public object mold;
		public object value;
		public int scoopNum;
		public int[] array_size;
		public object[] array_data;
		public bool selectItemFlag;
	}
	public struct STRUCT_DATA
	{
		public string name;
		public List<VARIABLE_DATA> variable_data;
	}
	public struct FUNC_DATA
	{

		public string name;
		public int line;
		public int begin;
		public int end;
		public string returnName;
		public List<VARIABLE_DATA> getVariable;

		public static bool operator ==(FUNC_DATA pro, FUNC_DATA fun)
		{
			if ((pro.returnName == fun.returnName) &&
				(pro.name == fun.name) &&
				(pro.getVariable.Count == fun.getVariable.Count))
			{
				int matchCount = 0;
				for (int i = 0; i < pro.getVariable.Count; i++)
				{
					if (pro.getVariable[i].mold == fun.getVariable[i].mold)
					{
						matchCount++;
					}
				}
				if (matchCount == pro.getVariable.Count)
				{
					return true;
				}
			}
			return false;
		}
		public static bool operator !=(FUNC_DATA pro, FUNC_DATA fun)
		{
			if ((pro.returnName == fun.returnName) &&
				(pro.name == fun.name) &&
				(pro.getVariable.Count == fun.getVariable.Count))
			{
				int matchCount = 0;
				for (int i = 0; i < pro.getVariable.Count; i++)
				{
					if (pro.getVariable[i].mold == fun.getVariable[i].mold)
					{
						matchCount++;
					}
				}
				if (matchCount == pro.getVariable.Count)
				{
					return false;
				}
			}
			return true;
		}
	}




}


public static partial class DataTable
{
	// INT = 単体
	// PTR = アドレス
	// ARRAY = 配列


	
	static List<DataTableList.FUNC_DATA> function = new List<DataTableList.FUNC_DATA>();
	static List<DataTableList.VARIABLE_DATA> variable = new List<DataTableList.VARIABLE_DATA>();
	static List<DataTableList.STRUCT_DATA> structDatas = new List<DataTableList.STRUCT_DATA>();
	public static void ClearData()
	{
		variable.Clear();
		function.Clear();
		structDatas.Clear();
	}

	public static void DeleteVariableScoopData(int scoopIndex)
	{
		//データを削除する（解放された)
		IEnumerable<DataTableList.VARIABLE_DATA> data = variable.Where(n => n.scoopNum > scoopIndex);
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

	public static void AddFuncData(DataTableList.FUNC_DATA fnc)
	{
		function.Add(fnc);
	}

	public static void AddVariableData(DataTableList.VARIABLE_DATA val, List<int> arraySize)
	{
		val.type = DataTableList.DATA_TYPE.ARRAY;
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
	public static void AddVariableData(DataTableList.VARIABLE_DATA val)
	{
		
		val.type = DataTableList.DATA_TYPE.INT;
		variable.Add(val);
	}

	public static object GetVariableValueData(string name)
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

	public static List<DataTableList.VARIABLE_DATA> GetVarialbleDataList()
	{
		return variable;
	}

	public static List<DataTableList.FUNC_DATA> GetFunctionDataLIst()
	{
		return function;
	}


	public static bool SetVarialbleData(string valName,string value, List<int> _arrayCount)
	{
		for(int i =0; i < variable.Count; i++)
		{
			if (variable[i].name == valName)
			{
				if(variable[i].type == DataTableList.DATA_TYPE.ARRAY)
				{
					DataTableList.VARIABLE_DATA vd = variable[i];
					// 要素番号が存在するか確認
					if(!CheckArrayNumber(vd,_arrayCount))
					{
						return false;
					}
					vd.array_data[(int)GetOneArrayNumber(vd,_arrayCount)] = value;
					variable[i] = vd;
					return true;
				}
				else
				{
					DataTableList.VARIABLE_DATA vd = variable[i];
					vd.value = value;
					variable[i] = vd;
					return true;
				}
			}
		}
		return false;
	}

	public static bool CheckArrayNumber(DataTableList.VARIABLE_DATA vd, List<int> _arrayCount)
	{
		for (int ci = 0; ci < _arrayCount.Count; ci++)
		{

			if (_arrayCount[ci] >= vd.array_size[ci] || _arrayCount[ci] < 0)
			{
				// そんな配列は存在しないので、エラー
				Debug.LogError("存在しない配列を番号を参照:" + vd.name);
				return false;
			}
		}
		return true;
	}

	public static object GetOneArrayNumber(DataTableList.VARIABLE_DATA vd, List<int> _arrayCount)
	{
		int number = _arrayCount.Count;
		return ChengeOneArray(vd, _arrayCount, ref number);
	}

	public static object GetOneArrayNumberData(DataTableList.VARIABLE_DATA vd, List<int> _arrayCount)
	{
		return vd.array_data[(int)GetOneArrayNumber(vd,_arrayCount)];
	}

	public static string GetArrayAddress(DataTableList.VARIABLE_DATA vd, int eleNum)
	{
		int mod = eleNum;
		int number = 0;
		var str = ReturnArrayAddress(vd, ref mod, ref number);
		return str;
	}
	
	
	public static string ReturnArrayAddress(DataTableList.VARIABLE_DATA vd, ref int mod, ref int number)
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

	public static int ChengeOneArray(DataTableList.VARIABLE_DATA vd, List<int> _arrayCount, ref int number)
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
			calc += ChengeOneArray(vd, _arrayCount, ref number);
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
						DataTableList.VARIABLE_DATA vd = function[i].getVariable[y];
						vd.value = value;
						function[i].getVariable[y] = vd;
						return true;
					}
				}
			}
		}
		return false;
	}

	public static void SetVariableItemFlag(string name)
	{
		for(int i=0; i<variable.Count;i++)
		{
			if(variable[i].name == name)
			{
				DataTableList.VARIABLE_DATA data = variable[i];
				data.selectItemFlag = true;
				variable[i] = data;
				return;
			}
		}

		Debug.Log("指定した変数名は存在しない");
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

	// 関数名・引数が同じものを返す
	public static bool GetFuncOneData(string name, out DataTableList.FUNC_DATA fd, params object[] mold)
	{
		foreach(var obj in function)
		{
			// 関数名が同じ
			if(obj.name == name)
			{
				int counter = 0;
				for(int i=0; i<mold.Length;i++)
				{
					if (obj.getVariable.Count != 0)
					{
						// 型が同じ
						if (mold[i] == obj.getVariable[i].mold)
						{
							counter++;
						}
					}
				}
				if(obj.getVariable.Count == counter)
				{
					fd = obj;
					return true;
				}
			}
		}
		fd = new DataTableList.FUNC_DATA();
		// 空を返す
		return false;
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