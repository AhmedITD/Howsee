# Wayl payment integration

The Howsee API uses **Wayl** as the payment gateway for invoices (pricing plan purchases). All amounts are in **IQD** (Iraqi Dinar).

## API reference

- **OpenAPI spec**: [wayl-api.json](wayl-api.json)
- **Servers**: Production `https://api.thewayl.com`, Staging `https://api.thewayl-staging.com`
- **Auth**: API key in header `X-WAYL-AUTHENTICATION`

## Flow in this app

1. **Create invoice**  
   When a user creates an invoice (POST `/invoices`), the API creates a Wayl payment link (POST `/api/v1/links`) with:
   - `referenceId` = invoice Id (GUID)
   - `total` = plan amount (IQD)
   - `currency` = `"IQD"`
   - `webhookUrl` = `{APP_URL}/api/payments/webhook/wayl`
   - `webhookSecret` = from config (optional)
   - `redirectionUrl` = client’s finish URL from the request  

   The Wayl link id is stored in `invoices.WaylPaymentId` (schema: `wayl_payment_id`). The response returns `data.url` as the payment URL for the client.

2. **Webhook**  
   Wayl calls `POST /api/payments/webhook/wayl` when payment status changes. The handler expects a body with `referenceId` (invoice id) and `status`. If `status` is `Complete` or `Delivered`, the invoice is marked paid (subscription created/extended, role updated if applicable).

3. **Cancel pending**  
   When marking another invoice as paid for the same user, the app invalidates other pending invoices by calling Wayl `POST /api/v1/links/{referenceId}/invalidate-if-pending` with each pending invoice id as `referenceId`.

## Configuration

In `appsettings.json` (or environment):

```json
"Wayl": {
  "BaseUrl": "https://api.thewayl.com",
  "ApiKey": "<your X-WAYL-AUTHENTICATION key>",
  "WebhookSecret": "<optional, 10–255 chars for webhook verification>"
}
```

- **BaseUrl**: Use `https://api.thewayl-staging.com` for testing.
- **ApiKey**: From your Wayl merchant dashboard; required for creating links and invalidating.
- **WebhookSecret**: Optional; if set, it is sent when creating the link so Wayl can sign/verify webhooks (see Wayl docs for verification method).
- **APP_URL**: Must be set so `webhookUrl` and callbacks are correct (e.g. your public API base URL).

## Migrations

After switching from Qi Card, the invoice payment column was renamed from `QiPaymentId` to `WaylPaymentId` (migration `RenameQiPaymentIdToWaylPaymentId`). Apply migrations as usual:

```bash
dotnet ef database update --project Howsee.Infrastructure --startup-project Howsee.Api
```
