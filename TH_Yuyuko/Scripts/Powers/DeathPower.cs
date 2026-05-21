using BaseLib.Abstracts;
using BaseLib.Extensions;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;


namespace TH_Yuyuko.Scrpits.Powers
{
    public sealed class DeathPower :HealthBarOverlayPowerModel
    {
		private const float _doomOverlayFadeInSeconds = 0.18f;
		private const float _doomOverlayHoldSeconds = 0.22f;
		private const float _doomOverlayFadeOutSeconds = 0.18f;
		private static readonly Color _deathLightTint = new Color(0.18f, 0.04f, 0.26f, 0.55f);
		private static readonly Color _deathMoteTint = new Color(0.92f, 0.35f, 1.0f, 1f);
		private static readonly CanvasItemMaterial _additiveMat = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };

		private bool _subscribedToOwnerPowerIncreased;
		private Node2D? _persistentApplyVfxRoot;

        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
        public override string? CustomPackedIconPath => "res://TH_Yuyuko/Artworks/Powers/DP32.png";
        public override string? CustomBigIconPath => "res://TH_Yuyuko/Artworks/Powers/DP64.png";
        //protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeathPower>()];
        public DeathPower() { }

		public override int GetHealthBarOverlayValue(Creature owner)
		{
			return GetEffectiveMaxHpLoss(owner);
		}

		public override bool IsOverlayFromEnd()
		{
			return true;
		}

		public override bool IsOverlayLethal(Creature owner)
		{
			return GetEffectiveMaxHpLoss(owner) >= owner.MaxHp;
		}

