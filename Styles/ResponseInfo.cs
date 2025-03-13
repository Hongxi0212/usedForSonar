using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest
{
    public class ResponseInfo : NotifyPropertyChangedBase
    {
        private int _ret = ResponseState.Error.GetHashCode();
        public int ret
        {
            get
            {
                return _ret;
            }
            set
            {
                _ret = value;
                RaisePropertyChanged();
            }
        }


        private Exception _exception;
        public Exception Exception
        {
            get { return _exception; }
            set { _exception = value; ret = ResponseState.Exception.GetHashCode(); RaisePropertyChanged(); }
        }

        private DateTime _startTime;
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                _startTime = value; RaisePropertyChanged();
            }
        }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }
            set
            {
                _endTime = value; RaisePropertyChanged();
            }
        }

        public string TotalMilliseconds
        {
            get
            {
                try
                {
                    if (ret == ResponseState.TimeOut.GetHashCode())
                    {
                        return "TimeOut";
                    }
                    TimeSpan ts = EndTime - StartTime;
                    return ts.TotalMilliseconds.ToString();
                }
                catch
                {
                    return string.Empty;
                }
            }
            set
            {
                RaisePropertyChanged();
            }
        }












    }
}
