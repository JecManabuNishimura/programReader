//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;

//public class ScriptEditor : EditorWindow
//{
//	string s = "";
//	int lines = 0;
//	string path = Application.dataPath;
//	Vector2 scrollPos = new Vector2();

//	GUIStyle textStyle = new GUIStyle();
//	Vector2 tSize = new Vector2();

//	bool isCommenting = false;
//	bool isString = false;
//	bool commentAffectsNextLine = false;

//	List<string> redos = new List<string>();

//	int toolbarMenuIndex = -1;
//	string[] fileOptions = new string[]
//			{
//				"New",
//				"Load",
//				"Save",
//								"Save as",
//				"Close Tab",
//				"Close All Tabs"
//			};

//	string[] editOptions = new string[]
//			{
//				"Cut",
//				"Copy",
//				"Paste"
//			};

//	#region cSharp namespaces
//	public List<string> cNamespaces = new List<string>()
//	{
//		"abstract",
//		"as",
//		"base",
//		"break",
//		"bool",
//		"by",
//		"byte",
//		"case",
//		"catch",
//		"char",
//		"checked",
//		"class",
//		"const",
//		"continue",
//		"decimal",
//		"default",
//		"delegate",
//		"do",
//		"double",
//		"else",
//		"enum",
//		"event",
//		"explicit",
//		"extern",
//		"false",
//		"finally",
//		"fixed",
//		"float",
//		"for",
//		"foreach",
//		"from",
//		"group",
//		"if",
//		"implicit",
//		"in",
//		"int",
//		"interface",
//		"internal",
//		"into",
//		"is",
//		"lock",
//		"long",
//		"namespace",
//		"new",
//		"null",
//		"object",
//		"operator",
//		"out",
//		"override",
//		"params",
//		"private",
//		"protected",
//		"public",
//		"readonly",
//		"ref",
//		"return",
//		"sbyte",
//		"sealed",
//		"select",
//		"sizeof",
//		"short",
//		"stackalloc",
//		"static",
//		"string",
//		"struct",
//		"switch",
//		"this",
//		"throw",
//		"true",
//		"try",
//		"typeof",
//		"unchecked",
//		"uint",
//		"ulong",
//		"unsafe",
//		"using",
//		"var",
//		"virtual",
//		"volatile",
//		"void",
//		"where",
//		"while",
//		"yield"
//	};
//	#endregion

//	List<Tab> fileTabs = new List<Tab>();
//	int fileTabIndex = -1;
//	// Use this for initialization
//	[MenuItem("Window/Script Editor")]
//	static void Init()
//	{
//		ScriptEditor window = (ScriptEditor)EditorWindow.GetWindow(typeof(ScriptEditor), false, "Script Editor");
//		window.s = "";
//		window.lines = 0;
//		window.textStyle = null;
//		window.minSize = new Vector2(200, 100);
//		window.cNamespaces = new List<string>()
//		{
//			"abstract",
//			"as",
//			"base",
//			"break",
//			"bool",
//			"by",
//			"byte",
//			"case",
//			"catch",
//			"char",
//			"checked",
//			"class",
//			"const",
//			"continue",
//			"decimal",
//			"default",
//			"delegate",
//			"do",
//			"double",
//			"else",
//			"enum",
//			"event",
//			"explicit",
//			"extern",
//			"false",
//			"finally",
//			"fixed",
//			"float",
//			"for",
//			"foreach",
//			"from",
//			"group",
//			"if",
//			"implicit",
//			"in",
//			"int",
//			"interface",
//			"internal",
//			"into",
//			"is",
//			"lock",
//			"long",
//			"namespace",
//			"new",
//			"null",
//			"object",
//			"operator",
//			"out",
//			"override",
//			"params",
//			"private",
//			"protected",
//			"public",
//			"readonly",
//			"ref",
//			"return",
//			"sbyte",
//			"sealed",
//			"select",
//			"sizeof",
//			"short",
//			"stackalloc",
//			"static",
//			"string",
//			"struct",
//			"switch",
//			"this",
//			"throw",
//			"true",
//			"try",
//			"typeof",
//			"unchecked",
//			"uint",
//			"ulong",
//			"unsafe",
//			"using",
//			"var",
//			"virtual",
//			"volatile",
//			"void",
//			"where",
//			"while",
//			"yield"
//		};
//	}

//	class Tab
//	{
//		public string fileName;
//		public string filePath;
//		public string fileText;

//		public Tab(string name, string path, string text)
//		{
//			fileName = name;
//			filePath = path;
//			fileText = text;
//		}
//	}

//	// Update is called once per frame
//	void OnGUI()
//	{
//		if (textStyle == null)
//			textStyle = new GUIStyle(GUI.skin.textArea);
//		TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
//		//backup color
//		Color backupColor = Color.white;
//		Color backupContentColor = Color.black;
//		Color backupBackgroundColor = GUI.backgroundColor;

//		GUI.Box(new Rect(0, 0, position.width, 20), "", "toolbar");

