﻿// <copyright file="Board.cs" company="Adrian Conlon">
// Copyright (c) Adrian Conlon. All rights reserved.
// </copyright>

namespace Invaders
{
    using System;
    using EightBit;

    public class Board : Bus
    {
        private static readonly char[] CharacterSet =
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
            'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X',
            'Y', 'Z', '0', '1', '2', '3', '4', '5',
            '6', '7', '8', '9', '<', '>', ' ', '=',
            '*', '^', '_', '_', '_', '_', '_', '_',
            'Y', '%', '_', '_', '_', '_', 'Y', '&',
            '?', '_', '_', '_', '_', '_', '_', '-',
        };

        private readonly Rom romE = new Rom(0x800);
        private readonly Rom romF = new Rom(0x800);
        private readonly Rom romG = new Rom(0x800);
        private readonly Rom romH = new Rom(0x800);
        private readonly Ram workRAM = new Ram(0x400);
        private readonly InputOutput ports = new InputOutput();
        private readonly Disassembler disassembler;

        private readonly Register16 shiftData = new Register16();

        private ShipSwitch ships = ShipSwitch.Three;
        private ExtraShipSwitch extraLife = ExtraShipSwitch.OneThousandFiveHundred;
        private DemoCoinInfoSwitch demoCoinInfo = DemoCoinInfoSwitch.On;

        private byte shiftAmount = 0;

        private int credit = 0;

        private int onePlayerStart = 0;
        private int onePlayerShot = 0;
        private int onePlayerLeft = 0;
        private int onePlayerRight = 0;

        private int twoPlayerStart = 0;
        private int twoPlayerShot = 0;
        private int twoPlayerLeft = 0;
        private int twoPlayerRight = 0;

        private int tilt = 0;

        private byte preSound1 = 0;
        private byte preSound2 = 0;

        private int allowed = 0;

        public Board()
        {
            this.CPU = new Intel8080(this, this.ports);
            this.disassembler = new Disassembler(this);
        }

        public event EventHandler<EventArgs> UfoSound;

        public event EventHandler<EventArgs> ShotSound;

        public event EventHandler<EventArgs> PlayerDieSound;

        public event EventHandler<EventArgs> InvaderDieSound;

        public event EventHandler<EventArgs> ExtendSound;

        public event EventHandler<EventArgs> Walk1Sound;

        public event EventHandler<EventArgs> Walk2Sound;

        public event EventHandler<EventArgs> Walk3Sound;

        public event EventHandler<EventArgs> Walk4Sound;

        public event EventHandler<EventArgs> UfoDieSound;

        public event EventHandler<EventArgs> EnableAmplifier;

        public event EventHandler<EventArgs> DisableAmplifier;

        public enum RasterSize
        {
            Width = 256,
            Height = 224,
        }

        private enum InputPorts
        {
            INP0 = 0,
            INP1 = 1,
            INP2 = 2,
            SHFT_IN = 3,
        }

        private enum OutputPorts
        {
            SHFTAMNT = 2,
            SOUND1 = 3,
            SHFT_DATA = 4,
            SOUND2 = 5,
            WATCHDOG = 6,
        }

        private enum ShipSwitch
        {
            Three = 0b00,
            Four = 0b01,
            Five = 0b10,
            Six = 0b11,
        }

        private enum ExtraShipSwitch
        {
            OneThousandFiveHundred = 0,
            OneThousand = 1,
        }

        private enum DemoCoinInfoSwitch
        {
            On = 0,
            Off = 1,
        }

        public static int CyclesPerScanLine => Configuration.CyclesPerRasterScan / (int)RasterSize.Height;

        public static int PixelSize => Configuration.PixelSize;

        public Intel8080 CPU { get; }

        public Ram VRAM { get; } = new Ram(0x1c00);

        public bool CocktailModeControl { get; private set; } = false;

        public override MemoryMapping Mapping(ushort absolute)
        {
            const ushort mask = 0b0011111111111111;
            absolute &= mask;

            if (absolute < 0x800)
            {
                return new MemoryMapping(this.romH, 0, mask, AccessLevel.ReadOnly);
            }

            if (absolute < 0x1000)
            {
                return new MemoryMapping(this.romG, 0x0800, mask, AccessLevel.ReadOnly);
            }

            if (absolute < 0x1800)
            {
                return new MemoryMapping(this.romF, 0x0800 * 2, mask, AccessLevel.ReadOnly);
            }

            if (absolute < 0x2000)
            {
                return new MemoryMapping(this.romE, 0x0800 * 3, mask, AccessLevel.ReadOnly);
            }

            if (absolute < 0x2400)
            {
                return new MemoryMapping(this.workRAM, 0x2000, mask, AccessLevel.ReadWrite);
            }

            if (absolute < 0x4000)
            {
                return new MemoryMapping(this.VRAM, 0x2400, mask, AccessLevel.ReadWrite);
            }

            throw new InvalidOperationException("Invalid memory mapping.");
        }

