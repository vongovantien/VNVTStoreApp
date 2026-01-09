CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

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

