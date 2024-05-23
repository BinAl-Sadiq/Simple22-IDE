using Godot;
using Simple22Ide.Scripts;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public partial class AssemblyEditor : CodeEdit
{
	[Export]
	private Color instructionsColor;
	[Export]
	private Color GPRsColor;
	[Export]
	private Color SpecialRegistorsColor;
	[Export]
	private Color CommentColor;
	[Export]
	private Color StringColor;
	[Export]
	private Color NewlineColor;

	[Export]
	private CodeEdit compiledCode;
	[Export]
	private TextEdit outputConsole;

	[Export]
	private Button compileButton;
	[Export]
	private Button helpButton;
	[Export]
	private Button clearButton;

	[Export]
	private Window helpWindoe;

	public List<UInt32> compiled_binary_lines = new List<UInt32>();

	#region Reserved Words
	readonly List<string> Instructions = new List<string>
	{
		"add",
		"sub",
		"mul",
		"div",
		"store",
		"load",
		"set",
		"int",
		"log",//sudo
		"special_get",
		"special_set",
		"exit"
	};

	readonly Dictionary<string, string> GPRs = new Dictionary<string, string>
	{
		{"GPR0","000"},
		{"GPR1","001"},
		{"GPR2","010"},
		{"GPR3","011"},
		{"GPR4","100"},
		{"GPR5","101"},
		{"GPR6","110"},
		{"GPR7","111"}
	};

	readonly Dictionary<string, string> SpecialRegisters = new Dictionary<string, string>
	{
		{"sp","0"},
		{"pc","1"}
	};
	#endregion

	public override void _Ready()
	{
		base._Ready();

		CodeHighlighter ch = SyntaxHighlighter as CodeHighlighter;
		ch.AddColorRegion("//", "", CommentColor);
		ch.AddColorRegion("\"", "\"", StringColor);
		ch.SymbolColor = NewlineColor;
		foreach (string str in Instructions) ch.AddKeywordColor(str, instructionsColor);
		foreach (string str in GPRs.Keys) ch.AddKeywordColor(str, GPRsColor);
		foreach (string str in SpecialRegisters.Keys) ch.AddKeywordColor(str, SpecialRegistorsColor);
		CodeHighlighter ch1 = compiledCode.SyntaxHighlighter as CodeHighlighter;
		foreach (string str in Instructions) ch1.AddKeywordColor(str, instructionsColor);
		foreach (string str in GPRs.Keys) ch1.AddKeywordColor(str, GPRsColor);
		foreach (string str in SpecialRegisters.Keys) ch1.AddKeywordColor(str, SpecialRegistorsColor);

		compileButton.Pressed += () =>
		{
			_on_compile_button_pressed();			
		};

		helpButton.Pressed += () => { helpWindoe.Popup(); };
		helpWindoe.CloseRequested += () => { helpWindoe.Hide(); };
		clearButton.Pressed += () => { if (outputConsole.Text.Length > 0) outputConsole.Clear(); };
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Input.IsActionJustPressed("Compile"))
		{
			_on_compile_button_pressed();
		}
	}

	private void _on_compile_button_pressed()
	{
		try
		{
			outputConsole.InsertTextAtCaret(Time.GetDatetimeStringFromSystem() + " - start compiling...\n");
			if (compiledCode.Text.Length > 0)
				compiledCode.Clear();
			compiled_binary_lines.Clear();
			int has_one_exit = 0;
			string[] assembly_lines = Text.Split('\n');
			List<List<string>> assembly_binary_pairs = new List<List<string>>();
			decimal line_index = 0;
			foreach (string assembly_line in assembly_lines)
			{
				line_index++;
				string clean_assembly_line = Regex.Replace(assembly_line.Trim(), @"\s+", " ");
				clean_assembly_line = Regex.Replace(clean_assembly_line, @"\s*//.*", "\n");
				if (clean_assembly_line.Length > 0)
				{
					string[] tokens = clean_assembly_line.Split();

					if (Instructions.Contains(tokens[0]))
					{
						switch (tokens[0])
						{
							case "add":
								if (Regex.IsMatch(clean_assembly_line, "^add( GPR[0-7]){3}$"))
								{
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									assembly_binary_pair.Add("000000000000000000" + GPRs[tokens[3]] + GPRs[tokens[2]] + GPRs[tokens[1]] + "00000");
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
							case "sub":
								if (Regex.IsMatch(clean_assembly_line, "^sub( GPR[0-7]){3}$"))
								{
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									assembly_binary_pair.Add("000000000000000000" + GPRs[tokens[3]] + GPRs[tokens[2]] + GPRs[tokens[1]] + "00001");
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
							case "mul":
								if (Regex.IsMatch(clean_assembly_line, "^mul( GPR[0-7]){3}$"))
								{
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									assembly_binary_pair.Add("000000000000000000" + GPRs[tokens[3]] + GPRs[tokens[2]] + GPRs[tokens[1]] + "00010");
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
							case "div":
								if (Regex.IsMatch(clean_assembly_line, "^div( GPR[0-7]){3}$"))
								{
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									assembly_binary_pair.Add("000000000000000000" + GPRs[tokens[3]] + GPRs[tokens[2]] + GPRs[tokens[1]] + "00011");
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
							case "store":
								if (Regex.IsMatch(clean_assembly_line, "^store GPR[0-7]$"))
								{
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									assembly_binary_pair.Add("000000000000000000000000" + GPRs[tokens[1]] + "00100");
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
							case "load":
								if (Regex.IsMatch(clean_assembly_line, "^load GPR[0-7]$"))
								{
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									assembly_binary_pair.Add("000000000000000000000000" + GPRs[tokens[1]] + "00101");
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
							case "set":
								if (Regex.IsMatch(clean_assembly_line, "^set GPR[0-7] \\d+$"))
								{
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									string immediate = Convert.ToString(Int32.Parse(tokens[2]), 2);
									while (immediate.Length < 24) immediate = "0" + immediate;
									assembly_binary_pair.Add(immediate + GPRs[tokens[1]] + "00110");
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
							case "int":
								if (Regex.IsMatch(clean_assembly_line, "^int \\d+$"))
								{
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									string immediate = Convert.ToString(Int32.Parse(tokens[1]), 2);
									while (immediate.Length < 27) immediate = "0" + immediate;
									assembly_binary_pair.Add(immediate + "00111");
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
							case "log":
								if (Regex.IsMatch(clean_assembly_line, "^log \".*\"$"))
								{
									clean_assembly_line = clean_assembly_line.Replace("\\n", "\n");
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									for (int index = clean_assembly_line.Find("\"") + 1; index < clean_assembly_line.Length - 1; index++)
									{
										string rest = Convert.ToString((byte)clean_assembly_line[index], 2);
										while (rest.Length < 24) rest = "0" + rest;
										assembly_binary_pair.Add(rest + "00000110");
										assembly_binary_pair.Add("00000000000000000000000000100111");
									}
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
							case "special_get":
								if (Regex.IsMatch(clean_assembly_line, "^special_get GPR[0-7] (sp|pc)$"))
								{
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									assembly_binary_pair.Add("00000000000000000000000" + SpecialRegisters[tokens[2]] + GPRs[tokens[1]] + "01000");
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
							case "special_set":
								if (Regex.IsMatch(clean_assembly_line, "^special_set (sp|pc) GPR[0-7]$"))
								{
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									assembly_binary_pair.Add("00000000000000000000000" + GPRs[tokens[2]] + SpecialRegisters[tokens[1]] + "01000");
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
							case "exit":
								if (Regex.IsMatch(clean_assembly_line, "^exit$"))
								{
									has_one_exit++;
									List<string> assembly_binary_pair = new List<string>();
									assembly_binary_pair.Add("#" + line_index + " " + clean_assembly_line);
									assembly_binary_pair.Add("11111111111111111111111111111111");
									assembly_binary_pairs.Add(assembly_binary_pair);
								}
								else
									throw new UnableToCompileException("error: unable to compile line " + line_index);
								break;
						}
					}
				}
			}

			if (has_one_exit != 1 || !assembly_binary_pairs[assembly_binary_pairs.Count - 1][0].Contains("exit"))
			{
				GD.Print(has_one_exit + " - " + assembly_binary_pairs[assembly_binary_pairs.Count - 1][0]);
				throw new UnableToCompileException("error: your code must contain one \"exit\" instruction at the end of it");
			}

			outputConsole.InsertTextAtCaret(Time.GetDatetimeStringFromSystem() + " - compield successfully\n");

			foreach (List<string> assembly_binary_pair in assembly_binary_pairs)
			{
				compiledCode.InsertTextAtCaret(assembly_binary_pair[0] + "\n");
				for (int i = 1; i < assembly_binary_pair.Count; i++)
				{
					compiledCode.InsertTextAtCaret("\t" + assembly_binary_pair[i] + "\n");
					try
					{
						compiled_binary_lines.Add(Convert.ToUInt32(assembly_binary_pair[i], 2));
					}
					catch (OverflowException ex)
					{
						throw new UnableToCompileException(assembly_binary_pair[0] + " the instruction is too big");
					}
				}
			}
		}
		catch (UnableToCompileException ex)
		{
			outputConsole.InsertTextAtCaret(ex.Message + "\n");
		}
		catch (Exception ex)
		{
			outputConsole.InsertTextAtCaret("unexpected error!\n");
		}
	}
}
