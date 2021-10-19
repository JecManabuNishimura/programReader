using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class TestScript2 : MonoBehaviour
{
    public static int[] iTest = new int[3];
    public static List<ttt.TTTT> iTest2 = new List<ttt.TTTT>();

    void Start()
    {
        iTest[0] = 1;
        ttt.TTTT t = new ttt.TTTT();
        t.test = 1;
        iTest2.Add(t);

    }

    public static void GetData(out int[] t)
	{
        t = iTest;
        
	}
    public static void GetData2(out List<ttt.TTTT> t)
    {
        t = iTest2;

    }
    public static void GetData3(out ttt.TTTT t)
    {
        t = iTest2[0];
        t.test = 4;

    }
}

public class ttt
{
	public struct TTTT
	{

        public int test;
    }

}
