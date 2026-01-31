CREATE SEQUENCE address_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE banner_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE cart_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE cartitem_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE category_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE coupon_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE file_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE order_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE orderitem_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE payment_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE product_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE productpromotion_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE promotion_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE quote_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE review_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE SEQUENCE user_code_seq START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;


CREATE TABLE "TblBanner" (
    "Code" character varying(100) NOT NULL DEFAULT (('BNN'::text || lpad((nextval('banner_code_seq'::regclass))::text, 6, '0'::text))),
    "Title" character varying(200) NOT NULL,
    "Content" character varying(500),
    "LinkUrl" character varying(200),
    "LinkText" character varying(50),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "Priority" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblBanner_pkey" PRIMARY KEY ("Code")
);


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


CREATE TABLE "TblCategory" (
    "Code" character varying(100) NOT NULL DEFAULT (('CAT'::text || lpad((nextval('category_code_seq'::regclass))::text, 6, '0'::text))),
    "Name" character varying(100) NOT NULL,
    "Description" text,
    "ParentCode" character varying(100),
    "ImageURL" character varying(255),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "ModifiedType" text DEFAULT 'ADD',
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "TblCategory_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblCategory_ParentCode_fkey" FOREIGN KEY ("ParentCode") REFERENCES "TblCategory" ("Code") ON DELETE SET NULL
);


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
    "MasterCode" character varying(100),
    "MasterType" character varying(50),
    CONSTRAINT "TblFile_pkey" PRIMARY KEY ("Code")
);


CREATE TABLE "TblPromotion" (
    "Code" character varying(100) NOT NULL DEFAULT (('PROM'::text || lpad((nextval('promotion_code_seq'::regclass))::text, 6, '0'::text))),
    "Name" character varying(100) NOT NULL,
    "Description" text,
    "DiscountType" character varying(20) NOT NULL,
    "DiscountValue" numeric(15,2) NOT NULL,
    "MinOrderAmount" numeric(15,2),
    "MaxDiscountAmount" numeric(15,2),
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone NOT NULL,
    "UsageLimit" integer,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "ModifiedType" text DEFAULT 'ADD',
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "TblPromotion_pkey" PRIMARY KEY ("Code")
);


CREATE TABLE "TblSupplier" (
    "Code" character varying NOT NULL,
    "Name" character varying NOT NULL,
    "ContactPerson" character varying,
    "Email" character varying,
    "ModifiedType" text DEFAULT 'ADD',
    "Phone" character varying,
    "Address" character varying,
    "TaxCode" character varying,
    "BankAccount" character varying,
    "BankName" character varying,
    "Notes" text,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "TblSupplier_pkey" PRIMARY KEY ("Code")
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
    "Name" character varying(50) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblUnit_pkey" PRIMARY KEY ("Code")
);


CREATE TABLE "TblUser" (
    "Code" character varying(100) NOT NULL DEFAULT (('USR'::text || lpad((nextval('user_code_seq'::regclass))::text, 6, '0'::text))),
    "Username" character varying(50) NOT NULL,
    "Email" character varying(100) NOT NULL,
    "PasswordHash" character varying(255) NOT NULL,
    "FullName" character varying(100),
    "Phone" character varying(15),
    "Role" character varying(20) NOT NULL DEFAULT ('customer'::character varying),
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "LastLogin" timestamp with time zone,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "ModifiedType" text DEFAULT 'Add',
    "RefreshToken" text,
    "RefreshTokenExpiryTime" timestamp with time zone,
    "LoyaltyPoints" integer NOT NULL,
    "EmailVerificationToken" text,
    "IsEmailVerified" boolean NOT NULL,
    "PasswordResetToken" text,
    "ResetTokenExpiry" timestamp with time zone,
    "AvatarUrl" character varying(1000),
    CONSTRAINT "TblUser_pkey" PRIMARY KEY ("Code")
);


CREATE TABLE "TblCoupon" (
    "Code" character varying(100) NOT NULL DEFAULT (('CPN'::text || lpad((nextval('coupon_code_seq'::regclass))::text, 6, '0'::text))),
    "PromotionCode" character varying(100),
    "UsageCount" integer DEFAULT 0,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "ModifiedType" text DEFAULT 'ADD',
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "TblCoupon_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblCoupon_PromotionCode_fkey" FOREIGN KEY ("PromotionCode") REFERENCES "TblPromotion" ("Code") ON DELETE SET NULL
);


