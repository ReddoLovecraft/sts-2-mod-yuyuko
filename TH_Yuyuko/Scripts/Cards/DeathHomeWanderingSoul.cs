using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;
using static TH_Yuyuko.Scripts.VFX.YuyukoVfxManager;
namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public sealed class DeathHomeWanderingSoul : YuyukoCardModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,CardKeyword.Ethereal];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[3]
    {
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<DeathDesirePower>()
    });
    protected override bool ShouldGlowGoldInternal => base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<DeathDesirePower>()) ?? false;
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new CardsVar(2) };

    public DeathHomeWanderingSoul()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
       YuyukoVfxManager.PlaySimple("Soul",cardPlay.Target,VfxTargetPositionType.TargetCreature);
       await PowerCmd.Apply<WeakPower>(cardPlay.Target, DynamicVars.Cards.IntValue,Owner.Creature,this);
       await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars.Cards.IntValue,Owner.Creature,this);
       await PowerCmd.Apply<DebilitatePower>(cardPlay.Target, DynamicVars.Cards.IntValue,Owner.Creature,this);
       if(cardPlay.Target.IsAlive&&cardPlay.Target.HasPower<DeathDesirePower>())
       {
         await PowerCmd.Apply<WeakPower>(cardPlay.Target, DynamicVars.Cards.IntValue,Owner.Creature,this);
         await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars.Cards.IntValue,Owner.Creature,this);
         await PowerCmd.Apply<DebilitatePower>(cardPlay.Target, DynamicVars.Cards.IntValue,Owner.Creature,this);
       }
    }

    protected override void OnUpgrade()
    {
        this.RemoveKeyword(CardKeyword.Exhaust);
    }
}

}
