using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
//using TMPro;

public class Syntax
{
	static Regex regex;
	static MatchEvaluator evaluator;

	// 指定文字
	public static readonly string[] cNamespaces = new string[] 
	{
		"abstract",
		"as",
		"base",
		"break",
		"bool",
		"by",
		"byte",
		"case",
		"catch",
		"char",
		"checked",
		"class",
		"const",
		"continue",
		"decimal",
		"default",
		"delegate",
		"double",
		"do",
		"else",
		"enum",
		"event",
		"explicit",
		"extern",
		"false",
		"finally",
		"fixed",
		"float",
		"foreach",
		"for",
		"from",
		"group",
		"if",
		"implicit",
		"int",
		"interface",
		"internal",
		"into",
		"is",
		"lock",
		"long",
		"namespace",
		"new",
		"null",
		"object",
		"operator",
		"out",
		"override",
		"params",
		"private",
		"protected",
		"public",
		"readonly",
		"ref",
		"return",
		"sbyte",
		"sealed",
		"select",
		"sizeof",
		"short",
		"stackalloc",
		"static",
		"string",
		"struct",
		"switch",
		"this",
		"throw",
		"true",
		"try",
		"typeof",
		"unchecked",
		"uint",
		"ulong",
		"unsafe",
		"using",
		"var",
		"virtual",
		"volatile",
		"void",
		"where",
		"while",
		"yield"
	};

	// 文字色
	static Dictionary<string, string> colorTable = new Dictionary<string, string>() {
			{ "type",    "#ff0000" },
			{ "symbol",  "#ff00ff" },
			{ "digit",   "#00ff00" },
			{ "comment", "#555555" },
	};

	public static void Init()
	{
		var symbol = @"[{}()=;,+\-*/<>|]+";
		var digit = @"(?<![a-zA-Z_])[+-]?[0-9]+\.?[0-9]?(([eE][+-]?)?[0-9]+)?";
		var comment = @"/\*[\s\S]*?\*/|//.*";

		var block = "(?<{0}>({1}))";
		var pattern = "(" + string.Join("|", new string[] {
			string.Format(block, "comment", comment),
			string.Format(block, "type",string.Join("|",cNamespaces)),
			string.Format(block, "symbol",  symbol),
			string.Format(block, "digit",   digit),
		}) + ")";

		regex = new Regex(pattern, RegexOptions.Compiled);

		

		evaluator = new MatchEvaluator(match => {
			foreach (var pair in colorTable)
			{
				if (match.Groups[pair.Key].Success)
				{
					return string.Format("<color={1}>{0}</color>", match.Value, pair.Value);
				}
			}
			return match.Value;
		});
	}

	public static string Highlight(string code)
	{
		return regex.Replace(code, evaluator);
	}
}

public class textGui : MonoBehaviour
{

	string stext;

	[SerializeField]
	private Text intext;
	[SerializeField]
	private Text displayText;

	[SerializeField]
	GUISkin skin;

	//[SerializeField]
	//TextMeshProUGUI textPro;

	string picupText;           // 表示用
	private string hideText = "";       // 入力用
	
	// ハイライト用関数
	public System.Func<string,string> highlighter { get; set; }

	// 表示高層化のために変更があった時だけコード更新
	string cachedHighlightedCode { get; set; }

	TextEditor te;
	bool tabFlag = false;
	int tabIndex = 0;

	Stack<int> funcIndex = new Stack<int>();

	Stack<LOOP_INDEX> loopIndex = new Stack<LOOP_INDEX>();
	int loopCount = 0;
	static  public LOOP_NUMBER loopStepNumber = LOOP_NUMBER.NONE;

	public enum LOOP_NUMBER
	{
		NONE = 0,
		INIT,
		TERM,
		PROCESSING,
		NEXT,
		END,
	}


	LOOP_INDEX li;

	public struct LOOP_INDEX
	{
		public LOOP_NUMBER StepNumber;
		public int endIndex;
		public int termIndex;
		public int nextIndex;
		public int startIndex;
	}

	private void Start()
	{
		Syntax.Init();
		highlighter = code => code;
	}
	
	private void OnGUI()
	{
		GUI.skin = skin;
		
		te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
		if(tabFlag)
		{
			te.cursorIndex = tabIndex;
			te.selectIndex = tabIndex;
			// 2f程値が0に強制的に戻る
			// 原因不明
			if(tabIndex == te.cursorIndex)
				tabFlag = false;
		}
	    var style = new GUIStyle(GUI.skin.textArea);
		style.fontSize = (int)intext.fontSize;
		style.wordWrap = true;
		hideText = Dorw(te, hideText, style);
	}

