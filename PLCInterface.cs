using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Snap7;

namespace Koncar_Siemens_WPF 
{
    public class PLCInterfaceEventArgs : EventArgs
    {
        private Control controlData;

        public Control ControlData
        {
            get { return controlData; }
            set { controlData = value; }
        }

        private Status statusData;

        public Status StatusData
        {
            get { return statusData; }
            set { statusData = value; }
        }

        private AppInterfaceManual manualData;

        public AppInterfaceManual ManualData
        {
            get { return manualData; }
            set { manualData = value; }
        }
    }

    public class OnlineMarkerEventArgs : EventArgs
    {
        bool onlineMark;

        public bool OnlineMark
        {
            get { return onlineMark; }
            set { onlineMark = value; }
        }
    }
    public class PLCInterface
    {
        private int activeScreen;
        public int ActiveScreen
        {
            get { return activeScreen; }
            set { activeScreen = value; }
        }

        int second_counter = 0;
        static int third_counter = 0;

        public static object StatusControlLock = new object();
        public static object TimerLock = new object();
        
        public delegate void UpdateHandler(PLCInterface sender, PLCInterfaceEventArgs e);
        public delegate void OnlineMarker(PLCInterface sender, OnlineMarkerEventArgs e);
        
        public event UpdateHandler Update_1_s;
        public event UpdateHandler Update_100_ms;
        public event OnlineMarker Update_Online_Flag; 


        public bool OnlineMark;
        public S7Client Client;

        System.Timers.Timer Clock_100_ms;
        System.Timers.Timer WatchDogTimer;

        private byte[] CyclicReadBuffer = new byte[65536];
        private byte[] ReadBuffer = new byte[65536];
        private byte[] CyclicWriteBuffer = new byte[65536];
        private byte[] WriteBuffer = new byte[65536];
        private byte[] WatchdogBuffer = new byte[2];
        private short updateCounter = 0;
        
        public Control CONTROL = new Control();
        public Status STATUS = new Status();
        //public Axes_DB AXES = new Axes_DB();
        public AppInterfaceManual MANUAL = new AppInterfaceManual();
        public PLCInterface()
        {
            Client = new S7Client();
            //int timeout = 50;
            //Client.SetParam(Snap7.S7Consts.p_i32_PingTimeout, ref timeout);
            //Client.SetParam(Snap7.S7Consts.p_i32_SendTimeout, ref timeout);
            //Client.SetParam(Snap7.S7Consts.p_i32_RecvTimeout, ref timeout);

            Clock_100_ms = new System.Timers.Timer(100);
            Clock_100_ms.Elapsed += onClock100msTick;
            Clock_100_ms.AutoReset = false;

            WatchDogTimer = new System.Timers.Timer(2000);
            WatchDogTimer.Elapsed += onClockWatchdogTick;
            WatchDogTimer.AutoReset = false;
        }
      
        public void StartCyclic()
        {
            Clock_100_ms.Start();
            WatchDogTimer.Start();
        }

        void StopCyclic()
        {
            Clock_100_ms.Stop();
            WatchDogTimer.Stop();
        }

        public void RestartInterface()
        {
            lock (PLCInterface.TimerLock)
            {
                Client = new S7Client();
                Clock_100_ms.Stop();
                Thread.Sleep(1000);
                while (!Client.Connected())
                {
                    Client.ConnectTo("192.168.10.1", 0, 1);
                    Thread.Sleep(200);
                    if (Client.Connected())
                    {
                        Clock_100_ms.Start();
                        WatchDogTimer.Start();
                    }
                }
            }
        }

        #region read functions
        private int ReadControl()
        {
            int result = -99;
            if (Client.Connected())
                result = Client.DBRead(28, 0, 52, CyclicReadBuffer);
            if (result == 0)
            {
                lock (StatusControlLock)
                {
                    // Ripple
                    CONTROL.Ripple.StartRippleMeas.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Ripple.StartCompesation.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Ripple.SetFirstPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Ripple.SetSecondPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Ripple.Stop.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Ripple.RippleScr.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Ripple.ManualRippleMeas.GetValueFromGroupBuffer(CyclicReadBuffer);
                    
                    // Burr
                    CONTROL.Burr.StartBurrMeas.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Burr.SetFirstPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Burr.SetSecondPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Burr.Reset.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Burr.Stop.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Burr.BurrScr.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Burr.SimpleBurrMeas.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Burr.NumberOfMeas.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Burr.ManualBurrMeas.GetValueFromGroupBuffer(CyclicReadBuffer);

                    // Dimension
                    CONTROL.Dimension.StartDimensionMeas.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.CancelPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.SetFirstPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.SetSecondPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.SetThirdPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.StopDimensionMeas.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.StartAutomaticTeachIn.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Reset.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Stop.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.DimensionScr.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Jog_plus.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Jog_plus.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Jog_minus.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Jog_minus.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Point.POINT1.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Point.POINT1.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Point.POINT2.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Point.POINT2.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Point.PARAMETER.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.Point.TYPE.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.VelocitySetpoint.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Dimension.VelocitySetpoint.Y.GetValueFromGroupBuffer(CyclicReadBuffer);

                    // Saber

                    CONTROL.Saber.StartSaberMeas.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Saber.Stop.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Saber.SetFirstPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Saber.SetSecondPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Saber.SaberScr.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Saber.NumberOfMeas.GetValueFromGroupBuffer(CyclicReadBuffer);
                
                    // Angle

                    CONTROL.Angle.StartAngleMeas.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Angle.SetFirstPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Angle.SetSecondPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Angle.SetThitdPoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Angle.Stop.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Angle.AngleScr.GetValueFromGroupBuffer(CyclicReadBuffer);
                    CONTROL.Angle.NuberOfPoints.GetValueFromGroupBuffer(CyclicReadBuffer);

                    // Temperature

                    CONTROL.Temperature.CompesationOn.GetValueFromGroupBuffer(CyclicReadBuffer);
                }
            }
            return result;
        }

