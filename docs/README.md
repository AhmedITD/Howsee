# Howsee schema and implementation

## Schema overview

The canonical data model is defined in **[Howse_Schema_Code.dbml](Howse_Schema_Code.dbml)** (DBML). It covers:

- **Users & auth**: `users`, `refresh_tokens`, `otp_codes` (with `otp_purpose`: Registration, PasswordReset)
- **Properties & tours**: `properties`, `property_categories`, `tours`, `property_images`, `property_image_variants`, `property_listings`, `rent_offers`, `saves`
- **Pricing & billing**: `currencies`, `pricing_plans`, `invoices`, `subscriptions`
- **Media**: `images` (e.g. user profile image)

The app is a .NET 8 API with EF Core; entities live in `Howsee.Domain`, DbContext and configurations in `Howsee.Infrastructure`.

---

## Implementation status

### Implemented

- **Users**: FullName, Phone, PasswordHash, Role, IsActive, UpdatedBy, IsDeleted, DeletedAt, CreatedAt, UpdatedAt
- **RefreshTokens**: TokenHash, UserId, ExpiresAt, CreatedAt, RevokedAt, DeviceClientId, IpAddress, ReplacedByTokenHash
- **Tours**: OwnerId, Title, MatterportModelId, StartSweepId, PasswordHash, ExpiresAt, IsActive, ShareToken, UpdatedBy
- **Properties**: OwnerId, Category (enum), Lat, Lng, Description, Area, Price, Active, TourId, Address (owned), UpdatedBy
- **Currencies**: Code, Name, Symbol, IsActive (seeded with IQD)
- **Invoices**: UserId, TotalAmount, CurrencyId (FK), Description, Status, WaylPaymentId (wayl_payment_id), PaidAt, IsDeleted, DeletedAt, CreatedAt, UpdatedAt, UpdatedBy, PricingPlanId
- **PricingPlans**: Key, Name, Amount, CurrencyId (FK), Unit, Role, IsActive, SortOrder
- **Subscriptions**: UserId, PricingPlanId, StartDate, EndDate, Status, InvoiceId
- **PhoneVerificationCodes** (aligned with docs `otp_codes`): PhoneNumber, Code (legacy), CodeHash, Purpose (OtpPurpose), ExpiresAt, UsedAt, VerifiedAt, IsUsed, IpAddress, CreatedAt
- **AuditLogs**: UserId, EntityType, EntityId, Action, Changes, Timestamp (app-only; not in DBML)

### Partially implemented

- **Property**: Price is on the entity; docs use `property_listings` (ListingType, Price, CurrencyId). Category is an enum in the app; docs use `property_categories` table.
- **Invoice**: Payment gateway is **Wayl**; `WaylPaymentId` maps to schema `wayl_payment_id` (Wayl link id).

### Implemented (Phase 3–4)

- **Images**: Url, AltText, CreatedAt, Type; **Users.ProfileImageId** (optional FK to Images)
- **PropertyListings**: PropertyId, ListingType (Sale/Rent), Price, CurrencyId, IsActive, CreatedAt, UpdatedAt
- **RentOffers**: ListingId, TenantId, OfferedPrice, CurrencyId, DurationMinutes, ProposedStart/EndDate, Status (RentOfferStatus), Message, ReviewedBy, ReviewedAt
- **Saves**: UserId, PropertyId, CreatedAt (unique on UserId, PropertyId)
- **PropertyCategories** (table): Id, Name (seeded: House, Apartment, Office, Villa, Other). Property entity still uses Category enum for now.
- **PropertyImages**: PropertyId, SortOrder, AltText, CreatedAt
- **PropertyImageVariants**: PropertyImageId, Resolution, Url, Width, Height, FileSizeBytes (unique on PropertyImageId, Resolution)

---

## Suggested additions to the DBML (for docs)

- **AuditLogs**: Add a table and relationship to Users if you want the doc to reflect the app fully (e.g. `audit_logs`: id, user_id, entity_type, entity_id, action, changes, timestamp).
- **Payment gateway**: The app uses **Wayl**; `wayl_payment_id` stores the Wayl payment link id. See [WAYL/README.md](WAYL/README.md) for integration details.
- **Property categories**: If the app keeps the category as an enum, add a short note that the app may use an enum instead of the `property_categories` table.

---

## How to run and migrate

### API

From the solution root:

```bash
dotnet run --project Howsee.Api
```

### Database migrations

Apply migrations (PostgreSQL):

```bash
dotnet ef database update --project Howsee.Infrastructure --startup-project Howsee.Api
```

Add a new migration after entity changes:

```bash
dotnet ef migrations add YourMigrationName --project Howsee.Infrastructure --startup-project Howsee.Api
```

### Frontend

See [../frontend/README.md](../frontend/README.md) for Vue 3 + Vite setup, env vars, and Matterport SDK configuration.
