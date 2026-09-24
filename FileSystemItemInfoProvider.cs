using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

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
        #endregion

        #region const field
        private const uint SHGFI_TYPENAME = 0x00000400;
        #endregion

        #region struct
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
                // フォルダー
                if (Directory.Exists(path))
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                    StorageItemThumbnail thumbnail = await folder.GetThumbnailAsync(ThumbnailMode.SingleItem,16);
                    if (thumbnail == null)
                    {
                        return null;
                    }
                    BitmapImage bitmap = new();
                    await bitmap.SetSourceAsync(thumbnail);
                    return bitmap;
                }
                // ファイル
                if (File.Exists(path))
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                    StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem,16);
                    if (thumbnail == null)
                    {
                        return null;
                    }
                    BitmapImage bitmap = new();
                    await bitmap.SetSourceAsync(thumbnail);
                    return bitmap;
                }
                return null;
            }
            catch 
            { 
                return null; 
            }
        }
    }
}