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
        NativeItem optionEquipJuggernautSuit = new NativeItem("Equip Juggernaut Suit", "Weighing roughly 200 lbs, this suit contains an assortment of Level IV Ballistic Plating and many protective Para-Aramid Fiber Layers underneath.", "FREE");
        NativeItem optionUnequipJuggernautSuit = new NativeItem("Unequip Juggernaut Suit", "Remove the suit?", "");
        NativeCheckboxItem optionAmmoRegenerationMinigun = new NativeCheckboxItem("Minigun Ammo Regeneration?", "If true, the Minigun's ammo will regenerate with time.\nDisabled by default because there is a bug with the Weapon Wheel.", false);
        NativeCheckboxItem optionAmmoRegenerationGrenadeLauncher = new NativeCheckboxItem("Grenade Launcher Ammo Regeneration?", "If true, the Grenade Launcher's ammo will regenerate with time.", true);
        NativeCheckboxItem optionAmmoRegenerationPipeBomb = new NativeCheckboxItem("Pipe Bomb Regeneration?", "If true, Pipe Bombs will regenerate with time.", true);
        NativeCheckboxItem optionOnlyMinigun = new NativeCheckboxItem("Only Minigun?", "If true, only the Minigun can be used when wearing the suit. The Weapon Wheel will also be disabled.", false);
        NativeCheckboxItem optionInfiniteAmmoMinigun = new NativeCheckboxItem("Infinite Ammo for Minigun?", "If true, the Minigun will never run out of ammo.", false);
        NativeCheckboxItem optionRegenerationHealth = new NativeCheckboxItem("Health Regeneration?", "If true, your Health will regenerate when wearing the suit.", true);
        NativeCheckboxItem optionRegenerationArmor = new NativeCheckboxItem("Armor Regeneration?", "If true, your Armor will regenerate when wearing the suit.", true);
        NativeCheckboxItem optionCanJump = new NativeCheckboxItem("Can Jump?", "If false, you cannot Jump while wearing the suit.", false);
        NativeCheckboxItem optionCanEnterVehicles = new NativeCheckboxItem("Can Enter Vehicles?", "If false, you cannot Enter Vehicles while wearing the suit.", true);
        NativeCheckboxItem optionCanTakeCover = new NativeCheckboxItem("Can Take Cover?", "If false, you cannot Take Cover while wearing the suit.", false);
        NativeCheckboxItem optionCanSneak = new NativeCheckboxItem("Can Sneak?", "If false, you cannot Sneak while wearing the suit.", false);
        public int ammoRegenerationCooldownMinigun = 15;
        public int ammoRegenerationCooldownGrenadeLauncher = 1800;
        public int ammoRegenerationCooldownPipeBomb = 600;
        public int regenerationCooldownHealth = 15;
        public int regenerationCooldownArmor = 15;

        protected override void OnStart()
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            var Inventory = Companion.Inventories.Current;
            Function.Call(Hash.REQUEST_ANIM_SET, "ANIM_GROUP_MOVE_BALLISTIC");
            pool.Add(menuJuggernaut);
            menuJuggernaut.Add(optionEquipJuggernautSuit);
            menuJuggernaut.Add(optionUnequipJuggernautSuit);
            menuJuggernaut.Add(optionAmmoRegenerationMinigun);
            menuJuggernaut.Add(optionAmmoRegenerationGrenadeLauncher);
            menuJuggernaut.Add(optionAmmoRegenerationPipeBomb);
            menuJuggernaut.Add(optionOnlyMinigun);
            menuJuggernaut.Add(optionInfiniteAmmoMinigun);
            menuJuggernaut.Add(optionRegenerationHealth);
            menuJuggernaut.Add(optionRegenerationArmor);
            menuJuggernaut.Add(optionCanJump);
            menuJuggernaut.Add(optionCanEnterVehicles);
            menuJuggernaut.Add(optionCanTakeCover);
            menuJuggernaut.Add(optionCanSneak);
            optionEquipJuggernautSuit.Activated += (sender, e) => TryToEquipJuggernautSuit(playerPed);
            optionUnequipJuggernautSuit.Activated += (sender, e) => TryToUnequipJuggernautSuit(playerPed);
            //  Better way of declaring the variable 2 lines below.
            //JuggernautSuit itemJuggernautSuit;
            //  Commenting this out for now because it crashes the script.
            /*JuggernautSuit itemJuggernautSuit = new JuggernautSuit();
            Inventory.Add(itemJuggernautSuit);*/
        }
        protected override void OnUpdate(object sender, EventArgs e)
        {
            //  This runs the UI stuff created within the class.
            pool.Process();
            if (isWearingJuggernautSuit)
            {
                Player player = Game.Player;
                Ped playerPed = Game.Player.Character;
                WeaponCollection weapon = Game.Player.Character.Weapons;
                Weapon minigun = weapon[WeaponHash.Minigun];
                Weapon grenadeLauncher = weapon[WeaponHash.GrenadeLauncher];
                Weapon pipeBomb = weapon[WeaponHash.PipeBomb];
                Function.Call(Hash.SET_PED_RESET_FLAG, playerPed, 200, true);
                Function.Call(Hash.CLEAR_PED_BLOOD_DAMAGE, playerPed);
                if (!optionCanJump.Checked) { Game.DisableControlThisFrame(Control.Jump); }
                if (!optionCanEnterVehicles.Checked) { Game.DisableControlThisFrame(Control.Enter); }
                if (!optionCanTakeCover.Checked) { Game.DisableControlThisFrame(Control.Cover); }
                if (!optionCanSneak.Checked) { Game.DisableControlThisFrame(Control.Duck); }
                if (optionOnlyMinigun.Checked)
                {
                    Game.DisableControlThisFrame(Control.SelectWeapon);
                }
                if (optionRegenerationHealth.Checked && playerPed.Health < 2000)
                {
                    if (regenerationCooldownHealth > 0)
                    {
                        regenerationCooldownHealth--;
                    }
                    else
                    {
                        playerPed.Health++;
                        regenerationCooldownHealth = 15;
                    }
                }
                if (optionRegenerationArmor.Checked && playerPed.Armor < 200)
                {
                    if (regenerationCooldownArmor > 0)
                    {
                        regenerationCooldownArmor--;
                    }
                    else
                    {
                        playerPed.Armor++;
                        regenerationCooldownArmor = 15;
                    }
                }
                if (optionAmmoRegenerationMinigun.Checked && weapon.HasWeapon(WeaponHash.Minigun) && minigun.Ammo < 9999)
                {
                    if (ammoRegenerationCooldownMinigun > 0)
                    {
                        ammoRegenerationCooldownMinigun--;
                    }
                    else
                    {
                        minigun.Ammo++;
                        ammoRegenerationCooldownMinigun = 15;
                    }
                }
                if (optionAmmoRegenerationGrenadeLauncher.Checked && weapon.HasWeapon(WeaponHash.GrenadeLauncher) && grenadeLauncher.Ammo < 9999)
                {
                    if (ammoRegenerationCooldownGrenadeLauncher > 0)
                    {
                        ammoRegenerationCooldownGrenadeLauncher--;
                    }
                    else
                    {
                        grenadeLauncher.Ammo += 1;
                        ammoRegenerationCooldownGrenadeLauncher = 1800;
                    }
                }
                if (optionAmmoRegenerationPipeBomb.Checked && weapon.HasWeapon(WeaponHash.PipeBomb) && pipeBomb.Ammo < 10)
                {
                    if (ammoRegenerationCooldownPipeBomb > 0)
                    {
                        ammoRegenerationCooldownPipeBomb--;
                    }
                    else
                    {
                        pipeBomb.Ammo += 1;
                        ammoRegenerationCooldownPipeBomb = 600;
                    }
                }
                if (optionAmmoRegenerationPipeBomb.Checked && !weapon.HasWeapon(WeaponHash.PipeBomb))
                {
                    weapon.Give(WeaponHash.PipeBomb, 1, false, true);
                }
                //  I could probably simplify these next 2 conditions..
                if (optionInfiniteAmmoMinigun.Checked)
                {
                    if (weapon.HasWeapon(WeaponHash.Minigun))
                    {
                        minigun.InfiniteAmmo = true;
                    }
                }
                if (!optionInfiniteAmmoMinigun.Checked)
                {
                    if (weapon.HasWeapon(WeaponHash.Minigun))
                    {
                        minigun.InfiniteAmmo = false;
                    }
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
                menuJuggernaut.Visible = true;
            }
        }
        public virtual bool CanEquipJuggernautSuit(Ped ped)
        {
            return true;
        }
        protected void TryToEquipJuggernautSuit(Ped ped)
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
            else
            {
                //  Nothing.
            }
        }
        protected void TryToUnequipJuggernautSuit(Ped ped)
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            if (isWearingJuggernautSuit)
            {
                UnequipJuggernautSuit(playerPed);
            }
            else
            {
                //  Nothing.
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
        public static void EquipJuggernautSuit(Ped ped)
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            WeaponCollection weapon = Game.Player.Character.Weapons;
            Weapon minigun = weapon.HasWeapon(WeaponHash.Minigun) ? weapon[WeaponHash.Minigun] : weapon.Give(WeaponHash.Minigun, 0, true, true);
            minigun.Ammo += 5000;
            Weapon grenadeLauncher = weapon.HasWeapon(WeaponHash.GrenadeLauncher) ? weapon[WeaponHash.GrenadeLauncher] : weapon.Give(WeaponHash.GrenadeLauncher, 0, true, true);
            grenadeLauncher.Ammo += 10;
            Weapon combatMG = weapon.HasWeapon(WeaponHash.CombatMG) ? weapon[WeaponHash.CombatMG] : weapon.Give(WeaponHash.CombatMG, 0, true, true);
            combatMG.Ammo += 400;
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
                //Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 2, 57, 0, 0); //Hair
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
                //Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 2, 49, 0, 0); //Hair
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
            Weapon grenadeLauncher = weapon[WeaponHash.GrenadeLauncher];
            Weapon pipeBomb = weapon[WeaponHash.PipeBomb];
            Weapon combatMG = weapon[WeaponHash.CombatMG];
            if (weapon.HasWeapon(WeaponHash.Minigun))
            {
                minigun.Ammo = 0;
                weapon.Remove(minigun);
            }
            if (weapon.HasWeapon(WeaponHash.GrenadeLauncher))
            {
                grenadeLauncher.Ammo = 0;
                weapon.Remove(grenadeLauncher);
            }
            if (weapon.HasWeapon(WeaponHash.PipeBomb))
            {
                pipeBomb.Ammo = 0;
                //  This line will probably throw an error..
                //weapon.Remove(pipeBomb);
            }
            if (weapon.HasWeapon(WeaponHash.CombatMG))
            {
                combatMG.Ammo = 0;
                weapon.Remove(combatMG);
            }
            isWearingJuggernautSuit = false;
            playerPed.MaxHealth = 200;
            playerPed.Health = 200;
            //player.MaxArmor = 100;
            playerPed.Armor = 100;
            Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, playerPed, 1.0f);
            Function.Call(Hash.RESET_PED_STRAFE_CLIPSET, playerPed);
            Function.Call(Hash.SET_WEAPON_ANIMATION_OVERRIDE, playerPed, 0x0);

            //  Setting Components
            var isMichael = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("player_zero"));
            var isFranklin = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("player_one"));
            var isTrevor = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("player_two"));
            var isGunman = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("hc_gunman"));
            var isMPMale = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("mp_m_freemode_01"));
            var isMPFemale = Function.Call<bool>(Hash.IS_PED_MODEL, playerPed, Game.GenerateHash("mp_f_freemode_01"));
            if (isMichael)
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 26, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 18, 0, 0); //Lower
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 0, 0, 0); //Hands
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 1, 0, 0); //Shoes
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, 0, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, 0, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 0, 0, 0); //Bedges
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 0); //Hats
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 1); //Glasses
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 3); //Misc
            }
            if (isFranklin)
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 26, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 18, 0, 0); //Lower
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 0, 0, 0); //Hands
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 5, 0, 0); //Shoes
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, 0, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, 0, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 0, 0, 0); //Bedges
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 11, 0, 0, 0); //Shirt Overlay
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 0); //Hats
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 1); //Glasses
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 3); //Misc
            }
            if (isTrevor)
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 16, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 26, 0, 0); //Lower
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 0, 0, 0); //Hands
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 17, 0, 0); //Shoes
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, 0, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, 0, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 0, 0, 0); //Bedges
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 0); //Hats
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 1); //Glasses
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 3); //Misc
            }
            if (isGunman)
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 1, 0, 0, 0); //Beard
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 2, 0, 0, 0); //Hair
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 0, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 0, 0, 0); //Lower
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 0, 0, 0); //Shoes
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, 0, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, 0, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 0, 0, 0); //Bedges
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 0); //Hats
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 1); //Glasses
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 3); //Misc
            }
            if (isMPMale)
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 0, 0, 0, 0); //Head
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 1, 0, 0, 0); //Beard
                //Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 2, 0, 0, 0); //Hair
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 15, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 61, 0, 0); //Lower
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 0, 0, 0); //Hands
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 34, 0, 0); //Shoes
                //Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 7, 0, 1, 0); //Teeth
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, -1, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, -1, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 0, 0, 0); //Bedges
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 11, -1, 0, 0); //Shirt Overlay
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 0); //Hats
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 1); //Glasses
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 2); //Misc
            }
            if (isMPFemale)
            {
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 0, 0, 0, 0); //Head
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 1, 0, 0, 0); //Beard
                //Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 2, 0, 0, 0); //Hair
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 3, 15, 0, 0); //Upper
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 4, 62, 0, 0); //Lower
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 5, 0, 0, 0); //Hands
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 6, 35, 0, 0); //Shoes
                //Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 7, 0, 1, 0); //Teeth
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 8, -1, 0, 0); //Accessory 0
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 9, -1, 0, 0); //Accessory 1
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 10, 0, 0, 0); //Bedges
                Function.Call(Hash.SET_PED_COMPONENT_VARIATION, playerPed, 11, 5, 0, 0); //Shirt Overlay
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 0); //Hats
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 1); //Glasses
                Function.Call(Hash.CLEAR_ALL_PED_PROPS, playerPed, 2); //Misc
            }
        }
        protected override void OnAbort(object sender, EventArgs e)
        {
            Player player = Game.Player;
            Ped playerPed = Game.Player.Character;
            UnequipJuggernautSuit(playerPed);
        }
    }
    public class JuggernautSuit : StackableItem
    {
        public override string Name => "Juggernaut Suit";
        public override ScaledTexture Icon => new ScaledTexture("", "");
        public JuggernautSuit()
        {
            Used += OnUseItem;
        }
        private void OnUseItem(object sender, EventArgs e)
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
