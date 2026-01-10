CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblCartItem" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblPayment" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblProductImage" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblProductPromotion" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblQuote" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblReview" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblCart" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblOrderItem" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblOrder" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblProduct" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblAddress" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblCoupon" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblSupplier" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblCategory" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblUser" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "TblPromotion" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP TABLE IF EXISTS "__EFMigrationsHistory" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "address_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "cart_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "cartitem_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "category_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "coupon_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "order_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "orderitem_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "payment_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "product_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "productimage_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "productpromotion_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "promotion_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "quote_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "review_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    DROP SEQUENCE IF EXISTS "user_code_seq" CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE address_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE cart_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE cartitem_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE category_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE coupon_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE order_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE orderitem_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE payment_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE product_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE productimage_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE productpromotion_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE promotion_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE quote_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE review_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE SEQUENCE user_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE TABLE "TblCoupon" (
        "Code" character varying(10) NOT NULL DEFAULT (('CPN'::text || lpad((nextval('coupon_code_seq'::regclass))::text, 6, '0'::text))),
        "PromotionCode" character varying(10),
        "UsageCount" integer DEFAULT 0,
        CONSTRAINT "TblCoupon_pkey" PRIMARY KEY ("Code"),
        CONSTRAINT "TblCoupon_PromotionCode_fkey" FOREIGN KEY ("PromotionCode") REFERENCES "TblPromotion" ("Code") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE TABLE "TblCart" (
        "Code" character varying(10) NOT NULL DEFAULT (('CRT'::text || lpad((nextval('cart_code_seq'::regclass))::text, 6, '0'::text))),
        "UserCode" character varying(10) NOT NULL,
        "CreatedAt" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp without time zone DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "TblCart_pkey" PRIMARY KEY ("Code"),
        CONSTRAINT "TblCart_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE TABLE "TblProductPromotion" (
        "Code" character varying(10) NOT NULL DEFAULT (('PPROM'::text || lpad((nextval('productpromotion_code_seq'::regclass))::text, 6, '0'::text))),
        "ProductCode" character varying(10) NOT NULL,
        "PromotionCode" character varying(10) NOT NULL,
        CONSTRAINT "TblProductPromotion_pkey" PRIMARY KEY ("Code"),
        CONSTRAINT "TblProductPromotion_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE,
        CONSTRAINT "TblProductPromotion_PromotionCode_fkey" FOREIGN KEY ("PromotionCode") REFERENCES "TblPromotion" ("Code") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblAddress_UserCode" ON "TblAddress" ("UserCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX idx_cart_user ON "TblCart" ("UserCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblCartItem_CartCode" ON "TblCartItem" ("CartCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblCartItem_ProductCode" ON "TblCartItem" ("ProductCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblCategory_ParentCode" ON "TblCategory" ("ParentCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblCoupon_PromotionCode" ON "TblCoupon" ("PromotionCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX idx_order_status ON "TblOrder" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblOrder_AddressCode" ON "TblOrder" ("AddressCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblOrder_CouponCode" ON "TblOrder" ("CouponCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblOrder_UserCode" ON "TblOrder" ("UserCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblOrderItem_OrderCode" ON "TblOrderItem" ("OrderCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblOrderItem_ProductCode" ON "TblOrderItem" ("ProductCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE UNIQUE INDEX "TblPayment_OrderCode_key" ON "TblPayment" ("OrderCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX idx_product_name ON "TblProduct" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblProduct_CategoryCode" ON "TblProduct" ("CategoryCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblProduct_SupplierCodeNavigationCode" ON "TblProduct" ("SupplierCodeNavigationCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE UNIQUE INDEX "TblProduct_SKU_key" ON "TblProduct" ("SKU");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblProductImage_ProductCode" ON "TblProductImage" ("ProductCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblProductPromotion_PromotionCode" ON "TblProductPromotion" ("PromotionCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE UNIQUE INDEX "TblProductPromotion_ProductCode_PromotionCode_key" ON "TblProductPromotion" ("ProductCode", "PromotionCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblQuote_ProductCode" ON "TblQuote" ("ProductCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblQuote_UserCode" ON "TblQuote" ("UserCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblReview_OrderItemCode" ON "TblReview" ("OrderItemCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE INDEX "IX_TblReview_UserCode" ON "TblReview" ("UserCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE UNIQUE INDEX "TblUser_Email_key" ON "TblUser" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    CREATE UNIQUE INDEX "TblUser_Username_key" ON "TblUser" ("Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108165353_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108165353_InitialCreate', '8.0.7');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    ALTER TABLE "TblProduct" DROP CONSTRAINT "FK_TblProduct_TblSupplier_SupplierCodeNavigationCode";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    DROP INDEX "IX_TblProduct_SupplierCodeNavigationCode";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    ALTER TABLE "TblProduct" DROP COLUMN "SupplierCodeNavigationCode";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    CREATE SEQUENCE banner_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    ALTER TABLE "TblProduct" ALTER COLUMN "SupplierCode" TYPE character varying;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    ALTER TABLE "TblOrderItem" ADD "Color" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    ALTER TABLE "TblOrderItem" ADD "Size" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    ALTER TABLE "TblOrder" ADD "ShippingFee" numeric NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    ALTER TABLE "TblCartItem" ADD "Color" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    ALTER TABLE "TblCartItem" ADD "Size" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    CREATE INDEX "IX_TblProduct_SupplierCode" ON "TblProduct" ("SupplierCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    ALTER TABLE "TblProduct" ADD CONSTRAINT "FK_TblProduct_TblSupplier_SupplierCode" FOREIGN KEY ("SupplierCode") REFERENCES "TblSupplier" ("Code") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260109235725_AddPaymentTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260109235725_AddPaymentTable', '8.0.7');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblUser" ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblUser" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblReview" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblQuote" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblPromotion" ALTER COLUMN "StartDate" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblPromotion" ALTER COLUMN "EndDate" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblProduct" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblPayment" ALTER COLUMN "PaymentDate" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblOrder" ALTER COLUMN "OrderDate" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblCartItem" ALTER COLUMN "AddedAt" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblCart" ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblCart" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblBanner" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    ALTER TABLE "TblAddress" ALTER COLUMN "CreatedAt" TYPE timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110004028_UpdateTimestampsToTz') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260110004028_UpdateTimestampsToTz', '8.0.7');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110113407_AddProductAttributes') THEN
    ALTER TABLE "TblProduct" ADD "Color" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110113407_AddProductAttributes') THEN
    ALTER TABLE "TblProduct" ADD "Material" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110113407_AddProductAttributes') THEN
    ALTER TABLE "TblProduct" ADD "Power" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110113407_AddProductAttributes') THEN
    ALTER TABLE "TblProduct" ADD "Size" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110113407_AddProductAttributes') THEN
    ALTER TABLE "TblProduct" ADD "Voltage" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260110113407_AddProductAttributes') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260110113407_AddProductAttributes', '8.0.7');
    END IF;
END $EF$;
COMMIT;

