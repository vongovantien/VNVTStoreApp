# Kiểm tra FE vs BE – Clean Architecture, SOLID & Best Practices

Báo cáo so sánh codebase Frontend (React/TS) và Backend (.NET) với các nguyên tắc bạn đã nêu, **vi phạm** và **đề xuất cải thiện**.

---

## 1. Clean Architecture & Design Patterns

### 1.1 Dependency Rule – Sự phụ thuộc chỉ hướng vào tâm

| Kiểm tra | Backend | Frontend |
|----------|---------|----------|
| **Hướng phụ thuộc** | API → Application + **Infrastructure** | N/A (SPA, không tách layer vật lý) |

**Backend – Vi phạm:**

1. **API phụ thuộc trực tiếp Infrastructure**
   - `VNVTStore.API.csproj` reference `VNVTStore.Infrastructure`.
   - `Program.cs`: `using VNVTStore.Infrastructure`, `app.MapHub<VNVTStore.Infrastructure.Hubs.NotificationHub>`.
   - Controllers: `PermissionsController`, `RolesController` dùng `VNVTStore.Infrastructure.Authorization`; **`SystemController` inject và dùng `ApplicationDbContext`** (concrete từ Infrastructure).

2. **Application phụ thuộc EF Core**
   - `VNVTStore.Application` reference `Microsoft.EntityFrameworkCore`.
   - `IApplicationDbContext` đã thuần: chỉ `IQueryable<T>` + Add/Remove/SaveChangesAsync (đã xửa) – interface nằm ở Application nhưng “dính” khái niệm EF (DbSet, SaveChangesAsync). Theo Clean Architecture thuần, Use Cases không nên biết EF.

**Đề xuất:**

- **API:** Chỉ đăng ký DI và map Hub trong composition root; **không** dùng type từ Infrastructure trong controller. `SystemController` nên dùng **`IApplicationDbContext`** (và logic đếm/chart nên đưa vào Application qua MediatR Query/Handler) thay vì inject `ApplicationDbContext` và truy vấn trực tiếp.
- **Application:** Giữ `IApplicationDbContext` nhưng cân nhắc:
  - Hoặc tách interface “thuần” (chỉ `IQueryable<T>`, `SaveChanges`) trong Application, để Infrastructure implement và map sang EF; hoặc
  - Chấp nhận “pragmatic” là Application biết `DbSet` nhưng **không** reference package EF – khi đó cần đưa interface vào project trung gian hoặc dùng abstraction khác (ví dụ `IQueryableProvider`).

---

### 1.2 Entities & Use Cases – Dữ liệu vs hành động nghiệp vụ

| Kiểm tra | Backend | Frontend |
|----------|---------|----------|
| **Entities** | Domain có `Tbl*` entities, Application có DTOs | Types trong `src/types`, DTO từ API |
| **Use Cases** | MediatR Commands/Queries + Handlers | Custom hooks (`useProducts`, `useOrders`, …) gọi services |

**Backend – Ổn:** Domain entities, Application DTOs, Handlers = use cases.  
**Frontend – Ổn:** Hooks bọc logic gọi API, components chỉ dùng hooks/services.

---

### 1.3 Interface Adapters – Mappers, Presenters, Repositories

| Kiểm tra | Backend | Frontend |
|----------|---------|----------|
| **Mappers** | AutoMapper, `MappingProfile` (Entity ↔ DTO) | Không mapper tầng – DTO từ API dùng trực tiếp |
| **Repositories** | `IRepository<T>` (Domain), implement trong Infrastructure | Không (API = boundary) |

**Backend – Ổn:** Repository pattern, AutoMapper cho Entity ↔ DTO.  
**Frontend – Ghi chú:** DTO từ API dùng trực tiếp; nếu sau này có “domain model” khác API thì nên thêm mapper (ví dụ DTO → ViewModel).

---

### 1.4 Dependency Injection & IoC

| Kiểm tra | Backend | Frontend |
|----------|---------|----------|
| **DI** | `AddApplication()`, `AddInfrastructure()`, MediatR, AutoMapper | React context + Zustand; services/hooks dùng trực tiếp |

**Backend – Ổn:** DI container, interface-based injection.  
**Frontend – Ổn:** Context + store; không bắt buộc DI container cho SPA.

---

### 1.5 Repository & Data Mapper

- **Backend:** Repository (IRepository + implement), Data Mapper (AutoMapper Entity ↔ DTO) – **đạt**.
- **Frontend:** Không repository; “data” = server state (TanStack Query) + UI state (Zustand) – **hợp lý**.

---

## 2. SOLID & Coding Principles

### 2.1 SRP (Single Responsibility)

| Nơi | Vi phạm / Rủi ro |
|-----|-------------------|
| **BE** | `SystemController`: vừa debug dashboard vừa đếm/chart trực tiếp từ DbContext – nên tách thành Query/Handler. Một số Handler file rất dài (ví dụ `BaseHandler`, `QueryHelper`) – có thể tách helper/strategy nhỏ hơn. |
| **FE** | Một số page (VD: `ProductsPage`, `OrdersPage`) vừa filter/pagination vừa table vừa modal – có thể tách thành sub-components hoặc hooks (useProductFilters, useOrderTable). |

