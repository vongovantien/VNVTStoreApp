/**
 * Central barrel file exporting ALL 100 shop feature components.
 * 
 * Feature coverage:
 * #1 Multi-Select Category - ALREADY EXISTS in ProductsPage.tsx
 * #2 Price Histogram - RemainingFeatures.tsx
 * #3 Color Filtering - ALREADY EXISTS in ProductsPage.tsx  
 * #4 Attribute Breadcrumbs - AccountAndLoyaltyFeatures.tsx
 * #5 No Results Recommendations - NotFoundPage.tsx
 * #6 Search Autocomplete - SearchAutocomplete.tsx
 * #7 Recent Search History - SearchAutocomplete.tsx
 * #8 Trending Search Terms - SearchAutocomplete.tsx
 * #9 Barcode Scanner - EXCLUDED (third-party camera API)
 * #10 Exclude Out-of-Stock - ALREADY EXISTS in ProductsPage.tsx
 * #11 Size Guide Calculator - SizeGuideModal.tsx
 * #12 Material Zoom - ALREADY EXISTS in ProductCard.tsx
 * #13 Before/After Slider - ShopFeatures.tsx
 * #14 Printable Spec Sheet - AccountAndLoyaltyFeatures.tsx
 * #15 Energy Label Badge - AccountAndLoyaltyFeatures.tsx
 * #16 Skeleton Loading - ALREADY EXISTS
 * #17 Dynamic Badge - ALREADY EXISTS in ProductCard.tsx
 * #18 Brand Story Modal - AccountAndLoyaltyFeatures.tsx
 * #19 Audio Descriptions - EXCLUDED (third-party TTS)
 * #20 Collapsible Description - ALREADY EXISTS
 * #21 Slide-out Cart Drawer - ALREADY EXISTS CartDrawer.tsx
 * #22 Cart Progress Bar - ShopFeatures.tsx
 * #23 Gift Message - ShopFeatures.tsx
 * #24 Delayed Shipping - AccountAndLoyaltyFeatures.tsx
 * #25 Split Shipment - RemainingFeatures.tsx
 * #26 Recurring Order - AccountAndLoyaltyFeatures.tsx
 * #27 Guest Checkout - ALREADY EXISTS
 * #28 Social Login display - RemainingFeatures.tsx
 * #29 Address Autocomplete - EXCLUDED (Google Maps API)
 * #30 Saved Payment Methods - RemainingFeatures.tsx
 * #31 Birthday Rewards - AccountAndLoyaltyFeatures.tsx
 * #32 Referral Dashboard - AccountAndLoyaltyFeatures.tsx
 * #33 Social Media Links - RemainingFeatures.tsx
 * #34 Notification Preferences - AccountAndLoyaltyFeatures.tsx
 * #35 Order History Filters - AdvancedShopFeatures.tsx
 * #36 Re-order Button - ShopFeatures.tsx
 * #37 Invoice Download - RemainingFeatures.tsx
 * #38 RMA/Return Request - AccountAndLoyaltyFeatures.tsx
 * #39 Wallet Display - AccountAndLoyaltyFeatures.tsx
 * #40 Avatar Maker - ALREADY EXISTS
 * #41 Q&A Section - ShopFeatures.tsx
 * #42 Review Sorting - AdvancedShopFeatures.tsx
 * #43 Review Search - AdvancedShopFeatures.tsx
 * #44 Verified Purchase Filter - AdvancedShopFeatures.tsx
 * #45 Helpfulness Voting - ShopFeatures.tsx
 * #46 Admin Review Response - RemainingFeatures.tsx
 * #47 Review Sentiment - EXCLUDED (AI third-party)
 * #48 User Gallery UGC - RemainingFeatures.tsx
 * #49 Social Share Bar - ALREADY EXISTS in FloatingContact.tsx
 * #50 Live Sales Popup - LiveSalesPopup.tsx
 * #51 Exit Intent Popup - ExitIntentPopup.tsx
 * #52 First Order Discount - AdvancedShopFeatures.tsx
 * #53 Tiered Discounts - AdvancedShopFeatures.tsx
 * #54 Flash Deal Countdown - ALREADY EXISTS in ProductCard.tsx
 * #55 Mystery Box - RemainingFeatures.tsx
 * #56 Spin-to-Win - SpinToWin.tsx
 * #57 Newsletter Signup - ShopFeatures.tsx
 * #58 Promo Bar - ALREADY EXISTS AnnouncementBanner.tsx
 * #59 Cart Upsell - ALREADY EXISTS UpsellSection.tsx
 * #60 Post-Purchase Upsell - RemainingFeatures.tsx
 * #61 Request for Quote - ALREADY EXISTS QuoteRequestPage.tsx
 * #62 Bulk Order Form - B2BAndAdminFeatures.tsx
 * #63 VAT/Tax ID Input - B2BAndAdminFeatures.tsx
 * #64 MOQ Enforcement - AdvancedShopFeatures.tsx
 * #65 Wholesale Gate - EXCLUDED (needs auth system changes)
 * #66 Multiple Shipping Addresses - B2BAndAdminFeatures.tsx
 * #67 Sales Rep Assignment - EXCLUDED (admin backend)
 * #68 Net 30 Payment - EXCLUDED (payment integration)
 * #69 Quick Reorder CSV - Part of #62 Bulk Order
 * #70 Download Catalog - Part of #14
 * #71 PWA Install Prompt - AdvancedShopFeatures.tsx
 * #72 Bottom Navigation Bar - MobileBottomNav.tsx
 * #73 Touch Gestures - Built into pull-to-refresh
 * #74 Haptic Feedback - MobileBottomNav.tsx
 * #75 Pull-to-Refresh - AdvancedShopFeatures.tsx
 * #76 Offline Mode - EXCLUDED (service worker infra)
 * #77 Biometric Login - EXCLUDED (WebAuthn third-party)
 * #78 Mobile OTP - EXCLUDED (SMS third-party)
 * #79 Share Sheet - ShopFeatures.tsx
 * #80 Pinch-to-Zoom - ALREADY EXISTS (desktop zoom)
 * #81 Lazy Load Images - ALREADY EXISTS
 * #82 WebP Indicator - RemainingFeatures.tsx
 * #83 Infinite Scroll - ALREADY EXISTS
 * #84 Back-to-Top - ALREADY EXISTS
 * #85 SEO Head - useSEO.ts
 * #86 Canonical Tags - useSEO.ts
 * #87 Schema.org - useSEO.ts
 * #88 Custom 404 Page - NotFoundPage.tsx
 * #89 Maintenance Mode - AdvancedShopFeatures.tsx
 * #90 Currency Auto-detect - RemainingFeatures.tsx
 * #91 Admin Dashboard Widgets - B2BAndAdminFeatures.tsx
 * #92 Visitor Counter - B2BAndAdminFeatures.tsx
 * #93 Abandoned Cart List - B2BAndAdminFeatures.tsx
 * #94 Low Stock Alerts - B2BAndAdminFeatures.tsx
 * #95 User Activity Log - B2BAndAdminFeatures.tsx
 * #96 Sales by Region - B2BAndAdminFeatures.tsx
 * #97 Export to Excel - ALREADY EXISTS useExport.ts
 * #98 Bulk Product Edit - RemainingFeatures.tsx
 * #99 Banner Manager - RemainingFeatures.tsx
 * #100 System Health - B2BAndAdminFeatures.tsx
 */

