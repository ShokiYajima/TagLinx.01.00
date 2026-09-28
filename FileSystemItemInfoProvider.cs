using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace TagLinx._01._00
{
    public static class FileSystemItemInfoProvider
    {
        #region instance field
        #endregion

        #region static field
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo
            (
                string pszPath,
                uint dwFileAttributes,
                out SHFILEINFO psfi,
                uint cbFileInfo,
                uint uFlags
            );

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("ole32.dll", ExactSpelling = true)]
        private static extern int CoInitializeEx(IntPtr reserved, uint coInit);

        [DllImport("ole32.dll", ExactSpelling = true)]
        private static extern void CoUninitialize();

        [DllImport("gdiplus.dll", ExactSpelling = true)]
        private static extern int GdiplusStartup(out UIntPtr token, ref GdiplusStartupInput input, IntPtr output);

        [DllImport("gdiplus.dll", ExactSpelling = true)]
        private static extern void GdiplusShutdown(UIntPtr token);

        [DllImport("gdiplus.dll", ExactSpelling = true)]
        private static extern int GdipCreateBitmapFromHICON(IntPtr icon, out IntPtr bitmap);

        [DllImport("gdiplus.dll", ExactSpelling = true)]
        private static extern int GdipSaveImageToStream(IntPtr image, IStream stream, ref Guid encoder, IntPtr parameters);

        [DllImport("gdiplus.dll", ExactSpelling = true)]
        private static extern int GdipDisposeImage(IntPtr image);

        [DllImport("shlwapi.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Interface)]
        private static extern IStream? SHCreateMemStream(IntPtr data, uint size);
        #endregion

        #region const field
        private const uint SHGFI_TYPENAME = 0x00000400;
        private const uint SHGFI_ICON = 0x00000100;
        private const uint SHGFI_SMALLICON = 0x00000001;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x00000010;
        #endregion

        #region struct
        [StructLayout(LayoutKind.Sequential)]
        private struct GdiplusStartupInput
        {
            public uint Version;
            public IntPtr DebugEventCallback;
            public int SuppressBackgroundThread;
            public int SuppressExternalCodecs;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
        #endregion

        /// <summary>
        /// 種類取得
        /// </summary>
        public static string GetTypeName(string path)
        {
            SHGetFileInfo
                (
                    path,
                    0,
                    out SHFILEINFO info,
                    (uint)Marshal.SizeOf<SHFILEINFO>(),
                    SHGFI_TYPENAME
                );
            return info.szTypeName;
        }

        /// <summary>
        /// アイコン取得
        /// </summary>
        public static async Task<BitmapImage?> GetSmallImageSourceAsync(string path)
        {
            try
            {
                // Shellの処理はバックグラウンドで行い、BitmapImageはUIスレッドで作成する。
                byte[]? png = await Task.Run(() => GetSmallIconPng(path));
                if (png == null)
                    return null;

                using var stream = new InMemoryRandomAccessStream();
                using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(png);
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                }
                stream.Seek(0);
                BitmapImage bitmap = new();
                await bitmap.SetSourceAsync(stream);
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static byte[]? GetSmallIconPng(string path)
        {
            bool isFolder = Directory.Exists(path);
            if (!isFolder && !File.Exists(path))
                return null;

            SHFILEINFO info = default;
            UIntPtr token = UIntPtr.Zero;
            IntPtr image = IntPtr.Zero;
            IStream? stream = null;
            // Task.RunのスレッドでShell APIに必要なCOMを初期化する。
            int comResult = CoInitializeEx(IntPtr.Zero, 0);
            // RPC_E_CHANGED_MODEの場合、既存のCOMアパートメントを使用する。
            if (comResult < 0 && comResult != unchecked((int)0x80010106))
                Marshal.ThrowExceptionForHR(comResult);
            try
            {
                uint size = (uint)Marshal.SizeOf<SHFILEINFO>();
                uint flags = SHGFI_ICON | SHGFI_SMALLICON;
                IntPtr result = SHGetFileInfo(path, 0, out info, size, flags);
                if (result == IntPtr.Zero || info.hIcon == IntPtr.Zero)
                {
                    if (info.hIcon != IntPtr.Zero)
                        DestroyIcon(info.hIcon);
                    info = default;
                    // 実体から取得できない場合は、属性から標準アイコンを取得する。
                    uint attributes = (uint)(isFolder ? FileAttributes.Directory : FileAttributes.Normal);
                    result = SHGetFileInfo(path, attributes, out info, size, flags | SHGFI_USEFILEATTRIBUTES);
                    if (result == IntPtr.Zero || info.hIcon == IntPtr.Zero)
                        return null;
                }

                var input = new GdiplusStartupInput { Version = 1 };
                if (GdiplusStartup(out token, ref input, IntPtr.Zero) != 0)
                    return null;
                if (GdipCreateBitmapFromHICON(info.hIcon, out image) != 0)
                    return null;

                stream = SHCreateMemStream(IntPtr.Zero, 0);
                if (stream == null)
                    return null;
                // Windows標準のPNGエンコーダー。
                var encoder = new Guid("557cf406-1a04-11d3-9a73-0000f81ef32e");
                if (GdipSaveImageToStream(image, stream, ref encoder, IntPtr.Zero) != 0)
                    return null;

                stream.Stat(out STATSTG stat, 1);
                byte[] bytes = new byte[checked((int)stat.cbSize)];
                stream.Seek(0, 0, IntPtr.Zero);
                stream.Read(bytes, bytes.Length, IntPtr.Zero);
                return bytes;
            }
            finally
            {
                if (stream != null)
                    Marshal.ReleaseComObject(stream);
                if (image != IntPtr.Zero)
                    GdipDisposeImage(image);
                if (token != UIntPtr.Zero)
                    GdiplusShutdown(token);
                if (info.hIcon != IntPtr.Zero)
                    DestroyIcon(info.hIcon);
                if (comResult >= 0)
                    CoUninitialize();
            }
        }
    }
}
