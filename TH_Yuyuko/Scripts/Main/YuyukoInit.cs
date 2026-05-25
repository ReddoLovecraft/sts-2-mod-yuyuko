using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using TH_Yuyuko.Scripts.Cards;
namespace TH_Yuyuko.Scripts.Main
{
    [ModInitializer("Init")]
    public class YuyukoInit
    {
    internal const string ModSfxPrefix = "mod_sfx://";
    private static readonly Dictionary<string, float> GainOverrides = new(StringComparer.OrdinalIgnoreCase)
		{
			["TH_Yuyuko/Artworks/SFX/characterselect.wav"] = 0.7f,
			["TH_Yuyuko/Artworks/SFX/summon.wav"] = 0.7f,
			["TH_Yuyuko/Artworks/SFX/cast.wav"] = 0.9f,
		};
    private const float DefaultGain = 0.45f;
	private static readonly HashSet<string> _loggedSfx = new(StringComparer.OrdinalIgnoreCase);
        public static string ToModSfxPath(string localPath)
        {
            return ModSfxPrefix + localPath;
        }
    private static Harmony? _harmony;
    public static void Init()
    {
         TryRegisterGodotScriptAssembly();
		ModConfigRegistry.Register("TH_Yuyuko", new YuyukoModConfig());
        _harmony = new Harmony("TH_Yuyuko");
        _harmony.PatchAll(typeof(YuyukoInit).Assembly);
        Log.Debug("Yuyuko mod has been loaded successfully");
    }
    private static void TryRegisterGodotScriptAssembly()
    {
        try
        {
            Assembly modAssembly = typeof(YuyukoInit).Assembly;
            Type? scriptManagerBridgeType = Type.GetType("Godot.Bridge.ScriptManagerBridge, GodotSharp");

            if (scriptManagerBridgeType == null)
            {
                return;
            }

            MethodInfo? lookupMethod = scriptManagerBridgeType.GetMethod(
                "LookupScriptsInAssembly",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                binder: null,
                types: [typeof(Assembly)],
                modifiers: null
            );

            lookupMethod ??= scriptManagerBridgeType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(m =>
                {
                    ParameterInfo[] ps = m.GetParameters();
                    return ps.Length == 1
                        && ps[0].ParameterType == typeof(Assembly)
                        && (m.Name.Contains("Lookup", StringComparison.OrdinalIgnoreCase)
                            || m.Name.Contains("Load", StringComparison.OrdinalIgnoreCase)
                            || m.Name.Contains("Register", StringComparison.OrdinalIgnoreCase));
                });

            lookupMethod?.Invoke(null, [modAssembly]);
        }
        catch (Exception e)
        {
            Log.Error($"Failed to register Godot scripts for TH_Yuyuko: {e}");
        }
    }

	private static bool TryGetOverrideGain(string localPath, out float overrideGain)
	{
		if (GainOverrides.TryGetValue(localPath, out overrideGain))
		{
			return true;
		}

		string fileName = Path.GetFileName(localPath);
		if (!string.IsNullOrEmpty(fileName) && GainOverrides.TryGetValue(fileName, out overrideGain))
		{
			return true;
		}

		overrideGain = default;
		return false;
	}

