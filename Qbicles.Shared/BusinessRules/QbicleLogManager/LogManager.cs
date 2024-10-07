using log4net;
using log4net.Appender.Loki;
using Newtonsoft.Json;
using Qbicles.BusinessRules.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public enum QbicleProcessEnum
    {
        OrderProcessing = 1,
        POSApiCall = 2
    }

    public static class LogManager
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger
            (MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ILog lokiLogger = log4net.LogManager.GetLogger("lokiLogger");

        private static readonly ILog fileAppenderLogger = log4net.LogManager.GetLogger("fileAppenderLogger");



        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/caller-information get method name,...
        /// <summary>
        /// logger Error
        /// </summary>
        /// <param name="method">MethodBase - get by MethodBase.GetCurrentMethod() in the current method</param>
        /// <param name="ex">Exception ex</param>
        /// <param name="userId">[User Id = null]</param>
        /// <param name="parameters">[object[] = null one or some parameters in the current method]</param>
        public static void Error(MethodBase method,
                                 Exception ex,
                                 string userId = null,
                                 params object[] parameters)
        {
            if (lokiLogger.IsErrorEnabled)
            {
                var lokiLogEntry = GetLokiInformationForLog(method, ex,
                                                                    logInfo: null,
                                                                    userId: userId,
                                                                    result: null,
                                                                    parameters: parameters);
                lokiLogger.Error(lokiLogEntry);
            }

            if (fileAppenderLogger.IsErrorEnabled)
            {
                var logEntry = GetFileAppenderInformationForLog(method, ex,
                                                                    logInfo: null,
                                                                    userId: userId,
                                                                    result: null,
                                                                    parameters: parameters);
                fileAppenderLogger.Error(logEntry);
            }

        }




        /// <summary>
        /// Process logger  Info
        /// </summary>
        /// <param name="method">MethodBase - get by MethodBase.GetCurrentMethod() in the current method</param>
        /// <param name="message"></param>
        /// <param name="result">[object result = null]</param>
        /// <param name="userId">[User Id = null]</param>
        /// <param name="parameters">[object[] = null one or some parameters in the current method]</param>
        public static void ApplicationInfo(System.Dynamic.ExpandoObject logLabelInfo = null,
                                            string message = null,
                                            string userId = null,
                                            object result = null,
                                            params object[] parameters
        )
        {
            StackTrace stackTrace = new StackTrace();
            MethodBase method = stackTrace.GetFrame(1).GetMethod();


            // Sort out a message
            var msg = message ?? method.Name.AddSpacesToSentence();
            var ex = new Exception(msg);

            if (lokiLogger.IsInfoEnabled)
            {
                var lokiLogEntry = GetLokiInformationForLog(method, ex,
                                                                    logInfo: null,
                                                                    userId: userId,
                                                                    result: result,
                                                                    parameters: parameters);

                var lokiMessage = new MessageLoki(lokiLogEntry);

                AddLokiLabelsFromLogInfo(ref lokiMessage, logLabelInfo);

                lokiLogger.Info(lokiMessage);
            }

            if (fileAppenderLogger.IsInfoEnabled)
            {
                var logEntry = GetFileAppenderInformationForLog(method, ex,
                                                                    logInfo: null,
                                                                    userId: userId,
                                                                    result: result,
                                                                    parameters: parameters);
                fileAppenderLogger.Info(logEntry);
            }


        }

        private static void AddLokiLabelsFromLogInfo(ref MessageLoki lokiMessage, System.Dynamic.ExpandoObject logInfo)
        {
            // Add labels to loki message from loginfo
            if (logInfo != null)
            {
                foreach (var property in (IDictionary<String, Object>)logInfo)
                {
                    var theKey = property.Key;
                    var theValue = property.Value;

                    if (theValue != null)
                    {
                        lokiMessage.AddLabel(theKey, theValue.ToString());
                    }
                    else
                    {
                        lokiMessage.AddLabel(theKey, string.Empty);
                    }



                }
            }
        }







        /// <summary>
        /// logger Info
        /// </summary>
        /// <param name="method">MethodBase - get by MethodBase.GetCurrentMethod() in the current method</param>
        /// <param name="message"></param>
        /// <param name="result">[object result = null]</param>
        /// <param name="userId">[User Id = null]</param>
        /// <param name="parameters">[object[] = null one or some parameters in the current method]</param>
        public static void Info(MethodBase method, string message = null, string userId = null, object result = null, params object[] parameters)
        {
            var msg = message ?? method.Name.AddSpacesToSentence();
            var ex = new Exception(msg);

            if (lokiLogger.IsInfoEnabled)
            {
                var lokiLogEntry = GetLokiInformationForLog(method, ex,
                                                                    logInfo: null,
                                                                    userId: userId,
                                                                    result: result,
                                                                    parameters: parameters);
                lokiLogger.Info(lokiLogEntry);
            }

            if (fileAppenderLogger.IsInfoEnabled)
            {
                var logEntry = GetFileAppenderInformationForLog(method, ex,
                                                                    logInfo: null,
                                                                    userId: userId,
                                                                    result: result,
                                                                    parameters: parameters);
                fileAppenderLogger.Info(logEntry);
            }

        }


        /// <summary>
        /// logger Debug
        /// </summary>
        /// <param name="method">MethodBase - get by MethodBase.GetCurrentMethod() in the current method</param>
        /// <param name="message"></param>
        /// <param name="result">[object result = null]</param>
        /// <param name="userId">[User Id = null]</param>
        /// <param name="parameters">[object[] = null one or some parameters in the current method]</param>
        public static void Debug(MethodBase method, string message = null, string userId = null, object result = null, params object[] parameters)
        {
            var msg = message ?? method.Name.AddSpacesToSentence();
            var ex = new Exception(msg);

            if (lokiLogger.IsDebugEnabled)
            {
                var lokiLogEntry = GetLokiInformationForLog(method, ex,
                                                                    logInfo: null,
                                                                    userId: userId,
                                                                    result: result,
                                                                    parameters: parameters);
                lokiLogger.Debug(lokiLogEntry);
            }

            if (fileAppenderLogger.IsDebugEnabled)
            {
                var logEntry = GetFileAppenderInformationForLog(method, ex, logInfo: null,
                                                                            userId: userId,
                                                                            result: result,
                                                                            parameters: parameters);
                fileAppenderLogger.Debug(logEntry);
            }


        }

        public static void Warn(MethodBase method, string message, string userId, params object[] parameters)
        {
            var msg = message ?? method.Name.AddSpacesToSentence();
            var ex = new Exception(msg);

            if (lokiLogger.IsWarnEnabled)
            {
                var lokiLogEntry = GetLokiInformationForLog(method, ex,
                                                                    logInfo: null,
                                                                    userId: userId,
                                                                    result: null,
                                                                    parameters: parameters);
                lokiLogger.Warn(lokiLogEntry);
            }

            if (fileAppenderLogger.IsWarnEnabled)
            {
                var logEntry = GetFileAppenderInformationForLog(method, ex,
                                                                        logInfo: null,
                                                                        userId: userId,
                                                                        result: null,
                                                                        parameters: parameters);
                fileAppenderLogger.Warn(logEntry);
            }

        }



        private static string GetFileAppenderInformationForLog(MethodBase method,
                                                                Exception ex,
                                                                System.Dynamic.ExpandoObject logInfo,
                                                                string userId = null,
                                                                object result = null,
                                                                params object[] parameters
                                                                )

        {
            var logInformation = GetInformationForLog(method, ex, logInfo, userId, result, parameters);
            var logDict = logInformation as IDictionary<string, Object>;
            logDict.Add("Instance", ConfigManager.LogInstance);
            logDict.Add("Enviroment", ConfigManager.LogEnvironment);
            logDict.Add("Application", ConfigManager.LogApplication);
            var jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.SerializeObject(logInformation, Formatting.None, jsonSerializerSettings);

        }

        private static string GetLokiInformationForLog(MethodBase method,
                                                        Exception ex,
                                                        System.Dynamic.ExpandoObject logInfo,
                                                        string userId = null,
                                                        object result = null,
                                                        params object[] parameters
                                                        )

        {
            var logInformation = GetInformationForLog(method, ex, logInfo, userId, result, parameters);
            var jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.SerializeObject(logInformation, Formatting.None, jsonSerializerSettings);

        }

        private static System.Dynamic.ExpandoObject GetInformationForLog(MethodBase method,
                                                                        Exception ex,
                                                                        System.Dynamic.ExpandoObject logInfo,
                                                                        string userId = null,
                                                                        object result = null,
                                                                        params object[] parameters
                                                                        )
        {


            var pObjects = new List<object>();
            if (parameters != null)
                parameters.ForEach(p =>
                {
                    pObjects.Add(p.ShallowCopy());
                });

            var parametersName = method.GetParameters().Select(n => n.ParameterType.Name + " " + n.Name);
            var methodName = $"{method.Name}({string.Join(",", parametersName)})";



            dynamic m = new System.Dynamic.ExpandoObject();
            m.ClassName = method.DeclaringType.FullName;
            m.ProcessName = methodName;
            m.UserId = userId;
            m.Parameters = pObjects.ToJsonIgnoreReferenceLoop().Replace("\"", "");
            m.Result = result?.ShallowCopy().ToJsonIgnoreReferenceLoop().Replace("\"", "");
            m.Message = ex?.Message;
            m.StackTrace = ex?.StackTrace;
            m.DateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");


            // Create properties for any information supplied by loginfo
            if (logInfo != null)
            {
                var mDict = m as IDictionary<string, Object>;
                foreach (var property in (IDictionary<String, Object>)logInfo)
                {
                    var theKey = property.Key;
                    var theValue = property.Value;

                    if (theValue != null)
                    {
                        mDict.Add(theKey, theValue.ToString());
                    }
                    else
                    {
                        mDict.Add(theKey, string.Empty);
                    }
                }
            }

            return m;
        }

    }

}
