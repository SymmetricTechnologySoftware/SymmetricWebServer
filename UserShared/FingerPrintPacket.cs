using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace UserShared
{
    public class FingerPrintPacket
    {
        public enum PacketTypes { FingerPrint, DeviceState, FinishedEnrollment, EnrollmentFailed, FoundUser };
        public PacketTypes PacketType { private set; get; }
        public enum DeviceStates { Connected, Disconnected };
        public DeviceStates DeviceState { private set; get; }
        public Bitmap FingerPrintImage { private set; get; }
        public byte[] EnrollmentTemplate { private set; get; }
        public int UserID { private set; get; }
        public int FingerprintNumber { private set; get; }

        public FingerPrintPacket(PacketTypes packetType)
        {
            this.PacketType = packetType;
        }

        public FingerPrintPacket(PacketTypes packetType, DeviceStates deviceState)
            : this(packetType)
        {
            this.DeviceState = deviceState;
        }

        public FingerPrintPacket(PacketTypes packetType, Bitmap fingerPrintImage)
            : this(packetType)
        {
            this.FingerPrintImage = fingerPrintImage;
        }

        public FingerPrintPacket(byte[] enrollmentTemplate)
            : this(PacketTypes.FinishedEnrollment)
        {
            this.EnrollmentTemplate = enrollmentTemplate;
        }

        public FingerPrintPacket(int userID, int fingerprintNumber)
            : this(PacketTypes.FoundUser)
        {
            this.UserID = userID;
            this.FingerprintNumber = fingerprintNumber;
        }
    }
}