### 2.2 OCP (Open/Closed)

- **BE:** Mở rộng bằng thêm Command/Query/Handler, Validator, mapping – **ổn**. Strategy (ConditionBuilder, Shipping) cho phép mở rộng không sửa code cũ.
- **FE:** Thêm feature thường là thêm page/hook/service – **ổn**. Form/table dùng config (columns, fields) – tránh sửa core.

### 2.3 LSP / ISP / DIP

- **LSP:** Không thấy vi phạm rõ (concrete repository, handlers thay thế được qua interface).
- **ISP:** Application có nhiều interface nhỏ (ICurrentUser, IJwtService, ICacheService, …) – **tốt**. `IApplicationDbContext` khá “béo” (nhiều DbSet) – có thể tách theo aggregate nếu cần.
- **DIP:** Application phụ thuộc Domain + interfaces; **vi phạm** là Application (và API) phụ thuộc Infrastructure/EF cụ thể ở vài chỗ (đã nêu trên).

### 2.4 KISS, DRY, YAGNI

- **BE:** Có DRY tốt (BaseHandler, QueryHelper, FluentValidation). Tránh over-engineer thêm layer nếu không cần.
- **FE:** Barrel files (`services/index.ts`, `hooks/index.ts`), shared `AsyncState`, `createSchemas` – **DRY tốt**. Một số form/table lặp pattern – có thể gom vào `useEntityManager`/DataTable hơn nữa.

### 2.5 Composition over Inheritance

- **BE:** Handlers kế thừa `BaseHandler`; có thể giảm inheritance bằng composition (inject helper services thay vì base class quá lớn).
- **FE:** Ưu tiên composition (components nhỏ, hooks) – **ổn**.

---

## 3. React & TypeScript Advanced

### 3.1 Custom Hooks as Use Case Wrappers

| Kiểm tra | Kết quả |
|----------|--------|
| **Hooks bọc logic** | `useProducts`, `useOrders`, `useEntityManager`, `useDataTable`, … – **tốt** |
| **Component gọn** | Page thường dùng hook + render – **ổn** |

### 3.2 Compound Components / Render Props / HOCs

- **DataTable, BaseForm:** Cấu hình qua props (columns, fields) – linh hoạt.
- Chưa thấy pattern Compound Components (VD: `<DataTable><DataTable.Header />...</DataTable>`) – không bắt buộc, chỉ cần nhất quán.

### 3.3 Discriminated Unions & Utility Types

| Kiểm tra | Kết quả |
|----------|--------|
| **AsyncState** | `AsyncState<T>` = `idle \| loading \| success \| error` + type guards (`isLoading`, `isSuccess`, …) – **đúng Discriminated Union** |
| **Utility Types** | Có dùng `Pick`, `Omit`, `Partial` trong types – **ổn** |

### 3.4 Zod / Yup

- **Zod:** Dùng trong `utils/schemas.ts` (`createSchemas`, `zField`) cho validation form – **tốt**.
- Có thể mở rộng Zod cho payload API (parse response) để đảm bảo dữ liệu đầu vào sạch.

---

## 4. Performance & State Management

### 4.1 Memoization

- **useMemo / useCallback:** Đã dùng trong nhiều file (DataTable, ProductCard, form, hooks) – **tốt**.
- **React.memo:** Có dùng cho một số component – có thể áp dụng thêm cho list item (ProductCard, table row) nếu re-render nhiều.

### 4.2 Code Splitting

- **React.lazy + Suspense:** Router dùng `lazy(() => import('@/pages/...'))` và `Suspense` với `PageLoader` – **tốt**.
- **Vite manualChunks:** Đã cấu hình `vendor`, `store`, `animations`, `i18n` – **tốt**.

### 4.3 Server State vs UI State

- **TanStack Query:** Dùng `useQuery`/`useMutation` trong hooks – server state rõ ràng.
- **Zustand:** Auth, cart, filter, review store – UI/local state.
- **Tách biệt:** Server state (Query) và UI state (Zustand) không trộn lẫn – **đúng hướng**.

### 4.4 Atomic State (Zustand)

- Đã dùng Zustand (store slices: auth, cart, …) – **nhẹ, phù hợp**.

---

## 5. Folder Structure & Tooling

### 5.1 Screaming Architecture

| Kiểm tra | Backend | Frontend |
|----------|---------|----------|
| **Cấu trúc theo tính năng** | Application theo feature (Orders, Products, Auth, …) | `pages/shop`, `pages/admin`, `pages/auth`; `components/common`, `components/ui` – **rõ** |
| **Feature folder** | Có (Orders, Products, …) | Có `@features/*` alias nhưng chưa thấy folder `features/` – có thể chuyển một số module thành `features/product`, `features/order` |

### 5.2 Barrel Files & Path Aliases