		public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			await base.AfterApplied(applier, cardSource);
			EnsureSubscribed();
			EnsurePersistentApplyVfx();
			TryPlayApplyVfx(base.Owner);
		}

		public override async Task AfterRemoved(Creature oldOwner)
		{
			if (oldOwner.CombatState != null && oldOwner.Side == CombatSide.Enemy && Amount > 0)
			{
				foreach (Player player in oldOwner.CombatState.Players)
				{
					foreach (NetherworldMysteryPower mystery in player.Creature.GetPowerInstances<NetherworldMysteryPower>())
					{
						await mystery.TriggerGold(Amount,Owner);
					}
				}
			}

			if (_subscribedToOwnerPowerIncreased)
			{
				oldOwner.PowerIncreased -= OnOwnerPowerIncreased;
				_subscribedToOwnerPowerIncreased = false;
			}
			if (_persistentApplyVfxRoot != null)
			{
				_persistentApplyVfxRoot.QueueFreeSafely();
				_persistentApplyVfxRoot = null;
			}
			await base.AfterRemoved(oldOwner);
		}

      public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side != Owner.Side)
            {
                return;
            }
		TryPlayLoseMaxHpVfx(base.Owner);
		decimal effectiveLoss = (decimal)Amount * (1m + (decimal)GetNoReturnPatternAmount(combatState));
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(),Owner,effectiveLoss,false);

		int springAmount = Applier?.GetPower<SpringPower>()?.Amount ?? 0;
		if (springAmount >= 100)
		{
			await PowerCmd.ModifyAmount(this, Amount * 2m / 3m, Owner, null, silent: true);
		}
		else if (springAmount < 75)
		{
			await PowerCmd.ModifyAmount(this, -Amount * 2m / 3m, Owner, null, silent: true);
		}
    }

		private static int GetNoReturnPatternAmount(CombatState? combatState)
		{
			if (combatState == null)
			{
				return 0;
			}

			int total = 0;
			foreach (Player p in combatState.Players)
			{
				total += p.Creature.GetPowerAmount<NoReturnPatternPower>();
			}
			return total;
		}

		private int GetEffectiveMaxHpLoss(Creature owner)
		{
			int amount = Amount;
			if (amount <= 0)
			{
				return 0;
			}

			int extra = GetNoReturnPatternAmount(owner.CombatState);
			long effective = (long)amount * (1L + (long)extra);
			if (effective > int.MaxValue)
			{
				return int.MaxValue;
			}
			return (int)effective;
		}

       public override Color GetHealthBarOverlayColor()
        {
            return new Color(0.16f, 0.03f, 0.22f, 0.95f);
        }

		private static void TryPlayApplyVfx(Creature creature)
		{
			if (TestMode.IsOn || NCombatRoom.Instance == null || !CombatManager.Instance.IsInProgress || creature.IsDead)
			{
				return;
			}
		}

		private static void TryPlayLoseMaxHpVfx(Creature creature)
		{
			if (TestMode.IsOn || NCombatRoom.Instance == null || !CombatManager.Instance.IsInProgress || creature.IsDead)
			{
				return;
			}

			NCreature nCreature = NCombatRoom.Instance.GetCreatureNode(creature);
			if (nCreature == null)
			{
				return;
			}
			PlayBreakAttackFrameSequence(nCreature);
		}

		private void EnsureSubscribed()
		{
			if (_subscribedToOwnerPowerIncreased)
			{
				return;
			}

			base.Owner.PowerIncreased += OnOwnerPowerIncreased;
			_subscribedToOwnerPowerIncreased = true;
		}

		private void OnOwnerPowerIncreased(PowerModel power, int change, bool silent)
		{
			if (silent || change <= 0 || power != this)
			{
				return;
			}

			EnsurePersistentApplyVfx();
			TryPlayApplyVfx(base.Owner);
		}

		private void EnsurePersistentApplyVfx()
		{
			if (TestMode.IsOn || NCombatRoom.Instance == null || !CombatManager.Instance.IsInProgress || base.Owner.IsDead)
			{
				return;
			}

			NCreature nCreature = NCombatRoom.Instance.GetCreatureNode(base.Owner);
			if (nCreature == null)
			{
				return;
			}

			if (_persistentApplyVfxRoot != null && GodotObject.IsInstanceValid(_persistentApplyVfxRoot))
			{
				return;
			}

			Texture2D lightTexture = ResourceLoader.Load<Texture2D>("res://TH_Yuyuko/Artworks/VFX/touhoueffect/healLightAa000.png", null, ResourceLoader.CacheMode.Reuse);
			Texture2D moteTexture = ResourceLoader.Load<Texture2D>("res://TH_Yuyuko/Artworks/VFX/touhoueffect/healLightAb000.png", null, ResourceLoader.CacheMode.Reuse);
			if (lightTexture == null || moteTexture == null)
			{
				return;
			}

			Node2D root = new Node2D
			{
				Name = "DeathPowerPersistentApplyVfx",
				Modulate = Colors.White,
				ZAsRelative = false,
				ZIndex = 80
			};
			_persistentApplyVfxRoot = root;
			nCreature.Visuals.VfxSpawnPosition.AddChildSafely(root);

			float lightTexW = lightTexture.GetWidth();
			float lightTexH = lightTexture.GetHeight();
			float scaleToCoverWidth = (nCreature.Hitbox.Size.X * 1.06f) / Mathf.Max(1f, lightTexW);
			float scaleToCoverHeight = (nCreature.Hitbox.Size.Y * 1.06f) / Mathf.Max(1f, lightTexH);
			float lightScale = Mathf.Max(scaleToCoverWidth, scaleToCoverHeight);
			float scaledLightHeight = lightTexH * lightScale;
			float desiredBottomY = nCreature.Hitbox.Size.Y * 0.60f;
			float lightPosY = desiredBottomY - scaledLightHeight * 0.5f;
			lightPosY = Mathf.Clamp(lightPosY, -nCreature.Hitbox.Size.Y * 0.85f, -nCreature.Hitbox.Size.Y * 0.05f);

			Sprite2D spotlight = new Sprite2D
			{
				Texture = lightTexture,
				Centered = true,
				Material = _additiveMat,
				Modulate = _deathLightTint,
				Position = new Vector2(0f, lightPosY),
				Scale = Vector2.One * lightScale,
				ZAsRelative = false,
				ZIndex = 0
			};
			root.AddChildSafely(spotlight);

			Tween pulseTween = spotlight.CreateTween().SetLoops();
			pulseTween.TweenProperty(spotlight, "modulate:a", _deathLightTint.A * 0.75f, 0.9).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
			pulseTween.TweenProperty(spotlight, "modulate:a", _deathLightTint.A, 0.9).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);

			SpawnHealMote(root, moteTexture, nCreature.Hitbox.Size);
			Tween spawnTween = root.CreateTween().SetLoops();
			spawnTween.TweenInterval(Rng.Chaotic.NextFloat(0.10f, 0.18f));
			spawnTween.TweenCallback(Callable.From(() => SpawnHealMote(root, moteTexture, nCreature.Hitbox.Size)));
		}

		private static void SpawnHealMote(Node2D parent, Texture2D moteTexture, Vector2 hitboxSize)
		{
			if (!GodotObject.IsInstanceValid(parent))
			{
				return;
			}

			float radiusX = hitboxSize.X * 0.65f;
			float radiusY = hitboxSize.Y * 0.70f;
			Vector2 offset = new Vector2(Rng.Chaotic.NextFloat(-radiusX, radiusX), Rng.Chaotic.NextFloat(-radiusY, radiusY));
			float baseScale = Mathf.Clamp(hitboxSize.Y / 320f, 0.75f, 1.45f);
			float scale = Rng.Chaotic.NextFloat(0.28f, 0.48f) * baseScale;

			Sprite2D mote = new Sprite2D
			{
				Texture = moteTexture,
				Centered = true,
				Material = _additiveMat,
				Position = offset,
				Scale = Vector2.One * scale,
				Rotation = Rng.Chaotic.NextFloat(-1.2f, 1.2f),
				Modulate = new Color(_deathMoteTint.R, _deathMoteTint.G, _deathMoteTint.B, 0f),
				ZAsRelative = false,
				ZIndex = 10
			};
			parent.AddChildSafely(mote);

			Vector2 drift = new Vector2(Rng.Chaotic.NextFloat(-10f, 10f), Rng.Chaotic.NextFloat(-30f, -10f));
			Tween tween = mote.CreateTween();
			tween.TweenProperty(mote, "modulate:a", 0.9f, 0.12).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
			tween.TweenProperty(mote, "position", mote.Position + drift, 0.55).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
			tween.TweenProperty(mote, "modulate:a", 0f, 0.22);
			tween.TweenCallback(Callable.From(mote.QueueFreeSafely));
		}

		private static void PlayBreakAttackFrameSequence(NCreature nCreature)
		{
			if (TestMode.IsOn || NCombatRoom.Instance == null || !GodotObject.IsInstanceValid(nCreature))
			{
				return;
			}

			Node2D attachPoint = nCreature.Visuals.VfxSpawnPosition;
			Node2D root = new Node2D { Name = "DeathPowerLoseMaxHpBreakAttackVfx" };
			attachPoint.AddChildSafely(root);

			Sprite2D sprite = new Sprite2D
			{
				Centered = true,
				Material = _additiveMat,
				Modulate = Colors.White,
				Scale = Vector2.One * Mathf.Clamp(nCreature.Hitbox.Size.Y / 320f, 0.7f, 1.4f)
			};
			root.AddChildSafely(sprite);

			TaskHelper.RunSafely(PlayFrames(sprite, root));

			static async Task PlayFrames(Sprite2D sprite, Node2D root)
			{
				const int frameCount = 15;
				const float frameSeconds = 0.03f;

				for (int i = 0; i < frameCount; i++)
				{
					if (!GodotObject.IsInstanceValid(sprite))
					{
						return;
					}

					string framePath = $"res://TH_Yuyuko/Artworks/VFX/touhoueffect/breakAttack{i:000}.png";
					Texture2D tex = ResourceLoader.Load<Texture2D>(framePath, null, ResourceLoader.CacheMode.Reuse);
					if (tex != null)
					{
						sprite.Texture = tex;
					}
					await Cmd.Wait(frameSeconds);
				}

				if (GodotObject.IsInstanceValid(root))
				{
					Tween tween = root.CreateTween();
					tween.TweenProperty(root, "modulate:a", 0f, 0.18);
					tween.TweenCallback(Callable.From(root.QueueFreeSafely));
				}
			}
		}

		private static void TryPlayShortDoomOverlayVfx()
		{
			if (TestMode.IsOn || NCombatRoom.Instance == null)
			{
				return;
			}

			NDoomOverlayVfx overlayVfx = NDoomOverlayVfx.GetOrCreate();
			if (overlayVfx == null)
			{
				return;
			}

			if (!overlayVfx.IsInsideTree())
			{
				NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(overlayVfx);
			}

			RetuneDoomOverlayTween(overlayVfx);
		}

		private static void RetuneDoomOverlayTween(NDoomOverlayVfx overlayVfx)
		{
			Type type = typeof(NDoomOverlayVfx);
			FieldInfo tweenField = type.GetField("_tween", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo instanceField = type.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
			if (tweenField == null || instanceField == null)
			{
				return;
			}

			if (tweenField.GetValue(overlayVfx) is Tween oldTween)
			{
				oldTween.Kill();
			}

			overlayVfx.Modulate = Colors.Transparent;

			Tween tween = overlayVfx.CreateTween();
			tween.TweenProperty(overlayVfx, "modulate:a", 1f, _doomOverlayFadeInSeconds).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			tween.TweenInterval(_doomOverlayHoldSeconds);
			tween.TweenProperty(overlayVfx, "modulate:a", 0f, _doomOverlayFadeOutSeconds);
			tween.TweenCallback(Callable.From(delegate
			{
				instanceField.SetValue(null, null);
				overlayVfx.QueueFreeSafely();
			}));

			tweenField.SetValue(overlayVfx, tween);
		}
    }
}
