using System;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public sealed class DeathbedManipulation : YuyukoCardModel
{
	public override bool GainsBlock => true;
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeathPower>()];

	public DeathbedManipulation() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		int total=0;
		foreach(Creature mos in base.CombatState.HittableEnemies)
		{
			int death = mos.GetPowerAmount<DeathPower>();
			total += death;
			if (death > 0)
			{
				SpawnDeathOrbToOwner(mos, base.Owner.Creature, death);
			}
		}
		await CreatureCmd.GainBlock(base.Owner.Creature, new BlockVar(total,ValueProp.Unpowered), cardPlay);
	}

	private static void SpawnDeathOrbToOwner(Creature from, Creature to, int deathStacks)
	{
		var room = MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance;
		Node? container = room?.CombatVfxContainer;
		if (room == null || container == null)
		{
			return;
		}

		var fromNode = room.GetCreatureNode(from);
		var toNode = room.GetCreatureNode(to);
		if (fromNode == null || toNode == null)
		{
			return;
		}

		var tex = ResourceLoader.Load<Texture2D>("res://TH_Yuyuko/Artworks/VFX/bulletAa000.png");
		if (tex == null)
		{
			return;
		}

		var additive = ResourceLoader.Load<Material>("res://TH_Yuyuko/Artworks/VFX/canvas_item_material_additive_shared.tres");

		float baseScale = 0.55f;
		float stackMul = 1f + Mathf.Max(0, deathStacks) * 0.01f;
		Vector2 scaleB = Vector2.One * (baseScale * stackMul);
		Vector2 scaleA = Vector2.One * baseScale;

		var wrapper = new Node2D();
		wrapper.GlobalPosition = fromNode.VfxSpawnPosition;

		var sprite = new Sprite2D
		{
			Texture = tex,
			Centered = true,
			Material = additive,
			Scale = scaleB,
			Modulate = new Color(0.55f, 0.12f, 0.75f, 0.95f),
			ZAsRelative = false,
			ZIndex = 95
		};
		wrapper.AddChild(sprite);
		container.AddChild(wrapper);

		Vector2 start = fromNode.VfxSpawnPosition;
		Vector2 end = toNode.VfxSpawnPosition;
		float dist = start.DistanceTo(end);
		float seconds = dist / 1200f;
		seconds = Mathf.Clamp(seconds, 0.18f, 0.75f);

		Tween tween = wrapper.CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(wrapper, "global_position", end, seconds).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(sprite, "scale", scaleA, seconds).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
		tween.SetParallel(false);
		tween.TweenProperty(sprite, "modulate:a", 0f, 0.08f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(wrapper.QueueFree));
	}
	protected override void OnUpgrade()
	{
		this.RemoveKeyword(CardKeyword.Exhaust);
	}
}

}
