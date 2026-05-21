using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public sealed class DeathHomeOriginalSin : YuyukoCardModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new DamageVar(11m, ValueProp.Move) ,new CardsVar(3) };
    protected override bool ShouldGlowGoldInternal => base.CombatState?.HittableEnemies.Any((Creature e) => ToolBox.GetDebuffTotalCount(e)>0) ?? false;
    public DeathHomeOriginalSin()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int addtion=ToolBox.GetDebuffKind(cardPlay.Target)*base.DynamicVars.Cards.IntValue;
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue+addtion).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_chain")
			.Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(1);
        base.DynamicVars.Cards.UpgradeValueBy(2);
    }
}

}