        private int ReadStatus()
        {
            int result = -99;
            if (Client.Connected())
                result = Client.DBRead(29, 0, 1536, CyclicReadBuffer); // was 1118
            if (result == 0)
            {
                lock (StatusControlLock)
                {
                    STATUS.Axes.ActualPosition.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.ActualPosition.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.ActualVelocity.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.ActualVelocity.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.ToAbsolute.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.ToRelative.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.ToHome.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.JogActive.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.ReferencedX.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.ReferencedY.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.FaultX.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.FaultY.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.InPositionX.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Axes.InPositionY.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Ripple.AutomaticActive.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Ripple.SheetAbsent.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Ripple.LaserActualValue.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Ripple.FirstPoint.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Ripple.FirstPoint.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Ripple.LastPoint.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Ripple.LastPoint.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Burr.AutomaticActive.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Burr.AutomaticSimpleActive.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Burr.LengthGaugeActualValue.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Burr.FirstPoint.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Burr.FirstPoint.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Burr.LastPoint.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Burr.LastPoint.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Dimension.AutomaticActive.GetValueFromGroupBuffer(CyclicReadBuffer);

                    for (int i = 0; i < 50; i++)
                    {
                        if (STATUS.Dimension.POINTS == null)
                            STATUS.Dimension.POINTS = new CameraOutput[50];
                        STATUS.Dimension.POINTS[i].POINT1.X = S7.GetRealAt(CyclicReadBuffer, 24+8+32 + i * 22);
                        STATUS.Dimension.POINTS[i].POINT1.Y = S7.GetRealAt(CyclicReadBuffer, 28+8+32 + i * 22);
                        STATUS.Dimension.POINTS[i].POINT2.X = S7.GetRealAt(CyclicReadBuffer, 32+8+32 + i * 22);
                        STATUS.Dimension.POINTS[i].POINT2.Y = S7.GetRealAt(CyclicReadBuffer, 36+8+32 + i * 22);
                        STATUS.Dimension.POINTS[i].PARAMETER = S7.GetRealAt(CyclicReadBuffer, 40+8+32 + i * 22);
                        STATUS.Dimension.POINTS[i].TYPE = (short)S7.GetIntAt(CyclicReadBuffer, 44+8+32 + i * 22);
                    }

                    STATUS.Saber.AutomaticActive.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Saber.FirstPoint.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Saber.FirstPoint.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Saber.SecondPoint.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Saber.SecondPoint.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    for (int i = 0; i < 20; i++)
                    {
                        if (STATUS.Saber.POINTS == null)
                            STATUS.Saber.POINTS = new AreaPoint[20];
                        STATUS.Saber.POINTS[i].X = S7.GetRealAt(CyclicReadBuffer, 1182 + i * 8);
                        STATUS.Saber.POINTS[i].Y = S7.GetRealAt(CyclicReadBuffer, 1186 + i * 8);
                        
                    }

                    STATUS.Angle.AutomaticActive.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Angle.FirstPoint.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Angle.FirstPoint.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Angle.SecondPoint.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Angle.SecondPoint.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Angle.LastPoint.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Angle.LastPoint.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    for (int i = 0; i < 10; i++)
                    {
                        if (STATUS.Angle.LINE1 == null)
                            STATUS.Angle.LINE1 = new AreaPoint[10];
                        STATUS.Angle.LINE1[i].X = S7.GetRealAt(CyclicReadBuffer, 1368 + i * 8);
                        STATUS.Angle.LINE1[i].Y = S7.GetRealAt(CyclicReadBuffer, 1372 + i * 8);
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        if (STATUS.Angle.LINE2 == null)
                            STATUS.Angle.LINE2 = new AreaPoint[10];
                        STATUS.Angle.LINE2[i].X = S7.GetRealAt(CyclicReadBuffer, 1448 + i * 8);
                        STATUS.Angle.LINE2[i].Y = S7.GetRealAt(CyclicReadBuffer, 1452 + i * 8);

                    }
                    STATUS.Temperature.Glass1.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Temperature.Glass2.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Temperature.LG1.GetValueFromGroupBuffer(CyclicReadBuffer);
                    STATUS.Temperature.LG2.GetValueFromGroupBuffer(CyclicReadBuffer);
                }
            }
            return result;
        }

        private int ReadManual()
        {
            int result = -99;
            if (Client.Connected())
                result = Client.DBRead(26, 0, 28, CyclicReadBuffer);
            if (result == 0)
            {
                lock (StatusControlLock)
                {
                    // Manual
                    MANUAL.ManualActive.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.JogPlus.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.JogPlus.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.JogMinus.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.JogMinus.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.VelocitySetpoint.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.AbsolutePosition.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.AbsolutePosition.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.RelativePosition.X.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.RelativePosition.Y.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.ToAbsPosition.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.ToRelPosition.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.ToHomePosition.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.StartReferencing.GetValueFromGroupBuffer(CyclicReadBuffer);
                    MANUAL.ReleseAxis.GetValueFromGroupBuffer(CyclicReadBuffer);
                }
            }
            return result;
        }

        public byte[] ReadCustom(int dbNumber, int startByte, int size)
        {
            int result = -99;
            lock (PLCInterface.TimerLock)
            {
                if (Client.Connected())
                {
                    result = Client.DBRead(dbNumber, startByte, size, ReadBuffer);
                }
            }
            return ReadBuffer;
        }
        #endregion

