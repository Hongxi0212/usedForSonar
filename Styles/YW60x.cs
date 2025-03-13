using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoScanFQCTest
{
    internal class YW60x : ScanCard
    {
        private bool IsUSB = false;

        private bool Isbusy = false;

        private bool Notbusy = false;

        public override event Action<State,string> OnScanCardEvent;

        public YW60x()
        {
            IsUSB = true;
        }

        public override ResponseInfo Open(int time = -1)
        {
            ResponseInfo response = new ResponseInfo();
            response.StartTime = DateTime.Now;
            try
            {
                Task timeOut = new Task(() =>
                {
                    if (IsUSB)
                    {
                        response.ret = YW605Reader.YW_USBHIDInitial() > 0 ?
                        ResponseState.Normal.GetHashCode() : ResponseState.Error.GetHashCode();
                    }
                    else
                    {
                        //response.ret = YW605Reader.YW_ComInitial(this.PortIndex, this.Bound) > 0 ?
                        //ResponseState.Normal.GetHashCode() : ResponseState.Error.GetHashCode();
                    }
                });
                timeOut.Start();

                if (!timeOut.Wait(time))
                {
                    response.ret = ResponseState.TimeOut.GetHashCode();
                }
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            response.EndTime = DateTime.Now;
            return response;
        }

        public override ResponseInfo Close(int time = -1)
        {
            ResponseInfo response = new ResponseInfo();
            response.StartTime = DateTime.Now;
            try
            {
                Task timeOut = new Task(() =>
                {
                    if (IsUSB)
                    {
                        response.ret = YW605Reader.YW_USBHIDFree() > 0 ?
                        ResponseState.Normal.GetHashCode() : ResponseState.Error.GetHashCode();
                    }
                    else
                    {
                        //response.ret = YW605Reader.YW_ComFree() > 0 ?
                        //ResponseState.Normal.GetHashCode() : ResponseState.Error.GetHashCode();
                    }
                });
                timeOut.Start();
                if (!timeOut.Wait(time))
                {
                    response.ret = ResponseState.TimeOut.GetHashCode();
                }
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            response.EndTime = DateTime.Now;
            return response;

        }

        public override ResponseInfo Read(out string result,int time = -1)
        {
            ResponseInfo response = new ResponseInfo();
            response.StartTime = DateTime.Now;
            result = string.Empty;
            string temp = string.Empty;
            try
            {
                Task timeOut = new Task(() =>
                {
                    #region 刷卡器自带SDK读取卡号
                    int readerId = 0;
                    int cardNumberLength = 0;
                    short cardType = 0;
                    byte cardMem = 0;
                    byte[] data = new byte[4];
                    //for (; ; )
                    //{
                    //    bool flag = YW605Reader.YW_AntennaStatus(readerId, true) >= 0; ;
                    //    if (flag)
                    //    {
                    //        break;
                    //    }
                    //    Thread.Sleep(1000);
                    //    YW605Reader.YW_USBHIDFree();
                    //    YW605Reader.YW_USBHIDInitial();
                    //}
                    bool flag2 = YW605Reader.YW_AntennaStatus(readerId, true) >= 0 &&
                        YW605Reader.YW_SearchCardMode(readerId, 65) > 0 &&
                        YW605Reader.YW_RequestCard(readerId, 82, ref cardType) > 0 &&
                        YW605Reader.YW_AntiCollideAndSelect(readerId, 0, ref cardMem, ref cardNumberLength, data) > 0;
                    if (flag2)
                    {
                        response.ret = ResponseState.Normal.GetHashCode();
                        temp = BitConverter.ToUInt32(data, 0).ToString();
                        temp = temp.Length <= 10 ? "0" + temp : temp;   
                    }
                    else
                    {
                        response.ret = ResponseState.Error.GetHashCode();
                    }
                    
                    #endregion
                });
                timeOut.Start();

                if (!timeOut.Wait(time))
                {
                    response.ret = ResponseState.TimeOut.GetHashCode();
                }
                result = temp;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            response.EndTime = DateTime.Now;
            return response;
        }


        private CancellationTokenSource cancelGrabData;

        internal override ResponseInfo StartGrabData()
        {
            ResponseInfo response = new ResponseInfo();
            response.StartTime = DateTime.Now;
            try
            {
                cancelGrabData = new CancellationTokenSource();

                Task task = new Task(() =>
                {
                    while (true)
                    {
                        string result = string.Empty;
                        ResponseInfo temp = Read(out result, 5000);
                        if (temp.ret > 0) //有卡
                        {
                            if (!this.Isbusy)
                            {
                                this.Isbusy = true;
                                this.Notbusy = false;
                                OnScanCardEvent(State.Online, result);
                            }
                        }
                        else  //无卡
                        {
                            if (!this.Notbusy)
                            {
                                this.Isbusy = false;
                                this.Notbusy = true;
                                OnScanCardEvent(State.Offline, result);
                            }
                        }
                        Thread.Sleep(2000);
                    }

                });
                task.Start();
                //Task GrabData = Task.Factory.StartNew(delegate
                //{
                //    while (!cancelGrabData.IsCancellationRequested)
                //    {
                //        string result = string.Empty;
                //        ResponseInfo temp = Read(out result, 1000);
                //        if (temp.ret > 0) //有卡
                //        {
                //            if (!this.Isbusy)
                //            {
                //                this.Isbusy = true;
                //                this.Notbusy = false;
                //                OnScanCardEvent(State.Online, result);
                //            }
                //        }
                //        else  //无卡
                //        {
                //            if (!this.Notbusy)
                //            {
                //                this.Isbusy = false;
                //                this.Notbusy = true;
                //                OnScanCardEvent(State.Offline, result);
                //            }
                //        }
                //        Thread.Sleep(1000);
                //    }

                //}, cancelGrabData);
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            response.EndTime = DateTime.Now;
            return response;
        }

        internal override ResponseInfo StopGrabData()
        {
            ResponseInfo response = new ResponseInfo();
            response.StartTime = DateTime.Now;
            try
            {
                this.Isbusy = false;
                this.Notbusy = false;
                cancelGrabData.Cancel();
                response.ret = ResponseState.Normal.GetHashCode();
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            response.EndTime = DateTime.Now;
            return response;
        }

    }
}
