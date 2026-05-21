using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public sealed class DeathHomeRoadless : YuyukoCardModel
{
   public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new BlockVar(10m, ValueProp.Move),new CardsVar(2) };

    public DeathHomeRoadless()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        IEnumerable<CardModel> enumerable = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => c.IsUpgradable).TakeRandom(base.DynamicVars.Cards.IntValue, base.Owner.RunState.Rng.CombatCardSelection);
		foreach (CardModel item in enumerable)
		{
				CardCmd.Upgrade(item);
				CardCmd.Preview(item);
		}
    }
    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(2);
        base.DynamicVars.Cards.UpgradeValueBy(1);
    }
}

}