CREATE TABLE "TblProduct" (
    "Code" character varying(100) NOT NULL DEFAULT (('PRD'::text || lpad((nextval('product_code_seq'::regclass))::text, 6, '0'::text))),
    "Name" character varying(100) NOT NULL,
    "Description" text,
    "Price" numeric(15,2) NOT NULL,
    "WholesalePrice" numeric(15,2),
    "CostPrice" numeric(15,2),
    "StockQuantity" integer DEFAULT 0,
    "CategoryCode" character varying(100),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    "SupplierCode" character varying,
    "BrandCode" character varying(50),
    "BaseUnit" character varying(50),
    "MinStockLevel" integer,
    "BinLocation" character varying(100),
    "VatRate" numeric(5,2),
    "CountryOfOrigin" character varying(100),
    CONSTRAINT "TblProduct_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "FK_TblProduct_TblBrand_BrandCode" FOREIGN KEY ("BrandCode") REFERENCES "TblBrand" ("Code") ON DELETE SET NULL,
    CONSTRAINT "FK_TblProduct_TblSupplier_SupplierCode" FOREIGN KEY ("SupplierCode") REFERENCES "TblSupplier" ("Code") ON DELETE SET NULL,
    CONSTRAINT "TblProduct_CategoryCode_fkey" FOREIGN KEY ("CategoryCode") REFERENCES "TblCategory" ("Code") ON DELETE SET NULL
);


CREATE TABLE "TblAddress" (
    "Code" character varying(100) NOT NULL DEFAULT (('ADR'::text || lpad((nextval('address_code_seq'::regclass))::text, 6, '0'::text))),
    "UserCode" character varying(100) NOT NULL,
    "AddressLine" character varying(255) NOT NULL,
    "FullName" character varying(100),
    "Phone" character varying(20),
    "Category" character varying(50),
    "City" character varying(50),
    "State" character varying(50),
    "PostalCode" character varying(100),
    "Country" character varying(50) DEFAULT ('Vietnam'::character varying),
    "IsDefault" boolean DEFAULT FALSE,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    CONSTRAINT "TblAddress_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblAddress_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code") ON DELETE CASCADE
);


CREATE TABLE "TblCart" (
    "Code" character varying(100) NOT NULL DEFAULT (('CRT'::text || lpad((nextval('cart_code_seq'::regclass))::text, 6, '0'::text))),
    "UserCode" character varying(100) NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    CONSTRAINT "TblCart_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblCart_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code") ON DELETE CASCADE
);


CREATE TABLE "TblUserLogin" (
    "LoginProvider" character varying(50) NOT NULL,
    "ProviderKey" character varying(255) NOT NULL,
    "ProviderDisplayName" character varying(100),
    "UserId" character varying(100) NOT NULL,
    CONSTRAINT "TblUserLogin_pkey" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "TblUserLogin_UserId_fkey" FOREIGN KEY ("UserId") REFERENCES "TblUser" ("Code") ON DELETE CASCADE
);


CREATE TABLE "TblProductDetail" (
    "Code" character varying(50) NOT NULL,
    "ProductCode" character varying(100) NOT NULL,
    "DetailType" character varying(50) NOT NULL,
    "SpecName" character varying(100) NOT NULL,
    "SpecValue" character varying(255) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblProductDetail_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblProductDetail_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE
);


CREATE TABLE "TblProductPromotion" (
    "Code" character varying(100) NOT NULL DEFAULT (('PPROM'::text || lpad((nextval('productpromotion_code_seq'::regclass))::text, 6, '0'::text))),
    "ProductCode" character varying(100) NOT NULL,
    "PromotionCode" character varying(100) NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    CONSTRAINT "TblProductPromotion_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblProductPromotion_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE,
    CONSTRAINT "TblProductPromotion_PromotionCode_fkey" FOREIGN KEY ("PromotionCode") REFERENCES "TblPromotion" ("Code") ON DELETE CASCADE
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


CREATE TABLE "TblProductUnit" (
    "Code" character varying(50) NOT NULL,
    "ProductCode" character varying(100) NOT NULL,
    "UnitCode" character varying(50) NOT NULL,
    "ConversionRate" numeric(10,2) NOT NULL,
    "Price" numeric(15,2) NOT NULL,
    "IsBaseUnit" boolean NOT NULL DEFAULT FALSE,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblProductUnit_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblProductUnit_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE,
    CONSTRAINT "TblProductUnit_UnitCode_fkey" FOREIGN KEY ("UnitCode") REFERENCES "TblUnit" ("Code") ON DELETE RESTRICT
);


CREATE TABLE "TblQuote" (
    "Code" character varying(100) NOT NULL DEFAULT (('QT'::text || lpad((nextval('quote_code_seq'::regclass))::text, 6, '0'::text))),
    "UserCode" character varying(100),
    "CustomerName" text,
    "CustomerEmail" text,
    "CustomerPhone" text,
    "ProductCode" character varying(100) NOT NULL,
    "Quantity" integer NOT NULL DEFAULT 1,
    "Note" text,
    "QuotedPrice" numeric(15,2),
    "AdminNote" text,
    "Status" character varying(20) NOT NULL DEFAULT ('pending'::character varying),
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblQuote_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblQuote_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE,
    CONSTRAINT "TblQuote_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code")
);


