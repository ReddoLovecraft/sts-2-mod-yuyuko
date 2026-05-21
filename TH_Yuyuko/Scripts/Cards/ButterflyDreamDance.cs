using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;
using static TH_Yuyuko.Scripts.VFX.YuyukoVfxManager;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public class ButterflyDreamDance : YuyukoCardModel
{
	protected override bool HasEnergyCostX => true;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[2]
    {
        Tools.GetStaticKeyword("SummonButterfly"),
        Tools.GetStaticKeyword("Butterfly")
    });
	public ButterflyDreamDance() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int num = ResolveEnergyXValue();
		await ToolBox.SummonButterfliesRandomly(choiceContext, Owner.Creature, num);

		var death = Owner.Creature.GetPower<ButterflyDeathPower>();
		var soul = Owner.Creature.GetPower<ButterflySoulPower>();
		var energy = Owner.Creature.GetPower<ButterflyEnergyPower>();

		async Task TriggerAllKinds(Creature target)
		{
			if (!target.IsAlive)
			{
				return;
			}

			if (death?.Amount > 0)
			{
				var r = new DamageResult(target, (ValueProp)0) { UnblockedDamage = death.Amount };
				await death.TirggerButterflyEffect(target, death.Amount, r);
			}
			if (soul?.Amount > 0)
			{
				var r = new DamageResult(target, (ValueProp)0) { UnblockedDamage = soul.Amount };
				await soul.TirggerButterflyEffect(target, soul.Amount, r);
			}
			if (energy?.Amount > 0)
			{
				var r = new DamageResult(target, (ValueProp)0) { UnblockedDamage = energy.Amount };
				await energy.TirggerButterflyEffect(target, energy.Amount, r);
			}
		}

		for (int i = 0; i < num; i++)
		{
			if (IsUpgraded)
			{
				foreach (Creature enemy in base.CombatState.HittableEnemies.ToList())
				{
					await TriggerAllKinds(enemy);
				}
			}
			else
			{
				var enemies = base.CombatState.HittableEnemies.Where(e => e.IsAlive).ToList();
				if (enemies.Count == 0)
				{
					break;
				}

				int idx = Rng.Chaotic.NextInt(0, enemies.Count);
				await TriggerAllKinds(enemies[idx]);
			}
		}
	}
	protected override void OnUpgrade()
	{
		
	}
}

}
