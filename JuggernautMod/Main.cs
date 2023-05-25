using System;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using Control = GTA.Control;
using Hash = GTA.Native.Hash;
using LemonUI;
using LemonUI.Elements;
using LemonUI.Menus;
using PlayerCompanion;

namespace JuggernautMod
{
    public class BaseClass : Script
    {
        protected virtual void OnStart() { }
        protected virtual void OnUpdate(object sender, EventArgs e) { }
        protected virtual void OnAbort(object sender, EventArgs e) { }
        protected virtual void OnKeyPressed(object sender, KeyEventArgs e) { }
        protected virtual void OnKeyReleased(object sender, KeyEventArgs e) { }
        public BaseClass()
        {
            OnStart();
            Tick += OnUpdate;
            Aborted += OnAbort;
            KeyDown += OnKeyPressed;
            KeyUp += OnKeyReleased;
            Interval = 0;
        }
    }
    public class JuggernautPlayer : BaseClass
    {
        public static bool isWearingJuggernautSuit;
        private readonly ObjectPool pool = new ObjectPool();
        readonly NativeMenu menuJuggernaut = new NativeMenu("Praesidium Armory", "Armor Menu");
        NativeItem itemEquipJuggernautSuit = new NativeItem("Equip/Unequip Juggernaut Suit", "Weighing roughly 200 lbs, this suit contains an assortment of Level IV Ballistic Plating and many protective Para-Aramid Fiber Layers underneath.", "FREE");

        protected override void OnStart()
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            Function.Call(Hash.REQUEST_ANIM_SET, "ANIM_GROUP_MOVE_BALLISTIC");
            pool.Add(menuJuggernaut);
            menuJuggernaut.Add(itemEquipJuggernautSuit);
            itemEquipJuggernautSuit.Activated += (sender, e) => ToggleJuggernautSuit(playerPed);
        }
        protected override void OnUpdate(object sender, EventArgs e)
        {
            //  This runs the UI stuff created within the class.
            pool.Process();
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
            //if (Game.IsControlPressed(Control.Sprint) && Game.IsControlJustPressed(Control.VehicleNextRadio))
            //if (Game.IsControlPressed(Control.Sprint) && e.KeyCode == Keys.OemPeriod)
            //if (e.KeyCode == Keys.LShiftKey && e.KeyCode == Keys.OemPeriod)
            if (e.KeyCode == Keys.OemPeriod)
            {
                //Item item = Companion.Inventories.GetRandomItem();
                //Companion.Inventories.Current.Add(item);
                menuJuggernaut.Visible = true;
            }
        }
        protected void ToggleJuggernautSuit(Ped ped)
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            if (!isWearingJuggernautSuit)
            {
                if (CanEquipJuggernautSuit(playerPed))
                {
                    EquipJuggernautSuit(playerPed);
                }
            }
            else { UnequipJuggernautSuit(playerPed); }
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

            //  Setting Components
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 5, 0, 0); //Upper
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 5, 0, 0); //Lower
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 1, 0, 0); //Hands
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 5, 0, 0); //Shoes
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, 5, 0, 0); //Accessory 0
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, 0, 0, 0); //Accessory 1
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 0, 0, 0); //Bedges
            Function.Call(Hash.SET_PED_PROP_INDEX, playerPed, 0, 26, 0, false); //Hats
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
