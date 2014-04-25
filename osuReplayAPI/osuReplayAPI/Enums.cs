using System;

namespace ReplayAPI
{
    [Flags]
    public enum KeyData
    {
        None = 0,
        K1 = 5,
        K2 = 10,
        K3 = 1,
        K4 = 2
    }

    [Flags]
    public enum Modifications
    {
        None = 0,
        NoFail = 0x1,
        Easy = 0x2,
        NoVideo = 0x4,
        Hidden = 0x8,
        HardRock = 0x10,
        SuddenDeath = 0x20,
        DoubleTime = 0x40,
        Relax = 0x80,
        HalfTime = 0x100,
        NightCore = 0x200,
        FlashLight = 0x400,
        Auto = 0x800,
        SpunOut = 0x1000,
        AutoPilot = 0x2000,
        Perfect = 0x4000,
        Mania4K = 0x8000,
        Mania5K = 0x10000,
        Mania6K = 0x20000,
        Mania7K = 0x40000,
        Mania8K = 0x80000,
    }
}