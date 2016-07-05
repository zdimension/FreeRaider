using System;
using static FreeRaider.Constants;
using static FreeRaider.Global;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int GF_MAX_ACTIONS = 32;

        public const int GF_MAX_SECRETS = 256;
    }

    public struct GameflowAction
    {
        public GF_OP Opcode;

        public byte Operand;
    }

    public class Gameflow
    {
        /// <summary>
        /// Initialisation, sets all actions to NoEntry (-1)
        /// </summary>
        public void Init()
        {
            for (var i = 0; i < actions.Length; i++)
            {
                actions[i].Opcode = GF_OP.NoEntry;
            }
        }

        /// <summary>
        /// Process next gameflow action from action list if nextAction true
        /// </summary>
        public void Do()
        {
            if(!nextAction)
            {
                return;
            }

            var completed = true;

            for(var i = 0; i < GF_MAX_ACTIONS; i++)
            {
                completed = false;

                switch(actions[i].Opcode)
                {
                    case GF_OP.LevelComplete:
                        // Switch level only when fade is complete AND all streams / sounds are unloaded!
                        if(Gui.FadeCheck(FaderType.LoadScreen) == FaderStatus.Complete
#if !NO_AUDIO
                            && !Audio.IsTrackPlaying()
#endif
                            )
                        {
                            var t = EngineLua.Call("getNextLevel", GameID, LevelID, actions[i].Operand);
                            CurrentLevelPath = (string)t[0];
                            currentLevelName = (string) t[1];
                            LevelID = Convert.ToUInt32(t[2]);
                            Engine.LoadMap(CurrentLevelPath);
                            actions[i].Opcode = GF_OP.NoEntry;
                        }
                        else
                        {
                            // FIXME Gameflow has NOTHING to do with faders! this should all be done elsewhere!
                            // If fadeout is in the process, we block level loading until it is complete.
                            // It is achieved by not resetting action marker and exiting the function instead.
                            continue;
                        }
                        break;

                    default:
                        actions[i].Opcode = GF_OP.NoEntry;
                        break;
                }
            }

            if (completed) nextAction = false; // Reset action marker!
        }

        /// <summary>
        /// Send opcode and operand to gameflow manager.
        /// </summary>
        public bool Send(GF_OP opcode, int operand = -1)
        {
            for (var i = 0; i < GF_MAX_ACTIONS; i++)
            {
                if(actions[i].Opcode == GF_OP.NoEntry)
                {
                    actions[i].Opcode = opcode;
                    actions[i].Operand = (byte) operand;
                    nextAction = true;
                    return true;
                }
            }
            return false;
        }

        public bool[] SecretsTriggerMap = new bool[GF_MAX_SECRETS + 1];

        public string CurrentLevelPath { get; set; }

        public byte GameID { get; set; }

        public uint LevelID { get; set; }


        private string currentLevelName;

        private bool nextAction;

        private GameflowAction[] actions = new GameflowAction[GF_MAX_ACTIONS + 1];
    }

    public enum GF_OP : sbyte
    {
        NoEntry = -1,
        Picture, // Unknown possibly TR1?
        ListStart, // Unknown possibly TR1?
        ListEnd, // Unknown possibly TR1?
        StartFmv, // Start a FMV
        StartLevel, // Start a level
        StartCine, // Start a cutscene
        LevelComplete, // Trigger level completion display
        StartDemo, // Start a demo level
        JumpToSequence, // Jump to an existing sequence
        EndSequence, // End current sequence
        SetTrack, // Set audio track
        EnableSunset, // ??? Used on Bartoli's hideout!
        LoadingPic, // Set loading screen picture
        DeadlyWater, // Set water kills lara (Used on that Rig level, Temple of Xian etc..)
        RemoveWeapons, // Remove Lara's weapons
        GameComplete, // Trigger game completion display
        CutAngle, // Cutscene start angle? Possibly rotation flags? Unknown!
        NoFloor, // Makes Lara infinitely fall at the bottom of the level
        AddToInventory, // Add an item to inventory
        LaraStartAnim, // Change Lara's start anim or the state? (Used on levels where Lara starts in water)
        NumSecrets, // Change the number of secrets?
        KillToComplete,
        // Kill to complete, used on levels like IcePalace, Nightmare in Vegas so killing the boss ends the level!
        RemoveAmmo, // Remove Ammo
        LastIndex
    }

    public partial class Global
    {
        public static Gameflow GameflowManager;
    }
}