        #region write functions
        /// <summary>
        /// Writes one bit in DBmemory location, returns result of operation
        /// </summary>
        /// <param name="dbNumber"> data block number </param>
        /// <param name="startByte"> byte address in data block </param>
        /// <param name="bitInWord"> bit address in data block </param>
        /// <param name="operation"> operation parameter: acceptible values are "set", "reset", "toggle" </param>
        /// <returns></returns>
        public int WriteBit(int dbNumber, int startByte, int bitInWord, string operation)
        {
            byte[] _tempBuffer = new byte[2];
            int result = -99;
            lock (PLCInterface.TimerLock)
            {
                if (Client.Connected())
                {
                    result = Client.DBRead(dbNumber, startByte, 2, _tempBuffer);
                    switch (operation)
                    {
                        case "set":
                            S7.SetBitAt(ref _tempBuffer, 0, bitInWord, true);
                            break;
                        case "reset":
                            S7.SetBitAt(ref _tempBuffer, 0, bitInWord, false);
                            break;
                        case "toggle":
                            S7.SetBitAt(ref _tempBuffer, 0, bitInWord, !S7.GetBitAt(_tempBuffer, 0, bitInWord));
                            break;
                        default:
                            break;
                    }
                    result += Client.DBWrite(dbNumber, startByte, 2, _tempBuffer);
                }
                try
                {
                    if (result != 0)
                        throw new System.InvalidOperationException("write error");
                }
                finally
                {
                }
            }
            return result;
        }

        public int WriteTag(plcTag tag, object value)
        {
            byte[] _tempBuffer = new byte[4];
            int result = -99;
            lock (PLCInterface.TimerLock)
            {
                if (Client.Connected())
                {
                    switch (tag.VType)
                    {
                        case varType.BOOL:
                            result = Client.DBRead(tag.DbNumber, tag.Offset.ByteOffset, 2, _tempBuffer);
                            S7.SetBitAt(ref _tempBuffer, 0, tag.Offset.BitOffset, (bool)value);
                            result += Client.DBWrite(tag.DbNumber, tag.Offset.ByteOffset, 2, _tempBuffer);
                            break;
                        case varType.BYTE:
                            result = Client.DBRead(tag.DbNumber, tag.Offset.ByteOffset, 2, _tempBuffer);
                            S7.SetByteAt(_tempBuffer, 0, (byte)value);
                            result += Client.DBWrite(tag.DbNumber, tag.Offset.ByteOffset, 2, _tempBuffer);
                            break;
                        case varType.WORD:
                            result = Client.DBRead(tag.DbNumber, tag.Offset.ByteOffset, 2, _tempBuffer);
                            S7.SetWordAt(_tempBuffer, 0, (ushort)value);
                            result += Client.DBWrite(tag.DbNumber, tag.Offset.ByteOffset, 2, _tempBuffer);
                            break;
                        case varType.DWORD:
                            result = Client.DBRead(tag.DbNumber, tag.Offset.ByteOffset, 4, _tempBuffer);
                            S7.SetDWordAt(_tempBuffer, 0, (uint)value);
                            result += Client.DBWrite(tag.DbNumber, tag.Offset.ByteOffset, 4, _tempBuffer);
                            break;
                        case varType.INT:
                            result = Client.DBRead(tag.DbNumber, tag.Offset.ByteOffset, 2, _tempBuffer);
                            S7.SetIntAt(_tempBuffer, 0, (short)value);
                            result += Client.DBWrite(tag.DbNumber, tag.Offset.ByteOffset, 2, _tempBuffer);
                            break;
                        case varType.DINT:
                            result = Client.DBRead(tag.DbNumber, tag.Offset.ByteOffset, 4, _tempBuffer);
                            S7.SetDIntAt(_tempBuffer, 0, (int)value);
                            result += Client.DBWrite(tag.DbNumber, tag.Offset.ByteOffset, 4, _tempBuffer);
                            break;
                        case varType.REAL:
                            result = Client.DBRead(tag.DbNumber, tag.Offset.ByteOffset, 4, _tempBuffer);
                            S7.SetRealAt(_tempBuffer, 0, (float)value);
                            result += Client.DBWrite(tag.DbNumber, tag.Offset.ByteOffset, 4, _tempBuffer);
                            break;
                    }
                }
                try
                {
                    if (result != 0)
                        throw new System.InvalidOperationException("write error");
                }
                catch { }
                finally
                {
                }
            }
            return result;
        }

        public int WriteToggle(plcTag tag)
        {
            byte[] _tempBuffer = new byte[4];
            int result = -99;
            lock (PLCInterface.TimerLock)
            {
                if (Client.Connected())
                {
                    if (tag.VType == varType.BOOL)
                    {
                        if (tag.DType == dataType.DB)
                        {
                            result = Client.DBRead(tag.DbNumber, tag.Offset.ByteOffset, 2, _tempBuffer);
                            S7.SetBitAt(ref _tempBuffer, 0, tag.Offset.BitOffset, !S7.GetBitAt(_tempBuffer, 0, tag.Offset.BitOffset));
                            result += Client.DBWrite(tag.DbNumber, tag.Offset.ByteOffset, 2, _tempBuffer);
                        }
                        if (tag.DType == dataType.Q)
                        {
                            result = Client.ABRead(tag.Offset.ByteOffset,2, _tempBuffer);
                            S7.SetBitAt(ref _tempBuffer, 0, tag.Offset.BitOffset, !S7.GetBitAt(_tempBuffer, 0, tag.Offset.BitOffset));
                            result += Client.ABWrite(tag.Offset.ByteOffset,2,_tempBuffer);
                        }
                        
                    }
                }
                try
                {
                    if (result != 0)
                        throw new System.InvalidOperationException("write error");
                }
                catch { }
                finally
                {
                }
            }
            return result;
        }

