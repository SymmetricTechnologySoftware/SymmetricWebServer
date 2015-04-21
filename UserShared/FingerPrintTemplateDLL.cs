using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserShared
{
    public abstract class FingerPrintTemplateDLL
    {
        public static FingerPrintTemplateDLL DLL { private set; get; }

        public Dictionary<int, Dictionary<int, byte[]>> FingerPrints { private set; get; } // User id, fingerprint id, fingerprint
        public enum InitTypes { Capture, Enrollment, Verification }
        public InitTypes InitType { private set; get; }
        private static object ControlPacketsLock = new object();
        private List<FingerPrintPacket> _lstControlPackets;

        private static object IsInitLock = new object();
        private bool _isInit;
        private bool IsInit
        {
            set
            {
                lock (IsInitLock)
                {
                    _isInit = value;
                }
            }
            get
            {
                lock (IsInitLock)
                {
                    return _isInit;
                }
            }
        }

        //Step 1
        public FingerPrintTemplateDLL()
        {
            _lstControlPackets = new List<FingerPrintPacket>();
            _isInit = false;
            FingerPrintTemplateDLL.DLL = this;
        }

        //Step 2
        public bool InitBase(InitTypes initType, Dictionary<int, Dictionary<int, byte[]>> fingerPrints, out string strError)
        {
            this.InitType = initType;
            strError = "";
            if (this.IsInit)
            {
                return true;
            }
            this.FingerPrints = fingerPrints;
            this._lstControlPackets.Clear();
            this.IsInit = Init(initType, fingerPrints, out strError);
            return this.IsInit;
        }
        protected abstract bool Init(InitTypes initType, Dictionary<int, Dictionary<int, byte[]>> fingerPrints, out string strError);

        //Step 3
        public FingerPrintPacket ReadPacket()
        {
            FingerPrintPacket result = null;
            lock (ControlPacketsLock)
            {
                if (_lstControlPackets.Count() > 0)
                {
                    result = _lstControlPackets[0];
                    _lstControlPackets.RemoveAt(0);
                }
            }
            return result;
        }

        public void AddFingerPrint(System.Drawing.Bitmap fingerPrintImage)
        {
            lock (ControlPacketsLock)
            {
                _lstControlPackets.Add(new FingerPrintPacket(FingerPrintPacket.PacketTypes.FingerPrint, fingerPrintImage));
            }
        }

        public void SetDeviceState(FingerPrintPacket.DeviceStates deviceState)
        {
            lock (ControlPacketsLock)
            {
                _lstControlPackets.Add(new FingerPrintPacket(FingerPrintPacket.PacketTypes.DeviceState, deviceState));
            }
        }

        //Step 4
        public void SetFingerPrintFound(int userID, int fingerPrintNumber)
        {
            lock (ControlPacketsLock)
            {
                _lstControlPackets.Add(new FingerPrintPacket(userID, fingerPrintNumber));
            }
        }

        //Step 5
        public void EnrollmentFailed()
        {
            lock (ControlPacketsLock)
            {
                _lstControlPackets.Add(new FingerPrintPacket(FingerPrintPacket.PacketTypes.EnrollmentFailed));
            }
        }

        public void EnrollTemplate(byte[] template)
        {
            if (template == null) return;
            lock (ControlPacketsLock)
            {
                _lstControlPackets.Add(new FingerPrintPacket(template));
            }
        }

        //Step 6
        public void DeInitBase()
        {
            if (!this.IsInit) return;
            this.IsInit = false;
            DeInit();
        }
        protected abstract void DeInit();
    }
}
