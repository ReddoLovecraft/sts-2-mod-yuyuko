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
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scrpits.Relics
{
[Pool(typeof(YuyukoRelicPool))]
public class FoldableUmbrella : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
	public override string PackedIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";
    protected override string PackedIconOutlinePath => $"res://TH_Yuyuko/Artworks/Relics/Outlines/{Id.Entry}.png";
    protected override string BigIconPath => $"res://TH_Yuyuko/Artworks/Relics/{Id.Entry}.png";
	private int addCount=0;
	private int cnt=0;
	[SavedProperty]
    public virtual int ToolCount
    {
        get{return cnt;}
        set
        {
            AssertMutable();
			cnt=value;
			InvokeDisplayAmountChanged();
        }
    }
	[SavedProperty]
    public virtual int  AddCount
    {
        get{return addCount;}
        set
        {
            AssertMutable();
			addCount=value;
        }
    }
	public void Refresh() { InvokeDisplayAmountChanged(); }
	public override bool ShowCounter => true;
	public override bool IsUsedUp => ToolCount<=0;
	public override bool IsAllowed(IRunState runState)
	{
		return runState.Players.Count == 1;
	}
	public override bool ShouldAllowFreeTravel()
	{
		return !IsUsedUp;
	}
	public override int DisplayAmount
	{
		get
		{
			return ToolCount;
		}
	}
	public override Task AfterRoomEntered(AbstractRoom room)
	{
		this.addCount++;
		if(addCount>=2)
		{
			addCount=0;
			cnt++;
		}
		Refresh();
		if (IsUsedUp)
		{
			return Task.CompletedTask;
		}
		if (base.Owner.RunState.CurrentRoomCount > 1)
		{
			return Task.CompletedTask;
		}
		if (!(base.Owner.RunState is RunState runState))
		{
			return Task.CompletedTask;
		}
		if (runState.VisitedMapCoords.Count <= 1)
		{
			return Task.CompletedTask;
		}
		IReadOnlyList<MapCoord> visitedMapCoords = runState.VisitedMapCoords;
		MapCoord coord = visitedMapCoords[visitedMapCoords.Count - 2];
		MapPoint point = runState.Map.GetPoint(coord);
		if (point == null)
		{
			return Task.CompletedTask;
		}
		MapPoint currentMapPoint = base.Owner.RunState.CurrentMapPoint;
		if (currentMapPoint == null)
		{
			return Task.CompletedTask;
		}
		if (point.Children.Contains(currentMapPoint))
		{
			return Task.CompletedTask;
		}
		cnt--;
		Refresh();
		return Task.CompletedTask;
	}
}
}
