# ShopNep E-Commerce Project Documentation

## 1. Project Overview

ShopNep is a college e-commerce project built with ASP.NET Core MVC and .NET 8. It allows customers to browse products, manage a shopping cart, place orders, and choose a payment method. It also includes an admin panel for managing the store.

## 2. Technologies Used

- ASP.NET Core MVC (.NET 8)
- C#
- Entity Framework Core 8
- PostgreSQL
- Npgsql Entity Framework Core provider
- ASP.NET Core Identity
- Razor Views
- Bootstrap 5 via CDN
- eSewa ePay v2 sandbox integration

## 3. Main Features

### Customer Features

- Register, log in, and log out
- Browse products and categories
- Search and filter products
- View product details
- Add products to the cart
- Update cart quantities
- Remove cart items
- Add and select delivery addresses
- Place orders
- Choose Cash on Delivery or eSewa payment
- View order history and order details

### Admin Features

- Admin-only dashboard
- View product, category, and order counts
- Add, edit, and deactivate products
- Add, edit, and delete categories
- View customer orders
- Update order statuses
- View calculated order revenue

## 4. Project Structure

```text
ECommerceApp/
├── Areas/Admin/                 Admin controllers and Razor views
├── Controllers/                 Customer, cart, checkout, payment, and order logic
├── Data/                        DbContext and database seeding
├── Migrations/                  Entity Framework Core migrations
├── Models/                      Product, category, cart, order, and user entities
├── Services/                    eSewa payment service
├── ViewModels/                  Models used by forms and views
├── Views/                       Razor pages
├── wwwroot/                     CSS, JavaScript, and images
├── appsettings.json             Application and database configuration
└── Program.cs                   Application and service configuration
```

## 5. Database Configuration

The application uses PostgreSQL with this connection format:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ecommerceapp;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

The database created for the project is named `ecommerceapp`.

The Entity Framework migration `InitialCreate` was created and applied successfully. The database contains tables for:

- `AspNetUsers`
- `AspNetRoles`
- `Categories`
- `Products`
- `Addresses`
- `CartItems`
- `Orders`
- `OrderItems`

## 6. Database Setup and Migration

From the project directory, run:

```powershell
cd C:\Users\ACER\Desktop\ECommerceApp_complete\ECommerceApp
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
```

The expected result after the database is already configured is:

```text
No migrations were applied. The database is already up to date.
```

