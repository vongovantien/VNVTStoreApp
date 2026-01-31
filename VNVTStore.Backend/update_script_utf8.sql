Using project 'D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.Infrastructure\VNVTStore.Infrastructure.csproj'.
Using startup project 'D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API\VNVTStore.API.csproj'.
dotnet msbuild /getProperty:ProjectName /getProperty:AssemblyName /getProperty:DesignAssembly /getProperty:Language /getProperty:OutputPath /getProperty:PlatformTarget /getProperty:ProjectAssetsFile /getProperty:ProjectDir /getProperty:RootNamespace /getProperty:RuntimeFrameworkVersion /getProperty:TargetFileName /getProperty:TargetFrameworkMoniker /getProperty:Nullable /getProperty:TargetFramework /getProperty:TargetPlatformIdentifier /getProperty:Platform /t:ResolvePackageAssets /getItem:RuntimeCopyLocalItems D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.Infrastructure\VNVTStore.Infrastructure.csproj
dotnet msbuild /getProperty:ProjectName /getProperty:AssemblyName /getProperty:DesignAssembly /getProperty:Language /getProperty:OutputPath /getProperty:PlatformTarget /getProperty:ProjectAssetsFile /getProperty:ProjectDir /getProperty:RootNamespace /getProperty:RuntimeFrameworkVersion /getProperty:TargetFileName /getProperty:TargetFrameworkMoniker /getProperty:Nullable /getProperty:TargetFramework /getProperty:TargetPlatformIdentifier /getProperty:Platform /t:ResolvePackageAssets /getItem:RuntimeCopyLocalItems D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API\VNVTStore.API.csproj
Build started...
dotnet build D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API\VNVTStore.API.csproj /verbosity:quiet /nologo /p:PublishAot=false

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.82

Build succeeded.
dotnet exec --depsfile D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API\bin\Debug\net8.0\VNVTStore.API.deps.json --additionalprobingpath C:\Users\ADMIN\.nuget\packages --additionalprobingpath "C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages" --runtimeconfig D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API\bin\Debug\net8.0\VNVTStore.API.runtimeconfig.json C:\Users\ADMIN\.dotnet\tools\.store\dotnet-ef\10.0.2\dotnet-ef\10.0.2\tools\net8.0\any\tools\net8.0\any\ef.dll migrations script --assembly D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API\bin\Debug\net8.0\VNVTStore.Infrastructure.dll --project D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.Infrastructure\VNVTStore.Infrastructure.csproj --startup-assembly D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API\bin\Debug\net8.0\VNVTStore.API.dll --startup-project D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API\VNVTStore.API.csproj --project-dir D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.Infrastructure\ --root-namespace VNVTStore.Infrastructure --language C# --framework net8.0 --design-assembly C:\Users\ADMIN\.nuget\packages\microsoft.entityframeworkcore.design\8.0.7\lib\net8.0\Microsoft.EntityFrameworkCore.Design.dll --nullable --working-dir D:\VNVTStoreApp\VNVTStore.Backend --verbose
Using assembly 'VNVTStore.Infrastructure'.
Using startup assembly 'VNVTStore.API'.
Using application base 'D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API\bin\Debug\net8.0'.
Using working directory 'D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API'.
Using root namespace 'VNVTStore.Infrastructure'.
Using project directory 'D:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.Infrastructure\'.
Remaining arguments: .
Finding DbContext classes...
Finding IDesignTimeDbContextFactory implementations...
Finding application service provider in assembly 'VNVTStore.API'...
Finding Microsoft.Extensions.Hosting service provider...
Using environment 'Development'.
[Warning] Failed to initialize Firebase: The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.
Using application service provider from Microsoft.Extensions.Hosting.
Found DbContext 'ApplicationDbContext'.
Finding DbContext classes in the project...
Using DbContext factory 'DesignTimeDbContextFactory'.
Using context 'ApplicationDbContext'.
Finding design-time services referenced by assembly 'VNVTStore.API'...
Finding design-time services referenced by assembly 'VNVTStore.Infrastructure'...
No referenced design-time services were found.
Finding design-time services for provider 'Npgsql.EntityFrameworkCore.PostgreSQL'...
Using design-time services from provider 'Npgsql.EntityFrameworkCore.PostgreSQL'.
Finding IDesignTimeServices implementations in assembly 'VNVTStore.API'...
No design-time services were found.
Generating up script for migration '20260108165353_InitialCreate'.
Generating up script for migration '20260109235725_AddPaymentTable'.
Generating up script for migration '20260110004028_UpdateTimestampsToTz'.
Generating up script for migration '20260110113407_AddProductAttributes'.
Generating up script for migration '20260111035701_AddRefreshTokenToUsers'.
Generating up script for migration '20260111054900_IncreaseCodeLength'.
Generating up script for migration '20260113101319_ForceTimestamptz'.
Generating up script for migration '20260115161258_AddModifiedTypeColumn'.
Generating up script for migration '20260117012025_AddGuestQuoteAndSnapshotOrderItem'.
Generating up script for migration '20260117112339_AddTblFile'.
Generating up script for migration '20260117141336_ReplaceProductImageWithFile'.
Generating up script for migration '20260118154421_AddMasterCodeToFile'.
Generating up script for migration '20260119084803_AddVerificationTokenToOrder'.
Generating up script for migration '20260119164551_RemoveSku'.
Generating up script for migration '20260123010224_StandardizeCommonFields'.
Generating up script for migration '20260124064712_AddLoyaltyPointsAndProductFields'.
Generating up script for migration '20260124070444_AddEmailVerification'.
Generating up script for migration '20260124070735_AddPasswordReset'.
Generating up script for migration '20260124131159_AddProductDetail'.
Generating up script for migration '20260124143757_EnumConversion'.
'ApplicationDbContext' disposed.
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DROP TABLE IF EXISTS "TblCartItem" CASCADE;