	string Dorw(TextEditor te, string code,GUIStyle style)
	{
		int cIndex = te.cursorIndex;
		var preBackGroundColor = GUI.backgroundColor;
		var preColor = GUI.color;
		//Rect rect1 = new Rect(0 ,0, intext.rectTransform.sizeDelta.x, intext.rectTransform.sizeDelta.y);
		Rect rect1 = new Rect(0 ,0, 960, 1080);

		var backStyle = new GUIStyle(style);
		
		backStyle.normal.textColor = Color.clear;
		backStyle.hover.textColor = Color.clear;
		backStyle.active.textColor = Color.clear;
		backStyle.focused.textColor = Color.clear;
		
		var backGroundColor = Color.clear;
		
		GUI.backgroundColor = backGroundColor;
		
		Event ev = Event.current;

		var editedCode = GUI.TextArea(rect1, code, backStyle);
		//buckupPos = te.cursorIndex;
		CheckKeys(te, ev, ref editedCode);
		GUIContent content = new GUIContent() ;
		content.text = editedCode;
		backStyle.richText = true;
		// シンタックスハイライトさせたコードを更新
		
		if ((editedCode != code))
		{
			CreateText(rect1, editedCode, style);
		}
		
		GUI.backgroundColor = preBackGroundColor;
		GUI.color = preColor;
		
		return editedCode;
	}

	void CheckKeys(TextEditor te, Event ev,ref string code)
	{

		if ((GUIUtility.keyboardControl == te.controlID) &&  ev.Equals(Event.KeyboardEvent("tab")) )
		{
			tabFlag = true;
			code = code.Insert(te.cursorIndex, "   ");
			tabIndex = te.cursorIndex + 3;
		}
		if (((Event.current.keyCode == KeyCode.F10) && (Event.current.type == EventType.KeyUp)) || ReadText.skipFlag)
		{
			ReadTextDataTest(te, ev);
		}
		// デバッグ用リセット
		if ((Event.current.keyCode == KeyCode.F1) && (Event.current.type == EventType.KeyUp))
		{
			FuncSerch(te, ev);
			ResetData(te);
		}
		// デバッグ用　一括送信
		if ((Event.current.keyCode == KeyCode.F2) && (Event.current.type == EventType.KeyUp))
		{
			FuncSerch(te, ev);
			ReadTextData(te, ev);
		}
	}

