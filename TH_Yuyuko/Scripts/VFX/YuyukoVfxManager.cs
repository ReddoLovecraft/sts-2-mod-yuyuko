using System;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace TH_Yuyuko.Scripts.VFX;

public static class YuyukoVfxManager
{
	private static readonly CanvasItemMaterial _additiveMat = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };

	public enum VfxTargetPositionType
	{
		TargetSide,
		Self,
		TargetCreature
	}

	public const string ModVfxPrefix = "mod_vfx://";
	public const string ModVfxBaseResPath = "res://TH_Yuyuko/Artworks/VFX/";

	public static string ToModVfxPath(string relativePath)
	{
		string p = (relativePath ?? string.Empty).Trim();
		if (p.StartsWith('/'))
		{
			p = p.Substring(1);
		}
		if (p.EndsWith(".tscn"))
		{
			p = p.Substring(0, p.Length - ".tscn".Length);
		}
		return ModVfxPrefix + p;
	}

	public static Node2D? CreateProjectileToTarget(string effectName, Creature from, Creature to, float speed = 1200f)
	{
		return CreateProjectileToTarget(effectName, from, to, Vector2.Zero, Vector2.Zero, speed);
	}

	public static Node2D? CreateProjectileToTarget(string effectName, Creature from, Creature to, Vector2 fromOffset, Vector2 toOffset, float speed = 1200f)
	{
		if (NCombatRoom.Instance == null)
		{
			return null;
		}

		var fromNode = NCombatRoom.Instance.GetCreatureNode(from);
		var toNode = NCombatRoom.Instance.GetCreatureNode(to);
		if (fromNode == null || toNode == null)
		{
			return null;
		}

		string scenePath = SceneHelper.GetScenePath(ToModVfxPath(effectName));
		PackedScene scene = PreloadManager.Cache.GetScene(scenePath);
		Node2D vfx = scene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);

		Vector2 start = fromNode.VfxSpawnPosition + fromOffset;
		Vector2 end = toNode.VfxSpawnPosition + toOffset;

		Node2D wrapper = new Node2D();
		wrapper.GlobalPosition = start;
		wrapper.AddChild(vfx);

		float dist = start.DistanceTo(end);
		float seconds = dist / Mathf.Max(0.001f, speed);
		seconds = Mathf.Max(0.05f, seconds);

		Tween tween = wrapper.CreateTween();
		tween.TweenProperty(wrapper, "global_position", end, seconds)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		tween.TweenCallback(Callable.From(wrapper.QueueFree));

		return wrapper;
	}

	public static Node2D? PlayOnCreatureCenter(Creature target, string relativePath, Vector2? startOffset = null, float speed = 1200f, float curveAmount = 0f)
	{
		Node? container = NCombatRoom.Instance?.CombatVfxContainer;
		if (container == null)
		{
			return null;
		}

		var creatureNode = NCombatRoom.Instance?.GetCreatureNode(target);
		if (creatureNode == null)
		{
			return null;
		}

		return PlayInternal(container, creatureNode.VfxSpawnPosition, relativePath, startOffset, speed, curveAmount);
	}

	public static Node2D? PlayAtPosition(Node container, Vector2 targetGlobalPosition, string relativePath, Vector2? startOffset = null, float speed = 1200f, float curveAmount = 0f)
	{
		return PlayInternal(container, targetGlobalPosition, relativePath, startOffset, speed, curveAmount);
	}

	public static Node2D? PlaySimple(string effectName, Creature? vfxTarget, VfxTargetPositionType positionType, float durationSeconds = 0.6f)
	{
		string path = ToModVfxPath(effectName);

		Node? container = NCombatRoom.Instance?.CombatVfxContainer;
		if (container == null)
		{
			return null;
		}

		if (vfxTarget == null)
		{
			CombatState? combatState = CombatManager.Instance.DebugOnlyGetState();
			if (combatState == null)
			{
				return null;
			}

			Vector2? sideCenter = VfxCmd.GetSideCenter(CombatSide.Enemy, combatState);
			if (!sideCenter.HasValue)
			{
				return null;
			}

			var fallback = PlayAtPosition(container, sideCenter.Value, path);
			if (fallback != null)
			{
				TrySetDurationSeconds(fallback, durationSeconds);
			}
			return fallback;
		}

		Node2D? node;
		switch (positionType)
		{
			case VfxTargetPositionType.TargetSide:
			{
				CombatState? combatState = vfxTarget.CombatState ?? vfxTarget.Player?.Creature.CombatState;
				if (combatState == null)
				{
					return null;
				}

				Vector2? sideCenter = VfxCmd.GetSideCenter(vfxTarget.Side, combatState);
				if (!sideCenter.HasValue)
				{
					return null;
				}

				node = PlayAtPosition(container, sideCenter.Value, path);
				break;
			}
			case VfxTargetPositionType.Self:
			case VfxTargetPositionType.TargetCreature:
			default:
				node = PlayOnCreatureCenter(vfxTarget, path);
				break;
		}

		if (node != null)
		{
			TrySetDurationSeconds(node, durationSeconds);
		}
		return node;
	}

	public static async Task PlayDoomKillVfxOnCreature(Creature target)
	{
		if (NCombatRoom.Instance == null)
		{
			return;
		}

		Node? container = NCombatRoom.Instance.CombatVfxContainer;
		if (container == null)
		{
			return;
		}

		NDoomOverlayVfx? overlay = NDoomOverlayVfx.GetOrCreate();
		if (overlay != null && !overlay.IsInsideTree())
		{
			container.AddChildSafely(overlay);
		}

		var nCreature = NCombatRoom.Instance.GetCreatureNode(target);
		if (nCreature == null)
		{
			return;
		}

		NDoomVfx? doomVfx = NDoomVfx.Create(nCreature.Visuals, nCreature.Hitbox.GlobalPosition, nCreature.Hitbox.Size, false);
		if (doomVfx != null)
		{
			container.AddChildSafely(doomVfx);
		}

		await Task.CompletedTask;
	}

	public static void PlayPhotoFlashOverlay()
	{
		if (NCombatRoom.Instance == null)
		{
			return;
		}

		Node? parent = NCombatRoom.Instance.CombatVfxContainer;
		if (parent == null)
		{
			return;
		}

		Viewport viewport = parent.GetViewport();
		Vector2 viewSize = viewport.GetVisibleRect().Size;

		var layer = new CanvasLayer
		{
			Name = "YuyukoPhotoFlashOverlay",
			Layer = 200
		};

		var root = new Control
		{
			AnchorLeft = 0f,
			AnchorTop = 0f,
			AnchorRight = 1f,
			AnchorBottom = 1f,
			OffsetLeft = 0f,
			OffsetTop = 0f,
			OffsetRight = 0f,
			OffsetBottom = 0f,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Modulate = new Color(1f, 1f, 1f, 0f)
		};
		layer.AddChildSafely(root);

		var desaturateTint = new ColorRect
		{
			AnchorLeft = 0f,
			AnchorTop = 0f,
			AnchorRight = 1f,
			AnchorBottom = 1f,
			OffsetLeft = 0f,
			OffsetTop = 0f,
			OffsetRight = 0f,
			OffsetBottom = 0f,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Color = new Color(0.70f, 0.70f, 0.70f, 0.45f)
		};
		root.AddChildSafely(desaturateTint);

		float margin = Mathf.Round(Mathf.Min(viewSize.X, viewSize.Y) * 0.06f);
		float borderWidth = Mathf.Round(Mathf.Min(viewSize.X, viewSize.Y) * 0.012f);

		var frame = new Panel
		{
			AnchorLeft = 0f,
			AnchorTop = 0f,
			AnchorRight = 1f,
			AnchorBottom = 1f,
			OffsetLeft = margin,
			OffsetTop = margin,
			OffsetRight = -margin,
			OffsetBottom = -margin,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};

		var style = new StyleBoxFlat
		{
			BgColor = new Color(1f, 1f, 1f, 0f),
			BorderColor = new Color(0.85f, 0.85f, 0.85f, 0.75f),
			BorderWidthLeft = (int)borderWidth,
			BorderWidthTop = (int)borderWidth,
			BorderWidthRight = (int)borderWidth,
			BorderWidthBottom = (int)borderWidth
		};
		frame.AddThemeStyleboxOverride("panel", style);
		root.AddChildSafely(frame);

		parent.AddChildSafely(layer);

		Tween tween = root.CreateTween();
		tween.TweenProperty(root, "modulate:a", 1f, 0.08).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		tween.TweenInterval(0.05);
		tween.TweenProperty(root, "modulate:a", 0f, 0.24).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
		tween.TweenCallback(Callable.From(layer.QueueFreeSafely));
	}

	

	private static Node2D? PlayInternal(Node container, Vector2 targetGlobalPosition, string relativePath, Vector2? startOffset, float speed, float curveAmount)
	{
		string trimmed = (relativePath ?? string.Empty).Trim();
		if (trimmed.StartsWith(ModVfxPrefix))
		{
			trimmed = trimmed.Substring(ModVfxPrefix.Length);
		}
		if (trimmed.StartsWith('/'))
		{
			trimmed = trimmed.Substring(1);
		}
		if (trimmed.EndsWith(".tscn"))
		{
			trimmed = trimmed.Substring(0, trimmed.Length - ".tscn".Length);
		}

		string scenePath = ModVfxBaseResPath + trimmed + ".tscn";
		var scene = ResourceLoader.Load<PackedScene>(scenePath);
		if (scene == null)
		{
			return null;
		}

		Node2D node = scene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
		node.GlobalPosition = targetGlobalPosition;

		if (node is YuyukoFlyToTarget fly)
		{
			if (startOffset.HasValue)
			{
				fly.StartOffset = startOffset.Value;
			}
			fly.Speed = speed;
			fly.CurveAmount = curveAmount;
		}
		else
		{
			if (startOffset.HasValue)
			{
				TrySetPropertyIfExists(node, "start_offset", startOffset.Value);
			}
			TrySetPropertyIfExists(node, "speed", speed);
			TrySetPropertyIfExists(node, "curve_amount", curveAmount);
		}
		container.AddChild(node);
		return node;
	}

	private static bool TrySetDurationSeconds(Node node, float durationSeconds)
	{
		if (node is YuyukoFlyToTarget fly)
		{
			fly.AutoFreeSeconds = durationSeconds;
			return true;
		}

		if (node is Godot.Timer timer)
		{
			timer.OneShot = true;
			timer.WaitTime = Mathf.Max(0.01f, durationSeconds);
			if (timer.IsInsideTree())
			{
				timer.Stop();
				timer.Start();
			}
			return true;
		}

		if (TrySetPropertyIfExists(node, "duration_seconds", durationSeconds))
		{
			return true;
		}

		foreach (Node child in node.GetChildren())
		{
			if (TrySetDurationSeconds(child, durationSeconds))
			{
				return true;
			}
		}

		return false;
	}

	private static bool TrySetPropertyIfExists(Node node, string propertyName, Variant value)
	{
		foreach (Godot.Collections.Dictionary dict in node.GetPropertyList())
		{
			if (!dict.TryGetValue("name", out Variant nameVar))
			{
				continue;
			}

			if (nameVar.VariantType == Variant.Type.String && ((string)nameVar) == propertyName)
			{
				node.Set(propertyName, value);
				return true;
			}
		}
		return false;
	}
}
