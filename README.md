# ShopNep — ASP.NET Core MVC E-Commerce Demo

A complete, beginner-friendly e-commerce web app built with **ASP.NET Core MVC (.NET 8)**, **Entity Framework Core**, and **ASP.NET Core Identity**. Built to match a typical E-commerce course syllabus: business model → payments → building the system → security → marketing.

---

## 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Any code editor (Visual Studio 2022, VS Code, Rider)
- **PostgreSQL** 14 or later, running locally or on a hosted instance.

---

## 2. Running the project

```bash
cd ECommerceApp
dotnet restore
dotnet run
```

Before running, update `ConnectionStrings:DefaultConnection` in `appsettings.json` with your PostgreSQL credentials and create the `ecommerceapp` database.

The first run automatically:

1. Creates the application schema in the configured PostgreSQL database.
2. Creates the `Admin` and `Customer` roles.
3. Creates a default admin account:
   - **Email:** `admin@shopnep.com`
   - **Password:** `Admin@123`
4. Seeds 4 categories and 6 sample products so the store isn't empty.

Open the URL shown in the console (usually `https://localhost:5001` or similar) in your browser.

> ⚠️ Change the seeded admin password (or remove the seeder) before deploying anywhere public.

---

## 3. Project structure & how it maps to the course syllabus

```
ECommerceApp/
├── Models/              Domain entities (Unit 4 — Building E-commerce Systems)
│   ├── Product.cs, Category.cs        → Catalog
│   ├── CartItem.cs                    → Shopping cart
│   ├── Address.cs, Order.cs, OrderItem.cs  → Checkout & order processing
│   └── ApplicationUser.cs             → extends ASP.NET Identity user
│
├── Data/
│   ├── ApplicationDbContext.cs        EF Core DbContext (database access layer)
│   └── DbSeeder.cs                    Seeds roles / admin / demo catalog
│
├── Services/            Payment gateway integration (Unit 3 — Electronic Payment Systems)
│   ├── EsewaPaymentService.cs         Builds & verifies signed eSewa ePay v2 requests (HMAC-SHA256)
│   └── IEsewaPaymentService.cs         Defines the eSewa payment service contract
│
├── Controllers/         Application logic ("Application Programs" — Unit 4)
│   ├── HomeController          Product listing, category filter, search, paging
│   ├── ProductController       Product detail page
│   ├── CartController          Add / update / remove cart items
│   ├── CheckoutController      Address selection + order creation
│   ├── PaymentController       Redirects to gateway, verifies callbacks
│   ├── OrderController         Customer's order history
│   └── AccountController       Register / Login / Logout (uses Identity)
│
├── Areas/Admin/          Admin panel (product / category / order management)
│   └── Controllers/ + Views/  Role-protected with [Authorize(Roles = "Admin")]
│
└── Views/                Razor views (Bootstrap 5, via CDN — no npm/node build step needed)
```

### Why these design choices (good for explaining in a viva/demo)

- **PostgreSQL + `EnsureCreated()`** instead of full EF Core Migrations: keeps the "getting started" experience simple. For production, use `dotnet ef migrations add InitialCreate` + `dotnet ef database update`.
- **Cart stored in the database**, one row per (user, product), rather than session/cookies. Simpler to reason about, survives browser restarts, and avoids merging guest-cart-into-user-cart logic — a common source of bugs in beginner projects. Trade-off: users must be logged in to use the cart (`[Authorize]` on `CartController`).
- **Order = snapshot, not a live reference.** `OrderItem` copies the product's name and price at time of purchase. This means changing a product's price later never rewrites past invoices — a basic but important integrity/security principle (Unit 5).
- **Stock is decremented at order creation, not at payment success**, and restored automatically if payment fails/is cancelled. This avoids overselling while a customer is on the payment gateway's page.
- **Payment verification happens server-to-server**, not by trusting the browser redirect:
  - eSewa: we recompute the HMAC-SHA256 signature ourselves and compare it to the one eSewa sent back.

  This directly reflects Unit 5's "Nonrepudiation" and "Data Transaction Security" concepts — never trust client-side redirect data for money-moving decisions.

- **Soft-delete for products** (`IsActive = false`) instead of a hard delete, so historical orders still resolve correctly even after a product is discontinued.

---

## 4. Payment gateway setup (eSewa)

eSewa is wired up to its **test/sandbox** endpoint out of the box, configured in `appsettings.json`:

```json
"Esewa": {
  "MerchantCode": "EPAYTEST",
  "SecretKey": "8gBm/:&EnhH.1/q",
  "PaymentFormUrl": "https://rc-epay.esewa.com.np/api/epay/main/v2/form"
}
```

- The eSewa test merchant code/key above are eSewa's **publicly documented sandbox credentials** — fine for testing, but a real deployment needs your own merchant code and secret key from eSewa.
- **Important:** Payment gateway APIs occasionally change field names or endpoints. Before a real deployment, re-check the current official docs:
  - eSewa: search "eSewa ePay v2 merchant integration documentation"

### Flow, step by step

1. Customer clicks **Place Order** on `/Checkout` → `CheckoutController.PlaceOrder` creates an `Order` with status `PendingPayment` and a unique `TransactionUuid`, then reduces stock.
2. Redirects to `/Payment/Pay?orderId=...`.
3. `PaymentController.Pay`:
   - **eSewa:** builds a signed form (`EsewaPaymentService.BuildPaymentForm`) and renders `Views/Payment/EsewaRedirect.cshtml`, which auto-submits a real HTML form POST to eSewa (required — eSewa needs a browser-level POST, not an AJAX call).
   - **Cash on Delivery:** skips the gateway entirely, marks the order `Processing`, clears the cart.
4. After the customer pays (or cancels) on the gateway's own site, they're redirected back to:
   - `/Payment/EsewaSuccess` or `/Payment/EsewaFailure` (eSewa)
5. The eSewa callback **re-verifies** the payment server-side before marking the order `Paid` and clearing the cart. If verification fails, the order is marked `Failed` and the stock is restored.

---

## 5. Roles & Admin Panel

- Two roles are seeded: `Admin` and `Customer`.
- New registrations via `/Account/Register` are automatically assigned `Customer`.
- Log in as `admin@shopnep.com` / `Admin@123` and visit **Admin Panel** in the navbar (only visible to Admins) to:
  - Add/edit/soft-delete products
  - Add/edit/delete categories (blocked if the category still has products)
  - View all orders, filter by status, and update order status (e.g. mark as Shipped/Delivered)

---

## 6. Suggested next steps (good "future work" talking points for a report)

- Add product image upload instead of pasting a URL (`IFormFile` + saving to `wwwroot/images` or cloud storage).
- Add product reviews/ratings (ties into Unit 7 — Recommendation Systems).
- Add a simple **content-based recommender**: "customers who viewed this also viewed..." using category/co-purchase data (Unit 7).
- Add email confirmation & password reset (ASP.NET Identity already supports the plumbing — currently disabled for demo simplicity via `RequireConfirmedAccount = false`).
  - Use real EF Core Migrations for a production deployment.
- Add SSL/TLS enforcement notes and CSRF/XSS discussion for the Unit 5 security write-up — the project already uses `[ValidateAntiForgeryToken]` on all state-changing POSTs and Razor's automatic HTML encoding (XSS protection).

---

## 7. A note on accuracy

The eSewa integration follows the commonly documented ePay v2 flow as of early 2026, but payment gateway APIs do change — verify field names and endpoints against the current official docs before a real deployment or an exam demo where marks depend on live payment success.
