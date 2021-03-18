using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;

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

	string picupText;           // 表示用
	private string hideText = "";       // 入力用
	
	// ハイライト用関数
	public System.Func<string,string> highlighter { get; set; }

	// 表示高層化のために変更があった時だけコード更新
	string cachedHighlightedCode { get; set; }

	TextEditor te;

	int buckupPos;

	private void Start()
	{
		Syntax.Init();
		highlighter = code => code;
	}

	private void Update()
	{
		
	}
	
	private void OnGUI()
	{
		GUI.skin = skin;
		te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
		var style = new GUIStyle(GUI.skin.textArea);
		style.fontSize = (int)intext.fontSize;
		style.wordWrap = true;
		hideText = Dorw(te, hideText, style);
	}

	string Dorw(TextEditor te, string code,GUIStyle style)
	{
		
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
		buckupPos = te.cursorIndex;
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
			ReadTextData(te, ev);
		}
	}

	void ReadTextData(TextEditor te, Event ev)//, ref string code)
	{
		ReadText.InitializeData();

		// 変数一覧削除
		DataTable.CrearData();
		
		te.MoveTextEnd();
		var endpos = te.selectIndex;
		te.MoveTextStart();
		int counter = 0, maxcount = 2000;
		int line = 1;
		string nowText = "";
		while (te.cursorIndex != endpos)
		{
			te.SelectToStartOfNextWord();
			ReadText.GetText(te.SelectedText, line);
			nowText = te.SelectedText;
			displayText.text = nowText;

			te.MoveToStartOfNextWord();
			counter++;
			if (te.SelectedText.Contains("\n"))
			{
				line++;
			}
			// 永久ループ回避
			if (maxcount <= counter)
			{
				break;
			}
		}
		ReadText.CreateData();
	}

	void CreateText(Rect rect, string code,GUIStyle style)
	{
		picupText = Syntax.Highlight(code);
		intext.text = picupText;
	}

}