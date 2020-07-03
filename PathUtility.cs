using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.IO;
using System.Text;

namespace PathManage
{
    public class PathUtility
    {
        private static object lockObj = new object();
          
        public static string GetOriginPath(string filename)
        {
            string appPath;

            if (MemoryCacheHelper.Contains(filename) == false)
            {
                appPath = PlatformServices.Default.Application.ApplicationBasePath; //Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath; 

                ErrorLog(string.Format("ApplicationBasePath = {0} line 2323 iuiu", appPath)); //-----------------------------------test
                string pathfilename = Path.Combine(appPath, filename);
                if (!File.Exists(pathfilename))
                {
                    System.Reflection.Assembly curPath = System.Reflection.Assembly.GetExecutingAssembly();
                    appPath = curPath.CodeBase;
                    if (appPath.ToLower().StartsWith("file"))
                    {
                        Uri uri = new Uri(appPath);
                        appPath = uri.LocalPath;
                    }
                    appPath = Path.GetFullPath(appPath);
                    appPath = PathRemoveBin(appPath);
                    pathfilename = Path.Combine(appPath, filename);
                    ErrorLog(string.Format("GetExecutingAssembly::CodeBase; = {0} line 789 ghj", appPath)); //-----------------------------------test
                    //-----------------------------------------------------------------------------------------------------
                    if (!File.Exists(pathfilename))
                    {
                        appPath = Path.GetFullPath(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                        appPath = Path.GetDirectoryName(appPath);
                        ErrorLog(string.Format("GetCurrentProcess::MainModule; = {0} line 455 xfv", appPath));//-----------------------------------test
                    }  
                    DateTimeOffset thisOffsetTime = DateTime.Now.AddHours(20);
                    MemoryCacheHelper.Set(filename, appPath, thisOffsetTime);
                    return appPath;
                }
                else
                {
                    DateTimeOffset thisOffsetTime = DateTime.Now.AddHours(20);
                    MemoryCacheHelper.Set(filename, appPath, thisOffsetTime); 
                    return appPath;
                }
            }
            else
            {
                appPath = MemoryCacheHelper.GetCacheItem<string>(filename);
                return appPath;
            }
        }

        public static string GetAppBasePathByAssembly(out bool consistency)
        {
            string CacheKey = "AppBasePath";
            string appBasePath;
            if (MemoryCacheHelper.Contains(CacheKey) == false)
            {
                string appPath = PlatformServices.Default.Application.ApplicationBasePath;
                appPath = Path.GetDirectoryName(appPath);

                //------------------------------------------------------------------------------------

                System.Reflection.Assembly curPath = System.Reflection.Assembly.GetExecutingAssembly();
                string appCodeBasePath = curPath.CodeBase;
                if (appCodeBasePath.ToLower().StartsWith("file"))
                {
                    Uri uri = new Uri(appCodeBasePath);
                    appCodeBasePath = uri.LocalPath;
                }
                appCodeBasePath = Path.GetFullPath(appCodeBasePath);
                appCodeBasePath = PathRemoveBin(appCodeBasePath);
                appCodeBasePath = Path.GetDirectoryName(appCodeBasePath);
                 
                //first time-----------------------------------------------------------------------
                if (appPath == appCodeBasePath)
                {
                    consistency = true;
                    appBasePath = appPath;
                }
                else
                {
                    consistency = false;
                    appBasePath = appCodeBasePath;  // prefer
                }

                return appBasePath;
            }
            else
            {
                consistency = true; //default
                appBasePath = MemoryCacheHelper.GetCacheItem<string>(CacheKey);
                return appBasePath;
            }
        }

        /// <summary>
        /// from config.cs
        /// </summary>
        /// <param name="AnchorFileName"></param>
        /// <returns></returns>
        public static string GetPath(string AnchorFileName)
        {
            string pathFileName;
            if (MemoryCacheHelper.Contains(AnchorFileName) == false)
            {
                string LoggerLine;
                System.Reflection.Assembly curPath = System.Reflection.Assembly.GetExecutingAssembly();
                string assemblypath = curPath.CodeBase;
                string basePath = Path.GetDirectoryName(assemblypath);
                if (File.Exists(Path.Combine(basePath, "Encryption.dll")) || File.Exists(Path.Combine(basePath, "Encryption.dll")))
                {
                    basePath = PathRemoveBin(basePath);
                    return basePath;
                }
                //supplement
                if (basePath.ToLower().StartsWith("file"))
                {
                    Uri uri = new Uri(basePath);
                    basePath = uri.LocalPath;
                }
                pathFileName = Path.Combine(basePath, AnchorFileName);
                if (!File.Exists(pathFileName))
                {
                    basePath = Path.GetFullPath(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    basePath = Path.GetDirectoryName(basePath);
                    pathFileName = Path.Combine(basePath, AnchorFileName);
                    if (!File.Exists(pathFileName))
                    {
                        basePath = AppDomain.CurrentDomain.BaseDirectory;
                        pathFileName = Path.Combine(basePath, AnchorFileName);
                        if (!File.Exists(pathFileName))
                        {
                            basePath = Environment.CurrentDirectory;
                            pathFileName = Path.Combine(basePath, AnchorFileName);
                            if (!File.Exists(pathFileName))
                            {
                                LoggerLine = string.Format("[FUN::GetPath::CASE::Environment.CurrentDirectory ][{0:yyyy-MM-dd HH:mm:ss fff}][ConnectionConfig.conf {1} Exist]", DateTime.Now, pathFileName);
                                Common.CommonBase.OperateDateLoger(LoggerLine);
                            }
                            else
                            {
                                return pathFileName;
                            }
                        }
                        else
                        {
                            return pathFileName;
                        }
                    }
                    else
                    {
                        return pathFileName;
                    }
                }
                else
                {
                    return pathFileName;
                }
                return pathFileName;
            }
            else
            {
                pathFileName = MemoryCacheHelper.GetCacheItem<string>(AnchorFileName);
                return pathFileName;
            }
        }
        public static string PathRemoveBin(string pathApp)
        {
            int pathIndex = pathApp.LastIndexOf("\\");
            if (pathIndex != -1)
            {
                string existBinPath = pathApp.Remove(0, pathIndex).ToLower();
                existBinPath = existBinPath.TrimStart('\\');

                if (existBinPath.ToLower() == "bin")
                {
                    pathApp = pathApp.Substring(0, pathIndex);
                    return pathApp;
                }
                else
                {
                    return pathApp.Substring(0, pathIndex);
                }
            }
            else
            {
                return pathApp;
            }
        }

        public static void ErrorLog(string msg)
        {
            string sysPath = PlatformServices.Default.Application.ApplicationBasePath; //string.Format("{0}\\", Environment.ExpandEnvironmentVariables("%systemdrive%"));
            lock (lockObj)
            {
                string filePath = Path.Combine(sysPath, "log");

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                string file = Path.Combine(filePath, "LogError.log");

                using (StreamWriter w = new StreamWriter(file, true, Encoding.UTF8))
                {
                    w.WriteLine(string.Format("[{0:yyyy-MM-dd HH:mm:ss fff}] [{1}]",DateTime.Now,msg));
                    w.Close();
                }
            }
        }
    }
}