        public override void Initialize()
        {
            this.ports.WritingPort += this.Ports_WritingPort_SpaceInvaders;
            this.ports.WrittenPort += this.Ports_WrittenPort_SpaceInvaders;
            this.ports.ReadingPort += this.Ports_ReadingPort_SpaceInvaders;

            if (Configuration.DebugMode)
            {
                this.CPU.ExecutingInstruction += this.CPU_ExecutingInstruction_Debug;
            }
        }

        public void LoadContent()
        {
            var romDirectory = Configuration.ContentRoot + Configuration.RomDirectory;

            this.romE.Load(romDirectory + "invaders.e");
            this.romF.Load(romDirectory + "invaders.f");
            this.romG.Load(romDirectory + "invaders.g");
            this.romH.Load(romDirectory + "invaders.h");
        }

        public override void RaisePOWER()
        {
            base.RaisePOWER();
            this.CPU.RaisePOWER();
            this.CPU.RaiseRESET();
            this.CPU.RaiseINT();
        }

        public override void LowerPOWER()
        {
            this.CPU.LowerPOWER();
            base.LowerPOWER();
        }

        public void TriggerInterruptScanLine224()
        {
            this.Data = 0xd7;  // RST 2
            this.CPU.INT.Lower();
        }

        public void TriggerInterruptScanLine96()
        {
            this.Data = 0xcf;  // RST 1
            this.CPU.INT.Lower();
        }

        public void RunScanLine() => this.RunCycles(CyclesPerScanLine);

        public void RunRasterScan() => this.RunCycles(Configuration.CyclesPerRasterScan);

        public void RunVerticalBlank() => this.RunCycles(Configuration.CyclesPerVerticalBlank);

        public void PressCredit() => this.credit = 1;

        public void ReleaseCredit() => this.credit = 0;

        public void Press1P() => this.onePlayerStart = 1;

        public void PressShoot1P() => this.onePlayerShot = 1;

        public void PressLeft1P() => this.onePlayerLeft = 1;

        public void PressRight1P() => this.onePlayerRight = 1;

        public void Release1P() => this.onePlayerStart = 0;

        public void ReleaseShoot1P() => this.onePlayerShot = 0;

        public void ReleaseLeft1P() => this.onePlayerLeft = 0;

        public void ReleaseRight1P() => this.onePlayerRight = 0;

        public void Press2P() => this.twoPlayerStart = 1;

        public void PressShoot2P() => this.twoPlayerShot = 1;

        public void PressLeft2P() => this.twoPlayerLeft = 1;

        public void PressRight2P() => this.twoPlayerRight = 1;

        public void Release2P() => this.twoPlayerStart = 0;

        public void ReleaseShoot2P() => this.twoPlayerShot = 0;

        public void ReleaseLeft2P() => this.twoPlayerLeft = 0;

        public void ReleaseRight2P() => this.twoPlayerRight = 0;

        protected void OnUfoSound() => this.UfoSound?.Invoke(this, EventArgs.Empty);

        protected void OnShotSound() => this.ShotSound?.Invoke(this, EventArgs.Empty);

        protected void OnPlayerDieSound() => this.PlayerDieSound?.Invoke(this, EventArgs.Empty);

        protected void OnInvaderDieSound() => this.InvaderDieSound?.Invoke(this, EventArgs.Empty);

        protected void OnExtendSound() => this.ExtendSound?.Invoke(this, EventArgs.Empty);

        protected void OnWalk1Sound() => this.Walk1Sound?.Invoke(this, EventArgs.Empty);

        protected void OnWalk2Sound() => this.Walk2Sound?.Invoke(this, EventArgs.Empty);

        protected void OnWalk3Sound() => this.Walk3Sound?.Invoke(this, EventArgs.Empty);

        protected void OnWalk4Sound() => this.Walk4Sound?.Invoke(this, EventArgs.Empty);

        protected void OnUfoDieSound() => this.UfoDieSound?.Invoke(this, EventArgs.Empty);

        protected void OnEnableAmplifier() => this.EnableAmplifier?.Invoke(this, EventArgs.Empty);

        protected void OnDisableAmplifier() => this.DisableAmplifier?.Invoke(this, EventArgs.Empty);

