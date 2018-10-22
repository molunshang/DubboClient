namespace Hessian.Lite
{
    public class Constants
    {
        public const byte Null = (byte)'N';
        public const byte False = (byte)'F';
        public const byte True = (byte)'T';
        //二进制数据非最后块
        public const byte BinaryChunk = 0x41;
        //二进制数据最后块
        public const byte BinaryFinalChunk = 0x42;
        public const int BinaryChunkLength = ushort.MaxValue;
        //压缩二进制长度
        public const int BinaryChunkMinLength = 15;
        public const int BinaryChunkMinStart = 0x20;

        public const byte DateTimeMillisecond = 0x4a;
        public const byte DateTimeMinute = 0x4b;
        public const int MinuteTotalMilliseconds = 60000;

        public const byte DoubleZero = 0x5b;
        public const byte DoubleOne = 0x5c;
        public const byte DoubleByte = 0x5d;
        public const byte DoubleShort = 0x5e;
        public const byte DoubleInt = 0x5f;
        public const byte Double = (byte)'D';

        #region Int相关常量
        public const byte Int = 0x49;

        public const byte IntOneByte = 0x90;
        public const int IntOneByteMin = -16;
        public const int IntOneByteMax = 47;

        public const byte IntTwoByte = 0xc8;
        public const int IntTwoByteMin = -2048;
        public const int IntTwoByteMax = 2047;

        public const byte IntThreeByte = 0xd4;
        public const int IntThreeMin = -262144;
        public const int IntThreeMax = 262143;
        #endregion

        #region Long相关常量

        public const byte Long = 0x4c;

        public const byte LongOneByte = 0xe0;
        public const int LongOneByteMin = -8;
        public const int LongOneByteMax = 15;

        public const byte LongTwoByte = 0xf8;
        public const int LongTwoByteMin = -2048;
        public const int LongTwoByteMax = 2047;

        public const byte LongThreeByte = 0x3c;
        public const int LongThreeMin = -262144;
        public const int LongThreeMax = 262143;

        public const byte LongFourByte = 0x59;
        #endregion

        #region String相关常量

        public const byte String = 0x52;
        public const byte StringFinal = 0x53;
        public const int StringChunkLength = ushort.MaxValue;
        public const int StringSmallLength = 31;
        public const int StringMediumLength = 1023;
        public const int StringMediumStart = 0x30;
        //代理对范围
        public const int SurrogateMin = 0xd800;
        public const int SurrogateMax = 0xdbff;

        #endregion

        #region 对象/集合/Map常量
        public const byte Ref = 0x51;

        public const byte VariableList = 0x55;
        public const byte FixedList = 0x56;
        public const byte VariableUnTypeList = 0x57;
        public const byte FixedUnTypeList = 0x58;
        public const byte SmallFixedList = 0x70;
        public const byte SmallFixedUnTypeList = 0x78;

        public const int SmallListMaxLength = 7;

        public const byte UnTypeMap = (byte)'H';
        public const byte Map = (byte)'M';

        public const byte ClassDef = (byte)'C';
        public const byte Object = (byte)'O';

        public const byte ObjectDirectMax = 0x0f;
        public const byte ObjectDirectMin = 0x60;


        public const byte End = (byte)'Z';
        #endregion
    }
}
