using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;


namespace TH_Yuyuko.Scrpits.Powers
{
    public abstract partial class ButterflyPowerModel :YuyukoPowerModel
    {
		private const int MaxOrbitCountPerKind = 9;
		private const string AdditiveMaterialPath = "res://TH_Yuyuko/Artworks/VFX/canvas_item_material_additive_shared.tres";
		private const string DeathButterflyTexPath = "res://TH_Yuyuko/Artworks/VFX/bulletAa000.png";
		private const string SoulButterflyTexPath = "res://TH_Yuyuko/Artworks/VFX/bulletAa001.png";
		private const string EnergyButterflyTexPath = "res://TH_Yuyuko/Artworks/VFX/bulletAa003.png";

		private static readonly Dictionary<(Creature owner, string kindKey), WeakReference<ButterflyOrbitVfx>> _orbitVfxByOwnerAndKind = new();

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
        //public override string? CustomPackedIconPath => "res://TH_Yuyuko/ArtWorks/Powers/DP32.png";
        //public override string? CustomBigIconPath => "res://TH_Yuyuko/ArtWorks/Powers/DP64.png";
        //protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeathPower>()];
    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			await Task.CompletedTask;
			return;
		}
		if (target == base.Owner)
		{
			await Task.CompletedTask;
			return;
		}
		if(!props.IsPoweredAttack_())
		{
			await Task.CompletedTask;
			return;
		}
        if(dealer==null||dealer!=base.Owner)
        {
			await Task.CompletedTask;
			return;
		}
        Flash();
		TryPlayTriggerVfx(target);
        await TirggerButterflyEffect(target,Amount,result);
		if(Owner.HasPower<ToxinMothPower>())
		{
			Owner.GetPower<ToxinMothPower>().Trigger();
			await PowerCmd.Apply<PoisonPower>(target,Owner.GetPowerAmount<ToxinMothPower>(),Owner,null);
		}
        await PowerCmd.Decrement(this);
    }
    public abstract Task TirggerButterflyEffect(Creature target,int num,DamageResult result);
    public ButterflyPowerModel() { }

		public static void EnsureOrbitVfx(Creature owner)
		{
			EnsureOrbitVfx(owner, "death", DeathButterflyTexPath, static o => o.GetPower<ButterflyDeathPower>()?.Amount ?? 0);
			EnsureOrbitVfx(owner, "soul", SoulButterflyTexPath, static o => o.GetPower<ButterflySoulPower>()?.Amount ?? 0);
			EnsureOrbitVfx(owner, "energy", EnergyButterflyTexPath, static o => o.GetPower<ButterflyEnergyPower>()?.Amount ?? 0);
		}

		private void TryPlayTriggerVfx(Creature target)
		{
			if (NCombatRoom.Instance == null)
			{
				return;
			}

			var owner = base.Owner;
			if (owner == null || owner.IsDead)
			{
				return;
			}

			var orbit = EnsureOrbitVfxForThisPower(owner);
			orbit?.FlyOneToTarget(target);
		}

		private ButterflyOrbitVfx? EnsureOrbitVfxForThisPower(Creature owner)
		{
			if (this is ButterflyDeathPower)
			{
				return EnsureOrbitVfx(owner, "death", DeathButterflyTexPath, static o => o.GetPower<ButterflyDeathPower>()?.Amount ?? 0);
			}
			if (this is ButterflySoulPower)
			{
				return EnsureOrbitVfx(owner, "soul", SoulButterflyTexPath, static o => o.GetPower<ButterflySoulPower>()?.Amount ?? 0);
			}
			if (this is ButterflyEnergyPower)
			{
				return EnsureOrbitVfx(owner, "energy", EnergyButterflyTexPath, static o => o.GetPower<ButterflyEnergyPower>()?.Amount ?? 0);
			}
			return null;
		}

		private static ButterflyOrbitVfx? EnsureOrbitVfx(Creature owner, string kindKey, string texturePath, Func<Creature, int> amountGetter)
		{
			if (NCombatRoom.Instance == null)
			{
				return null;
			}

			int amount = amountGetter(owner);
			if (amount <= 0)
			{
				return null;
			}

			var ownerNode = NCombatRoom.Instance.GetCreatureNode(owner);
			if (ownerNode == null)
			{
				return null;
			}

			var key = (owner, kindKey);
			if (_orbitVfxByOwnerAndKind.TryGetValue(key, out var weak) && weak.TryGetTarget(out var existing) && GodotObject.IsInstanceValid(existing))
			{
				if (existing.GetParent() != ownerNode)
				{
					existing.GetParent()?.RemoveChild(existing);
					ownerNode.AddChild(existing);
					ownerNode.MoveChild(existing, Mathf.Min(1, ownerNode.GetChildCount() - 1));
				}
				return existing;
			}

			var tex = ResourceLoader.Load<Texture2D>(texturePath);
			if (tex == null)
			{
				return null;
			}

			var additive = ResourceLoader.Load<Material>(AdditiveMaterialPath);
			var orbit = new ButterflyOrbitVfx(owner, kindKey, tex, additive, () => amountGetter(owner));
			ownerNode.AddChild(orbit);
			ownerNode.MoveChild(orbit, Mathf.Min(1, ownerNode.GetChildCount() - 1));
			_orbitVfxByOwnerAndKind[key] = new WeakReference<ButterflyOrbitVfx>(orbit);
			return orbit;
		}

		private static void UnregisterOrbitVfx(Creature owner, string kindKey, ButterflyOrbitVfx instance)
		{
			var key = (owner, kindKey);
			if (_orbitVfxByOwnerAndKind.TryGetValue(key, out var weak) && weak.TryGetTarget(out var existing) && ReferenceEquals(existing, instance))
			{
				_orbitVfxByOwnerAndKind.Remove(key);
			}
		}

		private sealed partial class ButterflyOrbitVfx : Node2D
		{
			private sealed class OrbitParticle
			{
				public required Sprite2D Sprite { get; init; }
				public required float Speed { get; init; }
				public required float Phase { get; init; }
				public required float RadiusX { get; init; }
				public required float RadiusY { get; init; }
				public required float FloatMul { get; init; }
				public required float Scale { get; init; }
			}

			private readonly Creature _owner;
			private readonly string _kindKey;
			private readonly Texture2D _texture;
			private readonly Material? _additiveMat;
			private readonly Func<int> _amountGetter;
			private readonly List<OrbitParticle> _particles = new();
			private float _t;
			private int _count;

			public ButterflyOrbitVfx(Creature owner, string kindKey, Texture2D texture, Material? additiveMat, Func<int> amountGetter)
			{
				_owner = owner;
				_kindKey = kindKey;
				_texture = texture;
				_additiveMat = additiveMat;
				_amountGetter = amountGetter;
				ProcessPriority = 10;
				ZIndex = 0;
			}

			public override void _ExitTree()
			{
				base._ExitTree();
				UnregisterOrbitVfx(_owner, _kindKey, this);
			}

			public override void _Process(double delta)
			{
				if (NCombatRoom.Instance == null || _owner == null || _owner.IsDead)
				{
					QueueFree();
					return;
				}

				int amount = _amountGetter?.Invoke() ?? 0;
				if (amount <= 0)
				{
					QueueFree();
					return;
				}

				int desiredCount = Mathf.Clamp(amount, 0, MaxOrbitCountPerKind);
				if (desiredCount != _count)
				{
					SetCount(desiredCount);
				}

				var ownerNode = NCombatRoom.Instance.GetCreatureNode(_owner);
				if (ownerNode == null)
				{
					QueueFree();
					return;
				}

				_t += (float)delta;

				Vector2 center = ownerNode.VfxSpawnPosition + new Vector2(0f, -120f);
				GlobalPosition = GlobalPosition.Lerp(center, (float)delta * 18f);

				float baseRadiusX = 340f;
				float baseRadiusY = 230f;
				float baseAngularSpeed = 1.4f;

				for (int i = 0; i < _particles.Count; i++)
				{
					var p = _particles[i];
					var s = p.Sprite;
					float angle = _t * baseAngularSpeed * p.Speed + p.Phase;

					float rx = baseRadiusX * p.RadiusX;
					float ry = baseRadiusY * p.RadiusY;

					float x = Mathf.Cos(angle) * rx + Mathf.Cos(angle * 2f) * (rx * 0.2f);
					float y = Mathf.Sin(angle) * ry + Mathf.Sin(angle * 2f) * (ry * 0.14f);

					float butterflySize = _texture.GetSize().Y * p.Scale;
					float floatAmp = butterflySize * 0.5f * p.FloatMul;
					y += Mathf.Sin(_t * (1.6f * p.Speed) + p.Phase) * floatAmp;

					Vector2 targetOffset = new Vector2(x, y);
					s.Position = s.Position.Lerp(targetOffset, (float)delta * 10f);
					Vector2 tangent = (targetOffset - s.Position);
					if (tangent.LengthSquared() > 0.0001f)
					{
						s.Rotation = tangent.Angle();
					}
					s.Scale = new Vector2(p.Scale, p.Scale);
				}
			}

			private void SetCount(int count)
			{
				_count = count;

				while (_particles.Count > _count)
				{
					var last = _particles[_particles.Count - 1];
					_particles.RemoveAt(_particles.Count - 1);
					last.Sprite.QueueFree();
				}

				while (_particles.Count < _count)
				{
					var s = new Sprite2D
					{
						Texture = _texture,
						Centered = true,
						Material = _additiveMat,
						ZIndex = 0
					};
					AddChild(s);

					float speed = Mathf.Lerp(0.75f, 1.35f, GD.Randf());
					float phase = GD.Randf() * Mathf.Tau;
					float radiusX = Mathf.Lerp(0.85f, 1.25f, GD.Randf());
					float radiusY = Mathf.Lerp(0.9f, 1.35f, GD.Randf());
					float floatMul = Mathf.Lerp(0.9f, 1.15f, GD.Randf());
					float scale = Mathf.Lerp(0.38f, 0.52f, GD.Randf());

					_particles.Add(new OrbitParticle
					{
						Sprite = s,
						Speed = speed,
						Phase = phase,
						RadiusX = radiusX,
						RadiusY = radiusY,
						FloatMul = floatMul,
						Scale = scale
					});
				}
			}

			public void FlyOneToTarget(Creature target)
			{
				if (NCombatRoom.Instance == null || target == null || target.IsDead)
				{
					return;
				}

				var container = NCombatRoom.Instance.SceneContainer;
				if (container == null || _particles.Count == 0)
				{
					return;
				}

				var targetNode = NCombatRoom.Instance.GetCreatureNode(target);
				if (targetNode == null)
				{
					return;
				}

				int index = (int)(GD.Randi() % (uint)_particles.Count);
				var src = _particles[index].Sprite;

				var proj = new Sprite2D
				{
					Texture = _texture,
					Centered = true,
					Material = _additiveMat,
					GlobalPosition = src.GlobalPosition,
					Scale = src.Scale,
					Rotation = src.Rotation,
					ZIndex = 5
				};
				container.AddChild(proj);

				Vector2 end = targetNode.VfxSpawnPosition + new Vector2(0f, -40f);
				float dist = proj.GlobalPosition.DistanceTo(end);
				float duration = Mathf.Clamp(dist / 650f, 0.18f, 0.48f);
				float waveAmp = Mathf.Lerp(22f, 46f, GD.Randf());
				float waves = Mathf.Lerp(1.25f, 2.25f, GD.Randf());

				var tween = proj.CreateTween();
				Vector2 start = proj.GlobalPosition;
				tween.TweenMethod(Callable.From<float>(t =>
					{
						float baseT = Mathf.Clamp(t, 0f, 1f);
						Vector2 pos = start.Lerp(end, baseT);
						float wobble = Mathf.Sin(baseT * Mathf.Pi * 2f * waves) * waveAmp * (1f - baseT);
						pos += Vector2.Up * wobble;
						proj.GlobalPosition = pos;
					}),
					0f,
					1f,
					duration
				).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
				tween.Parallel().TweenProperty(proj, "modulate", new Color(1f, 1f, 1f, 0f), duration * 0.45f).SetDelay(duration * 0.55f);
				tween.TweenCallback(Callable.From(proj.QueueFree));
			}
		}
       
    }
      public sealed class ButterflySoulPower : ButterflyPowerModel
    {
        //public override string? CustomPackedIconPath => "res://TH_Yuyuko/Artworks/Powers/BSP32.png";
        //public override string? CustomBigIconPath => "res://TH_Yuyuko/Artworks/Powers/BSP64.png";
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Soul>()];
        public ButterflySoulPower() { }

        public override async Task TirggerButterflyEffect(Creature target, int num,DamageResult result)
        {
            if (ToolBox.IsSpringEnhanced(Owner))
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Soul.Create(Owner.Player, 1, base.CombatState), PileType.Hand, addedByPlayer: true));
            else
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Soul.Create(Owner.Player, 1, base.CombatState), PileType.Discard, addedByPlayer: true));

        }
    }
      public sealed class ButterflyEnergyPower : ButterflyPowerModel
    {
       
        //public override string? CustomPackedIconPath => "res://TH_Yuyuko/Artworks/Powers/BEP32.png";
        //public override string? CustomBigIconPath => "res://TH_Yuyuko/Artworks/Powers/BEP64.png";
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];
        public ButterflyEnergyPower() { }
        protected override IEnumerable<DynamicVar> CanonicalVars => [ new EnergyVar(1)];
        public override async Task TirggerButterflyEffect(Creature target, int num,DamageResult result)
        {
            if (ToolBox.IsSpringEnhanced(Owner))
                await PlayerCmd.GainEnergy(num,Owner.Player);
            else
                await PlayerCmd.GainEnergy(1,Owner.Player);
        }
    }
      public sealed class ButterflyDeathPower : ButterflyPowerModel
    {
       
        //public override string? CustomPackedIconPath => "res://TH_Yuyuko/Artworks/Powers/BDP32.png";
        //public override string? CustomBigIconPath => "res://TH_Yuyuko/Artworks/Powers/BDP64.png";
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeathPower>()];
        public ButterflyDeathPower() { }

		public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			if (card.Owner != base.Owner.Player || originalCost < 0m)
			{
				modifiedCost = originalCost;
				return false;
			}

			if (card.Id.Entry != "TH_YUYUKO-DEAD_BUTTERFLY_SLEEP")
			{
				modifiedCost = originalCost;
				return false;
			}

			modifiedCost = originalCost - (decimal)Amount;
			return (int)modifiedCost != (int)originalCost;
		}

         public override async Task TirggerButterflyEffect(Creature target, int num,DamageResult result)
        {
            if (!target.IsAlive)
            {
                return;
            }

            if (ToolBox.IsSpringEnhanced(Owner))
            {
                await PowerCmd.Apply<DeathPower>(target, result.TotalDamage, Owner, null);
            }
            else
            {
                await PowerCmd.Apply<DeathPower>(target, num, Owner, null);
            }
        }
    }

}
