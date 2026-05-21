using BaseLib.Extensions;
using BaseLib.Hooks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scrpits.Cards
{
[Pool(typeof(StatusCardPool))]
public class Sakura : YuyukoCardModel, IMaxHandSizeModifier
{
	
    public override int MaxUpgradeLevel =>0;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal,CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1),new DynamicVar("Power",4),new HpLossVar(1)];
	protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[4]
    {
	   HoverTipFactory.FromPower<SpringPower>(),
	   HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
	   HoverTipFactory.FromPower<DeathPower>(),
	   HoverTipFactory.FromPower<DeathDesirePower>()
    });
	public Sakura() : base(0, CardType.Status, CardRarity.Status, TargetType.None)
	{
	}

	public int ModifyMaxHandSize(Player player, int currentMaxHandSize)
	{
		if (!IsMutable)
		{
			return currentMaxHandSize;
		}
		CardPile? pile = Pile;
		if (pile != null && pile.Type == PileType.Hand)
		{
			return currentMaxHandSize + 1;
		}
		return currentMaxHandSize;
	}

	public static IEnumerable<Sakura> Create(Player owner, int amount, CombatState combatState)
	{
		List<Sakura> list = new List<Sakura>();
		for (int i = 0; i < amount; i++)
		{
			list.Add((Sakura)combatState.CreateCard(ModelDb.Card<Sakura>(), owner));
		}
		return list;
	}

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card.Owner == base.Owner&&card ==this&&causedByEthereal)
		{
			GhostDanceOukaRanmanPower.SpawnPetalVfx(GD.RandRange(7, 9));
			await PowerCmd.Apply<DeathPower>(CombatState.HittableEnemies,this.DynamicVars["Power"].IntValue,Owner.Creature,this);
			await PowerCmd.Apply<DeathDesirePower>(CombatState.HittableEnemies,this.DynamicVars.HpLoss.IntValue,Owner.Creature,this);
		}
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		GhostDanceOukaRanmanPower.SpawnPetalVfx(GD.RandRange(7, 9));
		await PowerCmd.Apply<SpringPower>(Owner.Creature,this.DynamicVars.Cards.IntValue,Owner.Creature,this);
	}

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		if (card == this && oldPileType == PileType.None)
		{
			GhostDanceOukaRanmanPower.SpawnPetalVfx(GD.RandRange(7, 9));
		}
		return Task.CompletedTask;
	}

	public override void AfterTransformedTo()
	{
		GhostDanceOukaRanmanPower.SpawnPetalVfx(GD.RandRange(7, 9));
	}

}

}
