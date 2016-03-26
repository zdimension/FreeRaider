using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

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

        private int getItemsTypeCount(MenuItemType type)
        {
            return inventory.Count(i => Global.EngineWorld.GetBaseItemByID(i.ID)?.Type == type);
        }

        private void restoreItemAngle(float time)
        {
            if(itemAngle > 0.0f)
            {
                if(itemAngle <= 180.0f)
                {
                    itemAngle -= 180.0f * time / ringRotatePeriod;
                    if (itemAngle < 0.0f) itemAngle = 0.0f;
                }
                else
                {
                    itemAngle += 180.0f * time / ringRotatePeriod;
                    if (itemAngle >= 360.0f) itemAngle = 0.0f;
                }
            }
        }

        public TextLine LabelTitle;

        public TextLine LabelItemName;

        public InventoryManager()
        {
            CurrentState = InventoryState.Disabled;
            NextState = InventoryState.Disabled;
            ItemsType = MenuItemType.System;
            currentItemsCount = 0;
            itemsOffset = 0;
            nextItemsCount = 0;

            ringRotatePeriod = 0.5f;
            ringTime = 0.0f;
            ringAngle = 0.0f;
            ringVerticalAngle = 0.0f;
            ringAngleStep = 0.0f;
            baseRingRadius = 600.0f;
            ringRadius = 600.0f;
            verticalOffset = 0.0f;

            itemRotatePeriod = 4.0f;
            itemTime = 0.0f;
            itemAngle = 0.0f;

            inventory = new List<InventoryNode>();

            LabelTitle.X = 0.0f;
            LabelTitle.Y = 30.0f;
            LabelTitle.Xanchor = HorizontalAnchor.Center;
            LabelTitle.Yanchor = VerticalAnchor.Top;

            LabelTitle.FontID = FontType.Primary;
            LabelTitle.StyleID = FontStyle.MenuTitle;
            LabelTitle.Show = false;

            LabelItemName.X = 0.0f;
            LabelItemName.Y = 50.0f;
            LabelItemName.Xanchor = HorizontalAnchor.Center;
            LabelItemName.Yanchor = VerticalAnchor.Bottom;
                 
            LabelItemName.FontID = FontType.Primary;
            LabelItemName.StyleID = FontStyle.MenuContent;
            LabelItemName.Show = false;

            Gui.AddLine(LabelItemName);
            Gui.AddLine(LabelTitle);
        }

        ~InventoryManager()
        {
            CurrentState = InventoryState.Disabled;
            NextState = InventoryState.Disabled;
            inventory = new List<InventoryNode>();

            LabelItemName.Show = false;
            Gui.DeleteLine(LabelItemName);

            LabelTitle.Show = false;
            Gui.DeleteLine(LabelTitle);
        }

        public InventoryState CurrentState { get; private set; }

        public InventoryState NextState { get; set; }

        public MenuItemType ItemsType { get; }

        public void SetInventory(List<InventoryNode> i)
        {
            inventory = i.ToList();
            CurrentState = InventoryState.Disabled;
            NextState = InventoryState.Disabled;
        }

        public void SetTitle(MenuItemType itemsType)
        {
            var stringIndex = 0;

            switch (itemsType)
            {
                case MenuItemType.System:
                    stringIndex = Strings.STR_GEN_OPTIONS_TITLE;
                    break;

                case MenuItemType.Quest:
                    stringIndex = Strings.STR_GEN_ITEMS;
                    break;

                case MenuItemType.Supply:
                default:
                    stringIndex = Strings.STR_GEN_INVENTORY;
                    break;
            }

            LabelTitle.Text = Global.EngineLua.GetString(stringIndex);
        }

        public void Frame(float time);

        public void Render()
        {
            if(CurrentState != InventoryState.Disabled && inventory != null && inventory.Count > 0 && Global.FontManager != null)
            {
                var num = 0;
                foreach (var i in inventory)
                {
                    var bi = Global.EngineWorld.GetBaseItemByID(i.ID);
                    if(bi == null || bi.Type != ItemsType)
                    {
                        continue;
                    }

                    var matrix = new Transform();
                    matrix.SetIdentity();
                    VMath.Mat4_Translate(matrix, 0.0f, 0.0f, -baseRingRadius * 2.0f);
                    //VMath.Mat4_RotateX(matrix, 25.0f);
                    VMath.Mat4_RotateX(matrix, 25.0f + ringVerticalAngle);
                    var ang = ringAngleStep * (-itemsOffset + num) + ringAngle;
                    VMath.Mat4_RotateY(matrix, ang);
                    VMath.Mat4_Translate(matrix, 0.0f, verticalOffset, ringRadius);
                    VMath.Mat4_RotateX(matrix, -90.0f);
                    VMath.Mat4_RotateZ(matrix, 90.0f);
                    if(num == itemsOffset)
                    {
                        if(bi.Name[0] != 0)
                        {
                            LabelItemName.Text = bi.Name;

                            if(i.Count > 1)
                            {
                                var counter = Global.EngineLua.GetString(Strings.STR_GEN_MASK_INVHEADER);
                                LabelItemName.Text = Helper.Format(counter, bi.Name, i.Count);
                            }
                        }
                        VMath.Mat4_RotateZ(matrix, 90.0f + itemAngle - ang);
                        Gui.Item_Frame(bi.BoneFrame, 0.0f); // here will be time != 0 for using items animation
                    }
                    else
                    {
                        VMath.Mat4_RotateZ(matrix, 90.0f - ang);
                        Gui.Item_Frame(bi.BoneFrame, 0.0f);
                    }
                    VMath.Mat4_Translate(matrix, -0.5f * bi.BoneFrame.Centre);
                    VMath.Mat4_Scale(matrix, 0.7f, 0.7f, 0.7f);
                    Gui.RenderItem(bi.BoneFrame, 0.0f, matrix);

                    num++;
                }
            }
        }
    }

    public partial class Global
    {
        public static InventoryManager MainInventoryManager;
    }
}
