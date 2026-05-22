using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TH_Yuyuko.Scripts.Patches;

[HarmonyPatch(typeof(NChooseARelicSelection))]
[HarmonyPatch(nameof(NChooseARelicSelection._Ready))]
public static class ChooseARelicScrollPatch
{
	private sealed class ScrollState
	{
		public HScrollBar ScrollBar = null!;
		public List<Control> Holders = null!;
		public List<Vector2> BasePositions = null!;
		public float MaxShift;
		public float Range;
		public bool Initialized;
	}

	private static readonly ConditionalWeakTable<NChooseARelicSelection, ScrollState> ScrollStates = new();

	[HarmonyPostfix]
	private static void Postfix(NChooseARelicSelection __instance)
	{
		if (__instance.GetNodeOrNull<HScrollBar>("TH_Yuyuko_ChooseRelicScrollBar") != null)
		{
			return;
		}

		Control? relicRow = __instance.GetNodeOrNull<Control>("RelicRow");
		if (relicRow == null)
		{
			return;
		}

		relicRow.ClipContents = false;
		__instance.ClipContents = false;

		List<Control> holders = relicRow.GetChildren().OfType<Control>().ToList();
		int relicCount = holders.Count;
		if (relicCount <= 0)
		{
			return;
		}

		Vector2 viewportSize = __instance.GetViewportRect().Size;
		float viewportWidth = viewportSize.X;
		float viewportHeight = viewportSize.Y;

		float span = Math.Max(0f, (relicCount - 1) * 200f);
		float availableWidth = Math.Max(0f, viewportWidth - 400f);
		float estimatedContentWidth = span + 400f;
		float estimatedOverflow = Math.Max(0f, estimatedContentWidth - availableWidth);
		if (estimatedOverflow <= 0f)
		{
			return;
		}

		var scroll = new HScrollBar
		{
			Name = "TH_Yuyuko_ChooseRelicScrollBar",
			FocusMode = Control.FocusModeEnum.All,
			MouseFilter = Control.MouseFilterEnum.Stop,
			ZIndex = 10000,
			MinValue = 0,
			MaxValue = estimatedOverflow,
			Step = 5,
			Page = Math.Max(50, availableWidth * 0.25f),
			Value = estimatedOverflow * 0.5f,
			Size = new Vector2(viewportWidth * 0.6f, 24f),
			Position = new Vector2(viewportWidth * 0.2f, viewportHeight * 0.88f)
		};
		scroll.CustomMinimumSize = scroll.Size;

		__instance.AddChild(scroll);

		ScrollState state = ScrollStates.GetOrCreateValue(__instance);
		state.ScrollBar = scroll;
		state.Holders = holders;

		void ApplyFromScroll()
		{
			if (!state.Initialized)
			{
				return;
			}
			float shift = state.MaxShift - (float)state.ScrollBar.Value;
			for (int i = 0; i < state.Holders.Count; i++)
			{
				Control holder = state.Holders[i];
				if (!GodotObject.IsInstanceValid(holder))
				{
					continue;
				}
				Vector2 basePos = state.BasePositions[i];
				holder.Position = new Vector2(basePos.X + shift, basePos.Y);
			}
		}

		scroll.ValueChanged += _ => ApplyFromScroll();

		SceneTreeTimer timer = __instance.GetTree().CreateTimer(0.6);
		timer.Timeout += () =>
		{
			if (!GodotObject.IsInstanceValid(__instance) || !GodotObject.IsInstanceValid(relicRow))
			{
				return;
			}

			state.Holders = relicRow.GetChildren().OfType<Control>().ToList();
			state.BasePositions = state.Holders.Select(h => h.Position).ToList();

			float halfWidth = 0f;
			foreach (Control h in state.Holders)
			{
				halfWidth = Math.Max(halfWidth, h.Size.X * h.Scale.X * 0.5f);
			}
			halfWidth = Math.Max(halfWidth, 160f);

			float contentWidth = span + halfWidth * 2f;
			float visibleWidth = Math.Max(0f, viewportWidth - 400f);
			float overflow = Math.Max(0f, contentWidth - visibleWidth);
			float extraRight = halfWidth;
			float extraLeft = 0f;

			state.MaxShift = overflow * 0.5f + extraLeft;
			state.Range = overflow + extraLeft + extraRight;

			state.ScrollBar.MinValue = 0;
			state.ScrollBar.MaxValue = state.Range;
			state.ScrollBar.Value = state.Range * 0.5f;

			state.Initialized = true;
			ApplyFromScroll();
		};
	}
}

