using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;


namespace TH_Yuyuko.Scrpits.Powers
{
    public sealed class ToxinMothPower :YuyukoPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
        //public override string? CustomPackedIconPath => "res://TH_Yuyuko/Artworks/Powers/DP32.png";
        //public override string? CustomBigIconPath => "res://TH_Yuyuko/Artworks/Powers/DP64.png";
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Butterfly"),HoverTipFactory.FromPower<PoisonPower>()];
        public ToxinMothPower() { }
        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side != Owner.Side)
            {
                return;
            }
            await PowerCmd.Remove(this);
        }
    }
}