| Kiểm tra | Kết quả |
|----------|--------|
| **Barrel** | `services/index.ts`, `hooks/index.ts`, `components/common/index.ts`, `components/ui/index.ts` – **có** |
| **Path aliases** | `@/`, `@components/`, `@pages/`, `@hooks/`, … trong tsconfig + vite – **đã cấu hình** |
| **Dùng alias** | Nhiều file dùng `@/`; vẫn còn import tương đối `../` trong một số file – nên **thống nhất dùng alias** để tránh `../../..` |

### 5.3 ESLint & Prettier & Husky

| Công cụ | Hiện trạng | Đề xuất |
|---------|------------|---------|
| **ESLint** | Đã cấu hình (flat config, TypeScript, React, Testing Library) | Giữ, có thể bật thêm rule strict (VD: `@typescript-eslint/strict`) |
| **Prettier** | **Chưa** có trong `package.json` | Thêm Prettier, tích hợp ESLint (`eslint-config-prettier`), format on save |
| **Husky** | **Chưa** có | Thêm Husky + lint-staged: chạy lint (và format) trước commit |

---

## 6. Tóm tắt vi phạm & ưu tiên sửa

### Backend

| Mức | Vi phạm | Hành động đề xuất |
|-----|---------|-------------------|
| **Cao** | API dùng `ApplicationDbContext` trong `SystemController` | Dùng `IApplicationDbContext` trong controller; chuyển logic đếm/chart sang MediatR Query/Handler |
| **Cao** | API reference trực tiếp Infrastructure (controllers dùng `VNVTStore.Infrastructure.Authorization`, `Persistence`) | Đưa abstraction (VD: `IPermissionChecker`) vào Application; API chỉ phụ thuộc Application |
| **Trung bình** | Application reference `Microsoft.EntityFrameworkCore` (IApplicationDbContext, Handlers dùng EF APIs) | Đã xửa: interface thuần IQueryable + Add/Remove; Infrastructure implement explicit |
| **Thấp** | Một số Handler/file quá lớn | Tách helper, strategy nhỏ; có thể giảm kế thừa BaseHandler bằng composition |

### Frontend

| Mức | Vi phạm / Thiếu | Hành động đề xuất |
|-----|------------------|-------------------|
| **Trung bình** | Chưa Prettier, Husky | Thêm Prettier + Husky + lint-staged |
| **Thấp** | Một số import dùng `../` thay vì alias | Dần chuyển sang `@/`, `@components/`, … |
| **Thấp** | Cấu trúc “feature” chưa rõ (folder `features/`) | Tùy chọn: tách `features/product`, `features/order` từ pages/hooks hiện tại |

---

## 7. Kết luận

- **Backend:** Đúng hướng Clean Architecture (Domain ← Application ← Infrastructure), Repository + AutoMapper, MediatR, DI. Vi phạm chính: **Dependency Rule** (API/Controller phụ thuộc Infrastructure và concrete DbContext). Sửa `SystemController` và đưa abstraction cho Authorization/Persistence sẽ cải thiện rõ.
- **Frontend:** Custom hooks, TanStack Query, Zustand, Zod, lazy load, path aliases, barrel files, Discriminated Union `AsyncState` – **tốt**. Thiếu Prettier + Husky; có thể thống nhất alias và (tùy chọn) cấu trúc feature folder.

**Đã thực hiện:** `SystemController` đã được sửa: không còn inject `ApplicationDbContext` hay dùng `VNVTStore.Infrastructure.Persistence`. Logic đếm/chart đã chuyển sang MediatR:
- `GetSystemCountsQuery` + `GetSystemCountsHandler` (Application) dùng `IApplicationDbContext`;
- Controller chỉ gọi `_mediator.Send(new GetSystemCountsQuery())` và trả về kết quả.
- Đã thêm `TblSuppliers` vào `IApplicationDbContext` để handler dùng.

**Đã sửa thêm (fix vi phạm chính):**
- **Authorization:** `HasPermissionAttribute` và `PermissionRequirement` đã chuyển sang `VNVTStore.Application.Authorization`. API controllers (PermissionsController, RolesController) dùng `VNVTStore.Application.Authorization`; Infrastructure giữ `PermissionHandler` và `PermissionPolicyProvider` (reference Application).
- **MapHub:** API không còn reference type `NotificationHub`; dùng extension `app.MapInfrastructureHubs()` trong `VNVTStore.Infrastructure.Extensions`.
- **Frontend:** Đã thêm Prettier, eslint-config-prettier, Husky, lint-staged; cấu hình `.prettierrc`, `.prettierignore`; pre-commit hook tại `.husky/pre-commit` (git hooks path = `.husky` ở repo root).
- **CreateOrderHandler:** Sửa lỗi sẵn có `ICurrentUserService` → `ICurrentUser` và thêm field `_configuration`.
- **IApplicationDbContext (Application phụ thuộc EF):** Interface thuần – không dùng `DbSet<>`/`DbContext`; chỉ `IQueryable<T>` (System.Linq) và generic `Add`/`AddAsync`/`AddRangeAsync`/`Remove`/`RemoveRange`/`SaveChangesAsync`. Infrastructure (ApplicationDbContext) implement explicit; Handlers/Seeding/Infrastructure services dùng `context.Add()`, `context.RemoveRange()` thay vì `context.TblXxx.Add()`.
