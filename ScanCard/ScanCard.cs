using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest
{
    public class ScanCard : NotifyPropertyChangedBase
    {
        public enum Brand
        {
            YW60x
        }

        public enum State
        {
            Online,
            Offline
        }

        private ScanCard scanCard = null;

        public Brand scanCardBrand;

        internal ScanCard()
        {

        }

        public ScanCard(Brand scanCardBrand)
        {
            this.scanCardBrand = scanCardBrand;
            string nameSpace = this.GetType().Namespace;
            string className = nameSpace + "." + scanCardBrand.ToString();
            scanCard = Activator.CreateInstance(Type.GetType(className)) as ScanCard;
        }

        public virtual ResponseInfo Open(int time = -1)
        {
            return scanCard.Open(time);
        }

        public virtual ResponseInfo Close(int time = -1)
        {
            return scanCard.Close(time);
        }

        public virtual ResponseInfo Read(out string result,int time = -1)
        {
            return scanCard.Read(out result,time);
        }

        /// <summary>
        /// 开启回调侦听
        /// </summary>
        /// <returns>标准返回值</returns>
        internal virtual ResponseInfo StartGrabData()
        {
            return scanCard.StartGrabData();
        }

        /// <summary>
        /// 关闭回调侦听
        /// </summary>
        /// <returns>标准返回值</returns>
        internal virtual ResponseInfo StopGrabData()
        {
            return scanCard.StopGrabData();
        }


        public virtual event Action<State,string> OnScanCardEvent
        {
            add
            {
                if (value != null)
                {
                    scanCard.StartGrabData();
                    scanCard.OnScanCardEvent += value;
                }
            }
            remove
            {
                scanCard.StopGrabData();
                scanCard.OnScanCardEvent -= value;
            }
        }
    }
}
