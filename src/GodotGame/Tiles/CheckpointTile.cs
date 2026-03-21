namespace AiTest.Godot.Tiles;

using Godot;

public partial class CheckpointTile : Tile
{
	private Label _label = null!;
	private int _checkpointNumber = 1;

	protected override Color BaseColor => new(0.3f, 0.25f, 0.1f);
	protected override Color BorderColor => new(0.4f, 0.35f, 0.2f);

	protected override void OnReady()
	{
		base.OnReady();
		
		_label = new Label
		{
			Name = "CheckpointLabel",
			Text = "★1",
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};
		_label.AddThemeFontSizeOverride("font_size", 20);
		_label.AddThemeColorOverride("font_color", new Color(1.0f, 0.84f, 0.0f));
		
		var anchor = new Node2D { Name = "LabelAnchor", Position = new Vector2(24, 24) };
		anchor.AddChild(_label);
		AddChild(anchor);
	}

	public void Initialize(Core.Models.Position pos, Core.Models.TileType type, int checkpointNumber)
	{
		base.Initialize(pos, type);
		_checkpointNumber = checkpointNumber;
		
		if (_label != null)
			_label.Text = "★" + _checkpointNumber;
	}
}
