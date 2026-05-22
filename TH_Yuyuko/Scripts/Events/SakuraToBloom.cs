using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Relics;

namespace TH_Yuyuko.Scrpits.Events;

public sealed class SakuraToBloom : CustomEventModel
{
	 public override string? CustomInitialPortraitPath => "res://TH_Yuyuko/Artworks/Events/sakuratobloom.png";
	private EventOption CreateOption(Func<Task>? onChosen, string optionKey, IEnumerable<IHoverTip>? hoverTips = null)
	{
		LocString title = new LocString(LocTable, optionKey + ".title");
		LocString description = new LocString(LocTable, optionKey + ".description");
		return new EventOption(this, onChosen, title, description, optionKey, hoverTips ?? Enumerable.Empty<IHoverTip>());
	}

	public override bool IsAllowed(IRunState runState)
	{
		if (runState.CurrentActIndex != 2)
		{
			return false;
		}
		return runState.Players.All(p => p.Character is YuyukoCharacter);
	}

	protected override Task BeforeEventStarted(bool isPreFinished)
	{
		Owner!.CanRemovePotions = false;
		return Task.CompletedTask;
	}

	protected override void OnEventFinished()
	{
		Owner!.CanRemovePotions = true;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new EventOption[]
		{
			CreateOption(Boost, $"{Id.Entry}.pages.INITIAL.options.BOOST", HoverTipFactory.FromRelic<BloomMark>()),
			CreateOption(Prevent, $"{Id.Entry}.pages.INITIAL.options.PREVENT", HoverTipFactory.FromRelic<NewBorn>()),
		};
	}

	private async Task Boost()
	{
		Player owner = Owner!;
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), owner.Creature, 40, false);
		await RelicCmd.Obtain(ModelDb.Relic<BloomMark>().ToMutable(), owner);
		SetEventFinished(PageDescription("BOOST"));
	}

	private async Task Prevent()
	{
		Player owner = Owner!;
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), owner.Creature, 60, false);
		await RelicCmd.Obtain(ModelDb.Relic<NewBorn>().ToMutable(), owner);
		SetEventFinished(PageDescription("PREVENT"));
	}
}