CREATE TABLE "TblOrder" (
    "Code" character varying(100) NOT NULL DEFAULT (('ORD'::text || lpad((nextval('order_code_seq'::regclass))::text, 6, '0'::text))),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UserCode" character varying(100) NOT NULL,
    "ModifiedType" text DEFAULT 'ADD',
    "OrderDate" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "TotalAmount" numeric(15,2) NOT NULL,
    "ShippingFee" numeric NOT NULL,
    "DiscountAmount" numeric(15,2) DEFAULT (0),
    "FinalAmount" numeric(15,2) NOT NULL,
    "Status" character varying(20) NOT NULL DEFAULT ('pending'::character varying),
    "AddressCode" character varying(100),
    "CouponCode" character varying(100),
    "VerificationToken" text,
    "VerificationTokenExpiresAt" timestamp with time zone,
    CONSTRAINT "TblOrder_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblOrder_AddressCode_fkey" FOREIGN KEY ("AddressCode") REFERENCES "TblAddress" ("Code"),
    CONSTRAINT "TblOrder_CouponCode_fkey" FOREIGN KEY ("CouponCode") REFERENCES "TblCoupon" ("Code"),
    CONSTRAINT "TblOrder_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code") ON DELETE CASCADE
);


CREATE TABLE "TblCartItem" (
    "Code" character varying(100) NOT NULL DEFAULT (('CRI'::text || lpad((nextval('cartitem_code_seq'::regclass))::text, 6, '0'::text))),
    "CartCode" character varying(100) NOT NULL,
    "ProductCode" character varying(100) NOT NULL,
    "Quantity" integer NOT NULL DEFAULT 1,
    "Size" text,
    "Color" text,
    "AddedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    CONSTRAINT "TblCartItem_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblCartItem_CartCode_fkey" FOREIGN KEY ("CartCode") REFERENCES "TblCart" ("Code") ON DELETE CASCADE,
    CONSTRAINT "TblCartItem_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE CASCADE
);


CREATE TABLE "TblOrderItem" (
    "Code" character varying(100) NOT NULL DEFAULT (('ORI'::text || lpad((nextval('orderitem_code_seq'::regclass))::text, 6, '0'::text))),
    "OrderCode" character varying(100) NOT NULL,
    "ProductCode" character varying(100),
    "Quantity" integer NOT NULL,
    "Size" text,
    "Color" text,
    "ProductName" text,
    "ProductImage" text,
    "PriceAtOrder" numeric(15,2) NOT NULL,
    "DiscountAmount" numeric(15,2) DEFAULT (0),
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    CONSTRAINT "TblOrderItem_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblOrderItem_OrderCode_fkey" FOREIGN KEY ("OrderCode") REFERENCES "TblOrder" ("Code") ON DELETE CASCADE,
    CONSTRAINT "TblOrderItem_ProductCode_fkey" FOREIGN KEY ("ProductCode") REFERENCES "TblProduct" ("Code") ON DELETE SET NULL
);


CREATE TABLE "TblPayment" (
    "Code" character varying(100) NOT NULL DEFAULT (('PAY'::text || lpad((nextval('payment_code_seq'::regclass))::text, 6, '0'::text))),
    "OrderCode" character varying(100) NOT NULL,
    "PaymentDate" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "ModifiedType" text DEFAULT 'ADD',
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "Amount" numeric(15,2) NOT NULL,
    "Method" character varying(50) NOT NULL,
    "TransactionID" character varying(100),
    "Status" character varying(20) NOT NULL DEFAULT ('pending'::character varying),
    CONSTRAINT "TblPayment_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblPayment_OrderCode_fkey" FOREIGN KEY ("OrderCode") REFERENCES "TblOrder" ("Code") ON DELETE CASCADE
);


