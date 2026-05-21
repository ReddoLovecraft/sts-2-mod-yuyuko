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
using Patchoulib.Scrpits.Main;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scrpits.Potions;
[Pool(typeof(YuyukoPotionPool))]
public sealed class ButterflyGatherPotion : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("SummonButterfly"),Tools.GetStaticKeyword("Butterfly")];
    public override string? CustomPackedImagePath => "res://TH_Yuyuko/Artworks/Potions/BUTTERFLY_GATHER_POTION.png";
    public override string? CustomPackedOutlinePath => "res://TH_Yuyuko/Artworks/Potions/Outlines/BUTTERFLY_GATHER_POTION.png"; 
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
       await ToolBox.SummonButterfliesRandomly(choiceContext,Owner.Creature,4);
    }
}
