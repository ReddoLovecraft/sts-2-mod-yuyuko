using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scrpits.Potions;
[Pool(typeof(YuyukoPotionPool))]
public sealed class EclosionPotion : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Transform)];
    public override string? CustomPackedImagePath => "res://TH_Yuyuko/Artworks/Potions/ECLOSION_POTION.png";
    public override string? CustomPackedOutlinePath => "res://TH_Yuyuko/Artworks/Potions/Outlines/ECLOSION_POTION.png"; 
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
         CardPileAddResult result;
        foreach (CardModel item in await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 0, 999999999), context: choiceContext, player: base.Owner, filter: null, source: this))
		{
		    result = await CardCmd.TransformToRandom(item, base.Owner.RunState.Rng.CombatCardSelection);
            result.cardAdded.EnergyCost.SetThisTurnOrUntilPlayed(0);
		}
    }
}
