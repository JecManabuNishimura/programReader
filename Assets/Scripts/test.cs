using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{

    GUIContent content;
    GUIStyle style;
    string text = "";
    private void OnGUI()
	{

        Rect rect = new Rect(0, 0, 100, 100);
        Rect rect2 = new Rect(200, 0, 100, 100);
        GUIStyle st = new GUIStyle(style);
        
        text = GUI.TextArea(rect2, text);
        content.text = text;
        GUI.TextField(rect, (st.GetCursorPixelPosition(rect, content, content.text.Length)).x.ToString(), st);
    }
	void Start()
    {
        style = new GUIStyle();
        style.fontSize = 34;
        content = new GUIContent();
        content.text = "test";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
