using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TH_Yuyuko.Scripts.Ui;

public sealed partial class NChooseAPotionSelection : Control, IOverlayScreen, IScreenContext
{
	private const float XSpacing = 200f;

	private readonly TaskCompletionSource<PotionModel?> _completionSource = new();

	private Control _potionRow = null!;
	private HScrollBar _scrollBar = null!;
	private List<Control> _holders = null!;
	private List<Vector2> _basePositions = null!;
	private float _maxShift;
	private bool _initialized;
	private Tween? _fadeTween;

	private IReadOnlyList<PotionModel> _potions = null!;

	public NetScreenType ScreenType => NetScreenType.Rewards;
	public bool UseSharedBackstop => true;

	public Control? DefaultFocusedControl
	{
		get
		{
			if (_holders == null || _holders.Count == 0)
			{
				return null;
			}
			return _holders[_holders.Count / 2];
		}
	}

	public static async Task<PotionModel?> ChooseOne(Player player, IReadOnlyList<PotionModel> potions)
	{
		if (potions.Count == 0)
		{
			return null;
		}

		NChooseAPotionSelection screen = new NChooseAPotionSelection
		{
			Name = "TH_Yuyuko_NChooseAPotionSelection",
			_potions = potions
		};

		NOverlayStack stack = NOverlayStack.Instance!;
		stack.Push(screen);

		PotionModel? selected;
		try
		{
			selected = await screen._completionSource.Task;
		}
		catch
		{
			selected = null;
		}

		if (screen.IsValid())
		{
			stack.Remove(screen);
		}
		return selected;
	}

	public override void _Ready()
	{
		ClipContents = false;

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float viewportWidth = viewportSize.X;
		float viewportHeight = viewportSize.Y;

		var title = new Label
		{
			Text = "选择药水",
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};
		title.Position = new Vector2(0, viewportHeight * 0.1f);
		title.Size = new Vector2(viewportWidth, 50);
		AddChild(title);

		_potionRow = new Control
		{
			Name = "PotionRow",
			Position = new Vector2(viewportWidth * 0.5f, viewportHeight * 0.45f)
		};
		AddChild(_potionRow);

		Vector2 offset = Vector2.Left * (_potions.Count - 1) * XSpacing * 0.5f;
		for (int i = 0; i < _potions.Count; i++)
		{
			PotionModel potion = _potions[i];
			var holder = new PotionChoiceHolder(potion);
			holder.Position = offset + Vector2.Right * XSpacing * i;
			_potionRow.AddChildSafely(holder);
			holder.Selected += OnPotionSelected;
		}

		_holders = _potionRow.GetChildren().OfType<Control>().ToList();
		_basePositions = _holders.Select(h => h.Position).ToList();

		float span = Math.Max(0f, (_holders.Count - 1) * XSpacing);
		float contentWidth = span + 400f;
		float visibleWidth = Math.Max(0f, viewportWidth - 400f);
		float overflow = Math.Max(0f, contentWidth - visibleWidth);

		_scrollBar = new HScrollBar
		{
			Name = "TH_Yuyuko_ChoosePotionScrollBar",
			FocusMode = FocusModeEnum.All,
			MouseFilter = MouseFilterEnum.Stop,
			ZIndex = 10000,
			MinValue = 0,
			MaxValue = overflow,
			Step = 5,
			Page = Math.Max(50, visibleWidth * 0.25f),
			Value = overflow * 0.5f,
			Size = new Vector2(viewportWidth * 0.6f, 24f),
			Position = new Vector2(viewportWidth * 0.2f, viewportHeight * 0.88f),
			Visible = overflow > 0f
		};
		_scrollBar.CustomMinimumSize = _scrollBar.Size;
		AddChild(_scrollBar);

		_maxShift = overflow * 0.5f;
		_initialized = true;
		_scrollBar.ValueChanged += _ => ApplyScroll();
		ApplyScroll();

		DefaultFocusedControl?.TryGrabFocus();
	}

	public override void _ExitTree()
	{
		if (!_completionSource.Task.IsCompleted)
		{
			_completionSource.TrySetCanceled();
		}
	}

	private void ApplyScroll()
	{
		if (!_initialized || !_scrollBar.Visible)
		{
			return;
		}

		float shift = _maxShift - (float)_scrollBar.Value;
		for (int i = 0; i < _holders.Count; i++)
		{
			Control holder = _holders[i];
			Vector2 basePos = _basePositions[i];
			holder.Position = new Vector2(basePos.X + shift, basePos.Y);
		}
	}

	private void OnPotionSelected(PotionModel potion)
	{
		if (!_completionSource.Task.IsCompleted)
		{
			_completionSource.TrySetResult(potion);
		}
	}

	public void AfterOverlayOpened()
	{
		Modulate = Colors.Transparent;
		_fadeTween?.Kill();
		_fadeTween = CreateTween();
		_fadeTween.TweenProperty(this, "modulate:a", 1f, 0.2);
	}

	public void AfterOverlayClosed()
	{
		_fadeTween?.Kill();
		this.QueueFreeSafely();
	}

	public void AfterOverlayShown()
	{
		Visible = true;
	}

	public void AfterOverlayHidden()
	{
		Visible = false;
	}

	private sealed partial class PotionChoiceHolder : NClickableControl
	{
		public event Action<PotionModel>? Selected;

		private readonly PotionModel _potion;
		private NPotion? _icon;

		public PotionChoiceHolder(PotionModel potion)
		{
			_potion = potion;
			FocusMode = FocusModeEnum.All;
			MouseFilter = MouseFilterEnum.Stop;
			Size = new Vector2(200, 200);
		}

		public override void _Ready()
		{
			ConnectSignals();
			_icon = NPotion.Create(_potion);
			if (_icon != null)
			{
				_icon.Scale = Vector2.One * 1.6f;
				_icon.Position = new Vector2(0, 0);
				this.AddChildSafely(_icon);
			}
		}

		protected override void OnRelease()
		{
			base.OnRelease();
			Selected?.Invoke(_potion);
			NHoverTipSet.Remove(this);
		}

		protected override void OnPress()
		{
			base.OnPress();
			NHoverTipSet.Remove(this);
		}

		protected override void OnFocus()
		{
			base.OnFocus();
			NHoverTipSet.CreateAndShow(this, _potion.HoverTips, HoverTipAlignment.Left);
		}

		protected override void OnUnfocus()
		{
			base.OnUnfocus();
			NHoverTipSet.Remove(this);
		}
	}
}