//		toolbarMenuIndex = EditorGUI.Popup(new Rect(0, 0, 40, 20), toolbarMenuIndex, fileOptions, "toolbarButton");
//		GUI.Label(new Rect(0, 0, 40, 20), "File");
//		if (GUI.changed)
//		{
//			CheckFileOptions(te);
//		}

//		toolbarMenuIndex = EditorGUI.Popup(new Rect(40, 0, 40, 20), toolbarMenuIndex, editOptions, "toolbarButton");
//		GUI.Label(new Rect(40, 0, 40, 20), "Edit");
//		if (GUI.changed)
//		{
//			CheckEditOptions(te);
//		}
//		GUI.Box(new Rect(0, 18, position.width, 20), "", "toolbar");

//		tSize.y = textStyle.CalcHeight(new GUIContent(s), position.width);
//		tSize.x = position.width;

//		GUILayout.BeginArea(new Rect(0, 18, position.width, 20));
//		GUILayout.BeginHorizontal();
//		if (fileTabs.Count > 0)
//		{
//			for (int i = 0; i < fileTabs.Count; i++)
//			{
//				Tab tab = fileTabs[i];
//				if (GUILayout.Button(tab.fileName, "toolbarButton"))
//				{
//					fileTabs[fileTabIndex].fileText = s;
//					fileTabIndex = i;
//					s = tab.fileText;
//				}
//			}
//		}
//		GUILayout.EndHorizontal();
//		GUILayout.EndArea();
//		scrollPos = GUI.BeginScrollView(new Rect(0, 36, position.width, position.height - 36), scrollPos, new Rect(0, 0, tSize.x - 15, tSize.y + 20));

//		for (int i = 0; i < lines; i++)
//		{
//			Vector2 lSize = textStyle.CalcSize(new GUIContent(i.ToString()));
//			if (i % 2 == 1)
//			{
//				GUI.color = Color.gray;
//				GUI.Box(new Rect(-2, 13 * i + 13, position.width + 4 + tSize.x, 13), "");
//			}
//			GUI.color = Color.white;
//			GUI.Label(new Rect(0, 13 * i + 13, lSize.x + 50, 13), "" + i);
//		}

//		Event ev = Event.current;

//		//add textarea with transparent text
//		GUI.contentColor = new Color(1f, 1f, 1f, 0f);
//		Rect bounds = new Rect(60, 13, position.width - 80, position.height + tSize.y);
//		GUI.SetNextControlName("TextArea");

//		s = GUI.TextArea(bounds, s);
//		//get the texteditor of the textarea to control selection


//		CheckKeys(te, ev);
//		//set background of all textfield transparent
//		GUI.backgroundColor = new Color(1f, 1f, 1f, 0f);

//		//backup selection to remake it after process
//		int backupPos = te.pos;
//		int backupSelPos = te.selectPos;

//		//get last position in text
//		te.MoveTextEnd();
//		int endpos = te.pos;
//		//draw textfield with color on top of text area
//		UpdateText(te, ev, endpos, textStyle, backupPos, backupSelPos);

//		//Reset color
//		GUI.color = backupColor;
//		GUI.contentColor = backupContentColor;
//		GUI.backgroundColor = backupBackgroundColor;

//		GUI.EndScrollView();
//	}

//	private void CheckFileOptions(TextEditor te)
//	{
//		FileStream fs;
//		StreamWriter sw;
//		string path;
//		if (toolbarMenuIndex == -1)
//			return;
//		switch (toolbarMenuIndex)
//		{
//			//New
//			case 0:
//				this.path = "";
//				textStyle = new GUIStyle(GUI.skin.textArea);
//				s = "";
//				redos.Clear();
//				break;

//			//Open
//			case 1:
//				path = EditorUtility.OpenFilePanel("Load...", "", "*.*");
//				if (path != "")
//				{
//					this.path = path;

//					textStyle = new GUIStyle(GUI.skin.textArea);
//					bool fileIsOpen = false;
//					if (fileTabs.Count > 0)
//					{
//						for (int i = 0; i < fileTabs.Count; i++)
//						{
//							Tab t = fileTabs[i];
//							if (t.filePath == path)
//							{
//								fileIsOpen = true;
//							}
//						}
//						if (!fileIsOpen)
//						{
//							s = "";
//							lines = 0;
//							LoadFile(path);
//						}
//					}
//					else
//					{
//						s = "";
//						lines = 0;
//						LoadFile(path);
//					}

//				}
//				redos.Clear();

//				break;

//			//Save
//			case 2:
//				fs = new FileStream(this.path, FileMode.Open);
//				fs.SetLength(0);
//				fs.Close();
//				sw = new StreamWriter(this.path);
//				sw.Write(s);
//				sw.Close();
//				AssetDatabase.Refresh();
//				break;

//			//Save as
//			case 3:
//				path = EditorUtility.OpenFilePanel("Save as...", "", "*.*");
//				if (path != "")
//				{
//					this.path = path;
//					fs = new FileStream(path, FileMode.Open);
//					fs.SetLength(0);
//					fs.Close();
//					sw = new StreamWriter(path);
//					sw.Write(s);
//					sw.Close();
//					AssetDatabase.Refresh();
//				}
//				break;

