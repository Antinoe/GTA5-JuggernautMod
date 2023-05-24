using System;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using Control = GTA.Control;
using Hash = GTA.Native.Hash;
using LemonUI;
using PlayerCompanion;
using LemonUI.Elements;

namespace JuggernautMod
{
    public class BaseClass : Script
    {
        public BaseClass()
        {
            Tick += OnUpdate;
            Aborted += OnAbort;
            KeyDown += OnKeyPressed;
            KeyUp += OnKeyReleased;
            Interval = 0;
        }
        protected virtual void OnUpdate(object sender, EventArgs e) { }
        protected virtual void OnAbort(object sender, EventArgs e) { }
        protected virtual void OnKeyPressed(object sender, KeyEventArgs e) { }
        protected virtual void OnKeyReleased(object sender, KeyEventArgs e) { }
    }
    public class JuggernautPlayer : BaseClass
    {
        public static bool isWearingJuggernautSuit;
        protected override void OnUpdate(object sender, EventArgs e)
        {
            if (isWearingJuggernautSuit)
            {
                Player player = Game.Player;
                Ped playerPed = Game.Player.Character;
                Function.Call(Hash.SET_PED_RESET_FLAG, playerPed, 200, true);
                Function.Call(Hash.CLEAR_PED_BLOOD_DAMAGE, playerPed);
                Game.DisableControlThisFrame(Control.Jump);
                Game.DisableControlThisFrame(Control.Enter);
                Game.DisableControlThisFrame(Control.Cover);
                Game.DisableControlThisFrame(Control.Duck);
                //Game.DisableControlThisFrame(Control.SelectWeapon);
                if (playerPed.Health <= 200)
                {
                    UnequipJuggernautSuit(playerPed);
                }
            }
        }
        protected override void OnKeyPressed(object sender, KeyEventArgs e)
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            if (e.KeyCode == Keys.OemPeriod)
            //if (Game.IsControlJustReleased(Control.Sprint) && Game.IsControlJustReleased(Control.VehicleNextRadio))
            {
                //isWearingJuggernautSuit = ();
                //isWearingJuggernautSuit = !isWearingJuggernautSuit;
                //Item item = Companion.Inventories.GetRandomItem();
                //Companion.Inventories.Current.Add(item);
                ToggleJuggernautSuit(playerPed);
            }
        }
        protected void ToggleJuggernautSuit(Ped ped)
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            if (!isWearingJuggernautSuit && CanEquipJuggernautSuit(playerPed)) { EquipJuggernautSuit(playerPed); }
            if (isWearingJuggernautSuit) { UnequipJuggernautSuit(playerPed); }
        }
        public virtual bool CanEquipJuggernautSuit(Ped ped)
        {
            return true;
        }
        public static void EquipJuggernautSuit(Ped ped)
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            isWearingJuggernautSuit = true;
            playerPed.MaxHealth = 2000;
            playerPed.Health = 2000;
            //player.MaxArmor = 2000;
            playerPed.Armor = 2000;
            Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, playerPed, 1.0f);
            Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, playerPed, "ANIM_GROUP_MOVE_BALLISTIC", 1.0f);
            Function.Call(Hash.SET_PED_STRAFE_CLIPSET, playerPed, "MOVE_STRAFE_BALLISTIC");
            Function.Call(Hash.SET_WEAPON_ANIMATION_OVERRIDE, playerPed, 0x5534A626);
        }
        public static void UnequipJuggernautSuit(Ped ped)
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            isWearingJuggernautSuit = false;
            playerPed.MaxHealth = 200;
            playerPed.Health = 200;
            //player.MaxArmor = 100;
            playerPed.Armor = 100;
            Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, playerPed, 1.0f);
            Function.Call(Hash.RESET_PED_STRAFE_CLIPSET, playerPed);
            Function.Call(Hash.SET_WEAPON_ANIMATION_OVERRIDE, playerPed, 0x0);
        }
        protected override void OnAbort(object sender, EventArgs e)
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            UnequipJuggernautSuit(playerPed);
        }
    }
    public abstract class JuggernautSuit : StackableItem
    {
        public override string Name => "Juggernaut Suit";
        public override ScaledTexture Icon => new ScaledTexture("", "");
        public JuggernautSuit()
        {
            Used += UseItem;
        }
        private void UseItem(object sender, EventArgs e)
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            JuggernautPlayer.EquipJuggernautSuit(playerPed);
            if (Count == 1)
            {
                Remove();
            }
            else
            {
                Count -= 1;
            }
        }
    }
}