The `dotnet-ef` tool can be installed with:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.8
```

## 7. Running the Application

```powershell
cd C:\Users\ACER\Desktop\ECommerceApp_complete\ECommerceApp
dotnet run
```

Open the HTTPS URL printed in the terminal, for example:

```text
https://localhost:57141
```

The application connects to PostgreSQL during startup. The database connection is working when the application starts without database errors and displays messages such as `Application started`.

## 8. Database Seeding

On startup, `DbSeeder` ensures that the following demo data exists:

- `Admin` and `Customer` roles
- One administrator account
- Four product categories
- Six sample products

Default administrator login:

```text
Email: admin@shopnep.com
Password: Admin@123
```

The admin panel can be opened at:

```text
https://localhost:57141/Admin/Dashboard
```

The port may change when the application starts.

## 9. Customer Purchase Flow

1. Register a customer account or log in.
2. Open a product and select **Add to Cart**.
3. Open the cart and select **Proceed to Checkout**.
4. Add a delivery address.
5. Choose **Cash on Delivery** or **eSewa**.
6. Select **Place Order**.
7. View the order under **My Orders**.

For Cash on Delivery, the order is confirmed immediately. The application reduces stock and clears the cart.

For eSewa, the order starts with `PendingPayment`. After a successful callback, the application verifies the payment, marks the order as paid, and clears the cart. If payment fails, the order is marked failed and stock is restored.

## 10. Security and Access Control

- Cart, checkout, payment, and order actions require authentication.
- Admin controllers require the `Admin` role.
- State-changing forms use anti-forgery validation.
- ASP.NET Core Identity stores users and password hashes.
- Product and order ownership is checked before customer data is displayed.
- Order items store the product name and price at purchase time so old orders remain accurate.
- Local configuration, environment files, and certificate/private-key files are excluded in `.gitignore`.

## 11. Course Syllabus Alignment

### ASP.NET Core and C# syllabus

| Unit                                           | Coverage in this project                                                                                                                                                                                                                                                        |
| ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Unit 1: Language Preliminaries                 | **Partially covered.** The code uses classes, constructors, properties, enums, inheritance, interfaces, generics, collections, LINQ, exceptions, and async/await. Topics such as delegates, events, attributes, structs, and file I/O are not central features of this project. |
| Unit 2: Introduction to ASP.NET                | **Covered.** The project is a .NET 8 ASP.NET Core MVC application using the .NET CLI, dependency injection, Kestrel, and the MVC architecture.                                                                                                                                  |
| Unit 3: HTTP and ASP.NET Core                  | **Covered.** Controllers receive HTTP requests, actions return results, routing maps URLs, and Razor views generate HTML responses.                                                                                                                                             |
| Unit 4: Creating ASP.NET Core MVC Applications | **Strongly covered.** The project includes controllers, actions, Razor views, models, view models, tag helpers, model validation, routing, dependency injection, and form binding. It does not currently expose a separate Web API controller.                                  |
| Unit 5: Working with Database                  | **Strongly covered.** Entity Framework Core, Npgsql, PostgreSQL, a DbContext, migrations, relationships, LINQ queries, and create/read/update/delete operations are implemented.                                                                                                |
| Unit 6: State Management                       | **Partially covered.** The project uses authentication cookies, TempData, route/query values, hidden form fields, and database-backed cart state. Session state and application caching are not currently used.                                                                 |
| Unit 7: Client-side Development                | **Partially covered.** Razor forms, Bootstrap, HTML validation, responsive styling, and JavaScript from Bootstrap are used. Angular, React, SPA architecture, and substantial jQuery code are not included.                                                                     |
| Unit 8: Securing ASP.NET Core Applications     | **Strongly covered.** ASP.NET Core Identity, password hashing, roles, authorization, ownership checks, anti-forgery tokens, and Razor HTML encoding are used.                                                                                                                   |
| Unit 9: Hosting and Deployment                 | **Partially covered.** The application runs locally using Kestrel and `dotnet run`. IIS, Nginx, Apache, Docker, and Azure deployment are not implemented.                                                                                                                       |

### E-commerce syllabus

| Unit                                  | Coverage in this project                                                                                                                                                                                                                                                                   |
| ------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Unit 1: Introduction                  | **Partially covered.** The system demonstrates a local B2C e-commerce store with customer accounts, a product catalog, cart, checkout, and orders. Theory topics such as e-commerce history, policy, and the Electronic Transactions Act are outside the application code.                 |
| Unit 2: E-commerce Business Model     | **Partially covered.** The application represents a B2C e-tailer model. It does not implement multiple sellers, advertising revenue, marketplace functionality, EDI, or ERP integration.                                                                                                   |
| Unit 3: Electronic Payment System     | **Partially covered.** The project includes Cash on Delivery and an eSewa sandbox payment flow with callback verification and HMAC-SHA256 signatures. Credit-card processing, SET, digital cash, auctions, and other payment systems are not implemented.                                  |
| Unit 4: Building E-commerce System    | **Strongly covered.** The project includes a dynamic catalog, shopping cart, checkout, transaction/order processing, PostgreSQL persistence, admin application logic, and payment gateway integration.                                                                                     |
| Unit 5: Security in E-commerce        | **Strongly covered for a college prototype.** Authentication, role-based access, password hashing, authorization, anti-forgery protection, server-side payment verification, HTTPS development URLs, and ORM parameterized queries are used. Production hardening would still be required. |
| Unit 6: Digital Marketing             | **Not implemented.** Search is available, but advertising, SEO analytics, email marketing, affiliate marketing, and social-media marketing tools are outside the current project.                                                                                                          |
| Unit 7: Optimizing E-commerce Systems | **Partially covered.** Product search, category browsing, related products, and basic stock/order reporting are included. Google Analytics, page-rank optimization, collaborative recommendations, and a full content-based recommender are not implemented.                               |

### Overall assessment

The project is a good practical match for the core laboratory work: ASP.NET Core MVC, C# application development, database CRUD with EF Core, authentication and authorization, shopping cart, checkout, order processing, and payment integration. It is not intended to implement every theory topic in the complete syllabus. The unimplemented topics can be listed under **limitations and future improvements** in the college report.

## 12. Testing Checklist

- [ ] PostgreSQL server is running.
- [ ] `ecommerceapp` database exists.
- [ ] `dotnet ef database update` completes successfully.
- [ ] The application starts with `dotnet run`.
- [ ] A customer can register and log in.
- [ ] A product can be added to the cart.
- [ ] Cart quantity can be updated or removed.
- [ ] An address can be added.
- [ ] A Cash on Delivery order can be placed.
- [ ] The order appears in My Orders.
- [ ] The administrator can open the Admin Panel.
- [ ] Products, categories, and orders can be managed by the administrator.

## 13. Future Improvements

- Replace seeded credentials before public deployment.
- Move passwords and payment secrets to User Secrets or environment variables.
- Add product image upload.
- Add product reviews and ratings.
- Add email confirmation and password reset.
- Add order email notifications.
- Add automated unit and integration tests.
- Add stock and payment transaction handling for production deployment.

## 14. Conclusion

The completed project demonstrates a working e-commerce system using ASP.NET Core MVC, PostgreSQL, Entity Framework Core, Identity authentication, role-based administration, shopping cart functionality, checkout, order management, and payment integration.