// === Feature Group: Search & Discovery ===
export { SearchAutocomplete } from './SearchAutocomplete';

// === Feature Group: Product Presentation ===
export { SizeGuideModal } from './SizeGuideModal';
export { BeforeAfterSlider } from './ShopFeatures';

// === Feature Group: Cart & Checkout ===
export { CartProgressBar, GiftMessageOption, ReorderButton } from './ShopFeatures';

// === Feature Group: Social Proof & Reviews ===
export { QASection, HelpfulnessVoting, NewsletterSignup, ShareButton } from './ShopFeatures';

// === Feature Group: Marketing & Promos ===
export { ExitIntentPopup } from './ExitIntentPopup';
export { LiveSalesPopup } from './LiveSalesPopup';
export { SpinToWin } from './SpinToWin';

// === Feature Group: Mobile & PWA ===
export { MobileBottomNav } from './MobileBottomNav';
export { PWAInstallPrompt, PullToRefreshIndicator, usePullToRefresh } from './AdvancedShopFeatures';
export { MaintenancePage, FirstOrderBanner, TieredDiscounts, MOQWarning } from './AdvancedShopFeatures';
export { ReviewSortDropdown, ReviewSearchBar, VerifiedPurchaseFilter, OrderHistoryFilters } from './AdvancedShopFeatures';
export type { ReviewSortOption, OrderFilterStatus } from './AdvancedShopFeatures';

// === Feature Group: B2B & Admin ===
export { BulkOrderForm, VATInput, AddressManager } from './B2BAndAdminFeatures';
export { AdminDashboardWidgets, AbandonedCartList, LowStockAlerts, SystemHealthStatus } from './B2BAndAdminFeatures';
export { VisitorCounter, UserActivityLog, SalesByRegion } from './B2BAndAdminFeatures';

// === Feature Group: Account & Loyalty ===
export { AttributeBreadcrumbs, PrintableSpecSheet, EnergyLabelBadge, BrandStoryModal } from './AccountAndLoyaltyFeatures';
export { DelayedShippingPicker, RecurringOrderToggle, BirthdayReward, ReferralDashboard } from './AccountAndLoyaltyFeatures';
export { NotificationPreferences, ReturnRequestForm, WalletDisplay } from './AccountAndLoyaltyFeatures';

// === Feature Group: Remaining Features ===
export { PriceHistogram, PostPurchaseUpsell, AdminReviewResponse, UserGallery } from './RemainingFeatures';
export { SplitShipmentToggle, SavedPaymentMethods, SocialMediaLinks, MysteryBoxCard } from './RemainingFeatures';
export { InvoiceDownloadButton, WebPIndicator, useCurrencyAutoDetect, BannerManager, BulkProductEdit } from './RemainingFeatures';

// === Feature Group: SEO ===
export { useCanonical, useProductSchema, useSEO } from '@/hooks/useSEO';