        public int WriteCameraOutput(PrepoznavanjeOblika.CameraOutputType cameraOutput)
        {
            byte[] _tempBuffer = new byte[22];
            int result = -99;
            lock (PLCInterface.TimerLock)
            {
               
               
                if (Client.Connected())
                {
                    S7.SetRealAt(_tempBuffer, 0, cameraOutput.POINT1.X + (float)STATUS.Axes.ActualPosition.X.Value);
                    S7.SetRealAt(_tempBuffer, 4, cameraOutput.POINT1.Y + (float)STATUS.Axes.ActualPosition.Y.Value);
                    S7.SetRealAt(_tempBuffer, 8, cameraOutput.POINT2.X + (float)STATUS.Axes.ActualPosition.X.Value);
                    S7.SetRealAt(_tempBuffer, 12, cameraOutput.POINT2.Y + (float)STATUS.Axes.ActualPosition.Y.Value);
                    S7.SetRealAt(_tempBuffer, 16, cameraOutput.PARAMETER);
                    S7.SetIntAt(_tempBuffer, 20, (short)cameraOutput.TYPE);
                    result = Client.DBWrite(28, 12, 22, _tempBuffer);
                }
            }
            return result;
        }

        #endregion

        private void onClock100msTick(Object source, System.Timers.ElapsedEventArgs e)
        {
            Thread.CurrentThread.Name = "PLCinterface_100msTick_Thread_" + second_counter.ToString();
            second_counter++;

            int result;
            lock (TimerLock)
            {
                result = ReadStatus();
            }
            PLCInterfaceEventArgs p1 = new PLCInterfaceEventArgs();
            p1.StatusData = STATUS;
            if (Update_100_ms != null)
                Update_100_ms(this, p1);

            if (updateCounter == 10)
            {
                result = 0;
                lock (TimerLock)
                {
                    result = ReadControl();
                    result += ReadManual();
                }
                PLCInterfaceEventArgs p2 = new PLCInterfaceEventArgs();
                p2.ControlData = CONTROL;
                p2.StatusData = STATUS;
                p2.ManualData = MANUAL;
                
                if ((Update_1_s != null)&&(result==0))
                    Update_1_s(this, p2);

                updateCounter = 0;
            }

            updateCounter++;
            Clock_100_ms.Start();
        }

        private void onClockWatchdogTick(Object source, System.Timers.ElapsedEventArgs e)
        {
            Thread.CurrentThread.Name = "PLCinterface_WatchdogTick_Thread" + PLCInterface.third_counter.ToString();
            PLCInterface.third_counter++;

            lock (TimerLock)
            {
                int result = -99;
                Array.Clear(WatchdogBuffer, 0, WatchdogBuffer.Length);
                switch (activeScreen)
                {
                    case 0:
                        break;
                    case 1:
                        S7.SetBitAt(ref WatchdogBuffer, 0, 3, true);
                        break;
                    case 2:
                        S7.SetBitAt(ref WatchdogBuffer, 0, 4, true);
                        break;
                    case 3:
                        S7.SetBitAt(ref WatchdogBuffer, 0, 5, true);
                        break;
                    case 4:
                        S7.SetBitAt(ref WatchdogBuffer, 0, 6, true);
                        break;
                    case 5:
                        S7.SetBitAt(ref WatchdogBuffer, 0, 7, true);
                        break;
                    case 6:
                        S7.SetBitAt(ref WatchdogBuffer, 1, 0, true);
                        break;
                    case 7:
                        S7.SetBitAt(ref WatchdogBuffer, 1, 1, true);
                        break;
                    case 8:
                        S7.SetBitAt(ref WatchdogBuffer, 1, 2, true);
                        break;
                }
                S7.SetBitAt(ref WatchdogBuffer, 0, 1, true);
                result = Client.DBWrite(10, 0, 2, WatchdogBuffer);
                if (result == 0)
                    result = Client.DBRead(10, 0, 2, WatchdogBuffer);
                else
                    OnlineMark = false;
                if (result == 0)
                {
                    OnlineMark = S7.GetBitAt(WatchdogBuffer, 0, 2);
                }
                else
                {
                    OnlineMark = false;
                }
            }
            OnlineMarkerEventArgs p = new OnlineMarkerEventArgs();
            p.OnlineMark = OnlineMark;
            if (Update_Online_Flag != null)
                Update_Online_Flag(this, p);
            if (OnlineMark)
            {
                WatchDogTimer.Start();
            }
            else
            {
                RestartInterface();
            }
        }
    }

    #region Control and Status definitions
    public class Control
    {
        public ripple Ripple = new ripple();
        public burr Burr = new burr();
        public dimension Dimension = new dimension();
        public saber Saber = new saber();
        public angle Angle = new angle();
        public temperature Temperature = new temperature();

