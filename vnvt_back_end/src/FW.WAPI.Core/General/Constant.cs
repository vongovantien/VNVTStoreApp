namespace FW.WAPI.Core.General
{
    public class TableColumnConst
    {
        public const string CODE_COL = "Code";
        public const string MODIFIED_TYPE_COL = "ModifiedType";
        public const string COMPANY_CODE_COL = "CompanyCode";
        public const string DISTRIBUTED_COL = "DistributedCompanyCode";
        public const string CREATED_DATE = "CreatedDate";
        public const string CREATED_BY = "CreatedBy";
        public const string MODIFIED_DATE = "ModifiedDate";
        public const string MODIFIED_BY = "ModifiedBy";
        public const string SCOPE_TYPE = "ScopeType";
        public const string SCOPE = "Scope";
        public const string TRANSFER_TIME = "TransferTime";
        public const string CREATED_AT = "CreatedAt";
        public const string PARENT_CODE = "ParentCode";
    }

    public class TokenConst
    {
        public const string GRANT_PASSWORD = "password";
        public const string GRANT_REFRESH_TOKEN = "refresh_token";
        public const string TOKEN_POLICY = "TokenIsNotStop";
        public const string USER_NAME = "UserName";
        public const string TENANT_CODE = "TenantCode";
        public const string SESSION = "Session";
        public const string COMPANY_CODE = "CompanyCode";
        public const string COMPANY_CODES = "CompanyCodes";
        public const string TENANT_CODES = "TenantCodes";
        public const string ROLE_CODE = "RoleCode";
    }

    public class PostgresSql
    {
        public const string ILIKE = "ILIKE";
    }

    public class CorsPolicy
    {
        public const string CORS_POLICY_NAME = "AllowAll";
    }

    public class MultiTenancyConsts
    {
        public const string TenantIdResolveKey = "TenantCode";
    }

    public static class RoleType
    {
        public const string ADMIN = "Admin";
        public const string CONSUMNER = "User";
        public const string DRIVER = "Driver";
        public const string DEVICE = "Device";
    }

    public class PostgresConst
    {
        public const string TIME_STAMP_FORMAT = "dd/MM/yyyy HH24:MI:SS";
        public const string DATE_FORMAT = "dd/MM/yyyy";
        public const string QUOTE = "\"";
        public const string ILIKE = "ILIKE";
    }

    public static class GoogleAuthConst
    {
        public const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    }

    public static class CachingConfigConst
    {
        public const int CACHED_TENANT_TIME_OUT = 3600;
    }

    public static class GeneralConst
    {
        public const string CODE_PREFIX_DELIMITER = ".";
        public const string CODE_PREFIX_DEFAULT = "SMI";
    }

    public static class AuditLogConst
    {
        public const string AUDIT_LOG_DATA_KEY = "AUDIT_LOG_DATA_KEY";        
        public static string[] SENSITIVE_FIELDS = new string[]{"Password", "SecurityCode"};
        public const char SENSITIVE_VALUE_ALTERNATIVE = '*';
        public const int SENSITIVE_VALUE_ALTERNATIVE_LENGTH = 6;
    }

    public static class RabbitMQConst
    {
        public const int RETRY_DEATH_MESSAGE = 5;
        public const int PUBLISH_CONFIRM_TIMEOUT_MILISECONDS = 3000;
        public const int SLOW_PROCESS_EVENT_THRESHOLD_MILISECONDS = 1500;
    }
}