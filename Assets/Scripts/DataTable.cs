using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static partial class DataTable
{
	public struct VARIABLE_DATA
	{
		public string name;
		public string mold;
		public string value;
		public int scoopNum;
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
		
		//variable.RemoveAll(p => p.scoopNum > scoopIndex);
	}

	public static void DeleteVariableData(string name)
	{
		int i =0;
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

	public static void AddVariableData(VARIABLE_DATA val)
	{
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


	public static bool SetVarialbleData(string valName,string value)
	{

		for(int i =0; i < variable.Count; i++)
		{
			if (variable[i].name == valName)
			{
				VARIABLE_DATA vd = variable[i];
				vd.value = value;
				variable[i] = vd;
				return true;
			}
		}
		return false;
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