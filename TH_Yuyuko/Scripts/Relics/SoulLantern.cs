using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scrpits.Relics
{
[Pool(typeof(YuyukoRelicPool))]
public class SoulLantern : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;
	public override string PackedIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Yuyuko/Artworks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";
	    protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[1]
        {
		  HoverTipFactory.FromPower<DeathDesirePower>()
        });
	public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
		{
			Flash();
			await PowerCmd.Apply<DeathDesirePower>(combatState.HittableEnemies,1, base.Owner.Creature, null);
		}
	}
}
}
