using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System;
using System.Reflection;

public partial class NYuyukoEnergyCounter : NEnergyCounter
{
	private const string DarkenedMatPath = "res://materials/ui/energy_orb_dark.tres";
	private const float Layer3Fps = 24f;
	private const int Layer3FrameCount = 15;
	private static readonly Color EnergyTint = new Color(1f, 0.55f, 0.7f, 1f);

	private Player? _player;
	private Label? _label;
	private Control? _layers;
	private Control? _rotationLayers;
	private Material? _darkenedMat;
	private bool _isEnergyZero;

	private Node2D? _vfxBack;
	private Node2D? _vfxFront;

	private TextureRect? _layer3Rect;
	private readonly Godot.Collections.Array<Texture2D> _layer3Frames = [];
	private float _layer3AccumulatorSeconds;
	private int _layer3FrameIndex;
	private bool _initialized;

	public override void _EnterTree()
	{
	}

	public override void _Ready()
	{
		_player = GetPlayerFromBase();

		_label = GetNodeOrNull<Label>("Label");
		_layers = GetNodeOrNull<Control>("%Layers");
		_rotationLayers = GetNodeOrNull<Control>("%RotationLayers");
		_darkenedMat = GD.Load<Material>(DarkenedMatPath);
		_vfxBack = GetNodeOrNull<Node2D>("%EnergyVfxBack");
		_vfxFront = GetNodeOrNull<Node2D>("%EnergyVfxFront");
		_layer3Rect =
			GetNodeOrNull<TextureRect>("%Layer3") ??
			GetNodeOrNull<TextureRect>("Layers/RotationLayers/Layer3");

		LoadLayer3Frames();
		if (_layer3Rect != null && _layer3Frames.Count > 0)
		{
			_layer3Rect.Texture = _layer3Frames[0];
			_layer3Rect.Visible = true;
		}

		ApplyEnergyTint(EnergyTint);

		_initialized = true;

		if (_player != null)
		{
			CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
			_player.PlayerCombatState.EnergyChanged += OnEnergyChanged;
		}

		RefreshLabelSafe();
	}

	public override void _ExitTree()
	{
		if (_player != null)
		{
			CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
			_player.PlayerCombatState.EnergyChanged -= OnEnergyChanged;
		}
	}

	public override void _Process(double delta)
	{
		if (_rotationLayers != null)
		{
			var speed = _isEnergyZero ? 5f : 30f;
			for (var i = 0; i < _rotationLayers.GetChildCount(); i++)
			{
				if (_rotationLayers.GetChild(i) is Control c)
				{
					c.RotationDegrees += (float)delta * speed * (i + 1);
				}
			}
		}

		if (!_isEnergyZero)
		{
			if (_vfxBack != null)
			{
				_vfxBack.RotationDegrees += (float)delta * 25f;
			}

			if (_vfxFront != null)
			{
				_vfxFront.RotationDegrees -= (float)delta * 18f;
			}
		}

		if (_isEnergyZero || _layer3Rect == null || _layer3Frames.Count == 0)
		{
			return;
		}

		_layer3AccumulatorSeconds += (float)delta;
		var frameTime = 1f / Layer3Fps;
		while (_layer3AccumulatorSeconds >= frameTime)
		{
			_layer3AccumulatorSeconds -= frameTime;
			_layer3FrameIndex = (_layer3FrameIndex + 1) % _layer3Frames.Count;
			_layer3Rect.Texture = _layer3Frames[_layer3FrameIndex];
		}
	}

	private void OnCombatStateChanged(CombatState combatState)
	{
		RefreshLabelSafe();
	}

	private void OnEnergyChanged(int oldEnergy, int newEnergy)
	{
		if (!_initialized)
		{
			return;
		}

		RefreshLabelSafe();
	}

	private void RefreshLabelSafe()
	{
		if (!_initialized || _player == null || _label == null || _layers == null || _rotationLayers == null)
		{
			return;
		}

		var playerCombatState = _player.PlayerCombatState;
		_isEnergyZero = playerCombatState.Energy == 0;
		_label.Text = $"{playerCombatState.Energy}/{playerCombatState.MaxEnergy}";
		ApplyEnergyVisualState();
	}

	private static Player? GetPlayerFromBaseField(NEnergyCounter counter)
	{
		var field = typeof(NEnergyCounter).GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance);
		return field?.GetValue(counter) as Player;
	}

	private Player? GetPlayerFromBase()
	{
		try
		{
			return GetPlayerFromBaseField(this);
		}
		catch
		{
			return null;
		}
	}

	private void ApplyEnergyTint(Color tint)
	{
		var back = GetNodeOrNull<CanvasItem>("%EnergyVfxBack");
		if (back != null)
		{
			back.Modulate = tint;
		}

		var front = GetNodeOrNull<CanvasItem>("%EnergyVfxFront");
		if (front != null)
		{
			front.Modulate = tint;
		}
	}

	private void ApplyEnergyVisualState()
	{
		if (_layer3Rect != null)
		{
			_layer3Rect.Visible = !_isEnergyZero;
		}

		var back = GetNodeOrNull<CanvasItem>("%EnergyVfxBack");
		if (back != null)
		{
			back.Visible = !_isEnergyZero;
		}

		var front = GetNodeOrNull<CanvasItem>("%EnergyVfxFront");
		if (front != null)
		{
			front.Visible = !_isEnergyZero;
		}

		var backParticles = back?.GetNodeOrNull<CpuParticles2D>("BackParticles");
		if (backParticles != null)
		{
			backParticles.Emitting = !_isEnergyZero;
		}

		var frontParticles = front?.GetNodeOrNull<CpuParticles2D>("FrontParticles");
		if (frontParticles != null)
		{
			frontParticles.Emitting = !_isEnergyZero;
		}
	}

	private void LoadLayer3Frames()
	{
		_layer3Frames.Clear();

		for (int i = 0; i < Layer3FrameCount; i++)
		{
			var tex = GD.Load<Texture2D>($"res://TH_Yuyuko/Artworks/VFX/EnergySheet/bulletGa{i:000}.png");
			if (tex != null)
			{
				_layer3Frames.Add(tex);
			}
		}

		if (_layer3Frames.Count == 0)
		{
			return;
		}

		_layer3FrameIndex = 0;
		_layer3AccumulatorSeconds = 0f;
	}
}
