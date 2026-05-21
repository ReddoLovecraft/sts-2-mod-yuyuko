using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scrpits.Potions;
[Pool(typeof(YuyukoPotionPool))]
public sealed class BottleOfSakura : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Sakura>()];
    public override string? CustomPackedImagePath => "res://TH_Yuyuko/Artworks/Potions/BOTTLE_OF_SAKURA.png";
    public override string? CustomPackedOutlinePath => "res://TH_Yuyuko/Artworks/Potions/Outlines/BOTTLE_OF_SAKURA.png"; 
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
      CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Sakura.Create(base.Owner, 3, Owner.Creature.CombatState), PileType.Hand, addedByPlayer: true));
    }
}