	void Init()
	{

		loopCount = 0;

		li.endIndex = li.nextIndex = li.startIndex = li.termIndex = 0;

		loopIndex.Clear();

		loopStepNumber = 0;
	}
	void FuncSerch(TextEditor te, Event ev)
	{
		// cursorIndex = 今のカーソル位置
		// selectIndex = 移動後の位置（範囲選択）
		ReadText.InitializeData();

		// 変数一覧削除
		DataTable.ClearData();

		te.MoveTextEnd();
		var endpos = te.selectIndex;
		te.MoveTextStart();

		int counter = 0, maxcount = 2000;

		Init();

		while (te.cursorIndex != endpos)
		{
			int line = 1;

			te.SelectToStartOfNextWord();
			ReadText.CreateFuncData(te.SelectedText, line, te.cursorIndex);

			if (te.SelectedText.Contains("\n"))
			{
				line++;
			}
			te.MoveToStartOfNextWord();

			// 永久ループ回避
			if (maxcount <= counter)
			{
				Debug.LogError("無限ループしました");
				break;
			}
			counter++;
		}
	}
	void ReadTextProc(TextEditor te, Event ev)
	{
		int line = 1;
		string nowText = "";

		te.SelectToStartOfNextWord();
		ReadText.GetText(te.SelectedText, line, te.cursorIndex);


		nowText = te.SelectedText;
		displayText.text = nowText;
		if (te.SelectedText.Contains("\n"))
		{
			line++;
		}
		te.MoveToStartOfNextWord();

		if (ReadText.loopEndFlag)
		{
			te.selectIndex = te.cursorIndex = loopIndex.Peek().endIndex;
			loopIndex.Pop();
		}
		else if (ReadText.newLoopFlag)
		{
			li.endIndex = 0;
			li.nextIndex = 0;
			li.termIndex = 0;
			li.startIndex = 0;
			li.StepNumber = LOOP_NUMBER.NONE;
			loopIndex.Push(li);
		}
		else if(ReadText.searchFuncFlag)
		{
			if(funcIndex.Peek() == te.cursorIndex)
			{
				funcIndex.Push(te.cursorIndex);
			}
		}
		// ループ対応
		if (ReadText.nextLoopFlag)
		{
			switch(ReadText.loopType)
			{
				// for文の場合
				case ReadText.LOOP_TYPE_NAME.FOR:
					#region for文
					switch (loopIndex.Peek().StepNumber)
					{
						case LOOP_NUMBER.NONE:
							NextLoop(LOOP_NUMBER.INIT);
							break;
						case LOOP_NUMBER.INIT:
							li.termIndex = te.cursorIndex;
							NextLoop(LOOP_NUMBER.TERM);
							break;
						case LOOP_NUMBER.TERM:
							if (li.nextIndex == 0)
							{
								li.nextIndex = te.cursorIndex;
							}
							else
							{
								te.selectIndex = te.cursorIndex = loopIndex.Peek().startIndex;
							}
							NextLoop(LOOP_NUMBER.PROCESSING);
							break;

						case LOOP_NUMBER.PROCESSING:
							if (li.startIndex == 0)
							{
								li.startIndex = te.cursorIndex - 1;
								li.StepNumber = loopIndex.Peek().StepNumber;

								loopIndex.Pop();            // 初めにダミー用のものを削除する
								loopIndex.Push(li);         // その後新しいデータを入れる
							}
							NextLoop(LOOP_NUMBER.END);
							break;
						case LOOP_NUMBER.NEXT:
							te.selectIndex = te.cursorIndex = loopIndex.Peek().termIndex;
							NextLoop(LOOP_NUMBER.TERM);
							break;
						case LOOP_NUMBER.END:
							if(loopIndex.Peek().endIndex == 0)
							{
								SetEndStep(te.cursorIndex);
							}
							te.selectIndex = te.cursorIndex = loopIndex.Peek().nextIndex;
							NextLoop(LOOP_NUMBER.NEXT);
							break;
					}
					loopStepNumber = loopIndex.Peek().StepNumber;
					#endregion
					break;
				case ReadText.LOOP_TYPE_NAME.WHILE:
					switch (loopIndex.Peek().StepNumber)
					{
						case LOOP_NUMBER.NONE:
							NextLoop(LOOP_NUMBER.TERM);
							break;
						case LOOP_NUMBER.TERM:
							if(loopIndex.Peek().termIndex == 0)
							{
								li.termIndex = te.cursorIndex;
								NextLoop(LOOP_NUMBER.NEXT);
							}
							else
							{
								// 2回目以降
								NextLoop(LOOP_NUMBER.PROCESSING);
							}
							break;
						case LOOP_NUMBER.NEXT:
							// 2回目以降(やり方が思いつかなかったので、仮）
							NextLoop(LOOP_NUMBER.PROCESSING);
							break;
						case LOOP_NUMBER.PROCESSING:
							if (loopIndex.Peek().startIndex == 0)
							{
								li.startIndex = te.cursorIndex - 1;
								li.StepNumber = loopIndex.Peek().StepNumber;

								loopIndex.Pop();            // 初めにダミー用のものを削除する
								loopIndex.Push(li);         // その後新しいデータを入れる
							}
							NextLoop(LOOP_NUMBER.END);
							break;
						case LOOP_NUMBER.END:
							if (loopIndex.Peek().endIndex == 0)
							{
								SetEndStep(te.cursorIndex);
							}
							te.selectIndex = te.cursorIndex = loopIndex.Peek().termIndex;
							NextLoop(LOOP_NUMBER.TERM);
							break;
					}
					break;
			}
			ReadText.loopStep = loopIndex.Peek().StepNumber;
		}
	}

	void ReadTextData(TextEditor te, Event ev)//, ref string code)
	{
		// cursorIndex = 今のカーソル位置
		// selectIndex = 移動後の位置（範囲選択）
		ReadText.InitializeData();

		// 変数一覧削除
		DataTable.ClearData();
		
		te.MoveTextEnd();
		var endpos = te.selectIndex;
		te.MoveTextStart();
		
		int counter = 0, maxcount = 2000;

		Init();

		while (te.cursorIndex != endpos)
		{
			ReadTextProc(te, ev);

			// 永久ループ回避
			if (maxcount <= counter)
			{
				Debug.LogError("無限ループしました");
				break;
			}
			counter++;
		}
		ReadText.CreateData();
	}

	public void NextLoop(LOOP_NUMBER num)
	{
		LOOP_INDEX tmp = loopIndex.Peek();
		tmp.StepNumber = num;
		loopIndex.Pop();
		loopIndex.Push(tmp);
	}

	public void SetEndStep(int index)
	{
		LOOP_INDEX tmp = loopIndex.Peek();
		tmp.endIndex = index;
		loopIndex.Pop();
		loopIndex.Push(tmp);
	}


	void CreateText(Rect rect, string code,GUIStyle style)
	{
		picupText = Syntax.Highlight(code);
		intext.text = picupText;
	}
	void ResetData(TextEditor te)
	{
		// cursorIndex = 今のカーソル位置
		// selectIndex = 移動後の位置（範囲選択）
		ReadText.InitializeData();
		// 変数一覧削除
		DataTable.ClearData();
		te.MoveTextEnd();
		var endpos = te.selectIndex;
		te.MoveTextStart();

		Init();
	}
	void ReadTextDataTest(TextEditor te, Event ev)//, ref string code)
	{
		ReadTextProc(te, ev);
		ReadText.CreateData();
	}
}