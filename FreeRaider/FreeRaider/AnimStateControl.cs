using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using OpenTK;
using static FreeRaider.Constants;

namespace FreeRaider
{
    public partial class Constants
    {
        public const float PENETRATION_TEST_OFFSET = 48.0f;

        ///@TODO: tune it!
        public const float WALK_FORWARD_OFFSET = 96.0f;

        ///@FIXME: find real offset
        public const float WALK_BACK_OFFSET = 16.0f;

        public const float WALK_FORWARD_STEP_UP = 256.0f; // by bone frame bb
        public const float RUN_FORWARD_OFFSET = 128.0f;

        ///@FIXME: find real offset
        public const float RUN_FORWARD_STEP_UP = 320.0f; // by bone frame bb

        public const float CRAWL_FORWARD_OFFSET = 256.0f;
        public const float LARA_HANG_WALL_DISTANCE = 128.0f - 24.0f;
        public const float LARA_HANG_VERTICAL_EPSILON = 64.0f;

        public const float LARA_HANG_VERTICAL_OFFSET = 12.0f;
        // in original is 0, in real life hands are little more higher than edge

        public const float LARA_TRY_HANG_WALL_OFFSET = 72.0f; // It works more stable than 32 or 128

        public const float LARA_HANG_SENSOR_Z = 800.0f;
        // It works more stable than 1024 (after collision critical fix, of course)
    }


    /// <summary>
    /// Animation control flags
    /// </summary>
    [Flags]
    public enum AnimControlFlags
    {
        NormalControl = 0,
        LoopLastFrame = 1,

        /// <summary>
        /// Animation will be locked once and for all.
        /// </summary>
        Lock = 2
    }

    /// <summary>
    /// Surface movement directions
    /// </summary>
    [Flags]
    public enum ENT_MOVE : byte
    {
        Stay = 0x00000000,
        MoveForward = 0x00000001,
        MoveBackward = 0x00000002,
        MoveLeft = 0x00000004,
        MoveRight = 0x00000008,
        MoveJump = 0x00000010,
        MoveCrouch = 0x00000020
    }

    /// <summary>
    /// Lara's animations
    /// </summary>
    public enum TR_ANIMATION
    {
        // TR1 AND ABOVE (0-159)
        LaraRun = 0,
        LaraWalkForward = 1,
        LaraEndWalkRight = 2,
        LaraEndWalkLeft = 3,
        LaraWalkToRunRight = 4,
        LaraWalkToRunLeft = 5,
        LaraStayToRun = 6,
        LaraRunToWalkRight = 7,
        LaraRunToStayLeft = 8,
        LaraRunToWalkLeft = 9,
        LaraRunToStayRight = 10,
        LaraStaySolid = 11, // intermediate animation used to reset flags and states.
        LaraTurnRightSlow = 12, // used once before the fast one if all weapon are in holsters
        LaraTurnLeftSlow = 13, // used once before the fast one if all weapon are in holsters
        LaraLandingForwardBoth = 14, // original landing animation in the tr1 betas... but removed
        LaraLandingForwardBothContinue = 15, // original landing animation in the tr1 betas... but removed
        LaraJumpingForwardRight = 16, // ok
        LaraStartFlyForwardRight = 17, // ok
        LaraJumpingForwardLeft = 18, // ok
        LaraStartFlyForwardLeft = 19, // ok
        LaraWalkForwardBegin = 20,
        LaraWalkForwardBeginContinue = 21,
        LaraStartFreeFall = 22,
        LaraFreeFallLong = 23,
        LaraLandingHard = 24,
        LaraLandingDeath = 25,
        LaraStayToGrab = 26,
        LaraStayToGrabContinue = 27,
        LaraTryHangVertical = 28,
        LaraBeginHangingVertical = 29,
        LaraStopHangVertical = 30,
        LaraLandingLight = 31,
        LaraSmashJump = 32,
        LaraSmashJumpContinue = 33,
        LaraFreeFallForward = 34,
        LaraFreeFallMiddle = 35,
        LaraFreeFallLongNoHurt = 36,
        LaraHangToRelease = 37,
        // was meant to play when lara is hanging at a ledge and the player releases the action key
        LaraStopWalkBackRight = 38,
        LaraStopWalkBackLeft = 39,
        LaraWalkBack = 40,
        LaraStartWalkBack = 41,
        LaraClimb3click = 42,
        LaraUnknown2 = 43, // was meant to be used like the = 52,  but finally it got removed
        LaraRotateRight = 44,
        LaraJumpingForwardToFreefall = 45, // used after the forward jump if she keeps falling
        LaraFlyForwardTryToHang = 46,
        LaraRollAlternate = 47, // unused
        LaraRollEndAlternate = 48, // unused
        LaraFreeFallNoHurt = 49,
        LaraClimb2click = 50,
        LaraClimb2clickEnd = 51,
        LaraClimb2clickEndRunning = 52, // used if the player keeps pressing the up cursor key

        LaraWallSmashLeft = 53,
        LaraWallSmashRight = 54,
        LaraRunUpStepRight = 55,
        LaraRunUpStepLeft = 56,
        LaraWalkUpStepRight = 57,
        LaraWalkUpStepLeft = 58,
        LaraWalkDownLeft = 59,
        LaraWalkDownRight = 60,
        LaraWalkDownBackLeft = 61,
        LaraWalkDownBackRight = 62,

        LaraPullSwitchDown = 63,
        LaraPullSwitchUp = 64,

        LaraWalkLeft = 65,
        LaraWalkLeftEnd = 66,
        LaraWalkRight = 67,
        LaraWalkRightEnd = 68,
        LaraRotateLeft = 69,
        LaraSlideForward = 70,
        LaraSlideForwardEnd = 71,
        LaraSlideForwardStop = 72,
        LaraStayJumpSides = 73,
        LaraJumpBackBegin = 74,
        LaraJumpBack = 75,
        LaraJumpForwardBegin = 76,
        LaraContinueFlyForward = 77,
        LaraJumpLeftBegin = 78,
        LaraJumpLeft = 79,
        LaraJumpRightBegin = 80,
        LaraJumpRight = 81,
        LaraLandingMiddle = 82,
        LaraForwardToFreeFall = 83,
        LaraLeftToFreeFall = 84,
        LaraRightToFreeFall = 85,

        LaraUnderwaterSwimForward = 86,
        LaraUnderwaterSwimSolid = 87,
        LaraRunBackBegin = 88,
        LaraRunBack = 89,
        LaraRunBackEnd = 90,
        LaraTryHangVerticalBegin = 91, // native bug: glitchy intermediate animation.
        LaraLandingFromRun = 92,
        LaraFreeFallBack = 93,
        LaraFlyForwardTryHang = 94,
        LaraTryHangSolid = 95,
        LaraHangIdle = 96, // main climbing animation... triggers
        LaraClimbOn = 97,
        LaraFreeFallToLong = 98,
        LaraFallCrouchingLanding = 99, // unused
        LaraFreeFallToSideLanding = 100,
        LaraFreeFallToSideLandingAlternate = 101,
        // maybe it was used at the beginning of a forward jump when the player presses action? maybe it was used like this with the original beta anim = 73, 
        LaraClimbOnEnd = 102,
        LaraStayIdle = 103,
        LaraStartSlideBackward = 104,
        LaraSlideBackward = 105,
        LaraSlideBackwardEnd = 106,
        LaraUnderwaterSwimToIdle = 107,
        LaraUnderwaterIdle = 108,
        LaraUnderwarerIdleToSwim = 109,
        LaraOnwaterIdle = 110,

        LaraClimbOutOfWater = 111,
        LaraFreeFallToUnderwater = 112,
        LaraOnwaterDiveAlternate = 113, // this one is not used
        LaraUnderwaterToOnwater = 114,
        LaraOnwaterDive = 115,
        LaraOnwaterSwimForward = 116,
        LaraOnwaterSwimForwardToIdle = 117,
        LaraOnwaterIdleToSwim = 118,
        LaraFreeFallToUnderwaterAlternate = 119, // this one is used
        LaraStartObjectMoving = 120,
        LaraStopObjectMoving = 121,
        LaraObjectPull = 122,
        LaraObjectPush = 123,
        LaraUnderwaterDeath = 124,
        LaraAhForward = 125,
        LaraAhBackward = 126,
        LaraAhLeft = 127,
        LaraAhRight = 128,
        LaraUnderwaterSwitch = 129,
        LaraUnderwaterPickup = 130,
        LaraUseKey = 131,
        LaraOnwaterDeath = 132,
        LaraRunToDie = 133,
        LaraUsePuzzle = 134,
        LaraPickup = 135,
        LaraClimbLeft = 136,
        LaraClimbRight = 137,
        LaraStayToDeath = 138,
        LaraSquashBoulder = 139,
        LaraOnwaterIdleToSwimBack = 140,
        LaraOnwaterSwimBack = 141,
        LaraOnwaterSwimBackToIdle = 142,
        LaraOnwaterSwimLeft = 143,
        LaraOnwaterSwimRight = 144,
        LaraJumpToDeath = 145,
        LaraRollBegin = 146,
        LaraRollContinue = 147,
        LaraRollEnd = 148,
        LaraSpiked = 149,
        LaraOscillateHangOn = 150,
        LaraLandingRoll = 151,
        LaraFishToUnderwater1 = 152,
        LaraFreeFallFish = 153,
        LaraFishToUnderwater2 = 154,
        LaraFreeFallFishToDeath = 155,
        LaraStartFlyLikeFishLeft = 156,
        LaraStartFlyLikeFishRight = 157,
        LaraFreeFallFishStart = 158,
        LaraClimbOn2 = 159,

        // TR2 AND ABOVE (160-216)

        LaraStandToLadder = 160,
        LaraLadderUp = 161,
        LaraLadderUpStopRight = 162,
        LaraLadderUpStopLeft = 163,
        LaraLadderIdle = 164,
        LaraLadderUpStart = 165,
        LaraLadderDownStopLeft = 166,
        LaraLadderDownStopRight = 167,
        LaraLadderDown = 168,
        LaraLadderDownStart = 169,
        LaraLadderRight = 170,
        LaraLadderLeft = 171,
        LaraLadderHang = 172,
        LaraLadderHangToIdle = 173,
        LaraLadderToStand = 174,
        // laraUnknown5 = 175,                    // unknown use
        LaraOnwaterToWadeShallow = 176,
        LaraWade = 177,
        LaraRunToWadeLeft = 178,
        LaraRunToWadeRight = 179,
        LaraWadeToRunLeft = 180,
        LaraWadeToRunRight = 181,

        LaraLadderBackflipStart = 182,
        LaraLadderBackflipEnd = 183,
        LaraWadeToStayRight = 184,
        LaraWadeToStayLeft = 185,
        LaraStayToWade = 186,
        LaraLadderUpHands = 187,
        LaraLadderDownHands = 188,
        LaraFlareThrow = 189,
        LaraOnwaterToWadeDeep = 190,
        LaraOnwaterToLandLow = 191,
        LaraUnderwaterToWade = 192,
        LaraOnwaterToWade = 193,
        LaraLadderToHandsDown = 194,
        LaraSwitchSmallDown = 195,
        LaraSwitchSmallUp = 196,
        LaraButtonPush = 197,
        LaraUnderwaterSwimToStillHuddle = 198,
        LaraUnderwaterSwimToStillSprawl = 199,
        LaraUnderwaterSwimToStillMedium = 200,
        LaraLadderToHandsRight = 201,
        LaraLadderToHandsLeft = 202,
        LaraUnderwaterRollBegin = 203,
        LaraFlarePickup = 204,
        LaraUnderwaterRollEnd = 205,
        LaraUnderwaterFlarePickup = 206,
        LaraRunningJumpRollBegin = 207,
        LaraSomersault = 208,
        LaraRunningJumpRollEnd = 209,
        LaraStandingJumpRollBegin = 210,
        LaraStandingJumpRollEnd = 211,
        LaraBackwardsJumpRollBegin = 212,
        LaraBackwardsJumpRollEnd = 213,

        LaraTr2Kick = 214,
        LaraTr2ZiplineGrab = 215,
        LaraTr2ZiplineRide = 216,
        LaraTr2ZiplineFall = 217,

        // TR3 AND ABOVE (214-312)

        LaraTr345ZiplineGrab = 214,
        LaraTr345ZiplineRide = 215,
        LaraTr345ZiplineFall = 216,
        LaraTr345StandToCrouch = 217,

        LaraSlideForwardToRun = 246, // slide to run!

        LaraJumpForwardBeginToGrab = 248,
        LaraJumpForwardEndToGrab = 249,
        LaraRunToGrabRight = 250,
        LaraRunToGrabLeft = 251,

        LaraRunToSprintLeft = 224,
        LaraRunToSprintRight = 225,
        LaraSprint = 223,
        LaraSprintSlideStandRight = 226,
        LaraSprintSlideStandRightBeta = 227, // beta sprint-slide stand
        LaraSprintSlideStandLeft = 228,
        LaraSprintSlideStandLeftBeta = 229, // beta sprint-slide stand
        LaraSprintToRollLeft = 230,
        LaraSprintToRollLeftBeta = 231, // beta sprint roll
        LaraSprintRollLeftToRun = 232,
        LaraSprintToRollRight = 308,
        LaraSprintRollRightToRun = 309,
        LaraSprintToRollAlternateBegin = 240, // not used natively
        LaraSprintToRollAlternateContinue = 241, // not used natively
        LaraSprintToRollAlternateEnd = 242, // not used natively
        LaraSprintToRunLeft = 243,
        LaraSprintToRunRight = 244,
        LaraSprintToCrouchLeft = 310,
        LaraSprintToCrouchRight = 311,

        LaraMonkeyGrab = 233,
        LaraMonkeyIdle = 234,
        LaraMonkeyFall = 235,
        LaraMonkeyForward = 236,
        LaraMonkeyStopLeft = 237,
        LaraMonkeyStopRight = 238,
        LaraMonkeyIdleToForwardLeft = 239,
        LaraMonkeyIdleToForwardRight = 252,
        LaraMonkeyStrafeLeft = 253,
        LaraMonkeyStrafeLeftEnd = 254,
        LaraMonkeyStrafeRight = 255,
        LaraMonkeyStrafeRightEnd = 256,
        LaraMonkeyTurnAround = 257, // use titak's animation from trep patch
        LaraMonkeyTurnLeft = 271,
        LaraMonkeyTurnRight = 272,
        LaraMonkeyTurnLeftEarlyEnd = 283,
        LaraMonkeyTurnLeftLateEnd = 284,
        LaraMonkeyTurnRightEarlyEnd = 285,
        LaraMonkeyTurnRightLateEnd = 286,

        LaraCrouchRollForwardBegin = 218, // not used natively
        LaraCrouchRollForwardBeginAlternate = 247, // not used
        LaraCrouchRollForwardContinue = 219, // not used natively
        LaraCrouchRollForwardEnd = 220, // not used natively
        LaraCrouchToStand = 221,
        LaraCrouchIdle = 222,
        LaraCrouchPrepare = 245,
        LaraCrouchIdleSmash = 265, // not used natively
        LaraCrouchToCrawlBegin = 258,
        LaraCrouchToCrawlContinue = 273,
        LaraCrouchToCrawlEnd = 264,

        LaraCrawlToCrouchBegin = 259,
        LaraCrawlToCrouchEnd = 274,
        LaraCrawlForward = 260,
        LaraCrawlIdleToForward = 261,
        LaraCrawlBackward = 276,
        LaraCrawlIdleToBackward = 275,
        LaraCrawlIdle = 263,
        LaraCrawlForwardToIdleBeginRight = 262,
        LaraCrawlForwardToIdleEndRight = 266,
        LaraCrawlForwardToIdleBeginLeft = 267,
        LaraCrawlForwardToIdleEndLeft = 268,
        LaraCrawlBackwardToIdleBeginRight = 277,
        LaraCrawlBackwardToIdleEndRight = 278,
        LaraCrawlBackwardToIdleBeginLeft = 279,
        LaraCrawlBackwardToIdleEndLeft = 280,
        LaraCrawlTurnLeft = 269,
        LaraCrawlTurnLeftEnd = 281,
        LaraCrawlTurnRight = 270,
        LaraCrawlTurnRightEnd = 282,

        LaraHangToCrouchBegin = 287,
        LaraHangToCrouchEnd = 288,
        LaraCrawlToHangBegin = 289,
        LaraCrawlToHangContinue = 290,
        LaraCrawlToHangEnd = 302,

        LaraCrouchPickup = 291,
        LaraCrouchPickupFlare = 312,
        LaraCrawlPickup = 292, // not natively used - make it work

        LaraCrouchSmashForward = 293,
        LaraCrouchSmashBackward = 294,
        LaraCrouchSmashRight = 295,
        LaraCrouchSmashLeft = 296,

        LaraCrawlSmashForward = 297,
        LaraCrawlSmashBackward = 298,
        LaraCrawlSmashRight = 299,
        LaraCrawlSmashLeft = 300,

        LaraCrawlDeath = 301,
        LaraCrouchAbort = 303,

        LaraRunToCrouchLeftBegin = 304,
        LaraRunToCrouchRightBegin = 305,
        LaraRunToCrouchLeftEnd = 306,
        LaraRunToCrouchRightEnd = 307,

        // TR4 AND ABOVE (313-444)

