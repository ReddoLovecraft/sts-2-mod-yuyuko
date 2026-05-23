using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
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
using System.Reflection;
using TH_Yuyuko.Scripts.Cards;
namespace TH_Yuyuko.Scripts.Main
{
    [ModInitializer("Init")]
    public class YuyukoInit
    {
    private const string ModSfxPrefix = "mod_sfx://";
    private static readonly Dictionary<string, float> GainOverrides = new()
		{
			["TH_Yuyuko/Artworks/SFX/characterselect.wav"] = 1.0f,
		};
    private const float DefaultGain = 0.45f;
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
        _harmony.PatchAll();
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
    static IEnumerable<MethodBase> TargetMethods()
		{
			return typeof(NAudioManager)
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
				.Where(m =>
				{
					if (m.Name != "PlayOneShot")
					{
						return false;
					}

					ParameterInfo[] ps = m.GetParameters();
					return ps.Length >= 1 && ps[0].ParameterType == typeof(string);
				});
		}

		static bool Prefix(MethodBase __originalMethod, object[] __args)
		{
			return HandlePlay(__originalMethod, __args);
		}

		public static bool HandlePlay(MethodBase __originalMethod, object[] __args)
		{
			if (__args.Length < 1 || __args[0] is not string path || !path.StartsWith(ModSfxPrefix))
			{
				return true;
			}

			float volume = 1f;
			ParameterInfo[] ps = __originalMethod.GetParameters();
			for (int i = 1; i < __args.Length && i < ps.Length; i++)
			{
				if (__args[i] is float f && ps[i].ParameterType == typeof(float) && ps[i].Name != null && ps[i].Name.Contains("volume", StringComparison.OrdinalIgnoreCase))
				{
					volume = f;
					break;
				}
			}
			if (volume == 1f)
			{
				for (int i = 1; i < __args.Length; i++)
				{
					if (__args[i] is float f)
					{
						volume = f;
					}
				}
			}

			try
			{
				PlayModSfx(path, volume);
			}
			catch (System.Exception e)
			{
				Log.Error($"Failed to play mod sfx: {path}. Error: {e.Message}");
			}

			return false;
		}

		private static void PlayModSfx(string path, float volume)
		{
			string localPath = path.Substring(ModSfxPrefix.Length);
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
			if (GainOverrides.TryGetValue(localPath, out float overrideGain))
			{
				gain *= overrideGain;
			}
			player.VolumeDb = Mathf.LinearToDb(Mathf.Max(0.0001f, volume * gain));

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


}
