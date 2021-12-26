/*----------------------------------------------------------------------------------
// Copyright 2019 Huawei Technologies Co.,Ltd.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License.  You may obtain a copy of the
// License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations under the License.
//----------------------------------------------------------------------------------*/
using System;
using System.IO;
using System.Reflection;

namespace OBS.Internal.Log
{
   
    static internal class LoggerMgr
    {

        private static volatile object logger;

        private static readonly object _lock = new object();

        private volatile static bool inited = false;

        private static PropertyInfo isDebugEnabled;
        private static PropertyInfo isInfoEnabled;
        private static PropertyInfo isWarnEnabled;
        private static PropertyInfo isErrorEnabled;

        private static MethodInfo debug;
        private static MethodInfo info;
        private static MethodInfo warn;
        private static MethodInfo error;

        internal static void Initialize()
        {
            if (!inited && logger == null)
            {
                lock (_lock)
                {
                    if (!inited && logger == null)
                    {
                        try
                        {
                            FileInfo fileInfo = null;
                            if (File.Exists("Log4Net.config"))
                            {
                                fileInfo = new FileInfo("Log4Net.config");
                            }else if (File.Exists("Log4Net.xml"))
                            {
                                fileInfo = new FileInfo("Log4Net.xml");
                            }else if (File.Exists("log4net.config"))
                            {
                                fileInfo = new FileInfo("log4net.config");
                            }else if (File.Exists("log4net.xml"))
                            {
                                fileInfo = new FileInfo("log4net.xml");
                            }
                            if(fileInfo == null)
                            {
                                return;
                            }

                            Assembly log4netDll = Assembly.LoadFile(Environment.CurrentDirectory + "/log4net.dll");
                            Type logManagerType = log4netDll.GetType("log4net.LogManager", true, true);
                            Type repositoryType = log4netDll.GetType("log4net.Repository.ILoggerRepository", true, true);
                            object repository = logManagerType.GetMethod("CreateRepository", new Type[] { typeof(string) }).Invoke(null, new object[] { "LoggerMgrRepository" });

                            log4netDll.GetType("log4net.Config.XmlConfigurator", true, true).GetMethod("ConfigureAndWatch", new Type[] { repositoryType, typeof(FileInfo) }).Invoke(null, new object[] {repository, fileInfo });

                            object _logger = logManagerType.GetMethod("GetLogger", new Type[] { typeof(string), typeof(string) }).Invoke(null, new object[] { "LoggerMgrRepository", "LoggerMgr" });
                            Type loggerType = _logger.GetType();

                            isDebugEnabled = loggerType.GetProperty("IsDebugEnabled");
                            isInfoEnabled = loggerType.GetProperty("IsInfoEnabled");
                            isWarnEnabled = loggerType.GetProperty("IsWarnEnabled");
                            isErrorEnabled = loggerType.GetProperty("IsErrorEnabled");

                            debug = loggerType.GetMethod("Debug", new Type[] { typeof(object), typeof(Exception) });
                            info = loggerType.GetMethod("Info", new Type[] { typeof(object), typeof(Exception) });
                            warn = loggerType.GetMethod("Warn", new Type[] { typeof(object), typeof(Exception) });
                            error = loggerType.GetMethod("Error", new Type[] { typeof(object), typeof(Exception) });

                            logger = _logger;
                        }
                        catch (Exception)
                        {
                            logger = null;
                        }
                        finally
                        {
                            inited = true;
                        }
                    }
                }
            }

        }

        //private static readonly ILog _logger = LogManager.GetLogger("LoggerMgr");

        internal static bool IsDebugEnabled
        {
            get
            {
                //return _logger.IsDebugEnabled;
                return logger != null ? (bool)isDebugEnabled.GetValue(logger, null) : false;
            }
        }

        internal static bool IsInfoEnabled
        {
            get
            {
                //return _logger.IsInfoEnabled;
                return logger != null ? (bool)isInfoEnabled.GetValue(logger, null) : false;
            }
        }
        internal static bool IsWarnEnabled
        {
            get
            {
                //return _logger.IsWarnEnabled;
                return logger != null ? Convert.ToBoolean(isWarnEnabled.GetValue(logger, null)) : false;
            }
        }
        internal static bool IsErrorEnabled
        {
            get
            {
                //return _logger.IsErrorEnabled;
                return logger != null ? (bool)isErrorEnabled.GetValue(logger, null) : false;
            }
        }


        internal static void Debug(string param)
        {
            //_logger.Debug(param);
            Debug(param, null);
        }

        internal static void Error(string param)
        {
            //_logger.Error(param);
            Error(param, null);
        }

        internal static void Info(string param)
        {
            //_logger.Info(param);
            Info(param, null);
        }

        internal static void Warn(string param)
        {
            //_logger.Warn(param);
            Warn(param, null);
        }

        internal static void Debug(string param, Exception exception)
        {
            //_logger.Debug(param, exception);
            if(logger != null)
            {
                debug.Invoke(logger, new object[] { param, exception });
            }
        }

        internal static void Error(string param, Exception exception)
        {
            //_logger.Error(param, exception);
            if (logger != null)
            {
                error.Invoke(logger, new object[] { param, exception });
            }
        }

        internal static void Info(string param, Exception exception)
        {
            //_logger.Info(param, exception);
            if (logger != null)
            {
                info.Invoke(logger, new object[] { param, exception });
            }
        }

        internal static void Warn(string param, Exception exception)
        {
            //_logger.Warn(param, exception);
            if (logger != null)
            {
                warn.Invoke(logger, new object[] { param, exception });
            }
        }

    }
}