DROP TABLE IF EXISTS "TblPayment" CASCADE;

DROP TABLE IF EXISTS "TblProductImage" CASCADE;

DROP TABLE IF EXISTS "TblProductPromotion" CASCADE;

DROP TABLE IF EXISTS "TblQuote" CASCADE;

DROP TABLE IF EXISTS "TblReview" CASCADE;

DROP TABLE IF EXISTS "TblCart" CASCADE;

DROP TABLE IF EXISTS "TblOrderItem" CASCADE;

DROP TABLE IF EXISTS "TblOrder" CASCADE;

DROP TABLE IF EXISTS "TblProduct" CASCADE;

DROP TABLE IF EXISTS "TblAddress" CASCADE;

DROP TABLE IF EXISTS "TblCoupon" CASCADE;

DROP TABLE IF EXISTS "TblSupplier" CASCADE;

DROP TABLE IF EXISTS "TblCategory" CASCADE;

DROP TABLE IF EXISTS "TblUser" CASCADE;

DROP TABLE IF EXISTS "TblPromotion" CASCADE;

DROP TABLE IF EXISTS "__EFMigrationsHistory" CASCADE;

DROP SEQUENCE IF EXISTS "address_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "cart_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "cartitem_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "category_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "coupon_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "order_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "orderitem_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "payment_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "product_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "productimage_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "productpromotion_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "promotion_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "quote_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "review_code_seq" CASCADE;

DROP SEQUENCE IF EXISTS "user_code_seq" CASCADE;

CREATE SEQUENCE address_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE cart_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE cartitem_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE category_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE coupon_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE order_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE orderitem_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE payment_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE product_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE productimage_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE productpromotion_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE promotion_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE quote_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE review_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE SEQUENCE user_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE TABLE "TblCategory" (
    "Code" character varying(10) NOT NULL DEFAULT (('CAT'::text || lpad((nextval('category_code_seq'::regclass))::text, 6, '0'::text))),
    "Name" character varying(100) NOT NULL,
    "Description" text,
    "ParentCode" character varying(10),
    "ImageURL" character varying(255),
    "IsActive" boolean DEFAULT TRUE,
    CONSTRAINT "TblCategory_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblCategory_ParentCode_fkey" FOREIGN KEY ("ParentCode") REFERENCES "TblCategory" ("Code") ON DELETE SET NULL
);

CREATE TABLE "TblPromotion" (
    "Code" character varying(10) NOT NULL DEFAULT (('PROM'::text || lpad((nextval('promotion_code_seq'::regclass))::text, 6, '0'::text))),
    "Name" character varying(100) NOT NULL,
    "Description" text,
    "DiscountType" character varying(20) NOT NULL,
    "DiscountValue" numeric(15,2) NOT NULL,
    "MinOrderAmount" numeric(15,2),
    "MaxDiscountAmount" numeric(15,2),
    "StartDate" timestamp without time zone NOT NULL,
    "EndDate" timestamp without time zone NOT NULL,
    "UsageLimit" integer,
    "IsActive" boolean DEFAULT TRUE,
    CONSTRAINT "TblPromotion_pkey" PRIMARY KEY ("Code")
);

CREATE TABLE "TblSupplier" (
    "Code" character varying NOT NULL,
    "Name" character varying NOT NULL,
    "ContactPerson" character varying,
    "Email" character varying,
    "Phone" character varying,
    "Address" character varying,
    "TaxCode" character varying,
    "BankAccount" character varying,
    "BankName" character varying,
    "Notes" text,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "TblSupplier_pkey" PRIMARY KEY ("Code")
);

CREATE TABLE "TblUser" (
    "Code" character varying(10) NOT NULL DEFAULT (('USR'::text || lpad((nextval('user_code_seq'::regclass))::text, 6, '0'::text))),
    "Username" character varying(50) NOT NULL,
    "Email" character varying(100) NOT NULL,
    "PasswordHash" character varying(255) NOT NULL,
    "FullName" character varying(100),
    "Phone" character varying(15),
    "Role" character varying(20) DEFAULT ('customer'::character varying),
    "CreatedAt" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
    "LastLogin" timestamp with time zone,
    "IsActive" boolean NOT NULL,
    "RefreshToken" text,
    "RefreshTokenExpiry" timestamp with time zone,
    CONSTRAINT "TblUser_pkey" PRIMARY KEY ("Code")
);

