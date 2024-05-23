using Godot;
using System;
using System.Collections.Generic;
using System.IO.Ports;

public partial class UART : Node
{
	[Export]
	private Button loadButton;
	[Export]
	private AssemblyEditor assemblyEditor;

	[Export]
	private TextEdit outputConsole;

	private SerialPort port;
	private int intType = -1;
	private List<byte> inputBuffer = new List<byte>();

	public override void _Ready()
	{
		base._Ready();

		try
		{
			port = new SerialPort(SerialPort.GetPortNames()[0], 9600, Parity.None, 8, StopBits.One);
			port.ReadTimeout = 20;
			port.Open();
		}
		catch (Exception ex)
		{
			outputConsole.InsertTextAtCaret(Time.GetDatetimeStringFromSystem() + " - Could not connect to port: " + ex.Message);
		}

		loadButton.Pressed += () =>
		{
			load_instructions();
		};
	}

	public override void _Process(double delay)
	{
		base._Process(delay);

		if (Input.IsActionJustPressed("Load"))
		{
			load_instructions();
		}

		if (port.BytesToRead > 0)
		{
			int input_byte = port.ReadByte();
			if (intType == -1)
			{
				intType = input_byte;
			}
			else
			{
				switch (intType)
				{
					case 0:
						inputBuffer.Add((byte)input_byte);
						if (inputBuffer.Count == 4)
						{
							intType = -1;
							int integer = inputBuffer[3];
							integer = integer << 8 | inputBuffer[2];
							integer = integer << 8 | inputBuffer[1];
							integer = integer << 8 | inputBuffer[0];
							outputConsole.InsertTextAtCaret(integer + "\n");
							inputBuffer.Clear();
						}
						break;
					case 1:
						inputBuffer.Add((byte)input_byte);
						if (inputBuffer.Count == 4)
						{
							intType = -1;
							outputConsole.InsertTextAtCaret((char)(inputBuffer[0]) + "");
							inputBuffer.Clear();
						}
						break;
				}
			}
		}
	}

	private void load_instructions()
	{
		if (assemblyEditor.compiled_binary_lines.Count == 0)
		{
			outputConsole.InsertTextAtCaret("load failed\n");
			return;
		}

		outputConsole.InsertTextAtCaret(Time.GetDatetimeStringFromSystem() + " - start loading...\n");

		int instruction_index = 0;

		while (instruction_index < assemblyEditor.compiled_binary_lines.Count)
		{
			try
			{
				port.Write(new byte[]
				{
				(byte)assemblyEditor.compiled_binary_lines[instruction_index],
				(byte)(assemblyEditor.compiled_binary_lines[instruction_index] >> 8),
				(byte)(assemblyEditor.compiled_binary_lines[instruction_index] >> 16),
				(byte)(assemblyEditor.compiled_binary_lines[instruction_index] >> 24)
				}, 0, 4);
				instruction_index++;
			}
			catch (Exception ex)
			{
				outputConsole.InsertTextAtCaret(Time.GetDatetimeStringFromSystem() + " - Could not send the instruciton: " + ex.Message);
			}
		}

		outputConsole.InsertTextAtCaret(Time.GetDatetimeStringFromSystem() + " - finished loading\n");
	}

	~UART()
	{
		port.Close();
	}
}
