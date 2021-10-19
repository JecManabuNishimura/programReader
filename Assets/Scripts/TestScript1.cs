using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class TestScript1 : MonoBehaviour
{
    
    void  Start()
    {
        //TestScript2.GetData(out int[] data);
            
            
        //Debug.Log("before:" + data[0]);
        //data[0] = 2;
        //Debug.Log("after:" + data[0]);


        //TestScript2.GetData(out int[] data2);
          
        //Debug.Log("afterT:" + data2[0]);



        TestScript2.GetData2(out List<ttt.TTTT> data3);


        Debug.Log("before:" + data3[0].test);
        //data3[0].test = 2;
        Debug.Log("after:" + data3[0].test);


        TestScript2.GetData2(out List<ttt.TTTT> data4);

        Debug.Log("afterT:" + data4[0].test);
        TestScript2.GetData3(out ttt.TTTT data5);

        data5.test = 3;
        ttt.TTTT data6;
        TestScript2.GetData3(out data6);
        Debug.Log("afterT3:" + data6.test);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