        LaraDoorOpenForward = 313,
        LaraDoorOpenBack = 314,
        LaraDoorKick = 315,
        LaraGiantButtonPush = 316,
        LaraFloorTrapdoorOpen = 317,
        LaraCeilingTrapdoorOpen = 318,
        LaraRoundHandleGrabClockwise = 319,
        LaraRoundHandleGrabCounterclockwise = 320,
        LaraCogwheelPull = 321,
        LaraCogwheelGrab = 322,
        LaraCogwheelUngrab = 323,
        LaraLeverswitchPush = 324,
        LaraHoleGrab = 325,
        LaraStayToPoleGrab = 326,
        LaraPoleJump = 327,
        LaraPoleIdle = 328,
        LaraPoleClimbUp = 329,
        LaraPoleFall = 330,
        LaraJumpForwardToPoleGrab = 331,
        LaraPoleTurnLeftBegin = 332,
        LaraPoleTurnRightBegin = 333,
        LaraPoleIdleToClimbDown = 334,
        LaraPoleClimbDown = 335,
        LaraPoleClimbDownToIdle = 336,
        LaraJumpUpToPoleGrab = 337,
        LaraPoleClimbUpInbetween = 338,
        LaraPulleyGrab = 339,
        LaraPulleyPull = 340,
        LaraPulleyUngrab = 341,
        LaraPoleGrabToStay = 342,
        // laraUnknown8 = 343, 
        LaraPoleTurnLeftEnd = 344,
        // laraUnknown9 = 345, 
        LaraPoleTurnRightEnd = 346,
        LaraRoundHandlePushRightBegin = 347,
        LaraRoundHandlePushRightContinue = 348,
        LaraRoundHandlePushRightEnd = 349,
        LaraRoundHandlePushLeftBegin = 350,
        LaraRoundHandlePushLeftContinue = 351,
        LaraRoundHandlePushLeftEnd = 352,
        LaraCrouchTurnLeft = 353,
        LaraCrouchTurnRight = 354,
        LaraHangAroundLeftOuterBegin = 355,
        LaraHangAroundLeftOuterEnd = 356,
        LaraHangAroundRightOuterBegin = 357,
        LaraHangAroundRightOuterEnd = 358,
        LaraHangAroundLeftInnerBegin = 359,
        LaraHangAroundLeftInnerEnd = 360,
        LaraHangAroundRightInnerBegin = 361,
        LaraHangAroundRightInnerEnd = 362,
        LaraLadderAroundLeftOuterBegin = 363,
        LaraLadderAroundLeftOuterEnd = 364,
        LaraLadderAroundRightOuterBegin = 365,
        LaraLadderAroundRightOuterEnd = 366,
        LaraLadderAroundLeftInnerBegin = 367,
        LaraLadderAroundLeftInnerEnd = 368,
        LaraLadderAroundRightInnerBegin = 369,
        LaraLadderAroundRightInnerEnd = 370,
        LaraMonkeyToRopeBegin = 371,
        LaraTrainDeath = 372,

        LaraMonkeyToRopeEnd = 373,
        LaraRopeIdle = 374, // review all rope animations!
        LaraRopeDownBegin = 375,
        LaraRopeUp = 376,
        LaraRopeIdleToSwingSoft = 377, // unused
        LaraRopeGrabToFall = 378, // unused
        LaraRopeJumpToGrab = 379,
        LaraRopeIdleToBackflip = 380, // unused
        LaraRopeSwingToFallSemifront = 381, // unused
        LaraRopeSwingToFallMiddle = 382, // unused
        LaraRopeSwingToFallBack = 383, // unused

        LaraRopeIdleToSwingSemimiddle = 388, // unused
        LaraRopeIdleToSwingHalfmiddle = 389, // unused
        LaraRopeSwingToFallFront = 390, // unused
        LaraRopeGrabToFallAlternate = 391, // unused

        LaraRopeSwingForwardSemihard = 394, // the only one used!
        LaraRopeLadderToHandsDownAlternate = 395,
        // unused, make it work? (used in the tr4 demo if i'm right?) (then you will need to remove all the stateid changes related to the rope animations)
        LaraRopeSwingBackContinue = 396, // unused
        LaraRopeSwingBackEnd = 397, // unused
        LaraRopeSwingBackBegin = 398, // unused
        LaraRopeSwingForwardSoft = 399, // unused

        LaraRopeSwingForwardHard = 404, // not found... uhh, unused
        LaraRopeChangeRope = 405, // unused
        LaraRopeSwingToTryHangFront2 = 406, // not sure it's used?
        LaraRopeSwingToTryHangMiddle = 407, // not sure it's used?
        LaraRopeSwingBlock = 408, // unused
        LaraRopeSwingToTryHangSemimiddle = 409, // not sure it's used?
        LaraRopeSwingToTryHangFront3 = 410, // not sure it's used?

        LaraDoubledoorsPush = 412,
        LaraBigButtonPush = 413,
        LaraJumpswitch = 414,
        LaraUnderwaterPulley = 415,
        LaraUnderwaterDoorOpen = 416,
        LaraPushablePushToStand = 417,
        LaraPushablePullToStand = 418,
        LaraCrowbarUseOnWall = 419,
        LaraCrowbarUseOnFloor = 420,
        LaraCrawlJumpDown = 421,
        LaraHarpPlay = 422,
        LaraPutTrident = 423,
        LaraPickupPedestalHigh = 424,
        LaraPickupPedestalLow = 425,
        LaraRotateSenet = 426,
        LaraTorchLight1 = 427,
        LaraTorchLight2 = 428,
        LaraTorchLight3 = 429,
        LaraTorchLight4 = 430,
        LaraTorchLight5 = 431,
        LaraDetonatorUse = 432,

        LaraCorrectPositionFront = 433, // unused
        LaraCorrectPositionLeft = 434, // unused
        LaraCorrectPositionRight = 435, // unused

        LaraCrowbarUseOnFloorFail = 436, // unused
        LaraTr4DeathMagicTr5UseKeycard = 437, // unused?
        LaraDeathBlowup = 438,
        LaraPickupSarcophagus = 439,
        LaraDrag = 440,
        LaraBinoculars = 441,
        LaraDeathBigScorpion = 442,
        LaraTr4DeathSethTr5ElevatorSmash = 443,
        LaraBeetlePut = 444,

        // TR5 AND ABOVE (445-473)

        LaraElevatorRecover = 443,
        LaraDozy = 445,
        LaraTightropeWalk = 446,
        LaraTightropeWalkToStand = 447,
        LaraTightropeStand = 448,
        LaraTightropeWalkToStandCareful = 449,
        LaraTightropeStandToWalk = 450,
        LaraTightropeTurn = 451,
        LaraTightropeLooseLeft = 452,
        LaraTightropeRecoverLeft = 453,
        LaraTightropeFallLeft = 454,
        LaraTightropeLooseRight = 455,
        LaraTightropeRecoverRight = 456,
        LaraTightropeFallRight = 457,
        LaraTightropeStart = 458,
        LaraTightropeFinish = 459,
        LaraDoveswitchTurn = 460,
        LaraBarsGrab = 461,
        LaraBarsSwing = 462,
        LaraBarsJump = 463,
        LaraLootCabinet = 464,
        LaraLootDrawer = 465,
        LaraLootShelf = 466,
        LaraRadioBegin = 467,
        LaraRadioIdle = 468,
        LaraRadioEnd = 469,
        LaraValveTurn = 470,
        LaraCrowbarUseOnWall2 = 471,
        LaraLootChest = 472,
        LaraLadderToCrouch = 473,

        LastIndex
    }

    public enum TR_STATE : short
    {
        Current = -1,
        LaraWalkForward = 0,
        LaraRunForward = 1,
        LaraStop = 2,
        LaraJumpForward = 3,
        LaraPose = 4, // derived from leaked tomb.map
        LaraRunBack = 5,
        LaraTurnRightSlow = 6,
        LaraTurnLeftSlow = 7,
        LaraDeath = 8,
        LaraFreefall = 9,
        LaraHang = 10,
        LaraReach = 11,
        //unused2 = 12, 
        LaraUnderwaterStop = 13,
        LaraGrabToFall = 14,
        LaraJumpPrepare = 15,
        LaraWalkBack = 16,
        LaraUnderwaterForward = 17,
        LaraUnderwaterInertia = 18,
        LaraClimbing = 19,
        LaraTurnFast = 20,
        LaraWalkRight = 21,
        LaraWalkLeft = 22,
        LaraRollBackward = 23,
        LaraSlideForward = 24,
        LaraJumpBack = 25,
        LaraJumpLeft = 26,
        LaraJumpRight = 27,
        LaraJumpUp = 28,
        LaraFallBackward = 29,
        LaraShimmyLeft = 30,
        LaraShimmyRight = 31,
        LaraSlideBack = 32,
        LaraOnwaterStop = 33,
        LaraOnwaterForward = 34,
        LaraUnderwaterDiving = 35,
        LaraPushablePush = 36,
        LaraPushablePull = 37,
        LaraPushableGrab = 38,
        LaraPickup = 39,
        LaraSwitchDown = 40,
        LaraSwitchUp = 41,
        LaraInsertKey = 42,
        LaraInsertPuzzle = 43,
        LaraWaterDeath = 44,
        LaraRollForward = 45,
        LaraBoulderDeath = 46,
        LaraOnwaterBack = 47,
        LaraOnwaterLeft = 48,
        LaraOnwaterRight = 49,
        LaraUseMidas = 50, //  derived from leaked tomb.map
        LaraDieMidas = 51, //  derived from leaked tomb.map
        LaraSwandiveBegin = 52,
        LaraSwandiveEnd = 53,
        LaraHandstand = 54,
        LaraOnwaterExit = 55,
        LaraLadderIdle = 56,
        LaraLadderUp = 57,
        LaraLadderLeft = 58,
        //unused5 = 59, 
        LaraLadderRight = 60,
        LaraLadderDown = 61,
        //unused6 = 62, 
        //unused7 = 63, 
        //unused8 = 64, 
        LaraWadeForward = 65,
        LaraUnderwaterTurnaround = 66,
        LaraFlarePickup = 67,
        LaraJumpRoll = 68,
        //unused10 = 69, 
        LaraZiplineRide = 70,
        LaraCrouchIdle = 71,
        LaraCrouchRoll = 72,
        LaraSprint = 73,
        LaraSprintRoll = 74,
        LaraMonkeyswingIdle = 75,
        LaraMonkeyswingForward = 76,
        LaraMonkeyswingLeft = 77,
        LaraMonkeyswingRight = 78,
        LaraMonkeyswingTurnaround = 79,
        LaraCrawlIdle = 80,
        LaraCrawlForward = 81,
        LaraMonkeyswingTurnLeft = 82,
        LaraMonkeyswingTurnRight = 83,
        LaraCrawlTurnLeft = 84,
        LaraCrawlTurnRight = 85,
        LaraCrawlBack = 86,
        LaraClimbToCrawl = 87,
        LaraCrawlToClimb = 88,
        LaraMiscControl = 89,
        LaraRopeTurnLeft = 90,
        LaraRopeTurnRight = 91,
        LaraGiantButtonPush = 92,
        LaraTrapdoorFloorOpen = 93,
        //unused11 = 94, 
        LaraRoundHandle = 95,
        LaraCogwheel = 96,
        LaraLeverswitchPush = 97,
        LaraHole = 98,
        LaraPoleIdle = 99,
        LaraPoleUp = 100,
        LaraPoleDown = 101,
        LaraPoleTurnLeft = 102,
        LaraPoleTurnRight = 103,
        LaraPulley = 104,
        LaraCrouchTurnLeft = 105,
        LaraCrouchTurnRight = 106,
        LaraClimbCornerLeftOuter = 107,
        LaraClimbCornerRightOuter = 108,
        LaraClimbCornerLeftInner = 109,
        LaraClimbCornerRightInner = 110,
        LaraRopeIdle = 111,
        LaraRopeClimbUp = 112,
        LaraRopeClimbDown = 113,
        LaraRopeSwing = 114,
        LaraLadderToHands = 115,
        LaraPositionCorrector = 116,
        LaraDoubledoorsPush = 117,
        LaraDozy = 118,
        LaraTightropeIdle = 119,
        LaraTightropeTurnaround = 120,
        LaraTightropeForward = 121,
        LaraTightropeBalancingLeft = 122,
        LaraTightropeBalancingRight = 123,
        LaraTightropeEnter = 124,
        LaraTightropeExit = 125,
        LaraDoveswitch = 126,
        LaraTightropeRestoreBalance = 127,
        LaraBarsSwing = 128,
        LaraBarsJump = 129,
        //unused12 = 130, 
        LaraRadioListening = 131,
        LaraRadioOff = 132,
        //unused13 = 133, 
        //unused14 = 134, 
        //unused15 = 135, 
        //unused16 = 136, 
        LaraPickupFromChest = 137
    }

    /// <summary>
    /// Animation commands
    /// </summary>
    public enum TR_ANIMCOMMAND
    {
        SetPosition = 1,
        JumpDistance = 2,
        EmptyHands = 3,
        Kill = 4,
        PlaySound = 5,
        PlayEffect = 6,
        Interact = 7
    }

    /// <summary>
    /// Animation effects flags
    /// </summary>
    public enum TR_ANIMCOMMAND_CONDITION : ushort
    {
        Land = 0x4000,
        Water = 0x8000
    }

    /// <summary>
    /// Animation effects / flipeffects
    /// </summary>
    public enum TR_EFFECT
    {
        ChangeDirection = 0,
        ShakeScreen = 1,
        PlayFloodSound = 2,
        Bubble = 3,
        EndLevel = 4,
        ActivateCamera = 5,
        ActivateKey = 6,
        EnableEarthQuakes = 7,
        GetCrowbar = 8,
        CurtainFX = 9, // effect 9 is empty in tr4.
        PlaySound_TimerField = 10,
        PlayExplosionSound = 11,
        DisableGuns = 12,
        EnableGuns = 13,
        GetRightGun = 14,
        GetLeftGun = 15,
        FireRightGun = 16,
        FireLeftGun = 17,
        MeshSwap1 = 18,
        MeshSwap2 = 19,
        MeshSwap3 = 20,
        Inv_On = 21, // effect 21 is unknown at offset 4376f0.
        Inv_Off = 22, // effect 22 is unknown at offset 437700.
        HideObject = 23,
        ShowObject = 24,
        StatueFX = 25, // effect 25 is empty in tr4.
        ResetHair = 26,
        BoilerFX = 27, // effect 27 is empty in tr4.
        SetFogColour = 28,
        GhostTrap = 29, // effect 29 is unknown at offset 4372f0
        LaraLocation = 30,
        ClearScarabs = 31,
        PlayStepSound = 32, // also called footprint_fx in tr4 source code.

        // Effects 33 - 42 are assigned to FLIP_MAP0-FLIP_MAP9 in TR4 source code,
        // but are empty in TR4 binaries.

        GetWaterSkin = 43,
        RemoveWaterSkin = 44,
        LaraLocationPad = 45,
        KillAllEnemies = 46
    }

