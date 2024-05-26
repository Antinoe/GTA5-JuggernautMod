using GTA;
using LemonUI.Elements;
using PlayerCompanion;
using JuggernautMod.Common;

namespace JuggernautMod.Content{
    public class BaseItem : StackableItem{
		public override string Name => "Base Item";
		public override ScaledTexture Icon => new ScaledTexture("", "");
		public BaseItem(){
			//Used += OnUseItem;
			Used += (sender, e) => UseItem();
		}
		private void UseItem(){
			if(CanUseItem()){
				if(Count == 1){Remove();}
				else{Count--;}
				OnUseItem();
			}
		}
		public virtual bool CanUseItem(){return true;}
		public virtual void OnUseItem(){}
    }
    public class JuggernautSuit : BaseItem{
		public override string Name => "Juggernaut Suit";
		public override ScaledTexture Icon => new ScaledTexture("", "");
		public override string Description => "Weighing roughly 50 lbs, this suit contains an assortment of Level IV Ballistic Plating and many thick, protective layers of Para-Aramid Fiber material underneath.";
		public override int Maximum => 5;
		public override int Value => 5000;
		public override void OnUseItem(){
			Player player = Game.Player;
			Ped playerPed = Game.Player.Character;
			JuggernautScript.PreEquipJuggernautSuit(playerPed);
			base.OnUseItem();
		}
    }
}
