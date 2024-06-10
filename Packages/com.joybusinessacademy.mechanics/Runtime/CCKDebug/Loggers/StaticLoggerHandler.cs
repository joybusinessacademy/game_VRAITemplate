using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SkillsVRNodes.Diagnostics.Loggers
{
    // The auto class provides static log methods and custom log processor list for HANDLER_TYPE class.
    // Note that must add custom delegate to CustomLogProcessors before use otherwise it does nothing.
    // Make unique HANDLER_TYPE to ident different name of loggers. i.e. MyClass : StaticLoggerHandler<MyClass, CCKLogger>
    // To customize the logger, create a static method void OnInitMainLogger(LOGGER_TYPE logger) to the HANDLER_TYPE class.
    // This class if for advanced usage only. 
    // Use CustomCCKDebug<HANDLER_TYPE> class for easy access (and redirect logs to CCKDebug).
    public abstract class StaticLoggerHandler<HANDLER_TYPE, LOGGER_TYPE> where LOGGER_TYPE : CCKLogger
    {
        public static List<ICCKLogProcessor> CustomLogProcessors { get; } = new List<ICCKLogProcessor>();
        public static LOGGER_TYPE MainLogger { get; set; } = CreateInstance();

        public static LogTag Tag => MainLogger.Tag;

        class StaticHandlerProcesser : ICCKLogProcessor
        {
            public IFilter<CCKLogEntry> LogFilter { get; set; }

            public void OnReceiveLog(CCKLogEntry log)
            {
                if (null == log)
                {
                    return;
                }

                foreach (var proc in CustomLogProcessors)
                {
                    try
                    {
                        proc?.OnReceiveLog(log);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }
        }

        protected static LOGGER_TYPE CreateInstance()
        {
            LOGGER_TYPE logger = null;
            // Attempting to create an instance using Activator.CreateInstance
            // will throw an exception because the constructor is private.
            try
            {
                // Attempt to create an instance of MyClass using Activator.CreateInstance
                logger = Activator.CreateInstance<LOGGER_TYPE>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            if (null == logger)
            {
                // Creating an instance using reflection
                // by accessing the private constructor directly.
                var privateConstructor = typeof(LOGGER_TYPE).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);
                logger = (LOGGER_TYPE)privateConstructor.Invoke(null);
            }
            InvokeLoggerCreateCallback(logger); 
            logger.LogProcessors.Add(new StaticHandlerProcesser());

            return logger;
        }

        protected static void InvokeLoggerCreateCallback(LOGGER_TYPE logger)
        {
            var method = typeof(HANDLER_TYPE)
                 .GetMethod("OnInitMainLogger",
                 BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
                 , null
                 , new Type[] { typeof(LOGGER_TYPE)},
                 null
                );

            if (null == method)
            {
                return;
            }

            method?.Invoke(null, new object[] { logger });
        }

        public static CCKLogEntry Log(object message, params string[] tags)
        {
            return MainLogger.Log(message, tags);
        }

        public static CCKLogEntry Log(object message, object data, params string[] tags)
        {
            return MainLogger.Log(message, data, tags);
        }

        public static CCKLogEntry LogFormat(string format, params object[] args)
        {
            return MainLogger.LogFormat(format, args);
        }

        public static CCKLogEntry LogFormat(string[] tags, string format, params object[] args)
        {
            return MainLogger.LogFormat(tags, format, args);
        }

        public static CCKLogEntry LogWarning(object message, params string[] tags)
        {
            return MainLogger.LogWarning(message, tags);
        }

        public static CCKLogEntry LogWarning(object message, object data, params string[] tags)
        {
            return MainLogger.LogWarning(message, data, tags);
        }

        public static CCKLogEntry LogWarningFormat(string format, params object[] args)
        {
            return MainLogger.LogWarningFormat(format, args);
        }

        public static CCKLogEntry LogWarningFormat(string[] tags, string format, params object[] args)
        {
            return MainLogger.LogWarningFormat(tags, format, args);
        }

        public static CCKLogEntry LogError(object message, params string[] tags)
        {
            return MainLogger.LogError(message, tags);
        }

        public static CCKLogEntry LogError(object message, object data, params string[] tags)
        {
            return MainLogger.LogError(message, data, tags);
        }

        public static CCKLogEntry LogErrorFormat(string format, params object[] args)
        {
            return MainLogger.LogErrorFormat(format, args);
        }

        public static CCKLogEntry LogErrorFormat(string[] tags, string format, params object[] args)
        {
            return MainLogger.LogErrorFormat(tags, format, args);
        }

        public static CCKLogEntry LogException(Exception message, params string[] tags)
        {
            return MainLogger.LogException(message, tags);
        }

        public static CCKLogEntry LogException(Exception message, object data, params string[] tags)
        {
            return MainLogger.LogException(message, data, tags);
        }

        public static CCKLogEntry LogUnity(LogType logType, object message, string stackTrace, params string[] tags)
        {
            return MainLogger.LogUnity(logType, message, stackTrace, tags);
        }

        public static ICCKLogProcessor RegisterLogProcessor(ICCKLogProcessor processor)
        {
            if (null == processor)
            {
                return processor;
            }

            CustomLogProcessors.Add(processor);
            return processor;
        }

        public static ICCKLogProcessor RegisterLogProcessor<PROCESSOR_TYPE>(params object[] args) where PROCESSOR_TYPE : ICCKLogProcessor
        {
            var p = (PROCESSOR_TYPE)Activator.CreateInstance(typeof(PROCESSOR_TYPE), args);

            CustomLogProcessors.Add(p);
            return p;
        }

        public static bool RemoveLogProcessor(ICCKLogProcessor processor)
        {
            if (null == processor)
            {
                return false;
            }
            return CustomLogProcessors.Remove(processor);
        }
    }
}

