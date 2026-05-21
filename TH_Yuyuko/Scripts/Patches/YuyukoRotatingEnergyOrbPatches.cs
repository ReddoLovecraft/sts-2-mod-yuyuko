using Godot;
using HarmonyLib;
using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using Patchoulib.Scrpits.Main;

namespace TH_Yuyuko.Scripts.Patches
{
	internal static class YuyukoEnergyOrbPatchUtils
	{
		internal const string YuyukoBigOrbPath = "res://TH_Yuyuko/Artworks/Character/card_orb.png";
		internal const string YuyukoCostOrbPath = "res://TH_Yuyuko/Artworks/Character/cost_orb.png";

		internal static bool IsYuyukoOrbTexture(Texture2D texture, out bool isCostOrb)
		{
			isCostOrb = false;
			if (texture == null)
			{
				return false;
			}

			string path = texture.ResourcePath ?? string.Empty;
			if (string.Equals(path, YuyukoBigOrbPath, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if (string.Equals(path, YuyukoCostOrbPath, StringComparison.OrdinalIgnoreCase))
			{
				isCostOrb = true;
				return true;
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(AssetSets), "set_RunSet")]
	public static class YuyukoPreloadRunSetPatch
	{
		static void Prefix(ref IReadOnlySet<string> value)
		{
			IEnumerable<string> items = value != null ? value : Array.Empty<string>();
			HashSet<string> set = value as HashSet<string> ?? new HashSet<string>(items);
			set.Add(YuyukoEnergyOrbPatchUtils.YuyukoBigOrbPath);
			set.Add(YuyukoEnergyOrbPatchUtils.YuyukoCostOrbPath);
			set.Add("res://images/packed/vfx/combat/spooky_hand.png");
			value = set;
		}
	}

	[HarmonyPatch(typeof(TextureRect), nameof(TextureRect.SetTexture))]
	public static class YuyukoRotatingOrbTextureRectPatch
	{
		static void Prefix(ref Texture2D texture)
		{
			if (!YuyukoTextureGuard.IsTextureUsable(texture))
			{
				texture = null;
			}
		}

		static void Postfix(TextureRect __instance, Texture2D texture)
		{
			if (__instance == null || texture == null)
			{
				return;
			}

			if (!YuyukoEnergyOrbPatchUtils.IsYuyukoOrbTexture(texture, out bool isCostOrb))
			{
				YuyukoOrbMaterialFactory.ClearIfOwned(__instance);
				return;
			}

			YuyukoOrbMaterialFactory.Apply(
				__instance,
				isCostOrb ? YuyukoOrbMaterialFactory.GetCostOrbMat() : YuyukoOrbMaterialFactory.GetBigOrbMat()
			);
		}
	}

	[HarmonyPatch(typeof(Sprite2D), nameof(Sprite2D.SetTexture))]
	public static class YuyukoRotatingOrbSprite2DPatch
	{
		static void Prefix(ref Texture2D texture)
		{
			if (!YuyukoTextureGuard.IsTextureUsable(texture))
			{
				texture = null;
			}
		}

		static void Postfix(Sprite2D __instance, Texture2D texture)
		{
			if (__instance == null || texture == null)
			{
				return;
			}

			if (!YuyukoEnergyOrbPatchUtils.IsYuyukoOrbTexture(texture, out bool isCostOrb))
			{
				YuyukoOrbMaterialFactory.ClearIfOwned(__instance);
				return;
			}

			YuyukoOrbMaterialFactory.Apply(
				__instance,
				isCostOrb ? YuyukoOrbMaterialFactory.GetCostOrbMat() : YuyukoOrbMaterialFactory.GetBigOrbMat()
			);
		}
	}

	[HarmonyPatch(typeof(AssetCache), nameof(AssetCache.UnloadMissedCacheAssets))]
	public static class YuyukoPreventDisposingModTexturesPatch
	{
		private const string _modResPrefix = "res://TH_Yuyuko/";

		static void Prefix(AssetCache __instance)
		{
			try
			{
				var field = AccessTools.Field(typeof(AssetCache), "_missedCacheAssets");
				if (field?.GetValue(__instance) is HashSet<string> set && set.Count > 0)
				{
					set.RemoveWhere(p => p != null && p.StartsWith(_modResPrefix, StringComparison.OrdinalIgnoreCase));
				}
			}
			catch
			{
			}
		}
	}

	internal static class YuyukoOrbMaterialFactory
	{
		private static ShaderMaterial? _bigOrbMat;
		private static ShaderMaterial? _costOrbMat;

		internal static void Apply(CanvasItem item, ShaderMaterial mat)
		{
			if (item.Material == mat)
			{
				return;
			}

			item.Material = mat;

			if (item is Control control)
			{
				control.UseParentMaterial = false;
			}
		}

		internal static void ClearIfOwned(CanvasItem item)
		{
			if (item.Material == null)
			{
				return;
			}

			if (ReferenceEquals(item.Material, _bigOrbMat) || ReferenceEquals(item.Material, _costOrbMat))
			{
				item.Material = null;

				if (item is Control control)
				{
					control.UseParentMaterial = false;
				}
			}
		}

		internal static ShaderMaterial GetBigOrbMat()
		{
			_bigOrbMat ??= CreateRotatingAdditiveOrbMaterial(speed: 1.0f);
			return _bigOrbMat;
		}

		internal static ShaderMaterial GetCostOrbMat()
		{
			_costOrbMat ??= CreateRotatingAdditiveOrbMaterial(speed: 1.6f);
			return _costOrbMat;
		}

		private static ShaderMaterial CreateRotatingAdditiveOrbMaterial(float speed)
		{
			var shader = new Shader
			{
				Code = @"
shader_type canvas_item;
render_mode blend_add;

uniform float speed = 1.0;
uniform vec2 center = vec2(0.5, 0.5);

void fragment() {
	vec2 uv = UV - center;
	float a = TIME * speed;
	mat2 r = mat2(vec2(cos(a), -sin(a)), vec2(sin(a), cos(a)));
	uv = r * uv + center;

	if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
		COLOR = vec4(0.0);
	} else {
		COLOR = texture(TEXTURE, uv);
	}
}
"
			};

			var mat = new ShaderMaterial
			{
				Shader = shader
			};
			mat.SetShaderParameter("speed", speed);
			return mat;
		}
	}

	[HarmonyPatch(typeof(Tools), nameof(Tools.GetStaticKeyword))]
	[HarmonyPatch(new Type[] { typeof(string) })]
	public static class YuyukoSafeStaticKeywordHoverTipPatch
	{
		static bool Prefix(string entry, ref HoverTip __result)
		{
			if (!IsYuyukoKeyword(entry))
			{
				return true;
			}

			string key = ToUpperSnake(entry);
			__result = new HoverTip(
				new LocString("static_hover_tips", $"{key}.title"),
				new LocString("static_hover_tips", $"{key}.description")
			);
			return false;
		}

		private static bool IsYuyukoKeyword(string entry)
		{
			return string.Equals(entry, "Spring", StringComparison.Ordinal)
				|| string.Equals(entry, "SummonButterfly", StringComparison.Ordinal)
				|| string.Equals(entry, "Butterfly", StringComparison.Ordinal);
		}

		private static string ToUpperSnake(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			var chars = new List<char>(value.Length + 8);
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				if (char.IsUpper(c) && i > 0 && value[i - 1] != '_')
				{
					chars.Add('_');
				}
				chars.Add(char.ToUpperInvariant(c));
			}
			return new string(chars.ToArray());
		}
	}

	internal static class YuyukoTextureGuard
	{
		internal static bool IsTextureUsable(Texture2D texture)
		{
			if (texture == null)
			{
				return false;
			}

			try
			{
				return GodotObject.IsInstanceValid(texture);
			}
			catch (ObjectDisposedException)
			{
				return false;
			}
			catch
			{
				return false;
			}
		}
	}
}
