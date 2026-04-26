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
		
		_label = GetNode<Label>("LabelAnchor/CheckpointLabel");
	}

	public void Initialize(int x, int y, int type, int checkpointNumber)
	{
        Initialize(x, y, type);
		_checkpointNumber = checkpointNumber;
		
		if (_label != null)
			_label.Text = "★" + _checkpointNumber;
	}
}
