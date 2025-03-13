using System;
using System.IO;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Drawing;

namespace AutoScanFQCTest.Utilities
{
    public struct NetworkCredential
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
    }

    // 网络共享访问器
    public class NetworkShareAccesser : IDisposable
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

        private WindowsImpersonationContext _context = null;
        private SafeAccessTokenHandle _safeTokenHandle;

        public NetworkShareAccesser(string username, string domain, string password)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

            // Call LogonUser to obtain a handle to an access token.
            bool returnValue = LogonUser(username, domain, password,
                LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT,
                out _safeTokenHandle);

            if (!returnValue)
            {
                int ret = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(ret);
            }

            // Use the token handle returned by LogonUser.
            using (WindowsIdentity newId = new WindowsIdentity(_safeTokenHandle.DangerousGetHandle()))
            {
                _context = newId.Impersonate();
            }
        }

        // 读取文件内容并以字符串形式返回
        public string ReadFileContent(string networkFilePath)
        {
            try
            {
                // 确保文件存在
                if (!File.Exists(networkFilePath))
                {
                    throw new FileNotFoundException("文件未找到。", networkFilePath);
                }

                // 读取文件内容
                string fileContent = File.ReadAllText(networkFilePath);
                return fileContent;
            }
            catch (Exception ex)
            {
                // 异常处理逻辑
                Debugger.Log(0, "Error", string.Format("222222 An error occurred: {0}", ex.Message));
                throw;
            }
        }

        // 读取图片文件并返回一个Image对象
        public static Image ReadImageFile(string networkImagePath)
        {
            try
            {
                // 确保文件存在
                if (!File.Exists(networkImagePath))
                {
                    throw new FileNotFoundException("文件未找到。", networkImagePath);
                }

                // 以文件流的形式打开图片
                using (FileStream fs = new FileStream(networkImagePath, FileMode.Open, FileAccess.Read))
                {
                    // 从文件流创建图片
                    Image image = Image.FromStream(fs);
                    return image;
                }
                // 注意：FileStream的using块确保了文件被正确关闭
            }
            catch (Exception ex)
            {
                // 异常处理逻辑
                Debugger.Log(0, "Error", string.Format("222222 An error occurred: {0}", ex.Message));
                throw;
            }
        }

        public void Dispose()
        {
            // Releasing the context object stops the impersonation
            _context?.Undo();
            _safeTokenHandle?.Dispose();
        }

        public static void ListFiles(string networkPath, string username, string domain, string password)
        {
            using (var accesser = new NetworkShareAccesser(username, domain, password))
            {
                DirectoryInfo directory = new DirectoryInfo(networkPath);
                FileInfo[] files = directory.GetFiles();

                foreach (FileInfo file in files)
                {
                    Console.WriteLine(file.Name);
                }
            }
        }

        public void ConnectShare(string networkPath)
        {
            // 暂时没有实现，因为.NET通常自动处理连接
        }

        public void DisconnectShare(string networkPath)
        {
            // .NET不提供断开特定共享连接的方法，通常是自动管理的
        }

        public void CopyFileToShare(string sourceFilePath, string destNetworkPath)
        {
            File.Copy(sourceFilePath, Path.Combine(destNetworkPath, Path.GetFileName(sourceFilePath)));
        }

        public void CopyFileFromShare(string sourceNetworkPath, string destFilePath)
        {
            File.Copy(sourceNetworkPath, destFilePath);
        }

        public void DeleteFileFromShare(string networkFilePath)
        {
            File.Delete(networkFilePath);
        }

        public void CreateDirectory(string networkDirectoryPath)
        {
            Directory.CreateDirectory(networkDirectoryPath);
        }

        public void DeleteDirectory(string networkDirectoryPath)
        {
            Directory.Delete(networkDirectoryPath, true);
        }

        public FileInfo GetFileInformation(string networkFilePath)
        {
            FileInfo fileInfo = new FileInfo(networkFilePath);
            return fileInfo;
        }

        public DirectoryInfo GetDirectoryInformation(string networkDirectoryPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(networkDirectoryPath);
            return directoryInfo;
        }

        public bool CheckAccess(string networkPath)
        {
            try
            {
                System.Security.AccessControl.FileSystemSecurity security = Directory.GetAccessControl(networkPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
