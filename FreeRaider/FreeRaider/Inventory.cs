using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static FreeRaider.Global;
using static FreeRaider.Strings;

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

    public class InventoryManager : IDisposable
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

        private MenuItemType currentItemsType;

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
            return inventory.Count(i => EngineWorld.GetBaseItemByID(i.ID)?.Type == type);
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
            currentItemsType = MenuItemType.System;
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

            LabelTitle = new TextLine();
            LabelTitle.X = 0.0f;
            LabelTitle.Y = 30.0f;
            LabelTitle.Xanchor = HorizontalAnchor.Center;
            LabelTitle.Yanchor = VerticalAnchor.Top;

            LabelTitle.FontID = FontType.Primary;
            LabelTitle.StyleID = FontStyle.MenuTitle;
            LabelTitle.Show = false;

            LabelItemName = new TextLine();
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

        public void Dispose()
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

        public void SetInventory(List<InventoryNode> i)
        {
            inventory = i;
            CurrentState = InventoryState.Disabled;
            NextState = InventoryState.Disabled;
        }

        public void SetTitle(MenuItemType itemsType)
        {
            var stringIndex = 0;

            switch (itemsType)
            {
                case MenuItemType.System:
                    stringIndex = STR_GEN_OPTIONS_TITLE;
                    break;

                case MenuItemType.Quest:
                    stringIndex = STR_GEN_ITEMS;
                    break;

                case MenuItemType.Supply:
                default:
                    stringIndex = STR_GEN_INVENTORY;
                    break;
            }

            LabelTitle.Text = EngineLua.GetString(stringIndex);
        }

        public MenuItemType SetItemsType(MenuItemType type)
        {
            if(inventory == null || inventory.Count == 0)
            {
                currentItemsType = type;
                return type;
            }

            var count = getItemsTypeCount(type);
            if(count == 0)
            {
                foreach (var i in inventory)
                {
                    var bi = EngineWorld.GetBaseItemByID(i.ID);
                    if(bi != null)
                    {
                        type = bi.Type;
                        count = getItemsTypeCount(currentItemsType);
                        break;
                    }
                }
            }

            if(count > 0)
            {
                currentItemsCount = count;
                currentItemsType = type;
                ringAngleStep = 360.0f / currentItemsCount;
                itemsOffset %= count;
                ringTime = 0.0f;
                ringAngle = 0.0f;
                return type;
            }

            return MenuItemType.Invalid;
        }

        public void Frame(float time)
        {
            if(inventory == null || inventory.Count == 0)
            {
                CurrentState = InventoryState.Disabled;
                NextState = InventoryState.Disabled;
                return;
            }

            switch(CurrentState)
            {
                case InventoryState.RLeft:
                    ringTime += time;
                    ringAngle = ringAngleStep * ringTime / ringRotatePeriod;
                    NextState = InventoryState.RLeft;
                    if(ringTime >= ringRotatePeriod)
                    {
                        ringTime = 0.0f;
                        ringAngle = 0.0f;
                        NextState = InventoryState.Idle;
                        CurrentState = InventoryState.Idle;
                        itemsOffset--;
                        if(itemsOffset < 0)
                        {
                            itemsOffset = currentItemsCount - 1;
                        }
                    }
                    restoreItemAngle(time);
                    break;

                case InventoryState.RRight:
                    ringTime += time;
                    ringAngle = -ringAngleStep * ringTime / ringRotatePeriod;
                    NextState = InventoryState.RRight;
                    if (ringTime >= ringRotatePeriod)
                    {
                        ringTime = 0.0f;
                        ringAngle = 0.0f;
                        NextState = InventoryState.Idle;
                        CurrentState = InventoryState.Idle;
                        itemsOffset++;
                        if (itemsOffset >= currentItemsCount)
                        {
                            itemsOffset = 0;
                        }
                    }
                    restoreItemAngle(time);
                    break;

                case InventoryState.Idle:
                    ringTime = 0.0f;
                    switch(NextState)
                    {
                        default:
                        case InventoryState.Idle:
                            itemTime += time;
                            itemAngle = 360.0f * itemTime / itemRotatePeriod;
                            if(itemTime >= itemRotatePeriod)
                            {
                                itemTime = 0.0f;
                                itemAngle = 0.0f;
                            }
                            LabelItemName.Show = true;
                            LabelTitle.Show = true;
                            break;

                        case InventoryState.Closed:
                            Audio.Send((uint) EngineLua.GetGlobalSound((int) TR_AUDIO_SOUND_GLOBALID.MenuClose));
                            LabelItemName.Show = false;
                            LabelTitle.Show = false;
                            CurrentState = NextState;
                            break;

                        case InventoryState.RLeft:
                        case InventoryState.RRight:
                            Audio.Send((uint) TR_AUDIO_SOUND.MenuRotate);
                            LabelItemName.Show = false;
                            CurrentState = NextState;
                            itemTime = 0.0f;
                            break;

                        case InventoryState.Up:
                            nextItemsCount = getItemsTypeCount(StaticFuncs.NextItemType(currentItemsType));
                            if(nextItemsCount > 0)
                            {
                                CurrentState = NextState;
                                ringTime = 0.0f;
                            }
                            else
                            {
                                NextState = InventoryState.Idle;
                            }
                            LabelItemName.Show = false;
                            LabelTitle.Show = false;
                            break;

                        case InventoryState.Down:
                            nextItemsCount = getItemsTypeCount(StaticFuncs.PreviousItemType(currentItemsType));
                            if (nextItemsCount > 0)
                            {
                                CurrentState = NextState;
                                ringTime = 0.0f;
                            }
                            else
                            {
                                NextState = InventoryState.Idle;
                            }
                            LabelItemName.Show = false;
                            LabelTitle.Show = false;
                            break;
                    }
                    break;

                case InventoryState.Disabled:
                    if(NextState == InventoryState.Open)
                    {
                        if(SetItemsType(currentItemsType) != MenuItemType.Invalid)
                        {
                            Audio.Send((uint) EngineLua.GetGlobalSound((int) TR_AUDIO_SOUND_GLOBALID.MenuOpen));
                            CurrentState = InventoryState.Open;
                            ringAngle = 180.0f;
                            ringVerticalAngle = 180.0f;
                        }
                    }
                    break;

                case InventoryState.Up:
                    CurrentState = InventoryState.Up;
                    NextState = InventoryState.Up;
                    ringTime += time;
                    if(ringTime < ringRotatePeriod)
                    {
                        restoreItemAngle(time);
                        ringRadius = baseRingRadius * (ringRotatePeriod - ringTime) / ringRotatePeriod;
                        verticalOffset = -baseRingRadius * ringTime / ringRotatePeriod;
                        ringAngle += 180.0f * time / ringRotatePeriod;
                    }
                    else if(ringTime < 2.0f * ringRotatePeriod)
                    {
                        if(ringTime - time <= ringRotatePeriod)
                        {
                            ringRadius = 0.0f;
                            verticalOffset = baseRingRadius;
                            ringAngleStep = 360.0f / nextItemsCount;
                            ringAngle = 180.0f;
                            currentItemsType = StaticFuncs.NextItemType(currentItemsType);
                            currentItemsCount = nextItemsCount;
                            itemsOffset = 0;
                            SetTitle(currentItemsType);
                        }
                        ringRadius = baseRingRadius * (ringTime - ringRotatePeriod) / ringRotatePeriod;
                        verticalOffset -= baseRingRadius * time / ringRotatePeriod;
                        ringAngle -= 180.0f * time / ringRotatePeriod;
                    }
                    else
                    {
                        NextState = InventoryState.Idle;
                        CurrentState = InventoryState.Idle;
                        ringAngle = 0.0f;
                        verticalOffset = 0.0f;
                    }
                    break;

                case InventoryState.Down:
                    CurrentState = InventoryState.Down;
                    NextState = InventoryState.Down;
                    ringTime += time;
                    if (ringTime < ringRotatePeriod)
                    {
                        restoreItemAngle(time);
                        ringRadius = baseRingRadius * (ringRotatePeriod - ringTime) / ringRotatePeriod;
                        verticalOffset = baseRingRadius * ringTime / ringRotatePeriod;
                        ringAngle += 180.0f * time / ringRotatePeriod;
                    }
                    else if (ringTime < 2.0f * ringRotatePeriod)
                    {
                        if (ringTime - time <= ringRotatePeriod)
                        {
                            ringRadius = 0.0f;
                            verticalOffset = -baseRingRadius;
                            ringAngleStep = 360.0f / nextItemsCount;
                            ringAngle = 180.0f;
                            currentItemsType = StaticFuncs.PreviousItemType(currentItemsType);
                            currentItemsCount = nextItemsCount;
                            itemsOffset = 0;
                            SetTitle(currentItemsType);
                        }
                        ringRadius = baseRingRadius * (ringTime - ringRotatePeriod) / ringRotatePeriod;
                        verticalOffset += baseRingRadius * time / ringRotatePeriod;
                        ringAngle -= 180.0f * time / ringRotatePeriod;
                    }
                    else
                    {
                        NextState = InventoryState.Idle;
                        CurrentState = InventoryState.Idle;
                        ringAngle = 0.0f;
                        verticalOffset = 0.0f;
                    }
                    break;

                case InventoryState.Open:
                    ringTime += time;
                    ringRadius = baseRingRadius * ringTime / ringRotatePeriod;
                    ringAngle -= 180.0f * time / ringRotatePeriod;
                    ringVerticalAngle -= 180.0f * time / ringRotatePeriod;
                    if(ringTime >= ringRotatePeriod)
                    {
                        CurrentState = InventoryState.Idle;
                        CurrentState = InventoryState.Idle;
                        ringVerticalAngle = 0.0f;

                        ringRadius = baseRingRadius;
                        ringTime = 0.0f;
                        ringAngle = 0.0f;
                        verticalOffset = 0.0f;
                        SetTitle(MenuItemType.Supply);
                    }
                    break;

                case InventoryState.Closed:
                    ringTime += time;
                    ringRadius = baseRingRadius * (ringRotatePeriod - ringTime) / ringRotatePeriod;
                    ringAngle += 180.0f * time / ringRotatePeriod;
                    ringVerticalAngle += 180.0f * time / ringRotatePeriod;
                    if (ringTime >= ringRotatePeriod)
                    {
                        CurrentState = InventoryState.Disabled;
                        CurrentState = InventoryState.Disabled;
                        ringVerticalAngle = 0.0f;
                        ringTime = 0.0f;
                        LabelTitle.Show = false;
                        ringRadius = baseRingRadius;
                        currentItemsType = MenuItemType.Supply;
                    }
                    break;
            }
        }

        public void Render()
        {
            if(CurrentState != InventoryState.Disabled && inventory != null && inventory.Count > 0 && Global.FontManager != null)
            {
                var num = 0;
                foreach (var i in inventory)
                {
                    var bi = EngineWorld.GetBaseItemByID(i.ID);
                    if(bi == null || bi.Type != currentItemsType)
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
                                var counter = EngineLua.GetString(STR_GEN_MASK_INVHEADER);
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
