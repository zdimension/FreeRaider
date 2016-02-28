using System.Collections.Generic;

namespace FreeRaider
{
    public enum ITEM
    {
        Compass = 1, // Aka Watch in TR2-3, Timex in TR5
        Passport = 2, // Exists only in TR1-3, not used in TR4 (diary)
        Larahome = 3,
        Video = 4, // Video settings. Exists only in TR1-3.
        Audio = 5, // Audio settings. Exists only in TR1-3.
        Controls = 6, // Control settings. Exists only in TR1-3.
        Load = 7, // Load game. Exists only in TR4-5.
        Save = 8, // Save game. Exists only in TR4-5.
        Map = 9, // Map item only existed in TR1-3, not used.

        Pistols = 10, // Exists in all game versions
        Shotgun = 11, // Exists in all game versions
        Magnums = 12, // Aka Automags in TR2, Desert Eagle in TR3/5, Revolver in TR4/5.
        Uzis = 13, // Exists in all game versions
        M16 = 14, // Exists since TR2, aka MP5 in TR3, aka H&K in TR5
        Grenadegun = 15, // Exists since TR2, through all game versions
        Rocketgun = 16, // Exists in TR3 only
        Harpoongun = 17, // Exists in TR2-3 only
        Crossbow = 18, // Exists since TR4, aka Grappling Gun in TR5.

        Lasersight = 20,
        Binoculars = 21,
        Silencer = 22, // Exists only in TR5, not used.

        PistolAmmo = 30,
        ShotgunNormalAmmo = 31,
        ShotgunWideshotAmmo = 32,
        MagnumAmmo = 33,
        UziAmmo = 34,
        M16Ammo = 35,
        GrenadegunNormalAmmo = 36,
        GrenadegunSuperAmmo = 37,
        GrenadegunFlashAmmo = 38,
        RocketgunAmmo = 39,
        HarpoongunAmmo = 40,
        CrossbowNormalAmmo = 41,
        CrossbowPoisonAmmo = 42,
        CrossbowExplosiveAmmo = 43,

        Flares = 45,
        SingleFlare = 46,
        Torch = 47,

        SmallMedipack = 50,
        LargeMedipack = 51,

        Secret1 = 120,
        Secret2 = 121,
        Secret3 = 122
    }

    public enum MenuItemType
    {
        System,
        Supply,
        Quest,
        Invalid
    }

    public partial class StaticFuncs
    {
        public static MenuItemType NextItemType(MenuItemType t)
        {
            switch (t)
            {
                case MenuItemType.System:
                    return MenuItemType.Supply;
                case MenuItemType.Supply:
                    return MenuItemType.Quest;
                default:
                    return MenuItemType.Invalid;
            }
        }

        public static MenuItemType PreviousItemType(MenuItemType t)
        {
            switch (t)
            {
                case MenuItemType.Supply:
                    return MenuItemType.System;
                case MenuItemType.Quest:
                    return MenuItemType.Supply;
                default:
                    return MenuItemType.Invalid;
            }
        }
    }

    public class InventoryManager
    {
        public enum InventoryState
        {
            Disabled = 0,
            Idle,
            Open,
            Closed,
            RLeft,
            RRight,
            Up,
            Down,
            Activate
        }

        
        private List<InventoryNode> inventory = new List<InventoryNode>();

        private int nextItemsCount;

        private int currentItemsCount;

        private int itemsOffset;

        private float ringRotatePeriod;
        private float ringTime;

        private float ringAngle;

        private float ringVerticalAngle;

        private float ringAngleStep;

        private float baseRingRadius;

        private float ringRadius;

        private float verticalOffset;

        private float itemRotatePeriod;

        private float itemTime;

        private float itemAngle;

        private int getItemsTypeCount(MenuItemType type);

        private void restoreItemAngle(float time);

        public TextLine LabelTitle;

        public TextLine LabelItemName;

        public InventoryManager();

        ~InventoryManager();

        public InventoryState CurrentState { get; }

        public InventoryState NextState { get; set; }

        public MenuItemType ItemsType { get; }

        public void SetInventory(List<InventoryNode> i);

        public void SetTitle(MenuItemType itemsType);

        public void Frame(float time);

        public void Render();
    }

    public partial class Global
    {
        public static InventoryManager MainInventoryManager;
    }
}
