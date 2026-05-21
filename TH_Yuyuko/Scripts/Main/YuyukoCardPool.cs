using BaseLib.Abstracts;
using Godot;

namespace TH_Yuyuko.Scripts.Main
{
	public class YuyukoCardPool : CustomCardPoolModel
{
	public override string Title => "TH_Yuyuko";

 	public override Color ShaderColor => new Color("f6b6c1ff");
	public override float S => 0.85f;
	public override float V => 1.44f;
	public override Color DeckEntryCardColor => new Color("ff6b97ff");
  	public override string? BigEnergyIconPath => "res://TH_Yuyuko/Artworks/Character/card_orb.png";
	public override string? TextEnergyIconPath => "res://TH_Yuyuko/Artworks/Character/cost_orb.png";
	public override bool IsColorless => false;
}
}