		internal static void PlayModSfx(string path, float volume)
		{
			string localPath = path.Substring(ModSfxPrefix.Length);
			localPath = localPath.Replace('\\', '/').TrimStart('/');
			if (localPath.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
			{
				localPath = localPath.Substring("res://".Length).TrimStart('/');
			}

			string resPath = "res://" + localPath;
			AudioStream? stream = ResourceLoader.Load<AudioStream>(resPath);
			if (stream == null)
			{
				return;
			}

			var player = new AudioStreamPlayer();
			player.Stream = stream;
			player.Bus = FindSfxBusName();

			float gain = DefaultGain;
			if (TryGetOverrideGain(localPath, out float overrideGain))
			{
				gain *= overrideGain;
			}
			float linearVolume = volume * gain;
			if (linearVolume <= 0f || float.IsNaN(linearVolume) || float.IsInfinity(linearVolume))
			{
				player.QueueFree();
				return;
			}
			player.VolumeDb = Mathf.LinearToDb(linearVolume);
			if (_loggedSfx.Add(localPath))
			{
				Log.Debug($"mod_sfx '{localPath}' vol={volume} gain={gain} linear={linearVolume} db={player.VolumeDb} bus='{player.Bus}'");
			}

			if (NGame.Instance != null)
			{
				NGame.Instance.AddChild(player);
			}
			else
			{
				Log.Error($"TH_Yuyuko mod_sfx can't play because NGame.Instance is null. Path: {path}");
				player.QueueFree();
				return;
			}

			player.Play();
			player.Connect("finished", Callable.From(player.QueueFree));
		}

		private static string FindSfxBusName()
		{
			int count = AudioServer.BusCount;
			for (int i = 0; i < count; i++)
			{
				string bus = AudioServer.GetBusName(i);
				if (string.Equals(bus, "SFX", StringComparison.OrdinalIgnoreCase))
				{
					return bus;
				}
			}

			for (int i = 0; i < count; i++)
			{
				string bus = AudioServer.GetBusName(i);
				string lower = bus.ToLowerInvariant();
				if (lower.Contains("sfx") || lower.Contains("soundeffect") || lower.Contains("sound_effect") || lower == "se")
				{
					return bus;
				}
			}

			return "Master";
		}
    
    }

	[HarmonyPatch(typeof(NAudioManager))]
	internal static class YuyukoModSfxPatches
	{
		[HarmonyPrefix]
		[HarmonyPriority(Priority.First)]
		[HarmonyPatch(nameof(NAudioManager.PlayOneShot), new Type[] { typeof(string), typeof(float) })]
		private static bool PlayOneShot_Simple_Prefix(string path, float volume)
		{
			if (path == null || !path.StartsWith(YuyukoInit.ModSfxPrefix, StringComparison.Ordinal))
			{
				return true;
			}

			try
			{
				YuyukoInit.PlayModSfx(path, volume);
			}
			catch (Exception e)
			{
				Log.Error($"Failed to play mod sfx: {path}. Error: {e.Message}");
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPriority(Priority.First)]
		[HarmonyPatch(nameof(NAudioManager.PlayOneShot), new Type[] { typeof(string), typeof(Dictionary<string, float>), typeof(float) })]
		private static bool PlayOneShot_WithParams_Prefix(string path, Dictionary<string, float> parameters, float volume)
		{
			if (path == null || !path.StartsWith(YuyukoInit.ModSfxPrefix, StringComparison.Ordinal))
			{
				return true;
			}

			try
			{
				YuyukoInit.PlayModSfx(path, volume);
			}
			catch (Exception e)
			{
				Log.Error($"Failed to play mod sfx: {path}. Error: {e.Message}");
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(SfxCmd))]
	internal static class YuyukoModSfxSfxCmdPatches
	{
		[HarmonyPrefix]
		[HarmonyPriority(Priority.First)]
		[HarmonyPatch(nameof(SfxCmd.Play), new Type[] { typeof(string), typeof(float) })]
		private static bool Play_Prefix(string sfx, float volume)
		{
			if (sfx == null || !sfx.StartsWith(YuyukoInit.ModSfxPrefix, StringComparison.Ordinal))
			{
				return true;
			}
			if (NonInteractiveMode.IsActive || CombatManager.Instance.IsEnding)
			{
				return false;
			}

			try
			{
				YuyukoInit.PlayModSfx(sfx, volume);
			}
			catch (Exception e)
			{
				Log.Error($"Failed to play mod sfx: {sfx}. Error: {e.Message}");
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPriority(Priority.First)]
		[HarmonyPatch(nameof(SfxCmd.Play), new Type[] { typeof(string), typeof(string), typeof(float), typeof(float) })]
		private static bool Play_WithParam_Prefix(string sfx, string param, float val, float volume)
		{
			if (sfx == null || !sfx.StartsWith(YuyukoInit.ModSfxPrefix, StringComparison.Ordinal))
			{
				return true;
			}
			if (NonInteractiveMode.IsActive || CombatManager.Instance.IsEnding)
			{
				return false;
			}

			try
			{
				YuyukoInit.PlayModSfx(sfx, volume);
			}
			catch (Exception e)
			{
				Log.Error($"Failed to play mod sfx: {sfx}. Error: {e.Message}");
			}

			return false;
		}
	}


}
