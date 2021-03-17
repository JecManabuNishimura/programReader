using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataTable : MonoBehaviour
{
	public struct VARIABLE_DATA
	{
		public string name;
		public string mold;
		public string value;
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

	public static int GetVariableNum()
	{
		return variable.Count;
	}
	public static int GetFunctionNum()
	{
		return function.Count;
	}
}
