using Godot;

public partial class DataManager : MenuButton
{
	[Export]
	private AssemblyEditor assemblyEditor;
	[Export]
	private FileDialog saveASFileDialog;
	[Export]
	private FileDialog openFileDialog;

	private string assemblyFilePath = "";

	private bool is_file_saved = true;

	public override void _Ready()
	{
		base._Ready();

		GetPopup().IndexPressed += _on_file_menu_button_index_pressed;

		saveASFileDialog.FileSelected += (path) =>
		{
			assemblyFilePath = path;
			save();
		};

		openFileDialog.FileSelected += (path) =>
		{
			DisplayServer.WindowSetTitle("Simple22 IDE - " + path);
			open_file(path);
		};

		assemblyEditor.TextChanged += () => 
		{ 
			if (is_file_saved)
			{
				DisplayServer.WindowSetTitle("*Simple22 IDE - " + assemblyFilePath);
				is_file_saved = false;
			}
		};
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Input.IsActionJustPressed("Save As"))
		{
			save_as();
		}
		else if(Input.IsActionJustPressed("Save"))
		{
			if (assemblyFilePath != "")
				save();
			else
				save_as();
		}
		else if (Input.IsActionJustPressed("Open File"))
		{
			openFileDialog.Popup();
		}

	}

	private void _on_file_menu_button_index_pressed(long index)
	{
		switch (index)
		{
			case 0://save
				if (assemblyFilePath != "")
					save();
				else
					save_as();
				break;
			case 1://save as
				save_as();
				break;
			case 2://open
				openFileDialog.Popup();
				break;
			case 3://close
				close_file();
				break;
		}
	}

	private void save()
	{
		DisplayServer.WindowSetTitle("Simple22 IDE - " + assemblyFilePath);
		is_file_saved = true;
		FileAccess file = FileAccess.Open(assemblyFilePath, FileAccess.ModeFlags.Write);
		file.StoreString(assemblyEditor.Text);
		file.Close();
	}

	private void save_as()
	{
		saveASFileDialog.Popup();
	}

	private void open_file(string path)
	{
		assemblyFilePath = path;
		FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		assemblyEditor.Text = file.GetAsText();
		file.Close();
	}

	private void close_file()
	{
		DisplayServer.WindowSetTitle("Simple22 IDE");
		assemblyFilePath = "";
		assemblyEditor.Clear();
	}
}
