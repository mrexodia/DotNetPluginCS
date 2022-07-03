using System;
using System.Runtime.InteropServices;

namespace DotNetPlugin.NativeBindings.Win32
{
#pragma warning disable 0649

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BITMAPFILEHEADER
    {
        public ushort bfType;
        public uint bfSize;
        public ushort bfReserved1;
        public ushort bfReserved2;
        public uint bfOffBits;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BITMAPV5HEADER
    {
        public uint bV5Size;
        public int bV5Width;
        public int bV5Height;
        public ushort bV5Planes;
        public ushort bV5BitCount;
        public BitmapCompressionMode bV5Compression;
        public uint bV5SizeImage;
        public int bV5XPelsPerMeter;
        public int bV5YPelsPerMeter;
        public uint bV5ClrUsed;
        public uint bV5ClrImportant;
        public uint bV5RedMask;
        public uint bV5GreenMask;
        public uint bV5BlueMask;
        public uint bV5AlphaMask;
        public LCSCSTYPE bV5CSType;
        public CIEXYZTRIPLE bV5Endpoints;
        public uint bV5GammaRed;
        public uint bV5GammaGreen;
        public uint bV5GammaBlue;
        public LCSGAMUTMATCH bV5Intent;
        public uint bV5ProfileData;
        public uint bV5ProfileSize;
        public uint bV5Reserved;
    }

    public enum BitmapCompressionMode : uint
    {
        BI_RGB = 0,
        BI_RLE8 = 1,
        BI_RLE4 = 2,
        BI_BITFIELDS = 3,
        BI_JPEG = 4,
        BI_PNG = 5
    }

    [Serializable]
    public struct CIEXYZTRIPLE
    {
        public CIEXYZ ciexyzRed;
        public CIEXYZ ciexyzGreen;
        public CIEXYZ ciexyzBlue;
    }

    [Serializable]
    public struct CIEXYZ
    {
        public int ciexyzX;
        public int ciexyzY;
        public int ciexyzZ;
    }

    public enum LCSCSTYPE : uint
    {
        LCS_CALIBRATED_RGB = 0,
        LCS_sRGB = 0x73524742,
        LCS_WINDOWS_COLOR_SPACE = 0x57696e20,
    }

    [Flags]
    public enum LCSGAMUTMATCH : uint
    {
        LCS_GM_BUSINESS = 0x00000001,
        LCS_GM_GRAPHICS = 0x00000002,
        LCS_GM_IMAGES = 0x00000004,
        LCS_GM_ABS_COLORIMETRIC = 0x00000008,
    }
}
