using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeRaider
{
    public class Strings
    {
        public const int STR_GEN_NEWGAME = 0;
        public const int STR_GEN_SELECTGAME = 1;
        public const int STR_GEN_SELECTLEVEL = 2;
        public const int STR_GEN_LARAHOME = 3;
        public const int STR_GEN_SAVEGAME = 4;
        public const int STR_GEN_LOADGAME = 5;
        public const int STR_GEN_OPTIONS = 6;
        public const int STR_GEN_QUIT = 7;
        public const int STR_GEN_RESTARTLEVEL = 8;
        public const int STR_GEN_EXITTOTITLE = 9;
        public const int STR_GEN_YES = 10;
        public const int STR_GEN_NO = 11;
        public const int STR_GEN_APPLY = 12;
        public const int STR_GEN_CANCEL = 13;
        public const int STR_GEN_PREVIOUS = 14;
        public const int STR_GEN_NEXT = 15;
        public const int STR_GEN_OK = 16;
        public const int STR_GEN_DISCARD = 17;
        public const int STR_GEN_INVENTORY = 18;
        public const int STR_GEN_ITEMS = 19;
        public const int STR_GEN_PAUSED = 20;
        public const int STR_GEN_OPTIONS_TITLE = 21;
        public const int STR_GEN_STATISTICS = 22;
        public const int STR_GEN_EXITGAME = 23;
        public const int STR_GEN_SELECTTOLOAD = 24;
        public const int STR_GEN_SELECTTOSAVE = 25;
        public const int STR_GEN_SELECTTOCOMBINE = 26;
        public const int STR_GEN_EQUIP = 27;
        public const int STR_GEN_CHOOSEAMMO = 28;
        public const int STR_GEN_CHOOSEFIREMODE = 29;
        public const int STR_GEN_USE = 30;
        public const int STR_GEN_COMBINE = 31;
        public const int STR_GEN_SEPARATE = 32;
        public const int STR_GEN_EXAMINE = 33;
        public const int STR_GEN_THROWAWAY = 34;
        public const int STR_GEN_HINT_ACCEPT = 35;
        public const int STR_GEN_HINT_CANCEL = 36;
        public const int STR_GEN_HINT_TOUCHINV = 37;
        public const int STR_GEN_HINT_SCROLLINV = 38;
        public const int STR_GEN_HINT_EXAMINEINV = 39;

        public const int STR_GEN_STATS_LOC = 40;
        public const int STR_GEN_STATS_SECRETS = 41;
        public const int STR_GEN_STATS_DISTANCE = 42;
        public const int STR_GEN_STATS_AMMOUSED = 43;
        public const int STR_GEN_STATS_KILLS = 44;
        public const int STR_GEN_STATS_MEDIUSED = 45;

        public const int STR_GEN_TIP_1 = 46;
        public const int STR_GEN_TIP_2 = 47;
        public const int STR_GEN_TIP_3 = 48;
        public const int STR_GEN_TIP_4 = 49;

        public const int STR_GEN_PRESSTOSKIP = 50;

        public const int STR_GEN_MASK_INVHEADER = 90;
        public const int STR_GEN_MASK_AMMOHEADER = 91;
        public const int STR_GEN_MASK_TIMER = 92;
        public const int STR_GEN_MASK_TIMECOUNT = 93;

        public const int STR_GEN_GAMENAME_TR1 = 100;
        public const int STR_GEN_GAMENAME_TR1GOLD = 101;
        public const int STR_GEN_GAMENAME_TR2 = 200;
        public const int STR_GEN_GAMENAME_TR2GOLD = 201;
        public const int STR_GEN_GAMENAME_TR3 = 300;
        public const int STR_GEN_GAMENAME_TR3GOLD = 301;
        public const int STR_GEN_GAMENAME_TR4 = 400;
        public const int STR_GEN_GAMENAME_TR4TIMES = 401;
        public const int STR_GEN_GAMENAME_TR5 = 500;
        public const int STR_GEN_GAMENAME_CUSTOM = 600;

        // Generic system warnings enumeration

        public const int SYSWARN_ENTER_ENTITY_ID = 0;
        public const int SYSWARN_WRONG_ARGS = 1;
        public const int SYSWARN_WRONG_ARGS_COUNT = 2;
        public const int SYSWARN_NO_ENTITY = 3;
        public const int SYSWARN_WRONG_OPTION_INDEX = 4;
        public const int SYSWARN_NO_CHARACTER = 5;
        public const int SYSWARN_WRONG_ROOM = 6;
        public const int SYSWARN_MODELID_OVERFLOW = 7;
        public const int SYSWARN_WRONG_ACTION_NUMBER = 8;
        public const int SYSWARN_CANT_CREATE_FONT = 9;
        public const int SYSWARN_CANT_CREATE_STYLE = 10;
        public const int SYSWARN_CANT_REMOVE_FONT = 11;
        public const int SYSWARN_CANT_REMOVE_STYLE = 12;
        public const int SYSWARN_NO_SKELETAL_MODEL = 13;
        public const int SYSWARN_WRONG_ANIM_NUMBER = 14;
        public const int SYSWARN_WRONG_DISPATCH_NUMBER = 15;
        public const int SYSWARN_WRONG_FRAME_NUMBER = 16;
        public const int SYSWARN_WRONG_STREAM_ID = 17;
        public const int SYSWARN_WRONG_SOUND_ID = 18;
        public const int SYSWARN_AS_NOCHANNEL = 19;
        public const int SYSWARN_AS_NOSAMPLE = 20;
        public const int SYSWARN_AS_IGNORED = 21;
        public const int SYSWARN_AK_NOTPLAYED = 22;
        public const int SYSWARN_NOT_ACTIVE_FLIPMAP = 23;
        public const int SYSWARN_FILE_NOT_FOUND = 24;
        public const int SYSWARN_NOT_TRUECOLOR_IMG = 25;
        public const int SYSWARN_IMG_NOT_LOADED_SDL = 26;
        public const int SYSWARN_BAD_FRAME_OFFSET = 27;
        public const int SYSWARN_CANT_OPEN_FILE = 28;
        public const int SYSWARN_BAD_FILE_FORMAT = 29;
        public const int SYSWARN_INVALID_LINECOUNT = 30;
        public const int SYSWARN_WRONG_FLIPMAP_INDEX = 31;
        public const int SYSWARN_NO_HAIR_SETUP = 32;
        public const int SYSWARN_CANT_CREATE_HAIR = 33;
        public const int SYSWARN_CANT_RESET_HAIR = 34;
        public const int SYSWARN_NO_RAGDOLL_SETUP = 35;
        public const int SYSWARN_CANT_CREATE_RAGDOLL = 36;
        public const int SYSWARN_CANT_REMOVE_RAGDOLL = 37;
        public const int SYSWARN_WAD_OUT_OF_BOUNDS = 38;
        public const int SYSWARN_WAD_SEEK_FAILED = 39;
        public const int SYSWARN_TRACK_OUT_OF_BOUNDS = 40;
        public const int SYSWARN_TRACK_ALREADY_PLAYING = 41;
        public const int SYSWARN_TRACK_WRONG_INDEX = 42;
        public const int SYSWARN_NO_FREE_STREAM = 43;
        public const int SYSWARN_STREAM_LOAD_ERROR = 44;
        public const int SYSWARN_STREAM_PLAY_ERROR = 45;
        public const int SYSWARN_WRONG_SECTOR_INFO = 46;
        public const int SYSWARN_WRONG_BONE_NUMBER = 47;
        public const int SYSWARN_IMAGE_NOT_LOADED = 48;
        public const int SYSWARN_WRONG_MODEL_ID = 49;
        public const int SYSWARN_WRONG_ENTITY_OR_BODY = 50;
        public const int SYSWARN_CANT_APPLY_FORCE = 51;
        public const int SYSWARN_WRONG_AXIS = 52;

        public const int SYSNOTE_TRACK_OPENED = 1000;
        public const int SYSNOTE_READING_FILE = 1001;
        public const int SYSNOTE_GIVING_ITEM = 1002;
        public const int SYSNOTE_CHANGING_LEVEL = 1003;
        public const int SYSNOTE_CHANGING_GAME = 1004;
        public const int SYSNOTE_ENGINE_VERSION = 1005;
        public const int SYSNOTE_NUM_ROOMS = 1006;
        public const int SYSNOTE_NUM_TEXTURES = 1007;
        public const int SYSNOTE_CONSOLE_SPACING = 1008;
        public const int SYSNOTE_CONSOLE_LINECOUNT = 1009;
        public const int SYSNOTE_TRIGGER_INFO = 1010;
        public const int SYSNOTE_ACTIVATE_OBJECT = 1011;
        public const int SYSNOTE_LOADED_FADER = 1012;
        public const int SYSNOTE_TRIGGERS_CLEANED = 1013;
        public const int SYSNOTE_ENTFUNCS_CLEANED = 1014;
        public const int SYSNOTE_LOADED_PC_LEVEL = 1015;
        public const int SYSNOTE_LOADED_PSX_LEVEL = 1016;
        public const int SYSNOTE_LOADED_DC_LEVEL = 1017;
        public const int SYSNOTE_LOADED_OT_LEVEL = 1018;
        public const int SYSNOTE_WAD_PLAYING = 1019;
        public const int SYSNOTE_ENGINE_INITED = 1020;
        public const int SYSNOTE_LUA_STACK_INDEX = 1021;
        public const int SYSNOTE_LOADING_MAP = 1022;

        public const int SYSNOTE_COMMAND_HELP1 = 1023;
        public const int SYSNOTE_COMMAND_HELP2 = 1024;
        public const int SYSNOTE_COMMAND_HELP3 = 1025;
        public const int SYSNOTE_COMMAND_HELP4 = 1026;
        public const int SYSNOTE_COMMAND_HELP5 = 1027;
        public const int SYSNOTE_COMMAND_HELP6 = 1028;
        public const int SYSNOTE_COMMAND_HELP7 = 1029;
        public const int SYSNOTE_COMMAND_HELP8 = 1030;
        public const int SYSNOTE_COMMAND_HELP9 = 1031;
        public const int SYSNOTE_COMMAND_HELP10 = 1032;
        public const int SYSNOTE_COMMAND_HELP11 = 1033;
        public const int SYSNOTE_COMMAND_HELP12 = 1034;
        public const int SYSNOTE_COMMAND_HELP13 = 1035;
        public const int SYSNOTE_COMMAND_HELP14 = 1036;
        public const int SYSNOTE_COMMAND_HELP15 = 1037;
    }
}
