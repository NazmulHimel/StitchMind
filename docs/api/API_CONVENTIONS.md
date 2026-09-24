# GarmentsERP — API Design Conventions

## Base URL
```
https://{domain}/api/v1/{module}/{resource}
```

## Examples
```
GET    /api/v1/procurement/purchase-orders           <- paginated list
GET    /api/v1/procurement/purchase-orders/{id}      <- single record
POST   /api/v1/procurement/purchase-orders           <- create
PUT    /api/v1/procurement/purchase-orders/{id}      <- update
DELETE /api/v1/procurement/purchase-orders/{id}      <- soft delete
GET    /api/v1/reports/purchase-order/{id}           <- PDF report download
```

## Standard Response Envelope

### Success
```json
{
  "success": true,
  "data": { ... },
  "message": "Purchase order created successfully."
}
```

### Paginated List
```json
{
  "success": true,
  "data": {
    "items": [ ... ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

### Error
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed.",
  "errors": [
    { "field": "SupplierId", "message": "Supplier is required." }
  ]
}
```

## HTTP Status Codes
| Code | Meaning                    |
|------|----------------------------|
| 200  | OK (GET, PUT, DELETE)      |
| 201  | Created (POST)             |
| 400  | Bad Request (Validation)   |
| 401  | Unauthorized               |
| 403  | Forbidden (No Permission)  |
| 404  | Not Found                  |
| 409  | Conflict                   |
| 500  | Server Error               |

## Authentication
- Header: Authorization: Bearer {jwt_access_token}
- Refresh: POST /api/v1/auth/refresh-token

## Pagination Query Params
- ?page=1&pageSize=20
- ?search=keyword
- ?sortBy=CreatedAt&sortDir=desc
- ?fromDate=2026-01-01&toDate=2026-12-31

## Naming Conventions
- Route segments: kebab-case (purchase-orders)
- JSON properties: camelCase (supplierId, orderDate)
- Query params: camelCase (pageSize, sortBy)

---

*Last updated: 2026-09-24*