        public Control()
        {
            Temperature.CompesationOn.Value = false;
        }
        public class ripple
        {
            public plcTag StartRippleMeas           = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(0, 0), false);
            public plcTag StartCompesation          = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(0, 1), false);
            public plcTag SetFirstPoint             = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(0, 2), false);
            public plcTag SetSecondPoint            = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(0, 3), false);
            public plcTag Stop                      = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(0, 4), false);
            public plcTag ManualRippleMeas          = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(0, 5), false);
            public plcTag ReferenceLaser            = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(0, 6), false);
            public plcTag RippleScr                 = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(0, 7), false);
        }
        public class burr
        {
            public plcTag StartBurrMeas             = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(2, 0), false);
            public plcTag SetFirstPoint             = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(2, 1), false);
            public plcTag SetSecondPoint            = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(2, 2), false);
            public plcTag Reset                     = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(2, 3), false);
            public plcTag Stop                      = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(2, 4), false);
            public plcTag ManualBurrMeas            = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(2, 5), false);
            public plcTag ReferenceLengthGauge      = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(2, 6), false);
            public plcTag BurrScr                   = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(2, 7), false);
            public plcTag SimpleBurrMeas            = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(3, 0), false);
            public plcTag NumberOfMeas              = new plcTag(varType.INT,  dataType.DB, 28, new Offset(4, 0), 0);
        }
        public class dimension
        {
            public plcTag StartDimensionMeas        = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(6, 0), false);
            public plcTag CancelPoint               = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(6, 1), false);
            public plcTag SetFirstPoint             = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(6, 2), false);
            public plcTag SetSecondPoint            = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(6, 3), false);
            public plcTag SetThirdPoint             = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(6, 4), false);
            public plcTag StopDimensionMeas         = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(6, 5), false);
            public plcTag StartAutomaticTeachIn     = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(6, 6), false);
            public plcTag Reset                     = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(6, 7), false);
            public plcTag Stop                      = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(7, 0), false);
            public plcTag DimensionScr              = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(7, 1), false);
            public jog_plus Jog_plus                = new jog_plus();
            public jog_minus Jog_minus              = new jog_minus();

            public class jog_plus 
            { 
                public plcTag X                     = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(8, 0), false);
                public plcTag Y                     = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(8, 1), false);
            }
            public class jog_minus
            {
                public plcTag X                     = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(10, 0), false);
                public plcTag Y                     = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(10, 1), false);
            }
            public class point
            {
                public class pOINT1
                {
                    public plcTag X                 = new plcTag(varType.REAL, dataType.DB, 28, new Offset(12, 0), 0.0f);
                    public plcTag Y                 = new plcTag(varType.REAL, dataType.DB, 28, new Offset(16, 0), 0.0f);
                }
                public class pOINT2
                {
                    public plcTag X                 = new plcTag(varType.REAL, dataType.DB, 28, new Offset(20, 0), 0.0f);
                    public plcTag Y                 = new plcTag(varType.REAL, dataType.DB, 28, new Offset(24, 0), 0.0f);
                }
                public plcTag PARAMETER             = new plcTag(varType.REAL, dataType.DB, 28, new Offset(28, 0), 0.0f);
                public plcTag TYPE                  = new plcTag(varType.INT, dataType.DB, 28, new Offset(32, 0), 0.0f);
                
                public pOINT1 POINT1 = new pOINT1();
                public pOINT2 POINT2 = new pOINT2();
            }
            public class velocitySetpoint
            {
                public plcTag X                     = new plcTag(varType.REAL, dataType.DB, 28, new Offset(34, 0), 0.0f);
                public plcTag Y                     = new plcTag(varType.REAL, dataType.DB, 28, new Offset(38, 0), 0.0f);
            }
            
            public point Point = new point();
            public velocitySetpoint VelocitySetpoint = new velocitySetpoint();
            public CameraOutput[] SETPOINTS;   //koristiti na drukciji nacin
        }
        public class saber
        {
            public plcTag StartSaberMeas            = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(42, 0), false);
            public plcTag SetFirstPoint             = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(42, 1), false);
            public plcTag SetSecondPoint            = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(42, 2), false);
            public plcTag Stop                      = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(42, 3), false);
            public plcTag SaberScr                  = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(42, 4), false);
            public plcTag NumberOfMeas              = new plcTag(varType.INT, dataType.DB, 28, new Offset(44, 0), 0);
        }
        public class angle
        {
            public plcTag StartAngleMeas            = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(46, 0), false);
            public plcTag SetFirstPoint             = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(46, 1), false);
            public plcTag SetSecondPoint            = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(46, 2), false);
            public plcTag SetThitdPoint             = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(46, 3), false);
            public plcTag Stop                      = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(46, 4), false);
            public plcTag AngleScr                  = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(46, 5), false);
            public plcTag NuberOfPoints             = new plcTag(varType.INT, dataType.DB, 28, new Offset(48, 0), false);
        }
        public class temperature
        {
            public plcTag CompesationOn             = new plcTag(varType.BOOL, dataType.DB, 28, new Offset(50, 0), false);
        }
    }

    public class Status
    {
        public axes Axes = new axes();
        public ripple Ripple = new ripple();
        public burr Burr = new burr();
        public dimension Dimension = new dimension();
        public saber Saber = new saber();
        public angle Angle = new angle();
        public temperature Temperature = new temperature();

        public Status()
        {
            Axes.ActualPosition.X.Value = 0.0f;
            Axes.ActualPosition.Y.Value = 0.0f;
            Axes.ActualVelocity.X.Value = 0.0f;
            Axes.ActualVelocity.Y.Value = 0.0f;
            Axes.ToAbsolute.Value = false;
            Axes.ToRelative.Value = false;
            Axes.ToHome.Value = false;
            Axes.JogActive.Value = false;
            Axes.ReferencedX.Value = false;
            Axes.ReferencedY.Value = false;
            Axes.FaultX.Value = false;
            Axes.FaultY.Value = false;
            Axes.InPositionX.Value = false;
            Axes.InPositionY.Value = false;
            Ripple.AutomaticActive.Value = false;
            Ripple.SheetAbsent.Value = false;
            Ripple.LaserActualValue.Value = 0.0f;
            Ripple.FirstPoint.X.Value = 0.0f;
            Ripple.FirstPoint.Y.Value = 0.0f;
            Ripple.LastPoint.X.Value = 0.0f;
            Ripple.LastPoint.Y.Value = 0.0f;
            Burr.AutomaticActive.Value = false;
            Burr.AutomaticSimpleActive.Value = false;
            Burr.LengthGaugeActualValue.Value = 0.0f;
            Burr.FirstPoint.X.Value = 0.0f;
            Burr.FirstPoint.Y.Value = 0.0f;
            Burr.LastPoint.X.Value = 0.0f;
            Burr.LastPoint.Y.Value = 0.0f;
            Dimension.AutomaticActive.Value = false;
            for (int i = 0; i < 50; i++)
            {
                if (Dimension.POINTS == null)
                    Dimension.POINTS = new CameraOutput[50];
                Dimension.POINTS[i].POINT1.X = 0.0f;
                Dimension.POINTS[i].POINT1.Y = 0.0f;
                Dimension.POINTS[i].POINT2.X = 0.0f;
                Dimension.POINTS[i].POINT2.Y = 0.0f;
                Dimension.POINTS[i].PARAMETER = 0.0f;
                Dimension.POINTS[i].TYPE = 0;
            }

            Saber.AutomaticActive.Value = false;
            Saber.FirstPoint.X.Value = 0.0f;
            Saber.FirstPoint.Y.Value = 0.0f;
            Saber.SecondPoint.X.Value = 0.0f;
            Saber.SecondPoint.Y.Value = 0.0f;
            for (int i = 0; i < 20; i++)
            {
                if (Saber.POINTS == null)
                    Saber.POINTS = new AreaPoint[20];
                Saber.POINTS[i].X = 0.0f;
                Saber.POINTS[i].Y = 0.0f;
            }

            Angle.AutomaticActive.Value = false;
            Angle.FirstPoint.X.Value = 0.0f;
            Angle.FirstPoint.Y.Value = 0.0f;
            Angle.SecondPoint.X.Value = 0.0f;
            Angle.SecondPoint.Y.Value = 0.0f;
            Angle.LastPoint.X.Value = 0.0f;
            Angle.LastPoint.Y.Value = 0.0f;
            for (int i = 0; i < 10; i++)
            {
                if (Angle.LINE1 == null)
                    Angle.LINE1 = new AreaPoint[10];
                Angle.LINE1[i].X = 0.0f;
                Angle.LINE1[i].Y = 0.0f;
            }
            for (int i = 0; i < 10; i++)
            {
                if (Angle.LINE2 == null)
                    Angle.LINE2 = new AreaPoint[10];
                Angle.LINE2[i].X = 0.0f;
                Angle.LINE2[i].Y = 0.0f;
            }
            Temperature.Glass1.Value = 240;
            Temperature.Glass2.Value = 240;
            Temperature.LG1.Value = 240;
            Temperature.LG2.Value = 240;
        }
        public class axes
        {
            public class actualPosition
            {
                public plcTag X = new plcTag(varType.REAL, dataType.DB, 29, new Offset(0, 0), 0.0f);
                public plcTag Y = new plcTag(varType.REAL, dataType.DB, 29, new Offset(4, 0), 0.0f);
            }
            public class actualVelocity
            {
                public plcTag X = new plcTag(varType.REAL, dataType.DB, 29, new Offset(8, 0), 0.0f);
                public plcTag Y = new plcTag(varType.REAL, dataType.DB, 29, new Offset(12, 0), 0.0f);
            }
            public actualPosition ActualPosition = new actualPosition();
            public actualVelocity ActualVelocity = new actualVelocity();
            public plcTag ToAbsolute = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(16, 0), false);
            public plcTag ToRelative = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(16, 1), false);
            public plcTag ToHome = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(16, 2), false);
            public plcTag JogActive = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(16, 3), false);
            public plcTag ReferencedX = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(16, 4), false);
            public plcTag ReferencedY = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(16, 5), false);
            public plcTag FaultX = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(16, 6), false);
            public plcTag FaultY = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(16, 7), false);

            // Dodati u PLC DB29
            public plcTag InPositionX = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(17, 0), false);
            public plcTag InPositionY = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(17, 1), false);
        }

        public class ripple
        {
            public plcTag AutomaticActive = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(18, 0), false);
            public plcTag SheetAbsent = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(18, 1), false);
            public plcTag LaserActualValue = new plcTag(varType.REAL, dataType.DB, 29, new Offset(20, 0), 0.0f);
            public class firstPoint
            {
                public plcTag X = new plcTag(varType.REAL, dataType.DB, 29, new Offset(24, 0), 0.0f);
                public plcTag Y = new plcTag(varType.REAL, dataType.DB, 29, new Offset(28, 0), 0.0f);
            }
            public class lastPoint
            {
                public plcTag X = new plcTag(varType.REAL, dataType.DB, 29, new Offset(32, 0), 0.0f);
                public plcTag Y = new plcTag(varType.REAL, dataType.DB, 29, new Offset(36, 0), 0.0f);
            }
            public firstPoint FirstPoint = new firstPoint();
            public lastPoint LastPoint = new lastPoint();
        }
        public class burr
        {
            public plcTag AutomaticActive = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(40, 0), false);
            public plcTag AutomaticSimpleActive = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(40, 1), false);
            public plcTag LengthGaugeActualValue = new plcTag(varType.REAL, dataType.DB, 29, new Offset(42, 0), 0.0f);
            public class firstPoint
            {
                public plcTag X = new plcTag(varType.REAL, dataType.DB, 29, new Offset(46, 0), 0.0f);
                public plcTag Y = new plcTag(varType.REAL, dataType.DB, 29, new Offset(50, 0), 0.0f);
            }
            public class lastPoint
            {
                public plcTag X = new plcTag(varType.REAL, dataType.DB, 29, new Offset(54, 0), 0.0f);
                public plcTag Y = new plcTag(varType.REAL, dataType.DB, 29, new Offset(58, 0), 0.0f);
            }
            public firstPoint FirstPoint = new firstPoint();
            public lastPoint LastPoint = new lastPoint();
        }

        public class dimension
        {
            public plcTag AutomaticActive = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(62, 0), false);
            

            public CameraOutput[] POINTS;
        }

        public class saber
        {
            public plcTag AutomaticActive = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(1164, 0), false);
            public class firstPoint
            {
                public plcTag X = new plcTag(varType.REAL, dataType.DB, 29, new Offset(1166, 0), 0.0f);
                public plcTag Y = new plcTag(varType.REAL, dataType.DB, 29, new Offset(1170, 0), 0.0f);
            }
            public class secondPoint
            {
                public plcTag X = new plcTag(varType.REAL, dataType.DB, 29, new Offset(1174, 0), 0.0f);
                public plcTag Y = new plcTag(varType.REAL, dataType.DB, 29, new Offset(1178, 0), 0.0f);
            }
            public firstPoint FirstPoint = new firstPoint();
            public secondPoint SecondPoint = new secondPoint();
            public AreaPoint[] POINTS;
        }

        public class angle
        {
            public plcTag AutomaticActive = new plcTag(varType.BOOL, dataType.DB, 29, new Offset(1342, 0), false);
            public class firstPoint
            {
                public plcTag X = new plcTag(varType.REAL, dataType.DB, 29, new Offset(1344, 0), 0.0f);
                public plcTag Y = new plcTag(varType.REAL, dataType.DB, 29, new Offset(1348, 0), 0.0f);
            }
            public class secondPoint
            {
                public plcTag X = new plcTag(varType.REAL, dataType.DB, 29, new Offset(1352, 0), 0.0f);
                public plcTag Y = new plcTag(varType.REAL, dataType.DB, 29, new Offset(1356, 0), 0.0f);
            }
            public class lastPoint
            {
                public plcTag X = new plcTag(varType.REAL, dataType.DB, 29, new Offset(1360, 0), 0.0f);
                public plcTag Y = new plcTag(varType.REAL, dataType.DB, 29, new Offset(1364, 0), 0.0f);
            }
            public firstPoint FirstPoint = new firstPoint();
            public secondPoint SecondPoint = new secondPoint();
            public lastPoint LastPoint = new lastPoint();
            public AreaPoint[] LINE1;
            public AreaPoint[] LINE2;
        }

        public class temperature
        {
            public plcTag Glass1 = new plcTag(varType.INT, dataType.DB, 29, new Offset(1528, 0), 0.0f);
            public plcTag Glass2 = new plcTag(varType.INT, dataType.DB, 29, new Offset(1530, 0), 0.0f);
            public plcTag LG1 = new plcTag(varType.INT, dataType.DB, 29, new Offset(1532, 0), 0.0f);
            public plcTag LG2 = new plcTag(varType.INT, dataType.DB, 29, new Offset(1534, 0), 0.0f);
        }
    }
    


    public class Axes_DB
    {
        public Axes_DB()
        {

        }

        // Actual position
        public class actualPosition
        {
            public plcTag X = new plcTag(varType.REAL, dataType.DB, 100, new Offset(0, 0), 0.0f);
            public plcTag Y = new plcTag(varType.REAL, dataType.DB, 100, new Offset(4, 0), 0.0f);
        }
        public actualPosition ActualPosition = new actualPosition();

        // Previous position
        public class previousPosition
        {
            public plcTag X = new plcTag(varType.REAL, dataType.DB, 100, new Offset(8, 0), 0.0f);
            public plcTag Y = new plcTag(varType.REAL, dataType.DB, 100, new Offset(12, 0), 0.0f);
        }
        public previousPosition PreviousPosition = new previousPosition();

        // First absolute point
        public class firstAbsolutePoint
        {
            public plcTag X = new plcTag(varType.REAL, dataType.DB, 100, new Offset(16, 0), 0.0f);
            public plcTag Y = new plcTag(varType.REAL, dataType.DB, 100, new Offset(20, 0), 0.0f);
        }
        public firstAbsolutePoint FirstAbsolutePoint = new firstAbsolutePoint();

        // Second absolute point
        public class secondAbsolutePoint
        {
            public plcTag X = new plcTag(varType.REAL, dataType.DB, 100, new Offset(24, 0), 0.0f);
            public plcTag Y = new plcTag(varType.REAL, dataType.DB, 100, new Offset(28, 0), 0.0f);
        }
        public secondAbsolutePoint SecondAbsolutePoint = new secondAbsolutePoint();

        // Actual velocity
        public class actualVelocity
        {
            public plcTag X = new plcTag(varType.REAL, dataType.DB, 100, new Offset(32, 0), 0.0f);
            public plcTag Y = new plcTag(varType.REAL, dataType.DB, 100, new Offset(36, 0), 0.0f);
        }
        public actualVelocity ActualVelocity = new actualVelocity();

        // First position velocity
        public class firstPositionVelocity
        {
            public plcTag X = new plcTag(varType.REAL, dataType.DB, 100, new Offset(40, 0), 0.0f);
            public plcTag Y = new plcTag(varType.REAL, dataType.DB, 100, new Offset(44, 0), 0.0f);
        }
        public firstPositionVelocity FirstPositionVelocity = new firstPositionVelocity();

        // Second position velocity
        public class secondPositionVelocity
        {
            public plcTag X = new plcTag(varType.REAL, dataType.DB, 100, new Offset(48, 0), 0.0f);
            public plcTag Y = new plcTag(varType.REAL, dataType.DB, 100, new Offset(52, 0), 0.0f);
        }
        public secondPositionVelocity SecondPositionVelocity = new secondPositionVelocity();

        // Jog velocity
        public class jogVelocity
        {
            public plcTag X = new plcTag(varType.REAL, dataType.DB, 100, new Offset(56, 0), 0.0f);
            public plcTag Y = new plcTag(varType.REAL, dataType.DB, 100, new Offset(60, 0), 0.0f);
        }
        public jogVelocity JogVelocity = new jogVelocity();

        // Resolution
        public plcTag Resolution = new plcTag(varType.REAL, dataType.DB, 100, new Offset(64, 0), false);

        // Absolute velocity
        public plcTag AbsoluteVelocity = new plcTag(varType.REAL, dataType.DB, 100, new Offset(68, 0), false);

        // Jog+
        public class jogPlus
        {
            public plcTag X = new plcTag(varType.BOOL, dataType.DB, 100, new Offset(72, 0), false);
            public plcTag Y = new plcTag(varType.BOOL, dataType.DB, 100, new Offset(72, 1), false);
        }
        public jogPlus JogPlus = new jogPlus();

        // Jog-
        public class jogMinus
        {
            public plcTag X = new plcTag(varType.BOOL, dataType.DB, 100, new Offset(74, 0), false);
            public plcTag Y = new plcTag(varType.BOOL, dataType.DB, 100, new Offset(74, 1), false);
        }
        public jogMinus JogMinus = new jogMinus();

        // To position 1
        public plcTag ToPosition1 = new plcTag(varType.BOOL, dataType.DB, 100, new Offset(76, 0), false);

        // To position 2
        public plcTag ToPosition2 = new plcTag(varType.BOOL, dataType.DB, 100, new Offset(76, 1), false);

        // To home position
        public plcTag ToHomePosition = new plcTag(varType.BOOL, dataType.DB, 100, new Offset(76, 2), false);

        // Set home position
        public plcTag SetHomePosition = new plcTag(varType.BOOL, dataType.DB, 100, new Offset(76, 3), false);

        // Stop
        public plcTag Stop = new plcTag(varType.BOOL, dataType.DB, 100, new Offset(76, 4), false);

        // Error reset
        public plcTag ErrorReset = new plcTag(varType.BOOL, dataType.DB, 100, new Offset(76, 5), false);
    }

    public class AppInterfaceManual
    {
        public AppInterfaceManual()
        {
        }
        
        // Manual Active
        public plcTag ManualActive = new plcTag(varType.BOOL, dataType.DB, 26, new Offset(0, 0), false);

        // Jog+
        public class jogPlus
        {
            public plcTag X = new plcTag(varType.BOOL, dataType.DB, 26, new Offset(2, 0), false);
            public plcTag Y = new plcTag(varType.BOOL, dataType.DB, 26, new Offset(2, 1), false);
        }
        public jogPlus JogPlus = new jogPlus();

        // Jog-
        public class jogMinus
        {
            public plcTag X = new plcTag(varType.BOOL, dataType.DB, 26, new Offset(4, 0), false);
            public plcTag Y = new plcTag(varType.BOOL, dataType.DB, 26, new Offset(4, 1), false);
        }
        public jogMinus JogMinus = new jogMinus();

        // Velocity setpoint
        public plcTag VelocitySetpoint =new plcTag(varType.REAL, dataType.DB, 26, new Offset(6, 0), 0.0f);

        // Absolute position
        public class absolutePosition
        {
            public plcTag X = new plcTag(varType.REAL, dataType.DB, 26, new Offset(10, 0), 0.0f);
            public plcTag Y = new plcTag(varType.REAL, dataType.DB, 26, new Offset(14, 0), 0.0f);
        }
        public absolutePosition AbsolutePosition = new absolutePosition();

        // Relative position
        public class relativePosition
        {
            public plcTag X = new plcTag(varType.REAL, dataType.DB, 26, new Offset(18, 0), 0.0f);
            public plcTag Y = new plcTag(varType.REAL, dataType.DB, 26, new Offset(22, 0), 0.0f);
        }
        public relativePosition RelativePosition = new relativePosition();

        // To absolute position
        public plcTag ToAbsPosition = new plcTag(varType.BOOL, dataType.DB, 26, new Offset(26, 0), false);

        // To relative position
        public plcTag ToRelPosition = new plcTag(varType.BOOL, dataType.DB, 26, new Offset(26, 1), false);

        // To home position
        public plcTag ToHomePosition = new plcTag(varType.BOOL, dataType.DB, 26, new Offset(26, 2), false);

        // Start axis referencing
        public plcTag StartReferencing = new plcTag(varType.BOOL, dataType.DB, 26, new Offset(26, 3), false);

        // Relese axis 
        public plcTag ReleseAxis = new plcTag(varType.BOOL, dataType.DB, 26, new Offset(26, 4), false);
    }

    #endregion

    #region plcTag definition
    public class plcTag
    {
        varType vType;
        public varType VType
        {
            get
            {
                return vType;
            }
        }

        dataType dType;
        public dataType DType
        {
            get
            {
                return dType;
            }
        }

        int dbNumber;
        public int DbNumber
        {
            get
            {
                return dbNumber;
            }
        }

        Offset offset;
        public Offset Offset
        {
            get { return offset; }
        }


        object value;
        public object Value { get; set; }


        public plcTag(varType _vType, dataType _dType, int _dbNumber, Offset _offset, object _value)
        {
            vType = _vType;
            dType = _dType;
            offset = _offset;
            value = _value;
            if (dType != dataType.DB)
            {
                dbNumber = 0;
            }
            else
            {
                dbNumber = _dbNumber;
            }
        }
        /// <summary>
        /// extract value from raw buffer
        /// </summary>
        /// <param name="buffer"> buffer length must be greater than Offset.ByteOffset+4, else do nothing </param>
        public void GetValueFromGroupBuffer(byte[] buffer)
        {
            if (buffer.Length<Offset.ByteOffset + 4)
                return;
            switch (VType)
            {
                case varType.BOOL:
                    Value = S7.GetBitAt(buffer, Offset.ByteOffset, Offset.BitOffset);
                    break;
                case varType.BYTE:
                    Value = S7.GetByteAt(buffer, Offset.ByteOffset);
                    break;
                case varType.WORD:
                    Value = S7.GetWordAt(buffer, Offset.ByteOffset);
                    break;
                case varType.DWORD:
                    Value = S7.GetDWordAt(buffer, Offset.ByteOffset);
                    break;
                case varType.INT:
                    Value = S7.GetIntAt(buffer, Offset.ByteOffset);
                    break;
                case varType.DINT:
                    Value = S7.GetDIntAt(buffer, Offset.ByteOffset);
                    break;
                case varType.REAL:
                    Value = S7.GetRealAt(buffer, Offset.ByteOffset);
                    break;
            }
        }
    }
    public struct Offset
    {
        short byteOffset;
        public short ByteOffset
        {
            get { return byteOffset; }
            set { byteOffset = value; }
        }

        short bitOffset;
        public short BitOffset
        {
            get { return bitOffset; }
            set { bitOffset = value; }
        }
        public Offset(short _byteOffset, short _bitOffset)
        {
            byteOffset = _byteOffset;
            bitOffset = _bitOffset;
        }
    }
    public enum varType { BOOL, BYTE, WORD, DWORD, INT, DINT, REAL };
    public enum dataType { DB, I, Q, M, L, T };
    
    #endregion
}
