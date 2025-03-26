namespace vnvt_back_end.Core.General
{
    public class EnumTypes
    {
        public enum EventState
        {
            NotPublished = 0,
            InProgress = 1,
            Published = 2,
            PublishedFailed = 3,
            Processing = 4,
            ProcessFailed = 5,
            ProcessCompleted = 6,
            Rollback = 7, // Use for event if need to rollack data
            RollbackCompleted = 8, // Mark event for rollback completed
            RollbackFailed = 9 // Mark event for rollback failed
        }

        public enum ModifiedType
        {
            Add,
            Update,
            Delete,
            Pending,
            NotSuccess
        }

        public enum TokenIsStop
        {
            None = 0,
            TokenExpired = 1,
            RefreshTokenExpired = 2,
            SingleSessionLogin = 3
        }

        public enum MultiTenantType
        {
            Host,
            Tenant
        }

        public enum FilterType
        {
            All,
            ByCompany
        }

        public enum DatabaseProvider
        {
            MSSQL,
            POSTGRESQL
        }

        public enum ConnectionType
        {
            Plain,
            AppConfig
        }

        public enum AuditLogAction
        {
            Add = 0,
            Edit = 1,
            Delete = 2,
            Import = 3,
            Approval = 4,
            Login = 5
        }

        public enum NotifyMethod
        {
            Sms = 0,
            ZaloOA = 1,
            ZaloZNS = 2,
            Email = 3,
            Slack = 4,
            Telegram = 5,
            Firebase = 6
        }

        public enum NotifyType
        {
            TransactionSuccess = 1,
            DebtStatement = 2,
            ConfirmPhoneNumber = 3,
            TransactionConfirmation = 4,
            Voucher = 5,
            DriverRequest = 6,
            PaymentResult = 7,
            SystemWarning = 99
        }
    }
}