CREATE TABLE "TblCoupon" (
    "Code" character varying(10) NOT NULL DEFAULT (('CPN'::text || lpad((nextval('coupon_code_seq'::regclass))::text, 6, '0'::text))),
    "PromotionCode" character varying(10),
    "UsageCount" integer DEFAULT 0,
    CONSTRAINT "TblCoupon_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblCoupon_PromotionCode_fkey" FOREIGN KEY ("PromotionCode") REFERENCES "TblPromotion" ("Code") ON DELETE SET NULL
);

CREATE TABLE "TblProduct" (
    "Code" character varying(10) NOT NULL DEFAULT (('PRD'::text || lpad((nextval('product_code_seq'::regclass))::text, 6, '0'::text))),
    "Name" character varying(100) NOT NULL,
    "Description" text,
    "Price" numeric(15,2) NOT NULL,
    "CostPrice" numeric(15,2),
    "StockQuantity" integer DEFAULT 0,
    "CategoryCode" character varying(10),
    "SKU" character varying(50),
    "Weight" numeric(8,2),
    "IsActive" boolean DEFAULT TRUE,
    "CreatedAt" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
    "SupplierCode" text,
    "SupplierCodeNavigationCode" character varying,
    CONSTRAINT "TblProduct_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "FK_TblProduct_TblSupplier_SupplierCodeNavigationCode" FOREIGN KEY ("SupplierCodeNavigationCode") REFERENCES "TblSupplier" ("Code"),
    CONSTRAINT "TblProduct_CategoryCode_fkey" FOREIGN KEY ("CategoryCode") REFERENCES "TblCategory" ("Code") ON DELETE SET NULL
);