    public class AnimStateControl
    {
        private static void ent_stop_traverse(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                var v = ent.TraversedObject.Transform.Origin;
                var i = (int) (v.X / TR_METERING_SECTORSIZE);
                v.X = i * TR_METERING_SECTORSIZE + 512.0f;
                i = (int) (v.Y / TR_METERING_SECTORSIZE);
                v.Y = i * TR_METERING_SECTORSIZE + 512.0f;
                ent.TraversedObject.UpdateRigidBody(true);
                ent.TraversedObject = null;
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_set_on_floor(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                ent.MoveType = MoveType.OnFloor;
                ent.Transform.Origin.Z = ent.HeightInfo.FloorPoint.Z;
                ent.GhostUpdate();
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_set_turn_fast(Entity ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                ent.Bf.Animations.NextState = TR_STATE.LaraTurnFast;
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_set_on_floor_after_climb(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            var af = ssAnim.Model.Animations[(int) ssAnim.CurrentAnimation];

            if (ssAnim.CurrentFrame >= af.Frames.Count - 1)
            {
                var move = ent.Transform * ent.Bf.BoneTags[0].FullTransform.Origin;
                ent.SetAnimation(af.NextAnim.ID, af.NextFrame);
                var p = ent.Transform * ent.Bf.BoneTags[0].FullTransform.Origin;
                move -= p; // TODO: WTF???
                ent.Transform.Origin += move;
                ent.Transform.Origin.Z = ent.Climb.Point.Z;
                Entity.UpdateCurrentBoneFrame(ent.Bf, ent.Transform);
                ent.UpdateRigidBody(false);
                ent.GhostUpdate();
                ent.MoveType = MoveType.OnFloor;
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_set_underwater(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                ent.MoveType = MoveType.Underwater;
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_set_free_falling(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                ent.MoveType = MoveType.FreeFalling;
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_set_cmd_slide(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                ent.Response.Slide = SlideType.Back;
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_correct_diving_angle(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                ent.Angles.Y = -45.0f;
                ent.UpdateTransform();
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_to_on_water(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                ent.Transform.Origin.Z = ent.HeightInfo.TransitionLevel;
                ent.GhostUpdate();
                ent.MoveType = MoveType.OnWater;
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_climb_out_of_water(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                var v = ent.Climb.Point;
                ent.Transform.Origin = v + ent.Transform.Basis.Column1 * 48.0f;
                ent.Transform.Origin.Z = v.Z;
                ent.GhostUpdate();
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_to_edge_climb(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                var v = ent.Climb.Point;
                ent.Transform.Origin.X = v.X - ent.Transform.Basis.Column1.X * ent.Bf.BBMax.Y;
                ent.Transform.Origin.Y = v.Y - ent.Transform.Basis.Column1.Y * ent.Bf.BBMax.Y;
                ent.Transform.Origin.Z = v.Z - ent.Bf.BBMax.Z;
                ent.GhostUpdate();
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_to_monkey_swing(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                ent.MoveType = MoveType.Monkeyswing;
                ent.Transform.Origin.Z = ent.HeightInfo.CeilingPoint.Z - ent.Bf.BBMax.Z;
                ent.GhostUpdate();
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_to_tightrope(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                ent.MoveType = MoveType.Climbing;
                ent.GhostUpdate();
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_from_tightrope(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                ent.MoveType = MoveType.OnFloor;
                ent.GhostUpdate();
                ssAnim.ClearOnFrame();
            }
        }

        private static void ent_crawl_to_climb(Character ent, SSAnimation ssAnim, ENTITY_ANIM state)
        {
            if (state == ENTITY_ANIM.NewAnim)
            {
                var cmd = ent.Command;

                if (cmd.Action)
                {
                    ent.SetAnimation(TR_ANIMATION.LaraHangIdle, -1);
                }
                else
                {
                    ent.SetAnimation(TR_ANIMATION.LaraStartFreeFall, 0);
                    ent.MoveType = MoveType.FreeFalling;
                    ent.DirFlag = ENT_MOVE.MoveBackward;
                }

                ent.Bt.NoFixAll = false;
                ent_to_edge_climb(ent, ssAnim, state);
                ssAnim.ClearOnFrame();
            }
        }

        public static int StateControlLara(Character ent, SSAnimation ssAnim)
        {
            int i;
            var pos = ent.Transform.Origin;
            float t;
            Vector3 globalOffset, move;
            var climb = ent.Climb;
            var cmd = ent.Command;
            var resp = ent.Response;

            var currFc = ent.HeightInfo;
            var nextFc = new HeightInfo();
            nextFc.Sp = currFc.Sp;
            nextFc.Cb = ent.RayCb;
            nextFc.Cb.ClosestHitFraction = 1.0f;
            nextFc.Cb.CollisionObject = null;
            nextFc.Ccb = ent.ConvexCb;
            nextFc.Ccb.ClosestHitFraction = 1.0f;
            nextFc.Ccb.HitCollisionObject = null;
            ent.Bt.NoFixBodyParts = 0x00000000;

            ssAnim.AnimFlags = AnimControlFlags.NormalControl;
            ent.UpdateCurrentHeight();

            var lowVerticalSpace = currFc.FloorHit && currFc.CeilingHit &&
                                   currFc.CeilingPoint.Z - currFc.FloorPoint.Z <
                                   ent.Height - LARA_HANG_VERTICAL_EPSILON;
            var lastFrame = ssAnim.Model.Animations[(int) ssAnim.CurrentAnimation].Frames.Count <=
                            ssAnim.CurrentFrame + 1;

            if (resp.Killed) // Stop any music, if Lara is dead.
            {
                Audio.EndStreams(TR_AUDIO_STREAM_TYPE.OneShot);
                Audio.EndStreams(TR_AUDIO_STREAM_TYPE.Chat);
            }

            var nextStep = StepType.Horizontal;

            switch (ssAnim.LastState)
            {
                    #region Normal land animations

                case TR_STATE.LaraStop:
                    // Reset directional flag only on intermediate animation!

                    if (ssAnim.CurrentAnimation == TR_ANIMATION.LaraStaySolid)
                    {
                        ent.DirFlag = ENT_MOVE.Stay;
                    }

                    if (ent.MoveType == MoveType.OnFloor)
                    {
                        ent.Bt.NoFixBodyParts = BODY_PART.Hands | BODY_PART.Legs;
                    }

                    cmd.Rotation.X = 0;
                    cmd.Crouch |= lowVerticalSpace;
                    ent.Lean(cmd, 0.0f);

                    if ((climb.CanHang &&
                         climb.NextZSpace >= ent.Height - LARA_HANG_VERTICAL_EPSILON &&
                         ent.MoveType == MoveType.Climbing) ||
                        ssAnim.CurrentAnimation == TR_ANIMATION.LaraStaySolid)
                    {
                        ent.MoveType = MoveType.OnFloor;
                    }

                    if (ent.MoveType == MoveType.OnFloor)
                    {
                        ent.Bt.NoFixBodyParts = BODY_PART.Legs2 | BODY_PART.Legs3;
                    }

                    if (ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraStartFreeFall, 0);
                        ent.DirFlag = ENT_MOVE.Stay;
                    }
                    else if (resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraDeath;
                    }
                    else if (resp.Slide == SlideType.Front)
                    {
                        Audio.Send((uint) TR_AUDIO_SOUND.Landing, TR_AUDIO_EMITTER.Entity, (int) ent.ID);

                        if (cmd.Jump)
                        {
                            ent.DirFlag = ENT_MOVE.MoveForward;
                            ent.SetAnimation(TR_ANIMATION.LaraJumpForwardBegin, 0);
                        }
                        else
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraSlideForward, 0);
                        }
                    }
                    else if (resp.Slide == SlideType.Back)
                    {
                        if (cmd.Jump)
                        {
                            ent.DirFlag = ENT_MOVE.MoveBackward;
                            ent.SetAnimation(TR_ANIMATION.LaraJumpBackBegin, 0);
                            Audio.Send((uint) TR_AUDIO_SOUND.Landing, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                        }
                        else
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraStartSlideBackward, 0);
                        }
                    }
                    else if (cmd.Jump)
                    {
                        if (currFc.Quicksand == QuicksandPosition.None)
                            ssAnim.NextState = TR_STATE.LaraJumpPrepare; // Jump sideways
                    }
                    else if (cmd.Roll)
                    {
                        if (currFc.Quicksand == QuicksandPosition.None &&
                            ssAnim.CurrentAnimation != TR_ANIMATION.LaraClimb2click)
                        {
                            ent.DirFlag = ENT_MOVE.MoveForward;
                            ent.SetAnimation(TR_ANIMATION.LaraRollBegin, 0);
                        }
                    }
                    else if (cmd.Crouch)
                    {
                        if (currFc.Quicksand == QuicksandPosition.None)
                            ssAnim.NextState = TR_STATE.LaraCrouchIdle;
                    }
                    else if (cmd.Action && ent.FindTraverse() != 0)
                    {
                        ssAnim.NextState = TR_STATE.LaraPushableGrab;
                        var c1 = ent.Transform.Basis.Column1;
                        if (c1.X > 0.9f)
                        {
                            t = -ent.TraversedObject.Bf.BBMin.X + 72.0f;
                        }
                        else if (c1.X < -0.9f)
                        {
                            t = ent.TraversedObject.Bf.BBMin.X + 72.0f;
                        }
                        else if (c1.Y > 0.9f)
                        {
                            t = -ent.TraversedObject.Bf.BBMin.Y + 72.0f;
                        }
                        else if (c1.Y < -0.9f)
                        {
                            t = ent.TraversedObject.Bf.BBMin.Y + 72.0f;
                        }
                        else
                        {
                            t = 512.0f + 72.0f;
                        }
                        var v = ent.TraversedObject.Transform.Origin;
                        pos.X = v.X - c1.X * t;
                        pos.Y = v.Y - c1.Y * t;
                    }
                    else if (cmd.Move[0] == 1)
                    {
                        move = ent.Transform.Basis.Column1 * PENETRATION_TEST_OFFSET;
                        if (cmd.Shift)
                        {
                            globalOffset = ent.Transform.Basis.Column1 * WALK_FORWARD_OFFSET;
                            globalOffset.Z += ent.Bf.BBMax.Z;
                            globalOffset += pos;
                            Character.GetHeightInfo(globalOffset, nextFc);
                            if ((ent.CheckNextPenetration(move) == 0 || ent.Response.HorizontalCollide == 0x00) &&
                                nextFc.FloorHit && nextFc.FloorPoint.Z > pos.Z - ent.MaxStepUpHeight &&
                                nextFc.FloorPoint.Z <= pos.Z + ent.MaxStepUpHeight)
                            {
                                ent.MoveType = MoveType.OnFloor;
                                ent.DirFlag = ENT_MOVE.MoveForward;
                                if ((currFc.Water || currFc.Quicksand != QuicksandPosition.None) && currFc.FloorHit &&
                                    currFc.TransitionLevel - currFc.FloorPoint.Z > ent.WadeDepth)
                                {
                                    ssAnim.NextState = TR_STATE.LaraWadeForward;
                                }
                                else
                                {
                                    ssAnim.NextState = TR_STATE.LaraWalkForward;
                                }
                            }
                        }
                        else
                        {
                            globalOffset = ent.Transform.Basis.Column1 * RUN_FORWARD_OFFSET;
                            globalOffset.Z += ent.Bf.BBMax.Z;
                            globalOffset += pos;
                            ent.CheckNextStep(globalOffset, nextFc);
                            if ((ent.CheckNextPenetration(move) == 0 || ent.Response.HorizontalCollide == 0x00) &&
                                !ent.HasStopSlant(nextFc))
                            {
                                ent.MoveType = MoveType.OnFloor;
                                ent.DirFlag = ENT_MOVE.MoveForward;
                                if ((currFc.Water || currFc.Quicksand != QuicksandPosition.None) && currFc.FloorHit &&
                                    currFc.TransitionLevel - currFc.FloorPoint.Z > ent.WadeDepth)
                                {
                                    ssAnim.NextState = TR_STATE.LaraWadeForward;
                                }
                                else
                                {
                                    ssAnim.NextState = TR_STATE.LaraRunForward;
                                }
                            }
                        }

                        if (cmd.Action &&
                            new[]
                            {
                                TR_ANIMATION.LaraStayIdle,
                                TR_ANIMATION.LaraStaySolid,
                                TR_ANIMATION.LaraWallSmashLeft,
                                TR_ANIMATION.LaraWallSmashRight
                            }.Contains(ssAnim.CurrentAnimation))
                        {
                            t = ent.ForwardSize + LARA_TRY_HANG_WALL_OFFSET;
                            globalOffset = ent.Transform.Basis.Column1 * t;
                            globalOffset.Z += 0.5f * DEFAULT_CLIMB_UP_HEIGHT;
                            climb = ent.CheckClimbability(globalOffset, nextFc, 0.5f * DEFAULT_CLIMB_UP_HEIGHT);
                            if (climb.EdgeHit &&
                                climb.NextZSpace >= ent.Height - LARA_HANG_VERTICAL_EPSILON &&
                                pos.Z + ent.MaxStepUpHeight < nextFc.FloorPoint.Z &&
                                pos.Z + 2944.0f >= nextFc.FloorPoint.Z &&
                                nextFc.FloorNormale.Z >= ent.CriticalSlantZComponent)
                            {
                                if (pos.Z + 640.0f >= nextFc.FloorPoint.Z)
                                {
                                    ent.Angles.X = climb.EdgeZAngle;
                                    pos.Z = nextFc.FloorPoint.Z - 512.0f;
                                    climb.Point = nextFc.FloorPoint;
                                    ent.SetAnimation(TR_ANIMATION.LaraClimb2click, 0);
                                    ent.Bt.NoFixAll = true;
                                    ssAnim.SetOnFrame(ent_set_on_floor_after_climb);
                                    break;
                                }
                                else if (pos.Z + 896.0f >= nextFc.FloorPoint.Z)
                                {
                                    ent.Angles.X = climb.EdgeZAngle;
                                    pos.Z = nextFc.FloorPoint.Z - 768.0f;
                                    climb.Point = nextFc.FloorPoint;
                                    ent.SetAnimation(TR_ANIMATION.LaraClimb3click, 0);
                                    ent.Bt.NoFixAll = true;
                                    ssAnim.SetOnFrame(ent_set_on_floor_after_climb);
                                    break;
                                }
                            }

                            globalOffset.Z += 0.5f * DEFAULT_CLIMB_UP_HEIGHT;
                            climb = ent.CheckClimbability(globalOffset, nextFc, DEFAULT_CLIMB_UP_HEIGHT);
                            if (climb.EdgeHit &&
                                climb.NextZSpace >= ent.Height - LARA_HANG_VERTICAL_EPSILON &&
                                pos.Z + ent.MaxStepUpHeight < nextFc.FloorPoint.Z &&
                                pos.Z + 2944.0f >= nextFc.FloorPoint.Z)
                            {
                                if (pos.Z + 1920.0f >= nextFc.FloorPoint.Z)
                                {
                                    // MAGIC: Vertical speed override is based on ledge height - thanks to T4Larson!
                                    ssAnim.Model.Animations[(int) TR_ANIMATION.LaraStayToGrab].Frames[
                                        ssAnim.Model.Animations[(int) TR_ANIMATION.LaraStayToGrab].Frames.Count - 1]
                                        .V_Vertical = -3 - (int) Math.Sqrt(-9600 - 12 * -(nextFc.FloorPoint.Z - pos.Z));
                                    ssAnim.NextState = TR_STATE.LaraJumpUp;
                                    break;
                                }
                            }

                            climb = ent.CheckWallsClimbability();
                            if (climb.WallHit != ClimbType.None)
                            {
                                ssAnim.NextState = TR_STATE.LaraJumpUp;
                                break;
                            }
                        }
                    }
                    else if (cmd.Move[0] == -1)
                    {
                        move = ent.Transform.Basis.Column1 * -PENETRATION_TEST_OFFSET;
                        if (ent.CheckNextPenetration(move) == 0 || ent.Response.HorizontalCollide == 0x00)
                        {
                            if (cmd.Shift)
                            {
                                globalOffset = ent.Transform.Basis.Column1 * -WALK_BACK_OFFSET;
                                globalOffset.Z += ent.Bf.BBMax.Z;
                                globalOffset += pos;
                                Character.GetHeightInfo(globalOffset, nextFc);
                                if (nextFc.FloorHit && nextFc.FloorPoint.Z > pos.Z - ent.MaxStepUpHeight &&
                                    nextFc.FloorPoint.Z <= pos.Z + ent.MaxStepUpHeight)
                                {
                                    ent.DirFlag = ENT_MOVE.MoveBackward;
                                    ssAnim.NextState = TR_STATE.LaraWalkBack;
                                }
                            }
                            else
                            {
                                ent.DirFlag = ENT_MOVE.MoveBackward;
                                if ((currFc.Water || currFc.Quicksand != QuicksandPosition.None) && currFc.FloorHit &&
                                    currFc.TransitionLevel - currFc.FloorPoint.Z > ent.WadeDepth)
                                {
                                    ssAnim.NextState = TR_STATE.LaraWalkBack;
                                }
                                else
                                {
                                    ssAnim.NextState = TR_STATE.LaraRunBack;
                                }
                            }
                        }
                    }
                    else if (cmd.Move[1] == 1)
                    {
                        if (cmd.Shift)
                        {
                            move = ent.Transform.Basis.Column0 * PENETRATION_TEST_OFFSET;
                            if (ent.CheckNextPenetration(move) == 0 || ent.Response.HorizontalCollide == 0x00)
                            {
                                globalOffset = ent.Transform.Basis.Column0 * RUN_FORWARD_OFFSET;
                                globalOffset.Z += ent.Bf.BBMax.Z;
                                if (resp.HorizontalCollide == 0 &&
                                    ent.CheckNextStep(globalOffset, nextFc).IsLittleStep())
                                {
                                    cmd.Rotation.X = 0.0f;
                                    ent.DirFlag = ENT_MOVE.MoveRight;
                                    ssAnim.NextState = TR_STATE.LaraWalkRight;
                                }
                            }
                        }
                        else
                        {
                            ssAnim.NextState = TR_STATE.LaraTurnRightSlow;
                        }
                    }
                    else if (cmd.Move[1] == -1)
                    {
                        if (cmd.Shift)
                        {
                            move = ent.Transform.Basis.Column0 * -PENETRATION_TEST_OFFSET;
                            if (ent.CheckNextPenetration(move) == 0 || ent.Response.HorizontalCollide == 0x00)
                            {
                                globalOffset = ent.Transform.Basis.Column0 * -RUN_FORWARD_OFFSET;
                                globalOffset.Z += ent.Bf.BBMax.Z;
                                if (resp.HorizontalCollide == 0 &&
                                    ent.CheckNextStep(globalOffset, nextFc).IsLittleStep())
                                {
                                    cmd.Rotation.X = 0.0f;
                                    ent.DirFlag = ENT_MOVE.MoveLeft;
                                    ssAnim.NextState = TR_STATE.LaraWalkLeft;
                                }
                            }
                        }
                        else
                        {
                            ssAnim.NextState = TR_STATE.LaraTurnLeftSlow;
                        }
                    }
                    break;
                case TR_STATE.LaraJumpPrepare:
                    ent.Bt.NoFixBodyParts = BODY_PART.Legs | BODY_PART.Hands | BODY_PART.Head;
                    cmd.Rotation.X = 0;
                    ent.Lean(cmd, 0.0f);

                    if (resp.Slide == SlideType.Back)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraJumpBackBegin, 0);
                        cmd.Move[0] = -1;
                    }
                    else if (resp.Slide == SlideType.Front)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraJumpForwardBegin, 0);
                        cmd.Move[0] = 1;
                    }
                    if ((currFc.Water || currFc.Quicksand != QuicksandPosition.None) && currFc.FloorHit &&
                        currFc.TransitionLevel - currFc.FloorPoint.Z > ent.WadeDepth)
                    {
                        // Stay, directional jumps are not allowed whilst in wade depth
                    }
                    else if (cmd.Move[0] == 1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveForward;
                        move = ent.Transform.Basis.Column1 * PENETRATION_TEST_OFFSET;
                        if (ent.CheckNextPenetration(move) == 0)
                        {
                            ssAnim.NextState = TR_STATE.LaraJumpForward;
                        }
                    }
                    else if (cmd.Move[0] == -1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveBackward;
                        move = ent.Transform.Basis.Column1 * -PENETRATION_TEST_OFFSET;
                        if (ent.CheckNextPenetration(move) == 0)
                        {
                            ssAnim.NextState = TR_STATE.LaraJumpBack;
                        }
                    }
                    else if (cmd.Move[1] == 1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveRight;
                        move = ent.Transform.Basis.Column0 * PENETRATION_TEST_OFFSET;
                        if (ent.CheckNextPenetration(move) == 0)
                        {
                            ssAnim.NextState = TR_STATE.LaraJumpLeft;
                        }
                    }
                    else if (cmd.Move[1] == -1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveLeft;
                        move = ent.Transform.Basis.Column0 * -PENETRATION_TEST_OFFSET;
                        if (ent.CheckNextPenetration(move) == 0)
                        {
                            ssAnim.NextState = TR_STATE.LaraJumpRight;
                        }
                    }
                    break;
                case TR_STATE.LaraJumpBack:
                    ent.Bt.NoFixBodyParts = BODY_PART.Legs | BODY_PART.Hands | BODY_PART.Head;
                    cmd.Rotation.X = 0.0f;

                    if (resp.VerticalCollide.HasFlagSig(0x01) || ent.MoveType == MoveType.OnFloor)
                    {
                        if (currFc.Quicksand == QuicksandPosition.None)
                        {
                            ssAnim.NextState = TR_STATE.LaraStop;
                        }
                        else
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraStayIdle, 0);
                        }
                    }
                    else if (resp.HorizontalCollide.HasFlagSig(0x01))
                    {
                        Controls.JoyRumble(200.0f, 200);
                        ent.SetAnimation(TR_ANIMATION.LaraSmashJump, 0);
                        ent.DirFlag = ENT_MOVE.MoveForward;
                        ent.UpdateCurrentSpeed(true);
                    }
                    else if (ent.MoveType == MoveType.Underwater || ent.Speed.Z <= -FREE_FALL_SPEED_2)
                    {
                        ssAnim.NextState = TR_STATE.LaraFreefall;
                    }
                    else if (cmd.Roll)
                    {
                        ssAnim.NextState = TR_STATE.LaraJumpRoll;
                    }
                    break;
                case TR_STATE.LaraJumpLeft:
                    ent.Bt.NoFixBodyParts = BODY_PART.Legs | BODY_PART.Hands | BODY_PART.Head;
                    cmd.Rotation.X = 0.0f;

                    if (resp.VerticalCollide.HasFlagSig(0x01) || ent.MoveType == MoveType.OnFloor)
                    {
                        if (currFc.Quicksand == QuicksandPosition.None)
                        {
                            ssAnim.NextState = TR_STATE.LaraStop;
                        }
                        else
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraStayIdle, 0);
                        }
                    }
                    else if (resp.HorizontalCollide.HasFlagSig(0x01))
                    {
                        Controls.JoyRumble(200.0f, 200);
                        ent.SetAnimation(TR_ANIMATION.LaraSmashJump, 0);
                        ent.DirFlag = ENT_MOVE.MoveRight;
                        ent.UpdateCurrentSpeed(true);
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraFreefall;
                    }
                    break;
                case TR_STATE.LaraJumpRight:
                    ent.Bt.NoFixBodyParts = BODY_PART.Legs | BODY_PART.Hands | BODY_PART.Head;
                    cmd.Rotation.X = 0.0f;

                    if (resp.VerticalCollide.HasFlagSig(0x01) || ent.MoveType == MoveType.OnFloor)
                    {
                        if (currFc.Quicksand == QuicksandPosition.None)
                        {
                            ssAnim.NextState = TR_STATE.LaraStop;
                        }
                        else
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraStayIdle, 0);
                        }
                    }
                    else if (resp.HorizontalCollide.HasFlagSig(0x01))
                    {
                        Controls.JoyRumble(200.0f, 200);
                        ent.SetAnimation(TR_ANIMATION.LaraSmashJump, 0);
                        ent.DirFlag = ENT_MOVE.MoveLeft;
                        ent.UpdateCurrentSpeed(true);
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraFreefall;
                    }
                    break;
                case TR_STATE.LaraRunBack:
                    ent.DirFlag = ENT_MOVE.MoveBackward;

                    if (ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.DirFlag = ENT_MOVE.MoveForward;
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallBack, 0);
                    }
                    else if (resp.HorizontalCollide.HasFlagSig(0x01))
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraClimb2clickEnd, 0);
                    }
                    break;
                case TR_STATE.LaraTurnLeftSlow:
                case TR_STATE.LaraTurnRightSlow:
                    cmd.Rotation.X *= 0.7f;
                    ent.DirFlag = ENT_MOVE.Stay;
                    ent.Lean(cmd, 0.0f);
                    ent.Bt.NoFixBodyParts = BODY_PART.Legs2 | BODY_PART.Legs3;

                    if (cmd.Move[0] == 1)
                    {
                        var substanceState = ent.GetSubstanceState();
                        if (substanceState == Substance.None || substanceState == Substance.WaterShallow)
                        {
                            ssAnim.NextState = cmd.Shift ? TR_STATE.LaraWalkForward : TR_STATE.LaraRunForward;
                        }
                        else
                        {
                            ssAnim.NextState = TR_STATE.LaraWadeForward;
                        }
                        ent.DirFlag = ENT_MOVE.MoveForward;
                    }
                    else if ((ssAnim.LastState == TR_STATE.LaraTurnLeftSlow && cmd.Move[1] == -1) ||
                             (ssAnim.LastState == TR_STATE.LaraTurnRightSlow && cmd.Move[1] == 1))
                    {
                        var substanceState = ent.GetSubstanceState();
                        if (lastFrame &&
                            substanceState.NotEqual(Substance.WaterWade, Substance.QuicksandConsumed,
                                Substance.QuicksandShallow))
                        {
                            ssAnim.NextState = TR_STATE.LaraTurnFast;
                        }
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }
                    break;
                case TR_STATE.LaraTurnFast:
                    ent.DirFlag = ENT_MOVE.Stay;
                    ent.Bt.NoFixBodyParts = BODY_PART.Legs2 | BODY_PART.Legs3;
                    ent.Lean(cmd, 0.0f);

                    if (ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraStartFreeFall, 0);
                    }
                    else if (cmd.Move[0] == 1 && !cmd.Jump && !cmd.Crouch)
                    {
                        ssAnim.NextState = cmd.Shift ? TR_STATE.LaraWalkForward : TR_STATE.LaraRunForward;
                        ent.DirFlag = ENT_MOVE.MoveForward;
                    }
                    else if (cmd.Move[1] == 0)
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }
                    break;

                    #endregion

                    #region Run and walk animations

                case TR_STATE.LaraRunForward:
                    globalOffset = ent.Transform.Basis.Column1 * RUN_FORWARD_OFFSET;
                    globalOffset.Z += ent.Bf.BBMax.Z;
                    nextStep = ent.CheckNextStep(globalOffset, nextFc);
                    ent.DirFlag = ENT_MOVE.MoveForward;
                    cmd.Crouch |= lowVerticalSpace;

                    if (ent.MoveType == MoveType.OnFloor)
                        ent.Bt.NoFixBodyParts = BODY_PART.Hands | BODY_PART.Legs;

                    ent.Lean(cmd, 6.0f);

                    if (ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallForward, 0);
                    }
                    else if (resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraDeath;
                    }
                    else if (resp.Slide == SlideType.Front)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraSlideForward, 0);
                    }
                    else if (resp.Slide == SlideType.Back)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraStartSlideBackward, 0);
                        ent.DirFlag = ENT_MOVE.MoveBackward;
                    }
                    else if (ent.HasStopSlant(nextFc))
                    {
                        ent.DirFlag = ENT_MOVE.Stay;
                        ent.SetAnimation(TR_ANIMATION.LaraStayIdle, 0);
                    }
                    else if (cmd.Crouch)
                    {
                        ssAnim.NextState = TR_STATE.LaraCrouchIdle;
                    }
                    else if (cmd.Move[0] == 1 && !cmd.Crouch && nextFc.FloorNormale.Z >= ent.CriticalSlantZComponent &&
                             nextStep == StepType.UpBig)
                    {
                        ent.DirFlag = ENT_MOVE.Stay;
                        i = ent.GetAnimDispatchCase(TR_STATE.LaraStop); // Select correct anim dispatch.
                        ent.SetAnimation(i == 0 ? TR_ANIMATION.LaraRunUpStepRight : TR_ANIMATION.LaraRunUpStepLeft,
                            0);
                        pos.Z = nextFc.FloorPoint.Z;
                        ent.DirFlag = ENT_MOVE.MoveForward;
                    }
                    else if (resp.HorizontalCollide.HasFlagSig(0x01))
                    {
                        globalOffset = ent.Transform.Basis.Column1 * RUN_FORWARD_OFFSET;
                        globalOffset.Z += 1024.0f;
                        if (ssAnim.CurrentAnimation == TR_ANIMATION.LaraStayToRun)
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraStayIdle, 0);
                        }
                        else
                        {
                            Controls.JoyRumble(200.0f, 200);

                            if (cmd.Move[0] == 1)
                            {
                                i = ent.GetAnimDispatchCase(TR_STATE.LaraStop);
                                ent.SetAnimation(
                                    i == 1 ? TR_ANIMATION.LaraWallSmashLeft : TR_ANIMATION.LaraWallSmashRight, 0);
                            }
                            else
                            {
                                ent.SetAnimation(TR_ANIMATION.LaraStaySolid, 0);
                            }
                        }
                        ent.UpdateCurrentSpeed(false);
                    }
                    else if (cmd.Move[0] == 1)
                    {
                        if ((currFc.Water || currFc.Quicksand != QuicksandPosition.None) && currFc.FloorHit &&
                            currFc.TransitionLevel - currFc.FloorPoint.Z > ent.WadeDepth)
                        {
                            ssAnim.NextState = TR_STATE.LaraWadeForward;
                        }
                        else if (cmd.Shift)
                        {
                            ssAnim.NextState = TR_STATE.LaraWalkForward;
                        }
                        else if (cmd.Jump && ssAnim.LastAnimation != TR_ANIMATION.LaraStayToRun)
                        {
                            ssAnim.NextState = TR_STATE.LaraJumpForward;
                        }
                        else if (cmd.Roll)
                        {
                            ent.DirFlag = ENT_MOVE.MoveForward;
                            ent.SetAnimation(TR_ANIMATION.LaraRollBegin, 0);
                        }
                        else if (cmd.Sprint)
                        {
                            ssAnim.NextState = TR_STATE.LaraSprint;
                        }
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }
                    break;
                case TR_STATE.LaraSprint:
                    globalOffset = ent.Transform.Basis.Column1 * RUN_FORWARD_OFFSET;
                    ent.Lean(cmd, 12.0f);
                    globalOffset.Z += ent.Bf.BBMax.Z;
                    nextStep = ent.CheckNextStep(globalOffset, nextFc);
                    cmd.Crouch |= lowVerticalSpace;

                    if (ent.MoveType == MoveType.OnFloor)
                    {
                        ent.Bt.NoFixBodyParts = BODY_PART.Legs;
                    }

                    if (ent.GetParam(CharParameters.Stamina) == 0)
                    {
                        ssAnim.NextState = TR_STATE.LaraRunForward;
                    }
                    else if (ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallForward, 0);
                    }
                    else if (resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraRunForward; // Normal run then die
                    }
                    else if (resp.Slide == SlideType.Front)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraSlideForward, 0);
                    }
                    else if (resp.Slide == SlideType.Back)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraStartSlideBackward, 0);
                    }
                    else if (nextFc.FloorNormale.Z < ent.CriticalSlantZComponent && nextStep > StepType.Horizontal)
                    {
                        ent.CurrentSpeed = 0.0f;
                        ent.SetAnimation(TR_ANIMATION.LaraStayIdle, 0);
                    }
                    else if (nextFc.FloorNormale.Z >= ent.CriticalSlantZComponent && nextStep == StepType.UpBig)
                    {
                        ssAnim.NextState = TR_STATE.LaraRunForward; // Interrupt sprint
                    }
                    else if (resp.HorizontalCollide.HasFlagSig(0x01))
                    {
                        Controls.JoyRumble(200.0f, 200);
                        ent.SetAnimation(
                            ent.GetAnimDispatchCase(TR_STATE.LaraStop) == 1
                                ? TR_ANIMATION.LaraWallSmashLeft
                                : TR_ANIMATION.LaraWallSmashRight, 0);
                        ent.UpdateCurrentSpeed(false);
                    }
                    else if (!cmd.Sprint)
                    {
                        ssAnim.NextState = cmd.Move[0] == 1 ? TR_STATE.LaraRunForward : TR_STATE.LaraStop;
                    }
                    else
                    {
                        if (cmd.Jump)
                        {
                            ssAnim.NextState = TR_STATE.LaraSprintRoll;
                        }
                        else if (cmd.Roll)
                        {
                            ent.DirFlag = ENT_MOVE.MoveForward;
                            ent.SetAnimation(TR_ANIMATION.LaraRollBegin);
                        }
                        else if (cmd.Crouch)
                        {
                            ssAnim.NextState = TR_STATE.LaraCrouchIdle;
                        }
                        else if (cmd.Move[0] == 0)
                        {
                            ssAnim.NextState = TR_STATE.LaraStop;
                        }
                    }
                    break;
                case TR_STATE.LaraWalkForward:
                    cmd.Rotation.X *= 0.4f;
                    ent.Lean(cmd, 0.0f);

                    globalOffset = ent.Transform.Basis.Column1 * WALK_FORWARD_OFFSET;
                    globalOffset.Z += ent.Bf.BBMax.Z;
                    nextStep = ent.CheckNextStep(globalOffset, nextFc);
                    ent.DirFlag = ENT_MOVE.MoveForward;

                    if (ent.MoveType == MoveType.OnFloor)
                    {
                        ent.Bt.NoFixBodyParts = BODY_PART.Legs;
                    }

                    if (ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraStartFreeFall, 0);
                    }
                    else if (resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }
                    else if (nextFc.FloorNormale.Z >= ent.CriticalSlantZComponent &&
                             nextStep.IsAnyOf(StepType.UpBig, StepType.DownBig))
                    {
                        // Climb up or down

                        ent.DirFlag = ENT_MOVE.Stay;
                        i = ent.GetAnimDispatchCase(TR_STATE.LaraStop);
                        if (nextStep == StepType.DownBig)
                        {
                            ent.SetAnimation(i == 1 ? TR_ANIMATION.LaraWalkDownRight : TR_ANIMATION.LaraWalkDownLeft, 0);
                            climb.Point = nextFc.FloorPoint;
                        }
                        else
                        {
                            ent.SetAnimation(
                                i == 1 ? TR_ANIMATION.LaraWalkUpStepRight : TR_ANIMATION.LaraWalkUpStepLeft, 0);
                        }

                        pos = nextFc.FloorPoint;
                        ent.MoveType = MoveType.OnFloor;
                        ent.DirFlag = ENT_MOVE.MoveForward;
                    }
                    else if (resp.HorizontalCollide.HasFlagSig(0x01) || !nextStep.IsWalkableStep() || lowVerticalSpace)
                    {
                        // Too high!

                        ent.DirFlag = ENT_MOVE.Stay;
                        ent.SetAnimation(TR_ANIMATION.LaraStayIdle, 0);
                    }
                    else if (cmd.Move[0] != 1)
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }
                    else if ((currFc.Water || currFc.Quicksand != QuicksandPosition.None) && currFc.FloorHit &&
                             currFc.TransitionLevel - currFc.FloorPoint.Z > ent.WadeDepth)
                    {
                        ssAnim.NextState = TR_STATE.LaraWadeForward;
                    }
                    else if (cmd.Move[0] == 1 && !cmd.Crouch && !cmd.Shift)
                    {
                        ssAnim.NextState = TR_STATE.LaraRunForward;
                    }
                    break;
                case TR_STATE.LaraWadeForward:
                    cmd.Rotation.X *= 0.4f;
                    ent.DirFlag = ENT_MOVE.MoveForward;

                    if (ent.HeightInfo.Quicksand != QuicksandPosition.None)
                    {
                        ent.CurrentSpeed = Math.Min(ent.CurrentSpeed, MAX_SPEED_QUICKSAND);
                    }

                    if (cmd.Move[0] == 1)
                    {
                        move = ent.Transform.Basis.Column1 * PENETRATION_TEST_OFFSET;
                        ent.CheckNextPenetration(move);
                    }

                    if (resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }

                    if (!currFc.FloorHit || ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraStartFreeFall, 0);
                    }
                    else if (currFc.Water)
                    {
                        var tlfp = currFc.TransitionLevel - currFc.FloorPoint.Z;
                        if (tlfp <= ent.WadeDepth)
                        {
                            // Run / walk
                            if (cmd.Move[0] == 1 && resp.HorizontalCollide == 0)
                            {
                                ssAnim.NextState = TR_STATE.LaraRunForward;
                            }
                            else
                            {
                                ssAnim.NextState = TR_STATE.LaraStop;
                            }
                        }
                        else if (tlfp > ent.Height - ent.SwimDepth)
                        {
                            // Swim
                            if (tlfp > ent.Height + ent.MaxStepUpHeight)
                            {
                                ent.SetAnimation(TR_ANIMATION.LaraStartFreeFall, 0); // swim underwater
                            }
                            else
                            {
                                ent.SetAnimation(TR_ANIMATION.LaraOnwaterIdle, 0);
                                ent.MoveType = MoveType.OnWater;
                                pos.Z = currFc.TransitionLevel;
                            }
                        }
                        else if (tlfp > ent.WadeDepth)
                        {
                            // Wade
                            if (cmd.Move[0] != 1 && resp.HorizontalCollide != 0)
                            {
                                ssAnim.NextState = TR_STATE.LaraStop;
                            }
                        }
                    }
                    else
                    {
                        // No water, stay on run / walk
                        if (cmd.Move[0] == 1 && resp.HorizontalCollide == 0)
                        {
                            if (currFc.Quicksand == QuicksandPosition.None)
                            {
                                ssAnim.NextState = TR_STATE.LaraRunForward;
                            }
                        }
                        else
                        {
                            ssAnim.NextState = TR_STATE.LaraStop;
                        }
                    }
                    break;
                case TR_STATE.LaraWalkBack:
                    cmd.Rotation.X *= 0.4f;
                    ent.DirFlag = ENT_MOVE.MoveBackward;

                    if (ent.HeightInfo.Quicksand != QuicksandPosition.None)
                    {
                        ent.CurrentSpeed = Math.Min(ent.CurrentSpeed, MAX_SPEED_QUICKSAND);
                    }

                    globalOffset = ent.Transform.Basis.Column1 * -WALK_BACK_OFFSET;
                    globalOffset.Z += ent.Bf.BBMax.Z;
                    nextStep = ent.CheckNextStep(globalOffset, nextFc);
                    if (ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraStartFreeFall, 0);
                    }
                    else if (currFc.Water && currFc.FloorPoint.Z + ent.Height < currFc.TransitionLevel)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraOnwaterSwimBack, 0);
                        ssAnim.NextState = TR_STATE.LaraOnwaterBack;
                        ent.MoveType = MoveType.OnWater;
                    }
                    else if (!nextStep.IsWalkableStep())
                    {
                        ent.DirFlag = ENT_MOVE.Stay;
                        ent.SetAnimation(TR_ANIMATION.LaraClimb2clickEnd, 0);
                    }
                    else if (nextFc.FloorNormale.Z >= ent.CriticalSlantZComponent && nextStep == StepType.DownBig)
                    {
                        if (!ent.Bt.NoFixAll)
                        {
                            var framesCount =
                                ssAnim.Model.Animations[(int) TR_ANIMATION.LaraWalkDownBackLeft].Frames.Count;
                            var framesCount2 = (framesCount + 1) / 2;
                            var b = true;
                            if (ssAnim.CurrentFrame >= 0 && ssAnim.CurrentFrame <= framesCount2)
                            {
                                ent.SetAnimation(TR_ANIMATION.LaraWalkDownBackLeft, ssAnim.CurrentFrame);
                            }
                            else if (ssAnim.CurrentFrame >= framesCount &&
                                     ssAnim.CurrentFrame <= framesCount + framesCount2)
                            {
                                ent.SetAnimation(TR_ANIMATION.LaraWalkDownBackRight,
                                    ssAnim.CurrentFrame - framesCount);
                            }
                            else
                            {
                                b = false;
                            }
                            if (b)
                            {
                                ent.DirFlag = ENT_MOVE.MoveBackward;
                                ent.Transform.Origin.Z -= currFc.FloorPoint.Z - nextFc.FloorPoint.Z;
                                ent.Bt.NoFixAll = true;
                            }
                            else
                            {
                                ent.DirFlag = ENT_MOVE.Stay; // Waiting for correct frame
                            }
                        }
                    }
                    else if (cmd.Move[0] == -1 && (cmd.Shift || ent.HeightInfo.Quicksand != QuicksandPosition.None))
                    {
                        ent.DirFlag = ENT_MOVE.MoveBackward;
                        ssAnim.NextState = TR_STATE.LaraWalkBack;
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }
                    break;
                case TR_STATE.LaraWalkLeft:
                case TR_STATE.LaraWalkRight:
                    var r = ssAnim.LastState == TR_STATE.LaraWalkRight;
                    var n = r ? 1 : -1;

                    cmd.Rotation.X = 0;
                    ent.DirFlag = r ? ENT_MOVE.MoveRight : ENT_MOVE.MoveLeft;

                    if (ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraStartFreeFall, 0);
                    }
                    else if (cmd.Move[1] == n && cmd.Shift)
                    {
                        globalOffset = ent.Transform.Basis.Column0 * (RUN_FORWARD_OFFSET * n);
                        globalOffset.Z += ent.Bf.BBMax.Z;
                        globalOffset += pos;
                        Character.GetHeightInfo(globalOffset, nextFc);
                        if (nextFc.FloorHit && nextFc.FloorPoint.Z > pos.Z - ent.MaxStepUpHeight &&
                            nextFc.FloorPoint.Z <= pos.Z + ent.MaxStepUpHeight)
                        {
                            if (currFc.Water && !(currFc.FloorPoint.Z + ent.Height > currFc.TransitionLevel))
                            {
                                ssAnim.NextState = r ? TR_STATE.LaraOnwaterRight : TR_STATE.LaraOnwaterLeft;
                                ssAnim.SetOnFrame(ent_to_on_water);
                            }
                            // else continue walking
                        }
                        else
                        {
                            ent.DirFlag = ENT_MOVE.Stay;
                            ent.SetAnimation(TR_ANIMATION.LaraStaySolid, 0);
                        }
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }
                    break;

                    #endregion

                    #region Slide animations

                case TR_STATE.LaraSlideBack:
                    cmd.Rotation.X = 0;
                    ent.Lean(cmd, 0.0f);
                    ent.DirFlag = ENT_MOVE.MoveBackward;

                    if (ent.MoveType == MoveType.FreeFalling)
                    {
                        if (cmd.Action)
                        {
                            ent.Speed.X = -ent.Transform.Basis.Column1.X * 128.0f;
                            ent.Speed.Y = -ent.Transform.Basis.Column1.Y * 128.0f;
                        }

                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallBack, 0);
                    }
                    else if (resp.Slide == SlideType.None)
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }
                    else if (resp.Slide != SlideType.None && cmd.Jump)
                    {
                        ssAnim.NextState = TR_STATE.LaraJumpBack;
                    }
                    else
                    {
                        break;
                    }

                    Audio.Kill((int) TR_AUDIO_SOUND.Sliding, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                    break;
                case TR_STATE.LaraSlideForward:
                    cmd.Rotation.X = 0;
                    ent.Lean(cmd, 0.0f);
                    ent.DirFlag = ENT_MOVE.MoveForward;

                    if (ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallForward, 0);
                    }
                    else if (resp.Slide == SlideType.None)
                    {
                        if (cmd.Move[0] == 1 && Global.EngineWorld.EngineVersion >= Loader.Engine.TR3)
                        {
                            ssAnim.NextState = TR_STATE.LaraRunForward;
                        }
                        else
                        {
                            ssAnim.NextState = TR_STATE.LaraStop;
                        }
                    }
                    else if (resp.Slide != SlideType.None && cmd.Jump)
                    {
                        ssAnim.NextState = TR_STATE.LaraJumpBack;
                    }
                    else
                    {
                        break;
                    }

                    Audio.Kill((int) TR_AUDIO_SOUND.Sliding, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                    break;

                    #endregion

                    #region Miscellaneous animations

                case TR_STATE.LaraPushableGrab:
                    ent.MoveType = MoveType.OnFloor;
                    ent.Bt.NoFixAll = true;
                    cmd.Rotation.X = 0.0f;

                    if (cmd.Action)
                    {
                        // If Lara is grabbing the block
                        var tf = ent.CheckTraverse(ent.TraversedObject);
                        ent.DirFlag = ENT_MOVE.Stay;
                        ssAnim.AnimFlags = AnimControlFlags.LoopLastFrame; // We hold it (loop last frame)

                        if (cmd.Move[0] == 1 && tf.HasFlagSig(TraverseForward))
                        {
                            // If player presses up, then push
                            ent.DirFlag = ENT_MOVE.MoveForward;
                            ssAnim.AnimFlags = AnimControlFlags.NormalControl;
                            ssAnim.NextState = TR_STATE.LaraPushablePush;
                        }
                        else if (cmd.Move[0] == -1 && tf.HasFlagSig(TraverseBackward))
                        {
                            // If player presses down, then pull
                            ent.DirFlag = ENT_MOVE.MoveBackward;
                            ssAnim.AnimFlags = AnimControlFlags.NormalControl;
                            ssAnim.NextState = TR_STATE.LaraPushablePull;
                        }
                    }
                    else
                    {
                        // Lara has let go of the block
                        ent.DirFlag = ENT_MOVE.Stay;
                        ssAnim.AnimFlags = AnimControlFlags.NormalControl; // We're no longer looping last frame
                        ssAnim.NextState = TR_STATE.LaraStop; // Switch to next Lara state
                    }
                    break;
                case TR_STATE.LaraPushablePush:
                    ent.Bt.NoFixAll = true;
                    ssAnim.SetOnFrame(ent_stop_traverse);
                    cmd.Rotation.X = 0.0f;
                    ent.CamFollowCenter = 64;
                    i = ssAnim.Model.Animations[(int) ssAnim.CurrentAnimation].Frames.Count;

                    if (!cmd.Action || !(TraverseForward & ent.CheckTraverse(ent.TraversedObject)).ToBool())
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }

                    if (ent.TraversedObject != null && ssAnim.CurrentFrame > 16 && ssAnim.CurrentFrame < i - 16)
                    {
                        var wasTraversed = false;
                        var c1 = ent.Transform.Basis.Column1;
                        var or = ent.Transform.Origin;

                        if (c1.X > 0.9)
                        {
                            t = or.X + (ent.Bf.BBMax.Y - ent.TraversedObject.Bf.BBMin.X - 32.0f);
                            if (t > ent.TraversedObject.Transform.Origin.X)
                            {
                                ent.TraversedObject.Transform.Origin.X = t;
                                wasTraversed = true;
                            }
                        }
                        else if (c1.X < -0.9)
                        {
                            t = or.X - (ent.Bf.BBMax.Y + ent.TraversedObject.Bf.BBMax.X - 32.0f);
                            if (t < ent.TraversedObject.Transform.Origin.X)
                            {
                                ent.TraversedObject.Transform.Origin.X = t;
                                wasTraversed = true;
                            }
                        }
                        else if (c1.Y > 0.9)
                        {
                            t = or.Y + (ent.Bf.BBMax.Y - ent.TraversedObject.Bf.BBMin.Y - 32.0f);
                            if (t > ent.TraversedObject.Transform.Origin.Y)
                            {
                                ent.TraversedObject.Transform.Origin.Y = t;
                                wasTraversed = true;
                            }
                        }
                        else if (c1.Y < -0.9)
                        {
                            t = or.Y - (ent.Bf.BBMax.Y + ent.TraversedObject.Bf.BBMax.Y - 32.0f);
                            if (t < ent.TraversedObject.Transform.Origin.Y)
                            {
                                ent.TraversedObject.Transform.Origin.Y = t;
                                wasTraversed = true;
                            }
                        }

                        if (Global.EngineWorld.EngineVersion > Loader.Engine.TR3)
                        {
                            if (wasTraversed)
                            {
                                if (
                                    Audio.IsEffectPlaying((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity,
                                        (int) ent.ID) == -1)
                                {
                                    Audio.Send((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                                }
                            }
                            else
                            {
                                Audio.Kill((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                            }
                        }
                        else
                        {
                            if (ssAnim.IsAnyOf(49, 110, 142))
                            {
                                if (
                                    Audio.IsEffectPlaying((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity,
                                        (int) ent.ID) == -1)
                                {
                                    Audio.Send((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                                }
                            }
                        }

                        ent.TraversedObject.UpdateRigidBody(true);
                    }
                    else
                    {
                        if (Global.EngineWorld.EngineVersion > Loader.Engine.TR3)
                        {
                            Audio.Kill((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                        }
                    }
                    break;
                case TR_STATE.LaraPushablePull:
                    ent.Bt.NoFixAll = true;
                    ssAnim.SetOnFrame(ent_stop_traverse);
                    cmd.Rotation.X = 0.0f;
                    ent.CamFollowCenter = 64;
                    i = ssAnim.Model.Animations[(int) ssAnim.CurrentAnimation].Frames.Count;

                    if (!cmd.Action || !(TraverseBackward & ent.CheckTraverse(ent.TraversedObject)).ToBool())
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }

                    if (ent.TraversedObject != null && ssAnim.CurrentFrame > 20 && ssAnim.CurrentFrame < i - 16)
                    {
                        var wasTraversed = false;
                        var c1 = ent.Transform.Basis.Column1;
                        var or = ent.Transform.Origin;

                        if (c1.X > 0.9)
                        {
                            t = or.X + (ent.Bf.BBMax.Y - ent.TraversedObject.Bf.BBMin.X - 32.0f);
                            if (t < ent.TraversedObject.Transform.Origin.X)
                            {
                                ent.TraversedObject.Transform.Origin.X = t;
                                wasTraversed = true;
                            }
                        }
                        else if (c1.X < -0.9)
                        {
                            t = or.X - (ent.Bf.BBMax.Y + ent.TraversedObject.Bf.BBMax.X - 32.0f);
                            if (t > ent.TraversedObject.Transform.Origin.X)
                            {
                                ent.TraversedObject.Transform.Origin.X = t;
                                wasTraversed = true;
                            }
                        }
                        else if (c1.Y > 0.9)
                        {
                            t = or.Y + (ent.Bf.BBMax.Y - ent.TraversedObject.Bf.BBMin.Y - 32.0f);
                            if (t < ent.TraversedObject.Transform.Origin.Y)
                            {
                                ent.TraversedObject.Transform.Origin.Y = t;
                                wasTraversed = true;
                            }
                        }
                        else if (c1.Y < -0.9)
                        {
                            t = or.Y - (ent.Bf.BBMax.Y + ent.TraversedObject.Bf.BBMax.Y - 32.0f);
                            if (t > ent.TraversedObject.Transform.Origin.Y)
                            {
                                ent.TraversedObject.Transform.Origin.Y = t;
                                wasTraversed = true;
                            }
                        }

                        if (Global.EngineWorld.EngineVersion > Loader.Engine.TR3)
                        {
                            if (wasTraversed)
                            {
                                if (
                                    Audio.IsEffectPlaying((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity,
                                        (int) ent.ID) == -1)
                                {
                                    Audio.Send((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                                }
                            }
                            else
                            {
                                Audio.Kill((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                            }
                        }
                        else
                        {
                            if (ssAnim.IsAnyOf(40, 92, 124, 156))
                            {
                                if (
                                    Audio.IsEffectPlaying((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity,
                                        (int) ent.ID) == -1)
                                {
                                    Audio.Send((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                                }
                            }
                        }

                        ent.TraversedObject.UpdateRigidBody(true);
                    }
                    else
                    {
                        if (Global.EngineWorld.EngineVersion > Loader.Engine.TR3)
                        {
                            Audio.Kill((int) TR_AUDIO_SOUND.Pushable, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                        }
                    }
                    break;
                case TR_STATE.LaraRollForward:
                    ent.Bt.NoFixBodyParts = BODY_PART.Legs;
                    break;
                case TR_STATE.LaraRollBackward:
                    ent.Bt.NoFixBodyParts = BODY_PART.Hands;
                    if (ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallForward, 0);
                    }
                    else if (lowVerticalSpace)
                    {
                        ent.DirFlag = ENT_MOVE.Stay;
                    }
                    else if (resp.Slide == SlideType.Front)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraSlideForward, 0);
                    }
                    else if (resp.Slide == SlideType.Back)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraStartSlideBackward, 0);
                    }
                    break;

                    #endregion

                    #region Climbing animations

                case TR_STATE.LaraJumpUp:
                    cmd.Rotation.X = 0.0f;
                    if (cmd.Action && ent.MoveType != MoveType.WallsClimb && ent.MoveType != MoveType.Climbing)
                    {
                        t = LARA_TRY_HANG_WALL_OFFSET + LARA_HANG_WALL_DISTANCE;
                        globalOffset = ent.Transform.Basis.Column1 * t;
                        globalOffset.Z += ent.Bf.BBMax.Z + LARA_HANG_VERTICAL_EPSILON +
                                          Global.EngineFrameTime * ent.Speed.Z;
                        climb = ent.CheckClimbability(globalOffset, nextFc, 0.0f);
                        if (climb.EdgeHit)
                        {
                            climb.Point = climb.EdgePoint;
                            ent.Angles.X = climb.EdgeZAngle;
                            ent.UpdateTransform();
                            ent.MoveType = MoveType.Climbing; // hang on
                            ent.Speed = Vector3.Zero;

                            pos.X = climb.Point.X - LARA_HANG_WALL_DISTANCE * ent.Transform.Basis.Column1.X;
                            pos.Y = climb.Point.Y - LARA_HANG_WALL_DISTANCE * ent.Transform.Basis.Column1.Y;
                            pos.Z = climb.Point.Z - ent.Bf.BBMax.Z + LARA_HANG_VERTICAL_OFFSET;
                        }
                        else
                        {
                            climb = ent.CheckWallsClimbability();
                            if (climb.WallHit != ClimbType.None && ent.Speed.Z < 0.0f)
                            {
                                // Fix the position to the TR metering step.
                                ent.Transform.Origin.Z =
                                    (float) Math.Floor(ent.Transform.Origin.Z / TR_METERING_STEP) *
                                    TR_METERING_STEP;
                                ent.MoveType = MoveType.WallsClimb;
                                ent.SetAnimation(TR_ANIMATION.LaraHangIdle, -1);
                                break;
                            }
                        }
                    }

                    if (cmd.Move[0] == 1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveForward;
                    }
                    else if (cmd.Move[0] == -1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveBackward;
                    }
                    else if (cmd.Move[1] == 1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveRight;
                    }
                    else if (cmd.Move[1] == -1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveLeft;
                    }
                    else
                    {
                        ent.DirFlag = ENT_MOVE.MoveForward; // Lara can move forward towards walls in this state
                    }

                    if (ent.MoveType == MoveType.Underwater)
                    {
                        ent.Angles.Y = -45.0f;
                        cmd.Rotation.Y = 0.0f;
                        ent.UpdateTransform();
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallToUnderwater, 0);
                    }
                    else if (cmd.Action && currFc.CeilingClimb && currFc.CeilingHit &&
                             pos.Z + ent.Bf.BBMax.Z > currFc.CeilingPoint.Z - 64.0f)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingIdle;
                        ssAnim.SetOnFrame(ent_to_monkey_swing);
                    }
                    else if (cmd.Action && ent.MoveType == MoveType.Climbing)
                    {
                        ssAnim.NextState = TR_STATE.LaraHang;
                        ent.SetAnimation(TR_ANIMATION.LaraHangIdle, -1);
                    }
                    else if (resp.VerticalCollide.HasFlagSig(0x01) || ent.MoveType == MoveType.OnFloor)
                    {
                        ssAnim.NextState = TR_STATE.LaraStop; // Landing immediately
                    }
                    else
                    {
                        if (ent.Speed.Z < -FREE_FALL_SPEED_2) // Next free fall stage
                        {
                            ent.MoveType = MoveType.FreeFalling;
                            ssAnim.NextState = TR_STATE.LaraFreefall;
                        }
                    }
                    break;
                case TR_STATE.LaraReach:
                    ent.Bt.NoFixBodyParts = BODY_PART.Legs | BODY_PART.Hands1 | BODY_PART.Hands2;
                    cmd.Rotation.X = 0.0f;
                    if (ent.MoveType == MoveType.Underwater)
                    {
                        ent.Angles.Y = -45.0f;
                        cmd.Rotation.Y = 0.0f;
                        ent.UpdateTransform();
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallToUnderwater, 0);
                        break;
                    }

                    if (cmd.Action && ent.MoveType == MoveType.FreeFalling)
                    {
                        t = LARA_TRY_HANG_WALL_OFFSET + LARA_HANG_WALL_DISTANCE;
                        globalOffset = ent.Transform.Basis.Column1 * t;
                        globalOffset.Z += ent.Bf.BBMax.Z + LARA_HANG_VERTICAL_EPSILON +
                                          Global.EngineFrameTime * ent.Speed.Z;
                        climb = ent.CheckClimbability(globalOffset, nextFc, 0.0f);
                        if (climb.EdgeHit && climb.CanHang)
                        {
                            climb.Point = climb.EdgePoint;
                            ent.Angles.X = climb.EdgeZAngle;
                            ent.UpdateTransform();
                            ent.MoveType = MoveType.Climbing; // hang on
                            ent.Speed = Vector3.Zero;
                        }

                        // If Lara is moving backwards off the ledge we want to move Lara slightly forwards
                        // depending on the current angle.
                        if (ent.DirFlag == ENT_MOVE.MoveBackward && ent.MoveType == MoveType.Climbing)
                        {
                            pos.X = climb.Point.X - ent.Transform.Basis.Column1.X * (ent.ForwardSize + 16.0f);
                            pos.Y = climb.Point.Y - ent.Transform.Basis.Column1.Y * (ent.ForwardSize + 16.0f);
                        }
                    }

                    if (ent.MoveType != MoveType.OnFloor && cmd.Action && currFc.CeilingClimb && currFc.CeilingHit &&
                        pos.Z + ent.Bf.BBMax.Z > currFc.CeilingPoint.Z - 64.0f)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingIdle;
                        ssAnim.SetOnFrame(ent_to_monkey_swing);
                        break;
                    }
                    if (resp.VerticalCollide.HasFlagSig(0x01) ||
                        ent.MoveType == MoveType.OnFloor && (!cmd.Action || !climb.CanHang))
                    {
                        ssAnim.NextState = TR_STATE.LaraStop; // Middle landing
                        break;
                    }

                    if (ent.Speed.Z < -FREE_FALL_SPEED_2)
                    {
                        ent.MoveType = MoveType.FreeFalling;
                        ssAnim.NextState = TR_STATE.LaraFreefall;
                        break;
                    }

                    if (ent.MoveType == MoveType.Climbing)
                    {
                        ent.Speed = Vector3.Zero;
                        ssAnim.NextState = TR_STATE.LaraHang;
#if OSCILLATE_HANG_USE
                        move = ent.Transform.Basis.Column1 * Constants.PENETRATION_TEST_OFFSET;
                        if (ent.CheckNextPenetration(cmd, move) == 0)
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraOscillateHangOn, 0);
                            ent_to_edge_climb(ent);
                        }
#endif
                    }
                    break;

                // The code here prevents Lara's UGLY move in end of "climb on" states.
                // Do not delete ent_set_on_floor_after_climb callback here!

                case TR_STATE.LaraHandstand:
                case TR_STATE.LaraClimbing:
                case TR_STATE.LaraClimbToCrawl:
                    cmd.Rotation.X = 0;
                    ent.Bt.NoFixAll = true;
                    //ssAnim.SetOnFrame(ent_set_on_floor_after_climb); // FIXME: Buggy
                    break;
                case TR_STATE.LaraHang:
                    cmd.Rotation.X = 0.0f;

                    if (ent.MoveType == MoveType.WallsClimb)
                    {
                        if (cmd.Action)
                        {
                            if (climb.WallHit == ClimbType.FullBody && cmd.Move[0] == 0 && cmd.Move[1] == 0)
                            {
                                ssAnim.NextState = TR_STATE.LaraLadderIdle;
                            }
                            else if (cmd.Move[0] == 1) // UP
                            {
                                ent.SetAnimation(TR_ANIMATION.LaraLadderUpHands, 0);
                            }
                            else if (cmd.Move[0] == -1) // DOWN
                            {
                                ent.SetAnimation(TR_ANIMATION.LaraLadderDownHands, 0);
                            }
                            else if (cmd.Move[1] == 1) // RIGHT
                            {
                                ent.DirFlag = ENT_MOVE.MoveRight;
                                ent.SetAnimation(TR_ANIMATION.LaraClimbRight, 0); // Edge climb right
                            }
                            else if (cmd.Move[1] == -1)
                            {
                                ent.DirFlag = ENT_MOVE.MoveLeft;
                                ent.SetAnimation(TR_ANIMATION.LaraClimbLeft, 0); // Edge climb left
                            }
                            else if (climb.WallHit == ClimbType.None)
                            {
                                ent.MoveType = MoveType.FreeFalling;
                                ent.SetAnimation(TR_ANIMATION.LaraStopHangVertical, 0); // Fall down
                            }
                            else
                            {
                                ssAnim.AnimFlags = AnimControlFlags.LoopLastFrame; // Disable shake
                            }
                        }
                        else
                        {
                            ent.MoveType = MoveType.FreeFalling;
                            ent.SetAnimation(TR_ANIMATION.LaraStopHangVertical, 0); // Fall down
                        }
                        break;
                    }

                    if (!resp.Killed && cmd.Action)
                    {
                        t = LARA_TRY_HANG_WALL_OFFSET + LARA_HANG_WALL_DISTANCE;
                        globalOffset = ent.Transform.Basis.Column1 * t;
                        globalOffset.Z += ent.Bf.BBMax.Z + LARA_HANG_VERTICAL_EPSILON;
                        climb = ent.CheckClimbability(globalOffset, nextFc, 0.0f);
                        if (climb.CanHang)
                        {
                            climb.Point = climb.EdgePoint;
                            ent.Angles.X = climb.EdgeZAngle;
                            ent.UpdateTransform();
                            ent.MoveType = MoveType.Climbing; // Hang on
                        }
                    }
                    else
                    {
                        ent.MoveType = MoveType.FreeFalling;
                        ent.SetAnimation(TR_ANIMATION.LaraTryHangVertical, 0); // Fall down
                        break;
                    }

                    if (ent.MoveType == MoveType.Climbing)
                    {
                        if (cmd.Move[0] == 1)
                        {
                            if (climb.EdgeHit && climb.NextZSpace >= 512.0f &&
                                (climb.NextZSpace < ent.Height - LARA_HANG_VERTICAL_EPSILON || cmd.Crouch))
                            {
                                climb.Point = climb.EdgePoint;
                                ssAnim.NextState = TR_STATE.LaraClimbToCrawl; // Crawlspace climb
                            }
                            else if (climb.EdgeHit &&
                                     climb.NextZSpace >= ent.Height - LARA_HANG_VERTICAL_EPSILON)
                            {
                                Sys.DebugLog(LOG_FILENAME, "Zspace = {0}", climb.NextZSpace);
                                climb.Point = climb.EdgePoint;
                                ssAnim.NextState = cmd.Shift ? TR_STATE.LaraHandstand : TR_STATE.LaraClimbing;
                                // climb up
                            }
                            else
                            {
                                pos.X = climb.Point.X -
                                        LARA_HANG_WALL_DISTANCE * ent.Transform.Basis.Column1.X;
                                pos.Y = climb.Point.Y -
                                        LARA_HANG_WALL_DISTANCE * ent.Transform.Basis.Column1.Y;
                                pos.Z = climb.Point.Z - ent.Bf.BBMax.Z + LARA_HANG_VERTICAL_OFFSET;
                                ent.Speed = Vector3.Zero;
                                ssAnim.AnimFlags = AnimControlFlags.LoopLastFrame; // Disable shake
                            }
                        }
                        else if (cmd.Move[0] == -1) // Check walls climbing
                        {
                            climb = ent.CheckWallsClimbability();
                            if (climb.WallHit != ClimbType.None)
                            {
                                ent.MoveType = MoveType.WallsClimb;
                            }
                            ssAnim.AnimFlags = AnimControlFlags.LoopLastFrame; // Disable shake
                        }
                        else if (cmd.Move[1] == -1)
                        {
                            move = ent.Transform.Basis.Column0 * -PENETRATION_TEST_OFFSET;
                            if (ent.CheckNextPenetration(move) == 0 || ent.Response.HorizontalCollide == 0x00)
                                // We only want Lara to shimmy when last frame is reached!
                            {
                                ent.DirFlag = ENT_MOVE.MoveLeft;
                                ent.SetAnimation(TR_ANIMATION.LaraClimbLeft, 0);
                            }
                            else
                            {
                                ssAnim.AnimFlags = AnimControlFlags.LoopLastFrame; // Disable shake
                            }
                        }
                        else if (cmd.Move[1] == 1)
                        {
                            move = ent.Transform.Basis.Column0 * PENETRATION_TEST_OFFSET;
                            if (ent.CheckNextPenetration(move) == 0 || ent.Response.HorizontalCollide == 0x00)
                                // We only want Lara to shimmy when last frame is reached!
                            {
                                ent.DirFlag = ENT_MOVE.MoveRight;
                                ent.SetAnimation(TR_ANIMATION.LaraClimbRight, 0);
                            }
                            else
                            {
                                ssAnim.AnimFlags = AnimControlFlags.LoopLastFrame; // Disable shake
                            }
                        }
                        else
                        {
                            ssAnim.AnimFlags = AnimControlFlags.LoopLastFrame; // Disable shake
                            pos.X = climb.Point.X - LARA_HANG_WALL_DISTANCE * ent.Transform.Basis.Column1.X;
                            pos.Y = climb.Point.Y - LARA_HANG_WALL_DISTANCE * ent.Transform.Basis.Column1.Y;
                            pos.Z = climb.Point.Z - ent.Bf.BBMax.Z + LARA_HANG_VERTICAL_OFFSET;
                            ent.Speed = Vector3.Zero;
                        }
                    }
                    else if (cmd.Action && currFc.CeilingClimb && currFc.CeilingHit &&
                             pos.Z + ent.Bf.BBMax.Z > currFc.CeilingPoint.Z - 64.0f)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingIdle;
                        ssAnim.SetOnFrame(ent_to_monkey_swing);
                    }
                    else
                    {
                        ent.MoveType = MoveType.FreeFalling;
                        ent.SetAnimation(TR_ANIMATION.LaraTryHangVertical, 0); // Fall down
                    }
                    break;
                case TR_STATE.LaraLadderIdle:
                    cmd.Rotation.X = 0;
                    ent.MoveType = MoveType.WallsClimb;
                    ent.DirFlag = ENT_MOVE.Stay;
                    ent.CamFollowCenter = 64;
                    if (ent.MoveType == MoveType.Climbing)
                    {
                        ssAnim.NextState = TR_STATE.LaraClimbing;
                        break;
                    }
                    if (!cmd.Action)
                    {
                        ent.MoveType = MoveType.FreeFalling;
                        ent.SetAnimation(TR_ANIMATION.LaraStopHangVertical, 0); // Fall down
                    }
                    else if (cmd.Jump)
                    {
                        ssAnim.NextState = TR_STATE.LaraJumpBack;
                        ent.DirFlag = ENT_MOVE.MoveBackward;
                    }
                    else if (cmd.Move[0] == 1)
                    {
                        t = LARA_TRY_HANG_WALL_OFFSET + LARA_HANG_WALL_DISTANCE;
                        globalOffset = ent.Transform.Basis.Column1 * t;
                        globalOffset.Z += ent.Bf.BBMax.Z + LARA_HANG_VERTICAL_EPSILON;
                        climb = ent.CheckClimbability(globalOffset, nextFc, 0.0f);
                        if (climb.EdgeHit && climb.NextZSpace >= 512.0f)
                        {
                            ent.MoveType = MoveType.Climbing;
                            ssAnim.NextState = TR_STATE.LaraClimbing;
                        }
                        else if (!currFc.CeilingHit || pos.Z + ent.Bf.BBMax.Z < currFc.CeilingPoint.Z)
                        {
                            ssAnim.NextState = TR_STATE.LaraLadderUp;
                        }
                    }
                    else if (cmd.Move[0] == -1)
                    {
                        ssAnim.NextState = TR_STATE.LaraLadderDown;
                    }
                    else if (cmd.Move[1] == 11)
                    {
                        ssAnim.NextState = TR_STATE.LaraLadderRight;
                    }
                    else if (cmd.Move[1] == -1)
                    {
                        ssAnim.NextState = TR_STATE.LaraLadderLeft;
                    }
                    break;
                case TR_STATE.LaraLadderLeft:
                    ent.DirFlag = ENT_MOVE.MoveLeft;
                    if (!cmd.Action || ent.Climb.WallHit == ClimbType.None)
                    {
                        ssAnim.NextState = TR_STATE.LaraHang;
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraLadderIdle;
                    }
                    break;
                case TR_STATE.LaraLadderRight:
                    ent.DirFlag = ENT_MOVE.MoveRight;
                    if (!cmd.Action || ent.Climb.WallHit == ClimbType.None)
                    {
                        ssAnim.NextState = TR_STATE.LaraHang;
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraLadderIdle;
                    }
                    break;
                case TR_STATE.LaraLadderUp:
                    ent.CamFollowCenter = 64;
                    if (ent.MoveType == MoveType.Climbing)
                    {
                        ssAnim.NextState = TR_STATE.LaraLadderIdle;
                        break;
                    }

                    if (cmd.Action && ent.Climb.WallHit != ClimbType.None)
                    {
                        t = LARA_TRY_HANG_WALL_OFFSET + LARA_HANG_WALL_DISTANCE;
                        globalOffset = ent.Transform.Basis.Column1 * t;
                        globalOffset.Z += ent.Bf.BBMax.Z + LARA_HANG_VERTICAL_EPSILON;
                        climb = ent.CheckClimbability(globalOffset, nextFc, 0.0f);
                        if (climb.EdgeHit && climb.NextZSpace >= 512.0f)
                        {
                            ent.MoveType = MoveType.Climbing;
                            ssAnim.NextState = TR_STATE.LaraLadderIdle;
                        }
                        else if (cmd.Move[0] <= 0 &&
                                 (currFc.CeilingHit || pos.Z + ent.Bf.BBMax.Z >= currFc.CeilingPoint.Z))
                        {
                            ssAnim.NextState = TR_STATE.LaraLadderIdle;
                        }

                        if (currFc.CeilingHit && pos.Z + ent.Bf.BBMax.Z > currFc.CeilingPoint.Z)
                        {
                            pos.Z = currFc.CeilingPoint.Z - ent.Bf.BBMax.Z;
                        }
                    }
                    else
                    {
                        // Free fall after stop
                        ssAnim.NextState = TR_STATE.LaraLadderIdle;
                    }
                    break;
                case TR_STATE.LaraLadderDown:
                    ent.CamFollowCenter = 64;
                    if (cmd.Action && ent.Climb.WallHit != ClimbType.None && cmd.Move[1] < 0)
                    {
                        if (ent.Climb.WallHit != ClimbType.FullBody)
                        {
                            ssAnim.NextState = TR_STATE.LaraLadderIdle;
                        }
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraLadderIdle;
                    }
                    break;
                case TR_STATE.LaraShimmyLeft:
                    ent.Bt.NoFixBodyParts = BODY_PART.Legs;

                    cmd.Rotation.X = 0.0f;
                    ent.DirFlag = ENT_MOVE.MoveLeft;
                    if (!cmd.Action)
                    {
                        ent.Speed = Vector3.Zero;
                        ent.MoveType = MoveType.FreeFalling;
                        ent.SetAnimation(TR_ANIMATION.LaraTryHangVertical, 0); // fall down
                        break;
                    }

                    if (ent.MoveType == MoveType.WallsClimb)
                    {
                        if (ent.Climb.WallHit == ClimbType.None)
                        {
                            ent.MoveType = MoveType.FreeFalling;
                            ent.SetAnimation(TR_ANIMATION.LaraStopHangVertical, 0); // fall down
                        }
                    }
                    else
                    {
                        t = LARA_TRY_HANG_WALL_OFFSET + LARA_HANG_WALL_DISTANCE;
                        globalOffset = ent.Transform.Basis.Column1 * t;
                        globalOffset.Z += LARA_HANG_SENSOR_Z + LARA_HANG_VERTICAL_EPSILON;
                        climb = ent.CheckClimbability(globalOffset, nextFc, 0.0f);
                        if (climb.EdgeHit)
                        {
                            climb.Point = climb.EdgePoint;
                            ent.Angles.Z = climb.EdgeZAngle;
                            ent.UpdateTransform();
                            ent.MoveType = MoveType.Climbing; // hang on
                            pos.X = climb.Point.X - LARA_HANG_WALL_DISTANCE * ent.Transform.Basis.Column1.X;
                            pos.Y = climb.Point.Y - LARA_HANG_WALL_DISTANCE * ent.Transform.Basis.Column1.Y;
                            pos.Z = climb.Point.Z - ent.Bf.BBMax.Z + LARA_HANG_VERTICAL_OFFSET;
                            ent.Speed = Vector3.Zero;
                        }
                        else
                        {
                            ent.MoveType = MoveType.FreeFalling;
                            ent.SetAnimation(TR_ANIMATION.LaraStopHangVertical, 0); // fall down
                            break;
                        }
                    }

                    if (cmd.Move[1] == -1)
                    {
                        move = ent.Transform.Basis.Column0 * -PENETRATION_TEST_OFFSET;
                        if (ent.CheckNextPenetration(move) > 0 && ent.Response.HorizontalCollide != 0x00)
                        {
                            ssAnim.NextState = TR_STATE.LaraHang;
                        }
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraHang;
                    }
                    break;
                case TR_STATE.LaraShimmyRight:
                    ent.Bt.NoFixBodyParts = BODY_PART.Legs;

                    cmd.Rotation.X = 0.0f;
                    ent.DirFlag = ENT_MOVE.MoveRight;
                    if (!cmd.Action)
                    {
                        ent.Speed = Vector3.Zero;
                        ent.MoveType = MoveType.FreeFalling;
                        ent.SetAnimation(TR_ANIMATION.LaraTryHangVertical, 0); // fall down
                        break;
                    }

                    if (ent.MoveType == MoveType.WallsClimb)
                    {
                        if (ent.Climb.WallHit == ClimbType.None)
                        {
                            ent.MoveType = MoveType.FreeFalling;
                            ent.SetAnimation(TR_ANIMATION.LaraStopHangVertical, 0); // fall down
                        }
                    }
                    else
                    {
                        t = LARA_TRY_HANG_WALL_OFFSET + LARA_HANG_WALL_DISTANCE;
                        globalOffset = ent.Transform.Basis.Column1 * t;
                        globalOffset.Z += LARA_HANG_SENSOR_Z + LARA_HANG_VERTICAL_EPSILON;
                        climb = ent.CheckClimbability(globalOffset, nextFc, 0.0f);
                        if (climb.EdgeHit)
                        {
                            climb.Point = climb.EdgePoint;
                            ent.Angles.Z = climb.EdgeZAngle;
                            ent.UpdateTransform();
                            ent.MoveType = MoveType.Climbing; // hang on
                            pos.X = climb.Point.X - LARA_HANG_WALL_DISTANCE * ent.Transform.Basis.Column1.X;
                            pos.Y = climb.Point.Y - LARA_HANG_WALL_DISTANCE * ent.Transform.Basis.Column1.Y;
                            pos.Z = climb.Point.Z - ent.Bf.BBMax.Z + LARA_HANG_VERTICAL_OFFSET;
                            ent.Speed = Vector3.Zero;
                        }
                        else
                        {
                            ent.MoveType = MoveType.FreeFalling;
                            ent.SetAnimation(TR_ANIMATION.LaraStopHangVertical, 0); // fall down
                            break;
                        }
                    }

                    if (cmd.Move[1] == 1)
                    {
                        move = ent.Transform.Basis.Column0 * PENETRATION_TEST_OFFSET;
                        if (ent.CheckNextPenetration(move) > 0 && ent.Response.HorizontalCollide != 0x00)
                        {
                            ssAnim.NextState = TR_STATE.LaraHang;
                        }
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraHang;
                    }
                    break;
                case TR_STATE.LaraOnwaterExit:
                    cmd.Rotation[0] *= 0.0f;
                    ent.Bt.NoFixAll = true;
                    ssAnim.SetOnFrame(ent_set_on_floor_after_climb);
                    break;
                case TR_STATE.LaraJumpForward:
                case TR_STATE.LaraFallBackward:
                    ent.Bt.NoFixBodyParts = BODY_PART.Hands | BODY_PART.Legs | BODY_PART.Head;
                    ent.Lean(cmd, 4.0f);

                    if (resp.VerticalCollide.HasFlagSig(0x01) || ent.MoveType == MoveType.OnFloor)
                    {
                        if (ent.Self.Room.Flags.HasFlagUns(RoomFlag.Quicksand))
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraStayIdle, 0);
                        }
                        else if (!cmd.Action && cmd.Move[0] == 1 && !cmd.Crouch)
                        {
                            ent.MoveType = MoveType.OnFloor;
                            ssAnim.NextState = TR_STATE.LaraRunForward;
                        }
                        else
                        {
                            ssAnim.NextState = TR_STATE.LaraStop;
                        }
                    }
                    else if (ent.MoveType == MoveType.Underwater)
                    {
                        ent.Angles.Y = -45.0f;
                        cmd.Rotation.Y = 0.0f;
                        ent.UpdateTransform();
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallToUnderwater, 0);
                    }
                    else if (resp.HorizontalCollide.HasFlagSig(0x01))
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraSmashJump, 0);
                        ent.DirFlag = ENT_MOVE.MoveBackward;
                        ent.UpdateCurrentSpeed(true);
                    }
                    else if (ent.Speed.Z <= -FREE_FALL_SPEED_2)
                    {
                        ssAnim.NextState = TR_STATE.LaraFreefall; // free falling
                    }
                    else if (cmd.Action)
                    {
                        ssAnim.NextState = TR_STATE.LaraReach;
                    }
                    else if (cmd.Shift)
                    {
                        ssAnim.NextState = TR_STATE.LaraSwandiveBegin; // fly like fish
                    }
                    else if (cmd.Roll)
                    {
                        ssAnim.NextState = TR_STATE.LaraJumpRoll;
                    }
                    break;

                    #endregion

                    #region Freefall and underwater cases

                case TR_STATE.LaraUnderwaterDiving:
                    ent.Angles.Y = -45.0f;
                    cmd.Rotation.Y = 0.0f;
                    ent.UpdateTransform();
                    ssAnim.SetOnFrame(ent_correct_diving_angle);
                    break;
                case TR_STATE.LaraFreefall:
                    ent.Bt.NoFixBodyParts = BODY_PART.Hands | BODY_PART.Legs;
                    ent.Lean(cmd, 1.0f);

                    if (((float) (int) ent.Speed.Z).IsBetween(-FREE_FALL_SPEED_CRITICAL - 100,
                        -FREE_FALL_SPEED_CRITICAL))
                    {
                        ent.Speed.Z = -FREE_FALL_SPEED_CRITICAL - 101;
                        Audio.Send((uint) TR_AUDIO_SOUND.LaraScream, TR_AUDIO_EMITTER.Entity, (int) ent.ID); // Scream
                    }
                    else if (ent.Speed.Z <= -FREE_FALL_SPEED_MAXSAFE)
                    {
                        // Reset these to zero so Lara is only falling downwards
                        ent.Speed.X = 0.0f;
                        ent.Speed.Y = 0.0f;
                    }

                    if (ent.MoveType == MoveType.Underwater)
                    {
                        ent.Angles.Y = -45.0f;
                        cmd.Rotation.Y = 0.0f;
                        ent.UpdateTransform();
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallToUnderwater, 0);
                        Audio.Kill((int) TR_AUDIO_SOUND.LaraScream, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                        // Stop scream

                        // Splash sound is hardcoded, beginning with TR3
                        if (Global.EngineWorld.EngineVersion > Loader.Engine.TR2)
                        {
                            Audio.Send((uint) TR_AUDIO_SOUND.Splash, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                        }
                    }
                    else if (resp.VerticalCollide.HasFlagSig(0x01) || ent.MoveType == MoveType.OnFloor)
                    {
                        if (ent.Self.Room.Flags.HasFlagUns(RoomFlag.Quicksand))
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraStayIdle, 0);
                            Audio.Kill((int) TR_AUDIO_SOUND.LaraScream, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                        }
                        else if (ent.Speed.Z <= -FREE_FALL_SPEED_MAXSAFE)
                        {
                            if (!ent.ChangeParam(CharParameters.Health,
                                (ent.Speed.Z + FREE_FALL_SPEED_MAXSAFE) / 2))
                            {
                                resp.Killed = true;
                                ent.SetAnimation(TR_ANIMATION.LaraLandingDeath, 0);
                                Controls.JoyRumble(200.0f, 500);
                            }
                            else
                            {
                                ent.SetAnimation(TR_ANIMATION.LaraLandingHard, 0);
                            }
                        }
                        else if (ent.Speed.Z <= -FREE_FALL_SPEED_2)
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraLandingHard, 0);
                        }
                        else
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraLandingMiddle, 0);
                        }

                        if (resp.Killed)
                        {
                            ssAnim.NextState = TR_STATE.LaraDeath;
                            Audio.Kill((int) TR_AUDIO_SOUND.LaraScream, TR_AUDIO_EMITTER.Entity, (int) ent.ID);
                        }
                    }
                    else if (cmd.Action)
                    {
                        ent.DirFlag = ENT_MOVE.MoveForward;
                        ssAnim.NextState = TR_STATE.LaraReach;
                    }
                    break;
                case TR_STATE.LaraSwandiveBegin:
                    cmd.Rotation.X *= 0.4f;
                    if (resp.VerticalCollide.HasFlagSig(0x01) || ent.MoveType == MoveType.OnFloor)
                    {
                        ssAnim.NextState = TR_STATE.LaraStop; // landing - roll
                    }
                    else if (ent.MoveType == MoveType.Underwater)
                    {
                        ssAnim.NextState = TR_STATE.LaraUnderwaterDiving;
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraSwandiveEnd; // next stage
                    }
                    break;
                case TR_STATE.LaraSwandiveEnd:
                    cmd.Rotation.X = 0.0f;

                    // Reset these to zero so Lara is only falling downwards
                    ent.Speed.X = 0.0f;
                    ent.Speed.Y = 0.0f;

                    if (resp.VerticalCollide.HasFlagSig(0x01) || ent.MoveType == MoveType.OnFloor)
                    {
                        if (currFc.Quicksand != QuicksandPosition.None)
                        {
                            resp.Killed = true;
                            ent.SetParam(CharParameters.Health, 0.0f);
                            ent.SetParam(CharParameters.Air, 0.0f);
                            ent.SetAnimation(TR_ANIMATION.LaraLandingDeath, -1);
                        }
                        else
                        {
                            ent.SetParam(CharParameters.Health, 0.0f);
                            ssAnim.NextState = TR_STATE.LaraDeath;
                        }
                    }
                    else if (ent.MoveType == MoveType.Underwater)
                    {
                        ssAnim.NextState = TR_STATE.LaraUnderwaterDiving;
                    }
                    else if (cmd.Jump)
                    {
                        ssAnim.NextState = TR_STATE.LaraJumpRoll;
                    }
                    break;

                    #endregion

                    #region Water animations

                case TR_STATE.LaraUnderwaterStop:
                    if (ent.MoveType.NotEqual(MoveType.Underwater, MoveType.OnWater))
                    {
                        ent.SetAnimation(0, 0);
                    }
                    else if (resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraWaterDeath;
                    }
                    else if (cmd.Roll)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraUnderwaterRollBegin, 0);
                    }
                    else if (cmd.Jump)
                    {
                        ssAnim.NextState = TR_STATE.LaraUnderwaterForward;
                    }
                    else if (ent.MoveType == MoveType.OnWater)
                    {
                        ent.InertiaLinear = 0.0f;
                        ssAnim.NextState = TR_STATE.LaraOnwaterStop;
                        ent.SetAnimation(TR_ANIMATION.LaraUnderwaterToOnwater, 0); // go to the air
                    }
                    break;
                case TR_STATE.LaraWaterDeath:
                    if (ent.MoveType != MoveType.OnWater)
                    {
                        pos.Z = (TR_METERING_SECTORSIZE / 4) * Global.EngineFrameTime; // go to the air
                    }
                    break;
                case TR_STATE.LaraUnderwaterForward:
                    if (ent.MoveType.NotEqual(MoveType.Underwater, MoveType.OnWater))
                    {
                        ent.SetAnimation(0, 0);
                    }
                    else if (resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraWaterDeath;
                    }
                    else if (currFc.FloorHit && currFc.Water &&
                             currFc.TransitionLevel - currFc.FloorPoint.Z <= ent.MaxStepUpHeight)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraUnderwaterToWade, 0); // go to the air
                        ssAnim.NextState = TR_STATE.LaraStop;
                        ent.Climb.Point = currFc.FloorPoint;
                        ent.MoveType = MoveType.OnFloor;
                    }
                    else if (cmd.Roll)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraUnderwaterRollBegin, 0);
                    }
                    else if (ent.MoveType == MoveType.OnWater)
                    {
                        ent.InertiaLinear = 0.0f;
                        ssAnim.NextState = TR_STATE.LaraOnwaterStop;
                        ent.SetAnimation(TR_ANIMATION.LaraUnderwaterToOnwater, 0); // go to the air
                    }
                    else if (!cmd.Jump)
                    {
                        ssAnim.NextState = TR_STATE.LaraUnderwaterInertia;
                    }
                    break;
                case TR_STATE.LaraUnderwaterInertia:
                    if (ent.MoveType == MoveType.OnWater)
                    {
                        ent.InertiaLinear = 0.0f;
                        ent.SetAnimation(TR_ANIMATION.LaraUnderwaterToOnwater, 0); // go to the air
                    }
                    else if (resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraWaterDeath;
                    }
                    else if (cmd.Roll)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraUnderwaterRollBegin, 0);
                    }
                    else if (cmd.Jump)
                    {
                        ssAnim.NextState = TR_STATE.LaraUnderwaterForward;
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraUnderwaterStop;
                    }
                    break;
                case TR_STATE.LaraOnwaterStop:
                    if (resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraWaterDeath;
                    }
                    else if (cmd.Move[0] == 1 || cmd.Jump)
                    {
                        ent.DirFlag = ENT_MOVE.MoveForward;
                        ssAnim.NextState = TR_STATE.LaraOnwaterForward;
                    }
                    else if (cmd.Move[0] == -1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveBackward;
                        ssAnim.NextState = TR_STATE.LaraOnwaterBack;
                    }
                    else if (cmd.Move[1] == -1)
                    {
                        if (cmd.Shift)
                        {
                            ent.DirFlag = ENT_MOVE.MoveLeft;
                            cmd.Rotation.X = 0.0f;
                            ssAnim.NextState = TR_STATE.LaraOnwaterLeft;
                        }
                        else
                        {
                            // rotate on water
                        }
                    }
                    else if (cmd.Move[1] == 1)
                    {
                        if (cmd.Shift)
                        {
                            ent.DirFlag = ENT_MOVE.MoveRight;
                            cmd.Rotation.X = 0.0f;
                            ssAnim.NextState = TR_STATE.LaraOnwaterRight;
                        }
                        else
                        {
                            // rotate on water
                        }
                    }
                    else if (ent.MoveType == MoveType.Underwater)
                    {
                        ent.MoveType = MoveType.OnWater;
                    }
                    break;
                case TR_STATE.LaraOnwaterForward:
                    ent.Bt.NoFixBodyParts = BODY_PART.Hands;
                    ent.MoveType = MoveType.OnWater;

                    if (resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraWaterDeath;
                    }
                    else if (cmd.Jump)
                    {
                        t = pos.Z;
                        Character.GetHeightInfo(pos, nextFc);
                        pos.Z = t;
                        ssAnim.NextState = TR_STATE.LaraUnderwaterForward;
                        ssAnim.SetOnFrame(ent_set_underwater); // dive
                    }
                    else if (cmd.Move[0] == 1)
                    {
                        if (cmd.Action)
                        {
                            if (ent.MoveType != MoveType.Climbing)
                            {
                                t = LARA_TRY_HANG_WALL_OFFSET + LARA_HANG_WALL_DISTANCE;
                                globalOffset = ent.Transform.Basis.Column1 * t;
                                globalOffset[2] += LARA_HANG_VERTICAL_EPSILON;
                                climb = ent.CheckClimbability(globalOffset, nextFc, 0.0f);
                                if (climb.EdgeHit)
                                {
                                    lowVerticalSpace = true;
                                }
                                else
                                {
                                    lowVerticalSpace = false;
                                    globalOffset.Z += ent.MaxStepUpHeight + LARA_HANG_VERTICAL_EPSILON;
                                    climb = ent.CheckClimbability(globalOffset, nextFc, 0.0f);
                                }

                                if (climb.EdgeHit &&
                                    climb.NextZSpace >= ent.Height - LARA_HANG_VERTICAL_EPSILON)
                                {
                                    ent.DirFlag = ENT_MOVE.Stay;
                                    ent.MoveType = MoveType.Climbing;
                                    ent.Bt.NoFixAll = true;
                                    ent.Angles.X = climb.EdgeZAngle;
                                    ent.UpdateTransform();
                                    climb.Point = climb.EdgePoint;
                                }
                            }
                            else
                            {
                                ent.Speed = Vector3.Zero;
                                cmd.Rotation.X = 0.0f;
                                ent.Bt.NoFixAll = true;
                                if (lowVerticalSpace)
                                {
                                    ent.SetAnimation(TR_ANIMATION.LaraOnwaterToLandLow, 0);
                                }
                                else
                                {
                                    ent.SetAnimation(TR_ANIMATION.LaraClimbOutOfWater, 0);
                                }
                                ent_climb_out_of_water(ent, ssAnim, ENTITY_ANIM.NewAnim);
                            }
                        }
                        else if (!currFc.FloorHit || pos.Z - ent.Height > currFc.FloorPoint.Z - ent.SwimDepth)
                        {
                            //ent.LastState = ent.LastState; // swim forward
                        }
                        else
                        {
                            ssAnim.NextState = TR_STATE.LaraWadeForward;
                            ssAnim.SetOnFrame(ent_set_on_floor); // to wade
                        }
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraOnwaterStop;
                    }
                    break;
                case TR_STATE.LaraOnwaterBack:
                    if (cmd.Move[0] == -1 && !cmd.Jump)
                    {
                        if (!currFc.FloorHit || pos.Z + ent.Height < currFc.TransitionLevel)
                        {
                            //ent.CurrentState = TR_STATE.Current; // continue swimming
                        }
                        else
                        {
                            ssAnim.NextState = TR_STATE.LaraOnwaterStop;
                        }
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraOnwaterStop;
                    }
                    break;
                case TR_STATE.LaraOnwaterLeft:
                    cmd.Rotation.X = 0.0f;
                    if (!cmd.Jump)
                    {
                        if (cmd.Move[1] == -1)
                        {
                            if (!currFc.FloorHit || pos.Z - ent.Height > currFc.FloorPoint.Z)
                            {
                                // swim left
                                ssAnim.NextState = TR_STATE.LaraOnwaterLeft;
                            }
                            else
                            {
                                // walk left
                                ssAnim.NextState = TR_STATE.LaraWalkLeft;
                                ssAnim.SetOnFrame(ent_set_on_floor);
                            }
                        }
                        else
                        {
                            ssAnim.NextState = TR_STATE.LaraOnwaterStop;
                        }
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraUnderwaterDiving;
                    }
                    break;
                case TR_STATE.LaraOnwaterRight:
                    cmd.Rotation.X = 0.0f;
                    if (!cmd.Jump)
                    {
                        if (cmd.Move[1] == 1)
                        {
                            if (!currFc.FloorHit || pos.Z - ent.Height > currFc.FloorPoint.Z)
                            {
                                // swim right
                                ssAnim.NextState = TR_STATE.LaraOnwaterRight;
                            }
                            else
                            {
                                // walk right
                                ssAnim.NextState = TR_STATE.LaraWalkRight;
                                ssAnim.SetOnFrame(ent_set_on_floor);
                            }
                        }
                        else
                        {
                            ssAnim.NextState = TR_STATE.LaraOnwaterStop;
                        }
                    }
                    else
                    {
                        ssAnim.NextState = TR_STATE.LaraUnderwaterDiving;
                    }
                    break;

                    #endregion

                    #region Crouch

                case TR_STATE.LaraCrouchIdle:
                    ent.DirFlag = ENT_MOVE.MoveForward;
                    ent.Bt.NoFixBodyParts = BODY_PART.Hands2 | BODY_PART.Hands3 | BODY_PART.Legs3;
                    move.X = pos.X;
                    move.Y = pos.Y;
                    move.Z = pos.Z + 0.5f * (ent.Bf.BBMax.Z - ent.Bf.BBMin.Z);
                    Character.GetHeightInfo(move, nextFc);

                    ent.Lean(cmd, 0.0f);

                    if (!cmd.Crouch && !lowVerticalSpace)
                    {
                        ssAnim.NextState = TR_STATE.LaraStop; // Back to stand
                    }
                    else if (cmd.Move[0] != 0 || resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraCrawlIdle; // Both forward & back provoke crawl stage
                    }
                    else if (cmd.Jump)
                    {
                        ssAnim.NextState = TR_STATE.LaraCrouchRoll; // Crouch roll
                    }
                    else
                    {
                        if (Global.EngineWorld.EngineVersion > Loader.Engine.TR3)
                        {
                            if (cmd.Move[1] == 1)
                            {
                                ent.DirFlag = ENT_MOVE.MoveForward;
                                ssAnim.NextState = TR_STATE.LaraCrouchTurnRight;
                            }
                            else
                            {
                                ent.DirFlag = ENT_MOVE.MoveForward;
                                ssAnim.NextState = TR_STATE.LaraCrouchTurnLeft;
                            }
                        }
                        else
                        {
                            cmd.Rotation.X = 0.0f;
                        }
                    }
                    break;
                case TR_STATE.LaraCrouchRoll:
                case TR_STATE.LaraSprintRoll:
                    cmd.Rotation.X = 0.0f;
                    ent.Lean(cmd, 0.0f);
                    if (ent.MoveType == MoveType.FreeFalling)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallForward, 0);
                    }

                    move = ent.Transform.Basis.Column1 * PENETRATION_TEST_OFFSET;
                    if (ent.CheckNextPenetration(move) > 0 && resp.HorizontalCollide != 0x00) // Smash into wall
                    {
                        ssAnim.NextState = TR_STATE.LaraStop;
                    }
                    break;
                case TR_STATE.LaraCrawlIdle:
                    ent.DirFlag = ENT_MOVE.MoveForward;
                    ent.Bt.NoFixBodyParts = BODY_PART.Hands2 | BODY_PART.Hands3 | BODY_PART.Legs3;
                    if (resp.Killed)
                    {
                        ent.DirFlag = ENT_MOVE.Stay;
                        ssAnim.NextState = TR_STATE.LaraDeath;
                    }
                    else if (cmd.Move[1] == -1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveForward;
                        ent.SetAnimation(TR_ANIMATION.LaraCrawlTurnLeft, 0);
                    }
                    else if (cmd.Move[1] == 1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveForward;
                        ent.SetAnimation(TR_ANIMATION.LaraCrawlTurnRight, 0);
                    }
                    else if (cmd.Move[0] == 1)
                    {
                        move = ent.Transform.Basis.Column1 * PENETRATION_TEST_OFFSET;
                        if (ent.CheckNextPenetration(move) == 0 || resp.HorizontalCollide == 0x00)
                        {
                            globalOffset = ent.Transform.Basis.Column1 * CRAWL_FORWARD_OFFSET;
                            globalOffset.Z += 0.5f * (ent.Bf.BBMax.Z + ent.Bf.BBMin.Z);
                            globalOffset += pos;
                            Character.GetHeightInfo(globalOffset, nextFc);
                            if (nextFc.FloorPoint.Z.IsBetween(pos.Z - ent.MinStepUpHeight, pos.Z + ent.MinStepUpHeight,
                                false))
                            {
                                ssAnim.NextState = TR_STATE.LaraCrawlForward; // In TR4+, first state is crawlspace jump
                            }
                        }
                    }
                    else if (cmd.Move[0] == -1)
                    {
                        move = ent.Transform.Basis.Column1 * -PENETRATION_TEST_OFFSET;
                        if (ent.CheckNextPenetration(move) == 0 || resp.HorizontalCollide == 0x00)
                        {
                            globalOffset = ent.Transform.Basis.Column1 * -CRAWL_FORWARD_OFFSET;
                            globalOffset.Z += 0.5f * (ent.Bf.BBMax.Z + ent.Bf.BBMin.Z);
                            globalOffset += pos;
                            Character.GetHeightInfo(globalOffset, nextFc);
                            if (nextFc.FloorPoint.Z.IsBetween(pos.Z - ent.MinStepUpHeight, pos.Z + ent.MinStepUpHeight,
                                false))
                            {
                                ent.DirFlag = ENT_MOVE.MoveBackward;
                                ssAnim.NextState = TR_STATE.LaraCrawlBack;
                            }
                            else if (cmd.Action && resp.HorizontalCollide == 0 &&
                                     nextFc.FloorPoint.Z < pos.Z - ent.Height)
                            {
                                var temp = pos; // save entity position
                                pos.X = nextFc.FloorPoint.X;
                                pos.Y = nextFc.FloorPoint.Y;
                                globalOffset = ent.Transform.Basis.Column1 * 0.5f * CRAWL_FORWARD_OFFSET;
                                globalOffset.Z += 128.0f;
                                currFc.FloorHit = nextFc.FloorHit;
                                currFc.FloorPoint = nextFc.FloorPoint;
                                currFc.FloorNormale = nextFc.FloorNormale;
                                currFc.FloorObject = nextFc.FloorObject;
                                currFc.CeilingHit = nextFc.CeilingHit;
                                currFc.CeilingPoint = nextFc.CeilingPoint;
                                currFc.CeilingNormale = nextFc.CeilingNormale;
                                currFc.CeilingObject = nextFc.CeilingObject;

                                climb = ent.CheckClimbability(globalOffset, nextFc, 1.5f * ent.Bf.BBMax.Z);
                                pos = temp; // restore entity position
                                if (climb.CanHang)
                                {
                                    ent.Angles.X = climb.EdgeZAngle;
                                    ent.DirFlag = ENT_MOVE.MoveBackward;
                                    ent.MoveType = MoveType.Climbing;
                                    climb.Point = climb.EdgePoint;
                                    ssAnim.NextState = TR_STATE.LaraCrawlToClimb;
                                }
                            }
                        }
                    }
                    else if (!cmd.Crouch)
                    {
                        ssAnim.NextState = TR_STATE.LaraCrouchIdle; // Back to crouch
                    }
                    break;
                case TR_STATE.LaraCrawlToClimb:
                    ent.Bt.NoFixAll = true;
                    ssAnim.SetOnFrame(ent_crawl_to_climb);
                    break;
                case TR_STATE.LaraCrawlForward:
                    ent.DirFlag = ENT_MOVE.MoveForward;
                    ent.Bt.NoFixBodyParts = BODY_PART.Hands2 | BODY_PART.Hands3 | BODY_PART.Legs3;
                    cmd.Rotation.X *= 0.5f;
                    move = ent.Transform.Basis.Column1 * PENETRATION_TEST_OFFSET;
                    if (ent.CheckNextPenetration(move) > 0 && resp.HorizontalCollide != 0x00)
                    {
                        ent.DirFlag = ENT_MOVE.Stay;
                        ent.SetAnimation(TR_ANIMATION.LaraCrawlIdle, 0);
                        break;
                    }
                    globalOffset = ent.Transform.Basis.Column1 * CRAWL_FORWARD_OFFSET;
                    globalOffset.Z += 0.5f * (ent.Bf.BBMax.Z + ent.Bf.BBMin.Z);
                    globalOffset += pos;
                    Character.GetHeightInfo(globalOffset, nextFc);

                    if (cmd.Move[0] != 1 || resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraCrawlIdle; // Stop
                    }
                    else if (nextFc.FloorPoint.Z.IsBetween(pos.Z + ent.MinStepUpHeight, pos.Z - ent.MinStepUpHeight))
                    {
                        ent.DirFlag = ENT_MOVE.Stay;
                        ent.SetAnimation(TR_ANIMATION.LaraCrawlIdle, 0);
                    }
                    break;
                case TR_STATE.LaraCrawlBack:
                    ent.DirFlag = ENT_MOVE.MoveForward; // TODO: Absurd? No, Core Design.
                    ent.Bt.NoFixBodyParts = BODY_PART.Hands2 | BODY_PART.Hands3 | BODY_PART.Legs3;
                    cmd.Rotation.X *= 0.5f;
                    move = ent.Transform.Basis.Column1 * -PENETRATION_TEST_OFFSET;
                    if (ent.CheckNextPenetration(move) > 0 && resp.HorizontalCollide != 0x00)
                    {
                        ent.DirFlag = ENT_MOVE.Stay;
                        ent.SetAnimation(TR_ANIMATION.LaraCrawlIdle, 0);
                        break;
                    }
                    globalOffset = ent.Transform.Basis.Column1 * -CRAWL_FORWARD_OFFSET;
                    globalOffset.Z += 0.5f * (ent.Bf.BBMax.Z + ent.Bf.BBMin.Z);
                    globalOffset += pos;
                    Character.GetHeightInfo(globalOffset, nextFc);
                    if (cmd.Move[0] != -1 || resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraCrawlIdle; // Stop
                    }
                    else if (nextFc.FloorPoint.Z.IsBetween(pos.Z + ent.MinStepUpHeight, pos.Z - ent.MinStepUpHeight))
                    {
                        ent.DirFlag = ENT_MOVE.Stay;
                        ent.SetAnimation(TR_ANIMATION.LaraCrawlIdle, 0);
                    }
                    break;
                case TR_STATE.LaraCrawlTurnLeft:
                    ent.DirFlag = ENT_MOVE.MoveForward;
                    ent.Bt.NoFixBodyParts = BODY_PART.Hands2 | BODY_PART.Hands3 | BODY_PART.Legs3;
                    cmd.Rotation.X *= ((int) ssAnim.CurrentFrame).IsBetween(3, 14, false) ? 1.0f : 0.0f;

                    if (cmd.Move[1] != -1 || resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraCrawlIdle; // Stop;
                    }
                    break;
                case TR_STATE.LaraCrawlTurnRight:
                    ent.DirFlag = ENT_MOVE.MoveForward;
                    ent.Bt.NoFixBodyParts = BODY_PART.Hands2 | BODY_PART.Hands3 | BODY_PART.Legs3;
                    cmd.Rotation.X *= ssAnim.CurrentFrame.IsBetween(3, 14, false) ? 1.0f : 0.0f;

                    if (cmd.Move[1] != 1 || resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraCrawlIdle; // Stop;
                    }
                    break;
                case TR_STATE.LaraCrouchTurnLeft:
                case TR_STATE.LaraCrouchTurnRight:
                    ent.Bt.NoFixBodyParts = BODY_PART.Hands2 | BODY_PART.Hands3 | BODY_PART.Legs3;
                    cmd.Rotation.X *= ssAnim.CurrentFrame.IsBetween(3, 23, false) ? 0.6f : 0.0f;

                    if (cmd.Move[1] == 0 || resp.Killed)
                    {
                        ssAnim.NextState = TR_STATE.LaraCrouchIdle;
                    }
                    break;

                    #endregion

                    #region Monkeyswing

                case TR_STATE.LaraMonkeyswingIdle:
                    cmd.Rotation.X = 0.0f;
                    ent.DirFlag = ENT_MOVE.Stay;
                    // FIXME: stick for TR3+ monkey swing fix... something wrong with anim 150
                    if (cmd.Action && ent.MoveType != MoveType.Monkeyswing && currFc.CeilingClimb && currFc.CeilingHit &&
                        pos.Z + ent.Bf.BBMax.Z > currFc.CeilingPoint.Z - 96.0f)
                    {
                        ent.MoveType = MoveType.Monkeyswing;
                        ent.SetAnimation(TR_ANIMATION.LaraMonkeyIdle, 0);
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingIdle;
                        pos.Z = ent.HeightInfo.CeilingPoint.Z - ent.Bf.BBMax.Z;
                    }

                    if (ent.MoveType != MoveType.Monkeyswing || !cmd.Action)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraTryHangVertical, 0);
                        ent.DirFlag = ENT_MOVE.Stay;
                        ent.MoveType = MoveType.FreeFalling;
                    }
                    else if (cmd.Shift && cmd.Move[1] == -1)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingLeft;
                    }
                    else if (cmd.Shift && cmd.Move[1] == 1)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingRight;
                    }
                    else if (cmd.Move[0] == 1)
                    {
                        ent.DirFlag = ENT_MOVE.MoveForward;
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingForward;
                    }
                    else if (cmd.Move[1] == -1)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingTurnLeft;
                    }
                    else if (cmd.Move[1] == 1)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingTurnRight;
                    }
                    break;
                case TR_STATE.LaraMonkeyswingTurnLeft:
                    cmd.Rotation.X *= 0.5f;
                    if (ent.MoveType != MoveType.Monkeyswing || !cmd.Action)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraTryHangVertical, 0);
                        ent.DirFlag = ENT_MOVE.Stay;
                        ent.MoveType = MoveType.FreeFalling;
                    }
                    else if (cmd.Move[1] != -1)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingIdle;
                    }
                    break;
                case TR_STATE.LaraMonkeyswingTurnRight:
                    cmd.Rotation.X *= 0.5f;
                    if (ent.MoveType != MoveType.Monkeyswing || !cmd.Action)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraTryHangVertical, 0);
                        ent.DirFlag = ENT_MOVE.Stay;
                        ent.MoveType = MoveType.FreeFalling;
                    }
                    else if (cmd.Move[1] != 1)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingIdle;
                    }
                    break;
                case TR_STATE.LaraMonkeyswingForward:
                    cmd.Rotation.X *= 0.45f;
                    ent.DirFlag = ENT_MOVE.MoveForward;

                    if (ent.MoveType != MoveType.Monkeyswing || !cmd.Action)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraTryHangVertical, 0);
                        ent.MoveType = MoveType.FreeFalling;
                    }
                    else if (cmd.Move[0] != 1)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingIdle;
                    }
                    break;
                case TR_STATE.LaraMonkeyswingLeft:
                    cmd.Rotation.X = 0.0f;
                    ent.DirFlag = ENT_MOVE.MoveLeft;

                    if (ent.MoveType != MoveType.Monkeyswing || !cmd.Action)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraTryHangVertical, 0);
                        ent.MoveType = MoveType.FreeFalling;
                    }
                    else if (cmd.Move[0] != 1)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingIdle;
                    }
                    break;
                case TR_STATE.LaraMonkeyswingRight:
                    cmd.Rotation.X = 0.0f;
                    ent.DirFlag = ENT_MOVE.MoveRight;

                    if (ent.MoveType != MoveType.Monkeyswing || !cmd.Action)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraTryHangVertical, 0);
                        ent.MoveType = MoveType.FreeFalling;
                    }
                    else if (cmd.Move[0] != 1)
                    {
                        ssAnim.NextState = TR_STATE.LaraMonkeyswingIdle;
                    }
                    break;

                    #endregion

                    #region Tightrope

                case TR_STATE.LaraTightropeEnter:
                    cmd.Rotation.X = 0.0f;
                    ent.Bt.NoFixAll = true;
                    ent.DirFlag = ENT_MOVE.MoveForward;
                    ssAnim.SetOnFrame(ent_to_tightrope);
                    ssAnim.NextState = TR_STATE.LaraTightropeIdle;
                    break;
                case TR_STATE.LaraTightropeExit:
                    cmd.Rotation.X = 0.0f;
                    ent.Bt.NoFixAll = true;
                    ent.DirFlag = ENT_MOVE.MoveForward;
                    ssAnim.SetOnFrame(ent_from_tightrope);
                    ssAnim.NextState = TR_STATE.LaraStop;
                    break;
                case TR_STATE.LaraTightropeIdle:
                    cmd.Rotation.X = 0.0f;

                    if (ssAnim.CurrentAnimation == TR_ANIMATION.LaraTightropeStand)
                    {
                        if (ent.Response.Lean == LeanType.Left)
                        {
                            ssAnim.NextState = TR_STATE.LaraTightropeBalancingLeft;
                            ent.Response.Lean = LeanType.None;
                            break;
                        }
                        else if (ent.Response.Lean == LeanType.Right)
                        {
                            ssAnim.NextState = TR_STATE.LaraTightropeBalancingRight;
                            ent.Response.Lean = LeanType.None;
                            break;
                        }
                        else if (lastFrame)
                        {
                            var chanceToFall = Helper.CPPRand() % 0x7FFF;

                            ssAnim.NextState = chanceToFall > 0x5FFF
                                ? TR_STATE.LaraTightropeBalancingLeft
                                : TR_STATE.LaraTightropeBalancingRight;
                        }
                    }

                    if (cmd.Roll || cmd.Move[0] == -1)
                    {
                        ent.SetAnimation(TR_ANIMATION.LaraTightropeTurn, 0);
                        ent.DirFlag = ENT_MOVE.MoveForward;
                    }
                    else if (cmd.Move[0] == 1)
                    {
                        ssAnim.NextState = TR_STATE.LaraTightropeForward;
                    }
                    break;
                case TR_STATE.LaraTightropeForward:
                    cmd.Rotation.X = 0.0f;
                    ent.DirFlag = ENT_MOVE.MoveForward;

                    if (cmd.Move[0] != 1)
                    {
                        ssAnim.NextState = TR_STATE.LaraTightropeIdle;
                    }
                    else
                    {
                        var chanceToUnbal = Helper.CPPRand() % 0x7FFF;
                        if (chanceToUnbal < 0x00FF)
                        {
                            ssAnim.NextState = TR_STATE.LaraTightropeIdle;

                            ent.Response.Lean = chanceToUnbal > 0x007F ? LeanType.Left : LeanType.Right;
                        }
                    }
                    break;
                case TR_STATE.LaraTightropeBalancingRight:
                    cmd.Rotation.X = 0.0f;

                    if (ssAnim.CurrentAnimation == TR_ANIMATION.LaraTightropeFallRight && lastFrame)
                    {
                        ent.MoveType = MoveType.FreeFalling;
                        ent.Transform.Origin +=
                            ent.Transform.Basis.MultiplyByVector(new Vector3(256.0f, 192.0f, -640.0f));
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallLong, 0);
                    }
                    else if (ssAnim.CurrentAnimation == TR_ANIMATION.LaraTightropeLooseRight &&
                             ssAnim.CurrentFrame >=
                             ssAnim.Model.Animations[(int) ssAnim.CurrentAnimation].Frames.Count / 2 &&
                             cmd.Move[1] == -1)
                    {
                        // MAGIC: mirroring animation offset
                        ent.SetAnimation(TR_ANIMATION.LaraTightropeRecoverRight,
                            ssAnim.Model.Animations[(int) ssAnim.CurrentAnimation].Frames.Count - ssAnim.CurrentFrame);
                    }
                    break;
                case TR_STATE.LaraTightropeBalancingLeft:
                    cmd.Rotation.X = 0.0f;

                    if (ssAnim.CurrentAnimation == TR_ANIMATION.LaraTightropeFallLeft && lastFrame)
                    {
                        ent.MoveType = MoveType.FreeFalling;
                        ent.SetAnimation(TR_ANIMATION.LaraFreeFallLong, 0);
                        ent.Transform.Origin +=
                            ent.Transform.Basis.MultiplyByVector(new Vector3(-256.0f, 192.0f, -640.0f));
                    }
                    else if (ssAnim.CurrentAnimation == TR_ANIMATION.LaraTightropeLooseLeft &&
                             ssAnim.CurrentFrame >=
                             ssAnim.Model.Animations[(int) ssAnim.CurrentAnimation].Frames.Count / 2 && cmd.Move[1] == 1)
                    {
                        // MAGIC: mirroring animation offset
                        ent.SetAnimation(TR_ANIMATION.LaraTightropeRecoverLeft,
                            ssAnim.Model.Animations[(int) ssAnim.CurrentAnimation].Frames.Count - ssAnim.CurrentFrame);
                    }
                    break;

                #endregion

                default:
                    cmd.Rotation.X = 0.0f;
                    if(ent.MoveType.IsAnyOf(MoveType.Monkeyswing, MoveType.WallsClimb))
                    {
                        if(!cmd.Action)
                        {
                            ent.SetAnimation(TR_ANIMATION.LaraStartFreeFall, 0);
                            ent.DirFlag = ENT_MOVE.Stay;
                            ent.MoveType = MoveType.FreeFalling;
                        }
                    }
                    break;
            }

            // Extra animation control

            switch (ssAnim.CurrentAnimation)
            {
                case TR_ANIMATION.LaraStayJumpSides:
                    ent.Bt.NoFixBodyParts |= BODY_PART.Head;
                    break;

                case TR_ANIMATION.LaraTryHangSolid:
                case TR_ANIMATION.LaraFlyForwardTryHang:
                    if (ent.MoveType == MoveType.FreeFalling && ent.Command.Action &&
                        ent.Speed.X * ent.Transform.Basis.Column1.X + ent.Speed.Y * ent.Transform.Basis.Column1.Y < 0.0f)
                    {
                        ent.Speed.X = -ent.Speed.X;
                        ent.Speed.Y = -ent.Speed.Y;
                    }
                    break;

                case TR_ANIMATION.LaraAhBackward:
                case TR_ANIMATION.LaraAhForward:
                case TR_ANIMATION.LaraAhLeft:
                case TR_ANIMATION.LaraAhRight:
                    if (ssAnim.CurrentFrame > 12)
                        ent.SetAnimation(TR_ANIMATION.LaraStaySolid, 0);
                    break;
            }

            return 0;
        }
    }
}
