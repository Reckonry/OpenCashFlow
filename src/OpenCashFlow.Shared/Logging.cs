using Microsoft.Extensions.Logging;

namespace Shared.Logging
{
    public static class LogEvents
    {
        public static class System
        {
            public static readonly EventId StartupComplete = new(1000, "System_StartupComplete");
            public static readonly EventId Shutdown = new(1001, "System_Shutdown");
            public static readonly EventId ConfigLoaded = new(1002, "System_ConfigLoaded");
        }

        public static class Auth
        {
            public static readonly EventId LoginSuccess = new(3000, "Auth_LoginSuccess");
            public static readonly EventId LoginFailed = new(3001, "Auth_LoginFailed");
            public static readonly EventId UnauthorizedAccess = new(3002, "Auth_UnauthorizedAccess");
        }

        public static class Db
        {
            public static readonly EventId QuerySuccess = new(2000, "Db_QuerySuccess");
            public static readonly EventId NoUpdate = new(2001, "Db_NoUpdate");
            public static readonly EventId QueryError = new(2002, "Db_QueryError");
            public static readonly EventId ConnectionFailed = new(2003, "Db_ConnectionFailed");
            public static readonly EventId InsertFailed = new(2004, "Db_InsertFailed");
            public static readonly EventId DeleteFailed = new(2005, "Db_DeleteFailed");
        }

        public static class Api
        {
            public static readonly EventId EndpointHit = new(4000, "Api_EndpointHit");
            public static readonly EventId ValidationFailed = new(4001, "Api_ValidationFailed");
            public static readonly EventId UnexpectedInput = new(4002, "Api_UnexpectedInput");
            public static readonly EventId NotFound = new(4003, "Api_ResourceNotFound");
            public static readonly EventId ExportSuccess = new(4004, "Api_ExportSuccess");
            public static readonly EventId ExportFailed = new(4005, "Api_ExportFailed");
            public static readonly EventId ImportStarted = new(4006, "Api_ImportStarted");
            public static readonly EventId ImportCompleted = new(4007, "Api_ImportCompleted");
            public static readonly EventId ImportFailed = new(4008, "Api_ImportFailed");
            public static readonly EventId UnhandledException = new(4009, "Api_UnhandledException");
        }

        public static class Service
        {
            public static readonly EventId ProcessingStarted = new(5000, "Service_ProcessingStarted");
            public static readonly EventId ProcessingCompleted = new(5001, "Service_ProcessingCompleted");
            public static readonly EventId BusinessRuleViolated = new(5002, "Service_BusinessRuleViolated");
            public static readonly EventId ExternalServiceCall = new(5003, "Service_ExternalServiceCall");
            public static readonly EventId ExternalServiceFailed = new(5004, "Service_ExternalServiceFailed");
        }

        public static class Exception
        {
            public static readonly EventId CaughtHandled = new(9000, "Exception_CaughtHandled");
            public static readonly EventId CaughtUnhandled = new(9001, "Exception_CaughtUnhandled");
            public static readonly EventId Fatal = new(9999, "Exception_FatalCrash");
        }

        public static class Debug
        {
            public static readonly EventId TracePoint = new(8000, "Debug_TracePoint");
            public static readonly EventId TestLog = new(8001, "Debug_TestLog");
        }
    }
}