CREATE TABLE "TblAddress" (
    "Code" character varying(10) NOT NULL DEFAULT (('ADR'::text || lpad((nextval('address_code_seq'::regclass))::text, 6, '0'::text))),
    "UserCode" character varying(10) NOT NULL,
    "AddressLine" character varying(255) NOT NULL,
    "City" character varying(50),
    "State" character varying(50),
    "PostalCode" character varying(10),
    "Country" character varying(50) DEFAULT ('Vietnam'::character varying),
    "IsDefault" boolean DEFAULT FALSE,
    "CreatedAt" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "TblAddress_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblAddress_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblCart" (
    "Code" character varying(10) NOT NULL DEFAULT (('CRT'::text || lpad((nextval('cart_code_seq'::regclass))::text, 6, '0'::text))),
    "UserCode" character varying(10) NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "TblCart_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblCart_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblProductImage" (
    "Code" character varying(10) NOT NULL DEFAULT (('IMG'::text || lpad((nextval('productimage_code_seq'::regclass))::text, 6, '0'::text))),
    "ProductCode" character varying(10) NOT NULL,
    "ImageURL" character varying(255) NOT NULL,
    "AltText" character varying(255),
    "IsPrimary" boolean DEFAULT FALSE,
    "SortOrder" integer DEFAULT 0,
    CONSTRAINT "TblProductImage_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblProductImage_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblProductPromotion" (
    "Code" character varying(10) NOT NULL DEFAULT (('PPROM'::text || lpad((nextval('productpromotion_code_seq'::regclass))::text, 6, '0'::text))),
    "ProductCode" character varying(10) NOT NULL,
    "PromotionCode" character varying(10) NOT NULL,
    CONSTRAINT "TblProductPromotion_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblProductPromotion_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE,
    CONSTRAINT "TblProductPromotion_PromotionCode_fkey" FOREIGN KEY ("PromotionCode") REFERENCES "TblPromotion" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblQuote" (
    "Code" character varying(10) NOT NULL DEFAULT (('QT'::text || lpad((nextval('quote_code_seq'::regclass))::text, 6, '0'::text))),
    "UserCode" character varying(10) NOT NULL,
    "ProductCode" character varying(10) NOT NULL,
    "Quantity" integer NOT NULL DEFAULT 1,
    "Note" text,
    "QuotedPrice" numeric(15,2),
    "AdminNote" text,
    "Status" character varying(20) NOT NULL DEFAULT ('pending'::character varying),
    "CreatedAt" timestamp without time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "TblQuote_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblQuote_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE,
    CONSTRAINT "TblQuote_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblOrder" (
    "Code" character varying(10) NOT NULL DEFAULT (('ORD'::text || lpad((nextval('order_code_seq'::regclass))::text, 6, '0'::text))),
    "UserCode" character varying(10) NOT NULL,
    "OrderDate" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
    "TotalAmount" numeric(15,2) NOT NULL,
    "DiscountAmount" numeric(15,2) DEFAULT (0),
    "FinalAmount" numeric(15,2) NOT NULL,
    "Status" character varying(20) DEFAULT ('pending'::character varying),
    "AddressCode" character varying(10),
    "CouponCode" character varying(10),
    CONSTRAINT "TblOrder_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblOrder_AddressCode_fkey" FOREIGN KEY ("AddressCode") REFERENCES "TblAddress" ("Code"),
    CONSTRAINT "TblOrder_CouponCode_fkey" FOREIGN KEY ("CouponCode") REFERENCES "TblCoupon" ("Code"),
    CONSTRAINT "TblOrder_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblCartItem" (
    "Code" character varying(10) NOT NULL DEFAULT (('CRI'::text || lpad((nextval('cartitem_code_seq'::regclass))::text, 6, '0'::text))),
    "CartCode" character varying(10) NOT NULL,
    "ProductCode" character varying(10) NOT NULL,
    "Quantity" integer NOT NULL DEFAULT 1,
    "AddedAt" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "TblCartItem_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblCartItem_CartCode_fkey" FOREIGN KEY ("CartCode") REFERENCES "TblCart" ("Code") ON DELETE CASCADE,
    CONSTRAINT "TblCartItem_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblOrderItem" (
    "Code" character varying(10) NOT NULL DEFAULT (('ORI'::text || lpad((nextval('orderitem_code_seq'::regclass))::text, 6, '0'::text))),
    "OrderCode" character varying(10) NOT NULL,
    "ProductCode" character varying(10),
    "Quantity" integer NOT NULL,
    "PriceAtOrder" numeric(15,2) NOT NULL,
    "DiscountAmount" numeric(15,2) DEFAULT (0),
    CONSTRAINT "TblOrderItem_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblOrderItem_OrderCode_fkey" FOREIGN KEY ("OrderCode") REFERENCES "TblOrder" ("Code") ON DELETE CASCADE,
    CONSTRAINT "TblOrderItem_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE SET NULL
);

CREATE TABLE "TblPayment" (
    "Code" character varying(10) NOT NULL DEFAULT (('PAY'::text || lpad((nextval('payment_code_seq'::regclass))::text, 6, '0'::text))),
    "OrderCode" character varying(10) NOT NULL,
    "PaymentDate" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
    "Amount" numeric(15,2) NOT NULL,
    "Method" character varying(50) NOT NULL,
    "TransactionID" character varying(100),
    "Status" character varying(20) DEFAULT ('pending'::character varying),
    CONSTRAINT "TblPayment_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblPayment_OrderCode_fkey" FOREIGN KEY ("OrderCode") REFERENCES "TblOrder" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblReview" (
    "Code" character varying(10) NOT NULL DEFAULT (('REV'::text || lpad((nextval('review_code_seq'::regclass))::text, 6, '0'::text))),
    "OrderItemCode" character varying(10),
    "UserCode" character varying(10) NOT NULL,
    "Rating" integer,
    "Comment" text,
    "CreatedAt" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
    "IsApproved" boolean DEFAULT FALSE,
    CONSTRAINT "TblReview_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblReview_OrderItemCode_fkey" FOREIGN KEY ("OrderItemCode") REFERENCES "TblOrderItem" ("Code"),
    CONSTRAINT "TblReview_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code")
);

CREATE INDEX "IX_TblAddress_UserCode" ON "TblAddress" ("UserCode");

CREATE INDEX idx_cart_user ON "TblCart" ("UserCode");

CREATE INDEX "IX_TblCartItem_CartCode" ON "TblCartItem" ("CartCode");

CREATE INDEX "IX_TblCartItem_ProductCode" ON "TblCartItem" ("ProductCode");

CREATE INDEX "IX_TblCategory_ParentCode" ON "TblCategory" ("ParentCode");

CREATE INDEX "IX_TblCoupon_PromotionCode" ON "TblCoupon" ("PromotionCode");

CREATE INDEX idx_order_status ON "TblOrder" ("Status");

CREATE INDEX "IX_TblOrder_AddressCode" ON "TblOrder" ("AddressCode");

CREATE INDEX "IX_TblOrder_CouponCode" ON "TblOrder" ("CouponCode");

CREATE INDEX "IX_TblOrder_UserCode" ON "TblOrder" ("UserCode");

CREATE INDEX "IX_TblOrderItem_OrderCode" ON "TblOrderItem" ("OrderCode");

CREATE INDEX "IX_TblOrderItem_ProductCode" ON "TblOrderItem" ("ProductCode");

CREATE UNIQUE INDEX "TblPayment_OrderCode_key" ON "TblPayment" ("OrderCode");

CREATE INDEX idx_product_name ON "TblProduct" ("Name");

CREATE INDEX "IX_TblProduct_CategoryCode" ON "TblProduct" ("CategoryCode");

CREATE INDEX "IX_TblProduct_SupplierCodeNavigationCode" ON "TblProduct" ("SupplierCodeNavigationCode");

CREATE UNIQUE INDEX "TblProduct_SKU_key" ON "TblProduct" ("SKU");

CREATE INDEX "IX_TblProductImage_ProductCode" ON "TblProductImage" ("ProductCode");

CREATE INDEX "IX_TblProductPromotion_PromotionCode" ON "TblProductPromotion" ("PromotionCode");

CREATE UNIQUE INDEX "TblProductPromotion_ProductCode_PromotionCode_key" ON "TblProductPromotion" ("ProductCode", "PromotionCode");

CREATE INDEX "IX_TblQuote_ProductCode" ON "TblQuote" ("ProductCode");

CREATE INDEX "IX_TblQuote_UserCode" ON "TblQuote" ("UserCode");

CREATE INDEX "IX_TblReview_OrderItemCode" ON "TblReview" ("OrderItemCode");

CREATE INDEX "IX_TblReview_UserCode" ON "TblReview" ("UserCode");

CREATE UNIQUE INDEX "TblUser_Email_key" ON "TblUser" ("Email");

CREATE UNIQUE INDEX "TblUser_Username_key" ON "TblUser" ("Username");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260108165353_InitialCreate', '8.0.7');

COMMIT;

START TRANSACTION;

CREATE SEQUENCE banner_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

ALTER TABLE "TblProduct" ALTER COLUMN "SupplierCode" TYPE character varying;

ALTER TABLE "TblOrderItem" ADD "Color" text;

ALTER TABLE "TblOrderItem" ADD "Size" text;

ALTER TABLE "TblCartItem" ADD "Color" text;

ALTER TABLE "TblCartItem" ADD "Size" text;

CREATE TABLE "TblBanner" (
    "Code" character varying(10) NOT NULL DEFAULT (('BNN'::text || lpad((nextval('banner_code_seq'::regclass))::text, 6, '0'::text))),
    "Title" character varying(200) NOT NULL,
    "Content" character varying(500),
    "LinkUrl" character varying(200),
    "LinkText" character varying(50),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "Priority" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp without time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "TblBanner_pkey" PRIMARY KEY ("Code")
);

CREATE INDEX "IX_TblProduct_SupplierCode" ON "TblProduct" ("SupplierCode");

ALTER TABLE "TblProduct" ADD CONSTRAINT "FK_TblProduct_TblSupplier_SupplierCode" FOREIGN KEY ("SupplierCode") REFERENCES "TblSupplier" ("Code") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260109235725_AddPaymentTable', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblUser" ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone;

ALTER TABLE "TblUser" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;

ALTER TABLE "TblReview" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;

ALTER TABLE "TblQuote" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;

ALTER TABLE "TblPromotion" ALTER COLUMN "StartDate" TYPE timestamp with time zone;

ALTER TABLE "TblPromotion" ALTER COLUMN "EndDate" TYPE timestamp with time zone;

ALTER TABLE "TblProduct" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;

ALTER TABLE "TblPayment" ALTER COLUMN "PaymentDate" TYPE timestamp with time zone;

ALTER TABLE "TblOrder" ALTER COLUMN "OrderDate" TYPE timestamp with time zone;

ALTER TABLE "TblCartItem" ALTER COLUMN "AddedAt" TYPE timestamp with time zone;

ALTER TABLE "TblCart" ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone;

ALTER TABLE "TblCart" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;

ALTER TABLE "TblBanner" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;

ALTER TABLE "TblAddress" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260110004028_UpdateTimestampsToTz', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblProduct" ADD "Color" text;

ALTER TABLE "TblProduct" ADD "Material" text;

ALTER TABLE "TblProduct" ADD "Power" text;

ALTER TABLE "TblProduct" ADD "Size" text;

ALTER TABLE "TblProduct" ADD "Voltage" text;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260110113407_AddProductAttributes', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblUser" RENAME COLUMN "RefreshTokenExpiry" TO "RefreshTokenExpiryTime";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260111035701_AddRefreshTokenToUsers', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblUser" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblReview" ALTER COLUMN "UserCode" TYPE character varying(100);

ALTER TABLE "TblReview" ALTER COLUMN "OrderItemCode" TYPE character varying(100);

ALTER TABLE "TblReview" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblQuote" ALTER COLUMN "UserCode" TYPE character varying(100);

ALTER TABLE "TblQuote" ALTER COLUMN "ProductCode" TYPE character varying(100);

ALTER TABLE "TblQuote" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblPromotion" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblProductPromotion" ALTER COLUMN "PromotionCode" TYPE character varying(100);

ALTER TABLE "TblProductPromotion" ALTER COLUMN "ProductCode" TYPE character varying(100);

ALTER TABLE "TblProductPromotion" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblProductImage" ALTER COLUMN "ProductCode" TYPE character varying(100);

ALTER TABLE "TblProductImage" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblProduct" ALTER COLUMN "CategoryCode" TYPE character varying(100);

ALTER TABLE "TblProduct" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblPayment" ALTER COLUMN "OrderCode" TYPE character varying(100);

ALTER TABLE "TblPayment" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblOrderItem" ALTER COLUMN "ProductCode" TYPE character varying(100);

ALTER TABLE "TblOrderItem" ALTER COLUMN "OrderCode" TYPE character varying(100);

ALTER TABLE "TblOrderItem" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblOrder" ALTER COLUMN "UserCode" TYPE character varying(100);

ALTER TABLE "TblOrder" ALTER COLUMN "CouponCode" TYPE character varying(100);

ALTER TABLE "TblOrder" ALTER COLUMN "AddressCode" TYPE character varying(100);

ALTER TABLE "TblOrder" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblCoupon" ALTER COLUMN "PromotionCode" TYPE character varying(100);

ALTER TABLE "TblCoupon" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblCategory" ALTER COLUMN "ParentCode" TYPE character varying(100);

ALTER TABLE "TblCategory" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblCartItem" ALTER COLUMN "ProductCode" TYPE character varying(100);

ALTER TABLE "TblCartItem" ALTER COLUMN "CartCode" TYPE character varying(100);

ALTER TABLE "TblCartItem" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblCart" ALTER COLUMN "UserCode" TYPE character varying(100);

ALTER TABLE "TblCart" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblBanner" ALTER COLUMN "Code" TYPE character varying(100);

ALTER TABLE "TblAddress" ALTER COLUMN "UserCode" TYPE character varying(100);

ALTER TABLE "TblAddress" ALTER COLUMN "PostalCode" TYPE character varying(100);

ALTER TABLE "TblAddress" ALTER COLUMN "Code" TYPE character varying(100);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260111054900_IncreaseCodeLength', '8.0.7');

COMMIT;

START TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260113101319_ForceTimestamptz', '8.0.7');

COMMIT;

START TRANSACTION;

UPDATE "TblUser" SET "Role" = 'customer'::character varying WHERE "Role" IS NULL;
ALTER TABLE "TblUser" ALTER COLUMN "Role" SET NOT NULL;

ALTER TABLE "TblUser" ADD "ModifiedType" text DEFAULT 'Add';

ALTER TABLE "TblSupplier" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblReview" ADD "IsActive" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "TblReview" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblReview" ADD "UpdatedAt" timestamp with time zone;

ALTER TABLE "TblQuote" ALTER COLUMN "CreatedAt" DROP NOT NULL;

ALTER TABLE "TblQuote" ADD "IsActive" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "TblQuote" ADD "ModifiedType" text DEFAULT 'ADD';

UPDATE "TblProduct" SET "IsActive" = TRUE WHERE "IsActive" IS NULL;
ALTER TABLE "TblProduct" ALTER COLUMN "IsActive" SET NOT NULL;

ALTER TABLE "TblProduct" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblProduct" ADD "UpdatedAt" timestamp with time zone;

UPDATE "TblPayment" SET "Status" = 'pending'::character varying WHERE "Status" IS NULL;
ALTER TABLE "TblPayment" ALTER COLUMN "Status" SET NOT NULL;

UPDATE "TblOrder" SET "Status" = 'pending'::character varying WHERE "Status" IS NULL;
ALTER TABLE "TblOrder" ALTER COLUMN "Status" SET NOT NULL;

ALTER TABLE "TblOrder" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblCoupon" ADD "CreatedAt" timestamp with time zone;

ALTER TABLE "TblCoupon" ADD "IsActive" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "TblCoupon" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblCoupon" ADD "UpdatedAt" timestamp with time zone;

UPDATE "TblCategory" SET "IsActive" = TRUE WHERE "IsActive" IS NULL;
ALTER TABLE "TblCategory" ALTER COLUMN "IsActive" SET NOT NULL;

ALTER TABLE "TblCategory" ADD "CreatedAt" timestamp with time zone;

ALTER TABLE "TblCategory" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblCategory" ADD "UpdatedAt" timestamp with time zone;

ALTER TABLE "TblBanner" ALTER COLUMN "CreatedAt" DROP NOT NULL;

ALTER TABLE "TblBanner" ADD "ModifiedType" text DEFAULT 'ADD';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260115161258_AddModifiedTypeColumn', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblQuote" DROP CONSTRAINT "TblQuote_UserCode_fkey";

ALTER TABLE "TblQuote" ALTER COLUMN "UserCode" DROP NOT NULL;

ALTER TABLE "TblQuote" ADD "CustomerEmail" text;

ALTER TABLE "TblQuote" ADD "CustomerName" text;

ALTER TABLE "TblQuote" ADD "CustomerPhone" text;

ALTER TABLE "TblOrderItem" ADD "ProductImage" text;

ALTER TABLE "TblOrderItem" ADD "ProductName" text;

ALTER TABLE "TblQuote" ADD CONSTRAINT "TblQuote_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260117012025_AddGuestQuoteAndSnapshotOrderItem', '8.0.7');

COMMIT;

START TRANSACTION;

CREATE SEQUENCE file_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE TABLE "TblFile" (
    "Code" character varying(100) NOT NULL DEFAULT (('FIL'::text || lpad((nextval('file_code_seq'::regclass))::text, 6, '0'::text))),
    "FileName" character varying(255) NOT NULL,
    "OriginalName" character varying(255) NOT NULL,
    "Extension" character varying(10) NOT NULL,
    "MimeType" character varying(100) NOT NULL,
    "Size" bigint NOT NULL,
    "Path" character varying(500) NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblFile_pkey" PRIMARY KEY ("Code")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260117112339_AddTblFile', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblFile" ADD "TblProductCode" character varying(100);

CREATE INDEX "IX_TblFile_TblProductCode" ON "TblFile" ("TblProductCode");

ALTER TABLE "TblFile" ADD CONSTRAINT "FK_TblFile_TblProduct_TblProductCode" FOREIGN KEY ("TblProductCode") REFERENCES "TblProduct" ("Code");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260117141336_ReplaceProductImageWithFile', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblFile" DROP CONSTRAINT "FK_TblFile_TblProduct_TblProductCode";

DROP INDEX "IX_TblFile_TblProductCode";

ALTER TABLE "TblFile" DROP COLUMN "TblProductCode";

ALTER TABLE "TblFile" ADD "MasterCode" character varying(100);

ALTER TABLE "TblFile" ADD "MasterType" character varying(50);

CREATE INDEX idx_file_mastercode ON "TblFile" ("MasterCode");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260118154421_AddMasterCodeToFile', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblOrder" ADD "VerificationToken" text;

ALTER TABLE "TblOrder" ADD "VerificationTokenExpiresAt" timestamp with time zone;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260119084803_AddVerificationTokenToOrder', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblProduct" DROP CONSTRAINT IF EXISTS "TblProduct_SKU_key";

DROP INDEX IF EXISTS "TblProduct_SKU_key";

ALTER TABLE "TblProduct" DROP COLUMN "SKU";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260119164551_RemoveSku', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblUser" ALTER COLUMN "IsActive" SET DEFAULT TRUE;

ALTER TABLE "TblSupplier" ALTER COLUMN "UpdatedAt" SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblSupplier" ALTER COLUMN "CreatedAt" SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblReview" ALTER COLUMN "UpdatedAt" SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblReview" ALTER COLUMN "IsActive" SET DEFAULT TRUE;

ALTER TABLE "TblQuote" ALTER COLUMN "UpdatedAt" SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblQuote" ALTER COLUMN "IsActive" SET DEFAULT TRUE;

UPDATE "TblPromotion" SET "IsActive" = TRUE WHERE "IsActive" IS NULL;
ALTER TABLE "TblPromotion" ALTER COLUMN "IsActive" SET NOT NULL;

ALTER TABLE "TblPromotion" ADD "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblPromotion" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblPromotion" ADD "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblProductPromotion" ADD "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblProductPromotion" ADD "IsActive" boolean NOT NULL DEFAULT TRUE;

ALTER TABLE "TblProductPromotion" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblProductPromotion" ADD "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblProduct" ALTER COLUMN "UpdatedAt" SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblPayment" ADD "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblPayment" ADD "IsActive" boolean NOT NULL DEFAULT TRUE;

ALTER TABLE "TblPayment" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblPayment" ADD "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblOrderItem" ADD "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblOrderItem" ADD "IsActive" boolean NOT NULL DEFAULT TRUE;

ALTER TABLE "TblOrderItem" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblOrderItem" ADD "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblOrder" ADD "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblOrder" ADD "IsActive" boolean NOT NULL DEFAULT TRUE;

ALTER TABLE "TblOrder" ADD "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblCoupon" ALTER COLUMN "UpdatedAt" SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblCoupon" ALTER COLUMN "IsActive" SET DEFAULT TRUE;

ALTER TABLE "TblCoupon" ALTER COLUMN "CreatedAt" SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblCategory" ALTER COLUMN "UpdatedAt" SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblCategory" ALTER COLUMN "CreatedAt" SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblCartItem" ADD "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblCartItem" ADD "IsActive" boolean NOT NULL DEFAULT TRUE;

ALTER TABLE "TblCartItem" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblCartItem" ADD "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblCart" ADD "IsActive" boolean NOT NULL DEFAULT TRUE;

ALTER TABLE "TblCart" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblBanner" ALTER COLUMN "UpdatedAt" SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "TblAddress" ADD "IsActive" boolean NOT NULL DEFAULT TRUE;

ALTER TABLE "TblAddress" ADD "ModifiedType" text DEFAULT 'ADD';

ALTER TABLE "TblAddress" ADD "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP);

CREATE INDEX idx_user_phone ON "TblUser" ("Phone");

CREATE INDEX idx_product_active ON "TblProduct" ("IsActive");

CREATE INDEX idx_product_code ON "TblProduct" ("Code");

CREATE INDEX idx_order_date ON "TblOrder" ("OrderDate");

CREATE INDEX idx_category_name ON "TblCategory" ("Name");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260123010224_StandardizeCommonFields', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblUser" ADD "LoyaltyPoints" integer NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260124064712_AddLoyaltyPointsAndProductFields', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblUser" ADD "EmailVerificationToken" text;

ALTER TABLE "TblUser" ADD "IsEmailVerified" boolean NOT NULL DEFAULT FALSE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260124070444_AddEmailVerification', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblUser" ADD "PasswordResetToken" text;

ALTER TABLE "TblUser" ADD "ResetTokenExpiry" timestamp with time zone;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260124070735_AddPasswordReset', '8.0.7');

COMMIT;

START TRANSACTION;

DROP INDEX idx_product_code;

ALTER TABLE "TblProduct" DROP COLUMN "Color";

ALTER TABLE "TblProduct" DROP COLUMN "Material";

ALTER TABLE "TblProduct" DROP COLUMN "Power";

ALTER TABLE "TblProduct" DROP COLUMN "Size";

ALTER TABLE "TblProduct" DROP COLUMN "Voltage";

ALTER TABLE "TblProduct" DROP COLUMN "Weight";

ALTER TABLE "TblProduct" ADD "BaseUnit" character varying(50);

ALTER TABLE "TblProduct" ADD "BinLocation" character varying(100);

ALTER TABLE "TblProduct" ADD "BrandCode" character varying(50);

ALTER TABLE "TblProduct" ADD "CountryOfOrigin" character varying(100);

ALTER TABLE "TblProduct" ADD "MinStockLevel" integer;

ALTER TABLE "TblProduct" ADD "VatRate" numeric(5,2);

ALTER TABLE "TblProduct" ADD "WholesalePrice" numeric(15,2);

CREATE TABLE "TblBrand" (
    "Code" character varying(50) NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" text,
    "LogoUrl" text,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblBrand_pkey" PRIMARY KEY ("Code")
);

CREATE TABLE "TblProductDetail" (
    "Code" character varying(50) NOT NULL,
    "ProductCode" character varying(100) NOT NULL,
    "DetailType" text NOT NULL,
    "SpecName" character varying(100) NOT NULL,
    "SpecValue" character varying(255) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblProductDetail_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblProductDetail_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblTag" (
    "Code" character varying(50) NOT NULL,
    "Name" character varying(50) NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblTag_pkey" PRIMARY KEY ("Code")
);

CREATE TABLE "TblUnit" (
    "Code" character varying(50) NOT NULL,
    "ProductCode" character varying(100) NOT NULL,
    "UnitName" character varying(50) NOT NULL,
    "ConversionRate" numeric(10,2) NOT NULL,
    "Price" numeric(15,2) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblUnit_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblUnit_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblProductTag" (
    "Code" character varying(50) NOT NULL,
    "ProductCode" character varying(100) NOT NULL,
    "TagCode" character varying(50) NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblProductTag_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblProductTag_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE,
    CONSTRAINT "TblProductTag_TagCode_fkey" FOREIGN KEY ("TagCode") REFERENCES "TblTag" ("Code") ON DELETE CASCADE
);

CREATE INDEX idx_product_brand ON "TblProduct" ("BrandCode");

CREATE INDEX "IX_TblProductDetail_ProductCode" ON "TblProductDetail" ("ProductCode");

CREATE INDEX "IX_TblProductTag_ProductCode" ON "TblProductTag" ("ProductCode");

CREATE INDEX "IX_TblProductTag_TagCode" ON "TblProductTag" ("TagCode");

CREATE INDEX "IX_TblUnit_ProductCode" ON "TblUnit" ("ProductCode");

ALTER TABLE "TblProduct" ADD CONSTRAINT "FK_TblProduct_TblBrand_BrandCode" FOREIGN KEY ("BrandCode") REFERENCES "TblBrand" ("Code") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260124131159_AddProductDetail', '8.0.7');

COMMIT;

START TRANSACTION;

ALTER TABLE "TblProduct" ADD "BaseUnit" character varying(50);

ALTER TABLE "TblProduct" ADD "BinLocation" character varying(100);

ALTER TABLE "TblProduct" ADD "BrandCode" character varying(50);

ALTER TABLE "TblProduct" ADD "CountryOfOrigin" character varying(100);

ALTER TABLE "TblProduct" ADD "MinStockLevel" integer;

ALTER TABLE "TblProduct" ADD "VatRate" numeric(5,2);

ALTER TABLE "TblProduct" ADD "WholesalePrice" numeric(15,2);

CREATE TABLE "TblBrand" (
    "Code" character varying(50) NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" text,
    "LogoUrl" text,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblBrand_pkey" PRIMARY KEY ("Code")
);

CREATE TABLE "TblProductDetail" (
    "Code" character varying(50) NOT NULL,
    "ProductCode" character varying(100) NOT NULL,
    "DetailType" text NOT NULL,
    "SpecName" character varying(100) NOT NULL,
    "SpecValue" character varying(255) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblProductDetail_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblProductDetail_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblTag" (
    "Code" character varying(50) NOT NULL,
    "Name" character varying(50) NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblTag_pkey" PRIMARY KEY ("Code")
);

CREATE TABLE "TblUnit" (
    "Code" character varying(50) NOT NULL,
    "ProductCode" character varying(100) NOT NULL,
    "UnitName" character varying(50) NOT NULL,
    "ConversionRate" numeric(10,2) NOT NULL,
    "Price" numeric(15,2) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblUnit_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblUnit_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE
);

CREATE TABLE "TblProductTag" (
    "Code" character varying(50) NOT NULL,
    "ProductCode" character varying(100) NOT NULL,
    "TagCode" character varying(50) NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblProductTag_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblProductTag_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE,
    CONSTRAINT "TblProductTag_TagCode_fkey" FOREIGN KEY ("TagCode") REFERENCES "TblTag" ("Code") ON DELETE CASCADE
);

CREATE INDEX idx_product_brand ON "TblProduct" ("BrandCode");

CREATE INDEX "IX_TblProductDetail_ProductCode" ON "TblProductDetail" ("ProductCode");

CREATE INDEX "IX_TblProductTag_ProductCode" ON "TblProductTag" ("ProductCode");

CREATE INDEX "IX_TblProductTag_TagCode" ON "TblProductTag" ("TagCode");

CREATE INDEX "IX_TblUnit_ProductCode" ON "TblUnit" ("ProductCode");

ALTER TABLE "TblProduct" ADD CONSTRAINT "FK_TblProduct_TblBrand_BrandCode" FOREIGN KEY ("BrandCode") REFERENCES "TblBrand" ("Code") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260124143757_EnumConversion', '8.0.7');

COMMIT;


