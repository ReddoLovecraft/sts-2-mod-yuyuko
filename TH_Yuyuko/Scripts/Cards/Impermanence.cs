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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;
using static TH_Yuyuko.Scripts.VFX.YuyukoVfxManager;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public class Impermanence : YuyukoCardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[2]
    {
        HoverTipFactory.FromPower<DeathPower>(),
		HoverTipFactory.FromPower<DeathDesirePower>()
    });
	protected override bool ShouldGlowGoldInternal => base.CombatState?.HittableEnemies.Any((Creature e) => Trigger(e)) ?? false;
	public Impermanence() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		foreach (Creature creature in base.CombatState.HittableEnemies.ToList())
		{
			if (creature == null || !creature.IsAlive || creature.ShowsInfiniteHp)
			{
				continue;
			}
			if (Trigger(creature))
			{
				await DoomPower.DoomKill([creature]);
			}
		}
	}
	public bool Trigger(Creature target)
	{
		long deathIntent = target.GetPowerAmount<DeathPower>();
		long deathDesire = target.GetPowerAmount<DeathDesirePower>();
		long product = deathIntent * deathDesire;
		int threshold = IsUpgraded ? target.CurrentHp : target.MaxHp;
		return product >= threshold;
	}
	protected override void OnUpgrade()
	{
	}
}

}
