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
using TH_Yuyuko.Scrpits.Cards;


namespace TH_Yuyuko.Scrpits.Powers
{
    public sealed class SpringPower :YuyukoPowerModel
    {
		private static readonly IHoverTip _summonButterflyTip = Tools.GetStaticKeyword("SummonButterfly");
		private static readonly IHoverTip _butterflyTip = Tools.GetStaticKeyword("Butterfly");
		private static readonly IHoverTip _deathPowerTip = HoverTipFactory.FromPower<DeathPower>();
		private static readonly IHoverTip _demisePowerTip = HoverTipFactory.FromPower<DemisePower>();
		private static readonly IHoverTip _sakuraTip = HoverTipFactory.FromCard<Sakura>();

		private sealed class SpringComputedVar : DynamicVar
		{
			private readonly Func<SpringPower, int> _getter;

			public SpringComputedVar(string name, Func<SpringPower, int> getter)
				: base(name, 0m)
			{
				_getter = getter;
			}

			protected override decimal GetBaseValueForIConvertible()
			{
				return GetCurrentValue();
			}

			public override string ToString()
			{
				return GetCurrentValue().ToString();
			}

			private int GetCurrentValue()
			{
				if (_owner is SpringPower springPower)
				{
					return _getter(springPower);
				}
				return 0;
			}
		}

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
        //public override string? CustomPackedIconPath => "res://TH_Yuyuko/Artworks/Powers/SP32.png";
        //public override string? CustomBigIconPath => "res://TH_Yuyuko/Artworks/Powers/SP64.png";
        protected override IEnumerable<IHoverTip> ExtraHoverTips => Owner == null ? GetAllExtraHoverTips() : GetRelevantExtraHoverTips();
		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new SpringComputedVar("SummonCount", sp => sp.Amount >= 10 ? sp.Amount / 2 : 0),
			new SpringComputedVar("DemiseCount", sp => sp.Amount >= 20 ? sp.Amount / 4 : 0),
			new SpringComputedVar("SakuraCount", sp => sp.Amount >= 50 ? sp.Amount / 10 : 0)
		];
        public SpringPower() { }

		private static IEnumerable<IHoverTip> GetAllExtraHoverTips()
		{
			return new IHoverTip[]
			{
				_summonButterflyTip,
				_butterflyTip,
				_deathPowerTip,
				_demisePowerTip,
				_sakuraTip
			};
		}

		private IEnumerable<IHoverTip> GetRelevantExtraHoverTips()
		{
			var tips = new List<IHoverTip>(capacity: 5);

			if (Amount > 0)
			{
				tips.Add(_deathPowerTip);
			}
			if (Amount >= 10)
			{
				tips.Add(_summonButterflyTip);
				tips.Add(_butterflyTip);
			}
			if (Amount >= 20)
			{
				tips.Add(_demisePowerTip);
			}
			if (Amount >= 50)
			{
				tips.Add(_sakuraTip);
			}

			return tips;
		}

		 public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != base.Owner.Player)
            {
                return;
            }
			Flash();
			if (Amount > 0)
			{
				await PowerCmd.Apply<DeathPower>(Owner.CombatState.HittableEnemies, Amount, Owner, null);
			}
			int summonCount = Amount >= 10 ? Amount / 2 : 0;
			if (summonCount > 0)
			{
				await ToolBox.SummonButterfliesRandomly(choiceContext, Owner, summonCount);
			}
			int demiseCount = Amount >= 20 ? Amount / 4 : 0;
			if (demiseCount > 0)
			{
				await PowerCmd.Apply<DemisePower>(Owner.CombatState.HittableEnemies, demiseCount, Owner, null);
			}
			int sakuraCount = Amount >= 50 ? Amount / 10 : 0;
			if (sakuraCount > 0)
			{
				CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Sakura.Create(Owner.Player, sakuraCount, Owner.CombatState), PileType.Hand, addedByPlayer: true));
			}
		}
  
    }
}