        private void Ports_ReadingPort_SpaceInvaders(object sender, PortEventArgs e)
        {
            switch (e.Port)
            {
                case (byte)InputPorts.INP1:
                    this.ports.WriteInputPort(
                        e.Port,
                        (byte)((this.credit ^ 1) | (this.twoPlayerStart << 1) | (this.onePlayerStart << 2) | (1 << 3) | (this.onePlayerShot << 4) | (this.onePlayerLeft << 5) | (this.onePlayerRight << 6)));
                    break;
                case (byte)InputPorts.INP2:
                    this.ports.WriteInputPort(
                        e.Port,
                        (byte)((int)this.ships | (this.tilt << 2) | ((int)this.extraLife << 3) | (this.twoPlayerShot << 4) | (this.twoPlayerLeft << 5) | (this.twoPlayerRight << 6) | ((int)this.demoCoinInfo << 7)));
                    break;
                case (byte)InputPorts.SHFT_IN:
                    this.ports.WriteInputPort(
                        e.Port,
                        (byte)((((this.shiftData.High << 8) | this.shiftData.Low) << this.shiftAmount) >> 8));
                    break;
            }
        }

        private void Ports_WrittenPort_SpaceInvaders(object sender, PortEventArgs e)
        {
            var value = this.ports.ReadOutputPort(e.Port);
            switch (e.Port)
            {
                case (byte)OutputPorts.SHFTAMNT:
                    this.shiftAmount = (byte)(value & (byte)Mask.Mask3);
                    break;
                case (byte)OutputPorts.SHFT_DATA:
                    this.shiftData.Low = this.shiftData.High;
                    this.shiftData.High = value;
                    break;
                case (byte)OutputPorts.WATCHDOG:
                    if (Configuration.ShowWatchdogOutput)
                    {
                        System.Console.Out.Write(value < 64 ? CharacterSet[value] : '_');
                    }

                    break;
                case (byte)OutputPorts.SOUND1:
                    {
                        var soundUfo = (value & 1) != 0;
                        if (soundUfo)
                        {
                            this.OnUfoSound();
                        }

                        var soundShot = ((value & 2) != 0) && ((this.preSound1 & 2) == 0);
                        if (soundShot)
                        {
                            this.OnShotSound();
                        }

                        var soundPlayerDie = ((value & 4) != 0) && ((this.preSound1 & 4) == 0);
                        if (soundPlayerDie)
                        {
                            this.OnPlayerDieSound();
                        }

                        var soundInvaderDie = ((value & 8) != 0) && ((this.preSound1 & 8) == 0);
                        if (soundInvaderDie)
                        {
                            this.OnInvaderDieSound();
                        }

                        var extend = ((value & 0x10) != 0) && ((this.preSound1 & 0x10) == 0);
                        if (extend)
                        {
                            this.OnExtendSound();
                        }

                        var ampenable = ((value & 0x20) != 0) && ((this.preSound1 & 0x20) == 0);
                        if (ampenable)
                        {
                            this.OnEnableAmplifier();
                        }

                        var ampdisable = ((value & 0x20) == 0) && ((this.preSound1 & 0x20) != 0);
                        if (ampdisable)
                        {
                            this.OnDisableAmplifier();
                        }
                    }

                    break;

                case (byte)OutputPorts.SOUND2:
                    {
                        var soundWalk1 = ((value & 1) != 0) && ((this.preSound2 & 1) == 0);
                        if (soundWalk1)
                        {
                            this.OnWalk1Sound();
                        }

                        var soundWalk2 = ((value & 2) != 0) && ((this.preSound2 & 2) == 0);
                        if (soundWalk2)
                        {
                            this.OnWalk2Sound();
                        }

                        var soundWalk3 = ((value & 4) != 0) && ((this.preSound2 & 4) == 0);
                        if (soundWalk3)
                        {
                            this.OnWalk3Sound();
                        }

                        var soundWalk4 = ((value & 8) != 0) && ((this.preSound2 & 8) == 0);
                        if (soundWalk4)
                        {
                            this.OnWalk4Sound();
                        }

                        var soundUfoDie = ((value & 0x10) != 0) && ((this.preSound2 & 0x10) == 0);
                        if (soundUfoDie)
                        {
                            this.OnUfoDieSound();
                        }

                        this.CocktailModeControl = (value & 0x20) != 0;
                    }

                    break;
            }
        }

        private void Ports_WritingPort_SpaceInvaders(object sender, PortEventArgs e)
        {
            var value = this.ports.ReadOutputPort(e.Port);
            switch (e.Port)
            {
                case (byte)OutputPorts.SOUND1:
                    this.preSound1 = value;
                    break;
                case (byte)OutputPorts.SOUND2:
                    this.preSound2 = value;
                    break;
            }
        }

        private void CPU_ExecutingInstruction_Debug(object sender, System.EventArgs e) => System.Console.Error.WriteLine($"{EightBit.Disassembler.State(this.CPU)}\t{this.disassembler.Disassemble(this.CPU)}");

        private void RunCycles(int suggested)
        {
            this.allowed += suggested;
            var taken = this.CPU.Run(this.allowed);
            this.allowed -= taken;
        }
    }
}