CREATE TABLE "TblReview" (
    "Code" character varying(100) NOT NULL DEFAULT (('REV'::text || lpad((nextval('review_code_seq'::regclass))::text, 6, '0'::text))),
    "OrderItemCode" character varying(100),
    "UserCode" character varying(100) NOT NULL,
    "Rating" integer,
    "Comment" text,
    "CreatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone DEFAULT (CURRENT_TIMESTAMP),
    "IsApproved" boolean DEFAULT FALSE,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "ModifiedType" text DEFAULT 'ADD',
    CONSTRAINT "TblReview_pkey" PRIMARY KEY ("Code"),
    CONSTRAINT "TblReview_OrderItemCode_fkey" FOREIGN KEY ("OrderItemCode") REFERENCES "TblOrderItem" ("Code"),
    CONSTRAINT "TblReview_UserCode_fkey" FOREIGN KEY ("UserCode") REFERENCES "TblUser" ("Code")
);


CREATE INDEX idx_address_user ON "TblAddress" ("UserCode");


CREATE INDEX idx_cart_user ON "TblCart" ("UserCode");


CREATE INDEX "IX_TblCartItem_CartCode" ON "TblCartItem" ("CartCode");


CREATE INDEX "IX_TblCartItem_ProductCode" ON "TblCartItem" ("ProductCode");


CREATE INDEX idx_category_name ON "TblCategory" ("Name");


CREATE INDEX idx_category_parent ON "TblCategory" ("ParentCode");


CREATE INDEX "IX_TblCoupon_PromotionCode" ON "TblCoupon" ("PromotionCode");


CREATE INDEX idx_file_mastercode ON "TblFile" ("MasterCode");


CREATE INDEX idx_order_date ON "TblOrder" ("OrderDate");


CREATE INDEX idx_order_status ON "TblOrder" ("Status");


CREATE INDEX idx_order_user ON "TblOrder" ("UserCode");


CREATE INDEX "IX_TblOrder_AddressCode" ON "TblOrder" ("AddressCode");


CREATE INDEX "IX_TblOrder_CouponCode" ON "TblOrder" ("CouponCode");


CREATE INDEX "IX_TblOrderItem_OrderCode" ON "TblOrderItem" ("OrderCode");


CREATE INDEX "IX_TblOrderItem_ProductCode" ON "TblOrderItem" ("ProductCode");


CREATE UNIQUE INDEX "TblPayment_OrderCode_key" ON "TblPayment" ("OrderCode");


CREATE INDEX idx_product_active ON "TblProduct" ("IsActive");


CREATE INDEX idx_product_brand ON "TblProduct" ("BrandCode");


CREATE INDEX idx_product_category ON "TblProduct" ("CategoryCode");


CREATE INDEX idx_product_name ON "TblProduct" ("Name");


CREATE INDEX idx_product_supplier ON "TblProduct" ("SupplierCode");


CREATE INDEX "IX_TblProductDetail_ProductCode" ON "TblProductDetail" ("ProductCode");


CREATE INDEX "IX_TblProductPromotion_PromotionCode" ON "TblProductPromotion" ("PromotionCode");


CREATE UNIQUE INDEX "TblProductPromotion_ProductCode_PromotionCode_key" ON "TblProductPromotion" ("ProductCode", "PromotionCode");


CREATE INDEX "IX_TblProductTag_ProductCode" ON "TblProductTag" ("ProductCode");


CREATE INDEX "IX_TblProductTag_TagCode" ON "TblProductTag" ("TagCode");


CREATE INDEX "IX_TblProductUnit_ProductCode" ON "TblProductUnit" ("ProductCode");


CREATE INDEX "IX_TblProductUnit_UnitCode" ON "TblProductUnit" ("UnitCode");


CREATE INDEX "IX_TblQuote_ProductCode" ON "TblQuote" ("ProductCode");


CREATE INDEX "IX_TblQuote_UserCode" ON "TblQuote" ("UserCode");


CREATE INDEX "IX_TblReview_OrderItemCode" ON "TblReview" ("OrderItemCode");


CREATE INDEX "IX_TblReview_UserCode" ON "TblReview" ("UserCode");


CREATE INDEX idx_user_phone ON "TblUser" ("Phone");


CREATE UNIQUE INDEX "TblUser_Email_key" ON "TblUser" ("Email");


CREATE UNIQUE INDEX "TblUser_Username_key" ON "TblUser" ("Username");


CREATE INDEX "IX_TblUserLogin_UserId" ON "TblUserLogin" ("UserId");