//			//Close
//			case 4:

//				break;

//			//Close all
//			case 5:

//				break;
//		}
//		toolbarMenuIndex = -1;
//	}

//	private void LoadFile(string path)
//	{
//		StreamReader sr = new StreamReader(path);
//		string newText = "";
//		while (!sr.EndOfStream)
//		{
//			string newLine = sr.ReadLine();
//			newText += newLine + "\n";
//		}
//		int index = path.LastIndexOf("/");
//		string file = path.Substring(index + 1);
//		sr.Close();
//		fileTabs.Add(new Tab(file, path, newText));
//		fileTabIndex = fileTabs.Count - 1;
//		s = newText;
//		tSize.y = textStyle.CalcHeight(new GUIContent(s), position.width);
//		tSize.x = position.width;
//		Repaint();
//	}

//	private void CheckEditOptions(TextEditor te)
//	{
//		if (toolbarMenuIndex == -1)
//			return;

//		switch (toolbarMenuIndex)
//		{
//			//Cut
//			case 0:
//				te.Cut();
//				s = te.content.text;
//				break;

//			//Copy
//			case 1:
//				te.Copy();
//				break;

//			//Paste
//			case 2:
//				te.Paste();
//				s = te.content.text;
//				break;
//		}

//		toolbarMenuIndex = -1;
//	}

//	void UpdateText(TextEditor te, Event ev, int endpos, GUIStyle textStyle, int backupPos, int backupSelPos)
//	{
//		te.MoveTextStart();
//		lines = 0;
//		while (te.pos != endpos)
//		{
//			te.SelectToStartOfNextWord(); edText;

//			string wordtext = te.Select
//			//set word color
//			GUI.contentColor = CheckSyntax(wordtext);

//			Vector2 pixelselpos = textStyle.GetCursorPixelPosition(te.position, te.content, 10);
//			Vector2 pixelpos = textStyle.GetCursorPixelPosition(te.position, te.content, te.pos);

//			GUI.TextField(new Rect(pixelselpos.x - textStyle.border.left - 2f, pixelselpos.y - textStyle.border.top, pixelpos.x, pixelpos.y), wordtext);
//			if (wordtext.Contains("\n"))
//				lines++;
//			te.MoveToStartOfNextWord();
//		}
//		lines++;

//		//Reposition selection
//		Vector2 bkpixelselpos = textStyle.GetCursorPixelPosition(te.position, te.content, backupSelPos);
//		te.MoveCursorToPosition(bkpixelselpos);

//		//Remake selection
//		Vector2 bkpixelpos = textStyle.GetCursorPixelPosition(te.position, te.content, backupPos);
//		te.SelectToPosition(bkpixelpos);
//	}

//	Color CheckSyntax(string syntax)
//	{
//		string newSyntax = syntax.TrimEnd(' ');
//		foreach (string st in cNamespaces)
//		{
//			if (newSyntax == st!isCommenting)
//                return Color.cyan;
//		}

//		if (newSyntax.StartsWith("//") || newSyntax.StartsWith("*/"))
//		{
//			isCommenting = true;
//			if (newSyntax.StartsWith("*/"))
//				commentAffectsNextLine = true;
//			return Color.green;
//		}

//		if (newSyntax.StartsWith("\"") || newSyntax.StartsWith("\'"))
//		{
//			isString = true;
//			return Color.magenta;
//		}

//		if (newSyntax.StartsWith("/*"))
//		{
//			commentAffectsNextLine = false;
//			return Color.green;
//		}

//		if (isString  newSyntax != "\n"!isCommenting )
//            return Color.magenta;

//		if (isCommenting  newSyntax != "\n")
//            return Color.green;

//		if (isCommenting(newSyntax == "\n"  commentAffectsNextLine))
//			return Color.green;

//		if (newSyntax == "\n")
//		{
//			isCommenting = false;
//			isString = false;
//		}

//		return Color.white;
//	}

//	void CheckKeys(TextEditor te, Event ev)
//	{
//		if (GUIUtility.keyboardControl == te.controlID   ev.Equals(Event.KeyboardEvent("tab")) )
//        {
//			Debug.Log("tab pressed");
//			GUI.FocusControl("TextArea");
//			if (s.Length > te.pos)
//			{
//				s = s.Insert(te.pos, "\t");
//				te.pos++;
//				te.selectPos = te.pos;
//			}
//			ev.Use();
//			GUI.FocusControl("TextArea");
//		}

//		if ((Event.current.type == EventType.KeyUp)(Event.current.keyCode == KeyCode.Space)(GUI.GetNameOfFocusedControl() == "MyTextArea"))
//		{
//			Debug.Log("space pressed");
//		}

//		if ((Event.current.type == EventType.KeyUp)(Event.current.keyCode == KeyCode.Return)(GUI.GetNameOfFocusedControl() == "MyTextArea"))
//		{

//			Debug.Log("Enter pressed");
//			if (s.Length > te.pos)
//			{
//				s = s.Insert(te.pos, "\t");
//				te.pos++;
//				te.selectPos = te.pos;
//			}
//			ev.Use();
//		}
//	}
//}