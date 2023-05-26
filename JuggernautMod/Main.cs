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
    public class JuggernautScript : BaseClass
    {
        public static bool isWearingJuggernautSuit;
        private readonly ObjectPool pool = new ObjectPool();
        readonly NativeMenu menuJuggernaut = new NativeMenu("Praesidium Armory", "Armor Menu");
        NativeItem optionEquipJuggernautSuit = new NativeItem("Equip/Unequip Juggernaut Suit", "Weighing roughly 200 lbs, this suit contains an assortment of Level IV Ballistic Plating and many protective Para-Aramid Fiber Layers underneath.", "FREE");NativeCheckboxItem checkboxItem = new NativeCheckboxItem("Checkbox Item", "This is a NativeCheckboxItem that contains a checkbox that can be turned on and off.", true);
        NativeCheckboxItem optionOnlyMinigun = new NativeCheckboxItem("Only Minigun?", "Toggle this on to force the Minigun to be used whenever wearing the suit.", true);

        protected override void OnStart()
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            Function.Call(Hash.REQUEST_ANIM_SET, "ANIM_GROUP_MOVE_BALLISTIC");
            pool.Add(menuJuggernaut);
            menuJuggernaut.Add(optionEquipJuggernautSuit);
            menuJuggernaut.Add(optionOnlyMinigun);
            optionEquipJuggernautSuit.Activated += (sender, e) => ToggleJuggernautSuit(playerPed);
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
                if (optionOnlyMinigun.Checked)
                {
                    Game.DisableControlThisFrame(Control.SelectWeapon);
                }
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
            WeaponCollection weapon = Game.Player.Character.Weapons;
            Weapon minigun = weapon.HasWeapon(WeaponHash.Minigun) ? weapon[WeaponHash.Minigun] : weapon.Give(WeaponHash.Minigun, 0, true, true);
            minigun.Ammo += 9999;
            minigun.InfiniteAmmo = true;
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
            var isMichael = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("player_zero"));
            var isFranklin = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("player_one"));
            var isTrevor = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("player_two"));
            var isGunman = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("hc_gunman"));
            var isMPMale = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("mp_m_freemode_01"));
            var isMPFemale = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("mp_f_freemode_01"));
            //  Might use this for a bath/shower script in the future.
            //Function.Call(Hash.SET_PED_WETNESS_HEIGHT, playerPed, 1.0f);
            if (isMichael)
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 5, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 5, 0, 0); //Lower
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 1, 0, 0); //Hands
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 5, 0, 0); //Shoes
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, 5, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, 0, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 0, 0, 0); //Bedges
                Function.Call(Hash.SET_PED_PROP_INDEX, playerPed, 0, 26, 0, false); //Hats
            }
            if (isFranklin)
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 20, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 12, 0, 0); //Lower
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 1, 0, 0); //Hands
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 7, 0, 0); //Shoes
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, 0, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, 3, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 8, 0, 0); //Bedges
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 11, 0, 0, 0); //Shirt Overlay
                Function.Call(Hash.SET_PED_PROP_INDEX, playerPed, 0, 18, 0, false); //Hats
            }
            if (isTrevor)
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 2, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 2, 0, 0); //Lower
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 1, 0, 0); //Hands
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 2, 0, 0); //Shoes
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, 2, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, 0, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 0, 0, 0); //Bedges
                Function.Call(Hash.SET_PED_PROP_INDEX, playerPed, 0, 24, 0, false); //Hats
            }
            if (isGunman)
            {
                //Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 0, 6, 0, 0); //Head
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 1, 1, 0, 0); //Beard
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 2, 1, 0, 0); //Hair
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 1, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 1, 0, 0); //Lower
                //Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 1, 0, 0); //Hands
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 1, 0, 0); //Shoes
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, 2, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, 1, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 9, 0, 0); //Bedges
                //Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 11, 0, 0, 0); //Shirt Overlay
                Function.Call(Hash.SET_PED_PROP_INDEX, playerPed, 0, 5, 0, false); //Hats
            }
            if (isMPMale)
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 0, 0, 0, 0); //Head
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 1, 104, 25, 0); //Beard
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 2, 57, 0, 0); //Hair
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 31, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 84, 0, 0); //Lower
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 0, 0, 0); //Hands
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 33, 0, 0); //Shoes
                //Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 7, 0, 1, 0); //Teeth
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, 97, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, 0, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 0, 0, 0); //Bedges
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 11, 186, 0, 0); //Shirt Overlay
                Function.Call(Hash.SET_PED_PROP_INDEX, playerPed, 0, 39, 0, false); //Hats
                Function.Call(Hash.SET_PED_PROP_INDEX, playerPed, 1, 15, 1, false); //Glasses
                Function.Call(Hash.SET_PED_PROP_INDEX, playerPed, 2, 3, 0, false); //Misc
            }
            if (isMPFemale)
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 0, 0, 0, 0); //Head
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 1, 104, 25, 0); //Beard
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 2, 49, 0, 0); //Hair
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 163, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 86, 0, 0); //Lower
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 0, 0, 0); //Hands
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 34, 0, 0); //Shoes
                //Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 7, 0, 1, 0); //Teeth
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, 105, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, 0, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 0, 0, 0); //Bedges
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 11, 188, 0, 0); //Shirt Overlay
                Function.Call(Hash.SET_PED_PROP_INDEX, playerPed, 0, 38, 0, false); //Hats
                Function.Call(Hash.SET_PED_PROP_INDEX, playerPed, 1, 25, 6, false); //Glasses
                Function.Call(Hash.SET_PED_PROP_INDEX, playerPed, 2, 0, 0, false); //Misc
            }
        }
        public static void UnequipJuggernautSuit(Ped ped)
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            WeaponCollection weapon = Game.Player.Character.Weapons;
            Weapon minigun = weapon[WeaponHash.Minigun];
            //if (minigun.InfiniteAmmo)
            {
                minigun.Ammo = 0;
                weapon.Remove(minigun);
            }
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
            JuggernautScript.EquipJuggernautSuit(playerPed);
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
