# Angular API

The backend exposes JSON endpoints under `/api` while the existing MVC pages remain available.
The API uses the existing Identity cookie. Angular requests that require authentication must send credentials (`withCredentials: true`).

## Public endpoints

- `GET /api/catalog/products?search=&categoryId=`
- `GET /api/catalog/products/{id}`
- `GET /api/catalog/categories`
- `POST /api/auth/register`
- `POST /api/auth/login`

## Authenticated endpoints

- `GET /api/auth/me`
- `POST /api/auth/logout`
- `GET /api/cart`
- `POST /api/cart/items`
- `PUT /api/cart/items/{cartItemId}`
- `DELETE /api/cart/items/{cartItemId}`
- `GET /api/addresses`
- `POST /api/addresses`
- `GET /api/orders`
- `GET /api/orders/{id}`
- `POST /api/orders`

The default Angular development origin is `http://localhost:4200`. Add any deployed frontend origin to `Cors:AllowedOrigins` in the environment-specific settings file.
