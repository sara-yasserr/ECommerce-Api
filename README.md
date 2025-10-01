# E-Commerce Backend API - N-Tier Architecture

## 📋 Overview

A comprehensive E-Commerce Backend API built with **.NET 8.0** using **N-Tier Architecture**. This project includes JWT authentication, Entity Framework Core, SQL Server with stored procedures, complete unit and integration tests, and follows clean code principles.

---

## 🏗️ Architecture

This project implements **N-Tier (Multi-Tier) Architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                  Presentation Layer                      │
│                   (ECommerce.API)                        │
│              Controllers, Middleware, API Config         │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                Business Logic Layer                      │
│                   (ECommerce.BLL)                        │
│           Services, DTOs, Business Rules                 │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                 Data Access Layer                        │
│                   (ECommerce.DAL)                        │
│      Repositories, DbContext, Stored Procedures          │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                   Entities Layer                         │
│                 (ECommerce.Entities)                     │
│                   Domain Models                          │
└─────────────────────────────────────────────────────────┘
```

---

## 📁 Project Structure

```
ECommerce.Solution/
│
├── ECommerce.API/                      # Presentation Layer
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── ProductsController.cs
│   │   └── UsersController.cs
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs
│   ├── Extensions/
│   │   └── ServiceExtensions.cs
│   ├── wwwroot/uploads/
│   ├── Program.cs
│   └── appsettings.json
│
├── ECommerce.BLL/                      # Business Logic Layer
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── UserService.cs
│   │   ├── ProductService.cs
│   │   ├── JwtService.cs
│   │   ├── PasswordHasher.cs
│   │   └── FileService.cs
│   ├── Interfaces/
│   ├── DTOs/
│   └── Extensions/
│
├── ECommerce.DAL/                      # Data Access Layer
│   ├── Data/
│   │   └── ECommerceDbContext.cs
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   └── ProductRepository.cs
│   ├── Interfaces/
│   └── Extensions/
│
├── ECommerce.Entities/                 # Entities Layer
│   └── Models/
│       ├── User.cs
│       └── Product.cs
│
├── ECommerce.Tests/                    # Testing Layer
│   ├── Unit/
│   └── Integration/
│
└── Database/
    └── DatabaseScript.sql
```

---

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server 2019 or later
- Visual Studio 2022 / VS Code / Rider
- SQL Server Management Studio (optional)

### Installation Steps

#### 1. Clone or Download the Project

```bash
git clone <your-repo-url>
cd ECommerce.Solution
```

#### 2. Create the Solution Structure

```bash
# Create solution
dotnet new sln -n ECommerce

# Create projects
dotnet new webapi -n ECommerce.API
dotnet new classlib -n ECommerce.BLL
dotnet new classlib -n ECommerce.DAL
dotnet new classlib -n ECommerce.Entities
dotnet new xunit -n ECommerce.Tests

# Add projects to solution
dotnet sln add ECommerce.API/ECommerce.API.csproj
dotnet sln add ECommerce.BLL/ECommerce.BLL.csproj
dotnet sln add ECommerce.DAL/ECommerce.DAL.csproj
dotnet sln add ECommerce.Entities/ECommerce.Entities.csproj
dotnet sln add ECommerce.Tests/ECommerce.Tests.csproj

# Add project references
cd ECommerce.API
dotnet add reference ../ECommerce.BLL/ECommerce.BLL.csproj
dotnet add reference ../ECommerce.DAL/ECommerce.DAL.csproj

cd ../ECommerce.BLL
dotnet add reference ../ECommerce.DAL/ECommerce.DAL.csproj
dotnet add reference ../ECommerce.Entities/ECommerce.Entities.csproj

cd ../ECommerce.DAL
dotnet add reference ../ECommerce.Entities/ECommerce.Entities.csproj

cd ../ECommerce.Tests
dotnet add reference ../ECommerce.API/ECommerce.API.csproj
dotnet add reference ../ECommerce.BLL/ECommerce.BLL.csproj
dotnet add reference ../ECommerce.DAL/ECommerce.DAL.csproj
```

#### 3. Install NuGet Packages

**ECommerce.API:**
```bash
cd ECommerce.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.AspNetCore.Mvc.Versioning
```

**ECommerce.BLL:**
```bash
cd ../ECommerce.BLL
dotnet add package Microsoft.IdentityModel.Tokens
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Microsoft.AspNetCore.Http.Features
dotnet add package Microsoft.Extensions.Hosting.Abstractions
```

**ECommerce.DAL:**
```bash
cd ../ECommerce.DAL
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.Data.SqlClient
```

**ECommerce.Tests:**
```bash
cd ../ECommerce.Tests
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

#### 4. Setup Database

**Option A: Using SQL Script**
```bash
# Run the provided database script in SSMS or Azure Data Studio
sqlcmd -S localhost -i Database/DatabaseScript.sql
```

**Option B: Using Entity Framework Migrations**
```bash
cd ECommerce.DAL
dotnet ef migrations add InitialCreate --startup-project ../ECommerce.API
dotnet ef database update --startup-project ../ECommerce.API
```

#### 5. Configure Connection String

Edit `ECommerce.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "ThisIsAVerySecureSecretKeyWithAtLeast32CharactersForHS256!",
    "Issuer": "ECommerceAPI",
    "Audience": "ECommerceAPI",
    "AccessTokenExpirationMinutes": "30",
    "RefreshTokenExpirationDays": "7"
  }
}
```

#### 6. Run the Application

```bash
cd ECommerce.API
dotnet restore
dotnet build
dotnet run
```

The API will be available at:
- **HTTPS:** https://localhost:7000
- **HTTP:** http://localhost:5000
- **Swagger UI:** https://localhost:7000/swagger

---

## 🔌 API Endpoints

### Authentication (`/api/auth`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | ❌ | Register new user |
| POST | `/api/auth/login` | ❌ | Login user |
| POST | `/api/auth/refresh-token` | ❌ | Refresh access token |
| POST | `/api/auth/revoke-token` | ✅ | Revoke refresh token |
| POST | `/api/auth/logout` | ✅ | Logout user |

### Products (`/api/products`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/products` | ✅ | Get all products (paginated) |
| GET | `/api/products/{id}` | ✅ | Get product by ID |
| GET | `/api/products/by-code/{code}` | ✅ | Get product by code |
| POST | `/api/products` | ✅ | Create product (with image) |
| PUT | `/api/products/{id}` | ✅ | Update product |
| DELETE | `/api/products/{id}` | ✅ | Delete product |
| GET | `/api/products/categories` | ✅ | Get all categories |

### Users (`/api/users`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/users/profile` | ✅ | Get current user profile |
| GET | `/api/users` | ✅ | Get all users (paginated) |
| GET | `/api/users/{id}` | ✅ | Get user by ID |
| PUT | `/api/users/profile` | ✅ | Update current user |
| PUT | `/api/users/{id}` | ✅ | Update user by ID |
| POST | `/api/users/change-password` | ✅ | Change password |
| DELETE | `/api/users/{id}` | ✅ | Delete user |

---

## 📝 API Usage Examples

### 1. Register a New User

```bash
curl -X POST https://localhost:7000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "johndoe",
    "email": "john@example.com",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!"
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "Registration successful",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "xYz123ABC...",
  "tokenExpiry": "2025-10-07T12:30:00Z",
  "user": {
    "id": 1,
    "userName": "johndoe",
    "email": "john@example.com",
    "lastLoginTime": null
  }
}
```

### 2. Login

```bash
curl -X POST https://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "johndoe",
    "password": "SecurePass123!"
  }'
```

### 3. Create a Product (with Image)

```bash
curl -X POST https://localhost:7000/api/products \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -F "category=Electronics" \
  -F "productCode=P001" \
  -F "name=Wireless Mouse" \
  -F "price=29.99" \
  -F "minimumQuantity=1" \
  -F "discountRate=10" \
  -F "image=@/path/to/image.jpg"
```

### 4. Get Products with Filters

```bash
# Get all products (page 1, 10 items)
curl -X GET "https://localhost:7000/api/products?page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"

# Filter by category
curl -X GET "https://localhost:7000/api/products?category=Electronics" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"

# Search products
curl -X GET "https://localhost:7000/api/products?searchTerm=Mouse" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 5. Refresh Token

```bash
curl -X POST https://localhost:7000/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "xYz123ABC..."
  }'
```

### 6. Change Password

```bash
curl -X POST https://localhost:7000/api/users/change-password \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "SecurePass123!",
    "newPassword": "NewSecurePass456!",
    "confirmNewPassword": "NewSecurePass456!"
  }'
```

---

## 🗄️ Database Schema

### Users Table

| Column | Type | Constraints |
|--------|------|-------------|
| UserId | INT | PRIMARY KEY, IDENTITY |
| UserName | NVARCHAR(50) | NOT NULL, UNIQUE |
| Password | NVARCHAR(255) | NOT NULL |
| Email | NVARCHAR(100) | NOT NULL, UNIQUE |
| LastLoginTime | DATETIME2 | NULL |
| RefreshToken | NVARCHAR(500) | NULL |
| RefreshTokenExpiryTime | DATETIME2 | NULL |
| CreatedDate | DATETIME2 | NOT NULL, DEFAULT GETDATE() |
| ModifiedDate | DATETIME2 | NOT NULL, DEFAULT GETDATE() |
| IsActive | BIT | NOT NULL, DEFAULT 1 |

### Products Table

| Column | Type | Constraints |
|--------|------|-------------|
| ProductId | INT | PRIMARY KEY, IDENTITY |
| Category | NVARCHAR(100) | NOT NULL |
| ProductCode | NVARCHAR(20) | NOT NULL, UNIQUE |
| ProductName | NVARCHAR(200) | NOT NULL |
| ImagePath | NVARCHAR(255) | NULL |
| Price | DECIMAL(18,2) | NOT NULL |
| MinimumQuantity | INT | NOT NULL |
| DiscountRate | DECIMAL(5,2) | NOT NULL, DEFAULT 0 |
| CreatedDate | DATETIME2 | NOT NULL, DEFAULT GETDATE() |
| ModifiedDate | DATETIME2 | NOT NULL, DEFAULT GETDATE() |
| IsActive | BIT | NOT NULL, DEFAULT 1 |

---

## 🔐 Authentication & Security

### JWT Authentication Flow

1. **User Registration/Login** → Receives Access Token (30 min) + Refresh Token (7 days)
2. **API Requests** → Include `Authorization: Bearer {access_token}` header
3. **Token Expiry** → Use Refresh Token endpoint to get new Access Token
4. **Logout** → Revoke Refresh Token

### Security Features

- ✅ JWT Bearer Token Authentication
- ✅ Password Hashing with SHA256 + Salt
- ✅ Refresh Token Mechanism
- ✅ Token Expiration & Renewal
- ✅ Unique Constraints (Username, Email, ProductCode)
- ✅ Soft Delete (Data Preservation)
- ✅ SQL Injection Prevention (Parameterized Queries)
- ✅ Global Exception Handling Middleware

---

## 🧪 Testing

### Run Unit Tests

```bash
cd ECommerce.Tests
dotnet test --filter "FullyQualifiedName~Unit"
```

### Run Integration Tests

```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

### Run All Tests

```bash
dotnet test
```

### Test Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Tests Included

**Unit Tests:**
- ✅ AuthService Tests (Register, Login, Token Refresh)
- ✅ UserService Tests (CRUD, Password Change)
- ✅ ProductService Tests (CRUD, File Upload)
- ✅ PasswordHasher Tests
- ✅ JwtService Tests

**Integration Tests:**
- ✅ Authentication Flow Tests
- ✅ Product Management Tests
- ✅ User Management Tests
- ✅ End-to-End Workflow Tests

---

## 📊 Stored Procedures

### User Operations
- `sp_AuthenticateUser` - Authenticate user credentials
- `sp_GetTotalUsersCount` - Get total active users
- `sp_GetUserById` - Get user by ID
- `sp_GetUsersPaginated` - Get paginated users
- `sp_UpdateUserLastLogin` - Update last login time

### Product Operations
- `sp_GetProductsByCategory` - Get products by category
- `sp_GetDiscountedProducts` - Get discounted products
- `sp_GenerateNextProductCode` - Auto-generate product code
- `sp_GetProductsPaginated` - Get paginated products
- `sp_GetProductStatistics` - Get product statistics
- `sp_UpdateProductStock` - Update product stock

### Utility Procedures
- `sp_DatabaseHealthCheck` - Database health check
- `sp_CleanExpiredRefreshTokens` - Clean expired tokens

---

## 🎯 Key Features

### Technical Features
- ✅ N-Tier Architecture with clear separation
- ✅ Repository Pattern for data access
- ✅ Service Pattern for business logic
- ✅ Dependency Injection throughout
- ✅ Entity Framework Core 8.0
- ✅ Stored Procedures integration
- ✅ JWT Authentication with Refresh Tokens
- ✅ File Upload (Image storage)
- ✅ Pagination & Filtering
- ✅ Soft Delete
- ✅ Automatic Timestamp Updates
- ✅ Global Exception Handling
- ✅ Comprehensive Logging
- ✅ Swagger Documentation
- ✅ Unit & Integration Tests

### Business Features
- ✅ User Registration & Login
- ✅ Password Change
- ✅ Profile Management
- ✅ Product CRUD with Image Upload
- ✅ Product Search & Filtering
- ✅ Category Management
- ✅ Discount Rate Support
- ✅ Minimum Quantity Validation

---

## 🛠️ Technologies Used

- **.NET 8.0** - Framework
- **ASP.NET Core Web API** - REST API
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Database
- **JWT Bearer Authentication** - Security
- **Swagger/OpenAPI** - API Documentation
- **xUnit** - Unit Testing
- **Moq** - Mocking Framework
- **FluentAssertions** - Test Assertions

---

## 📚 Design Patterns

- **N-Tier Architecture** - Layered separation
- **Repository Pattern** - Data access abstraction
- **Service Pattern** - Business logic encapsulation
- **Dependency Injection** - Loose coupling
- **DTO Pattern** - Data transfer
- **Factory Pattern** - Object creation
- **Middleware Pattern** - Request pipeline

---

## 🔧 Configuration

### JWT Settings

```json
{
  "Jwt": {
    "Key": "Your-Secret-Key-At-Least-32-Characters",
    "Issuer": "ECommerceAPI",
    "Audience": "ECommerceAPI",
    "AccessTokenExpirationMinutes": "30",
    "RefreshTokenExpirationDays": "7"
  }
}
```

### File Upload Settings

```json
{
  "FileUpload": {
    "MaxFileSizeInMB": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp"],
    "UploadPath": "wwwroot/uploads"
  }
}
```

---

## 📈 Performance Optimizations

- ✅ Database Indexes on frequently queried columns
- ✅ AsNoTracking for read-only queries
- ✅ Pagination for large datasets
- ✅ Connection Pooling
- ✅ Async/Await throughout
- ✅ Stored Procedures for complex queries
- ✅ Soft Delete for data integrity

---

## 🐛 Troubleshooting

### Common Issues

**Issue: Database connection fails**
```
Solution: Verify SQL Server is running and connection string is correct
```

**Issue: JWT token validation fails**
```
Solution: Ensure JWT Key is at least 32 characters in appsettings.json
```

**Issue: File upload fails**
```
Solution: Check wwwroot/uploads folder exists and has write permissions
```

**Issue: Migrations fail**
```
Solution: Ensure correct startup project is set
dotnet ef database update --startup-project ../ECommerce.API
```

---

## 📖 Documentation

- **Swagger UI**: Available at `/swagger` when running the application
- **API Documentation**: Auto-generated from XML comments
- **Database Schema**: Included in `Database/DatabaseScript.sql`

---

## ✅ Assignment Requirements

All assignment requirements have been successfully implemented:

### Architecture
- ✅ N-Tier Architecture (Presentation → BLL → DAL → Entities)
- ✅ Clean and readable code
- ✅ Proper naming conventions
- ✅ Dependency Injection

### Features
- ✅ User entity with all required properties
- ✅ Product entity with all required properties
- ✅ Unique constraints on Username, Email, ProductCode
- ✅ Image storage in local file system

### Security
- ✅ JWT authentication on all endpoints
- ✅ Refresh token mechanism
- ✅ Password hashing
- ✅ Token validation

### Database
- ✅ SQL Server database
- ✅ Entity Framework Core
- ✅ Stored Procedures
- ✅ Database script provided
- ✅ Performance indexes

### Testing
- ✅ Unit tests with xUnit and Moq
- ✅ Integration tests
- ✅ Comprehensive test coverage

---

## 👨‍💻 Author

**Full Stack Developer**  
E-Commerce Backend API - N-Tier Architecture Assignment

---

## 📄 License

This project is created for educational and assignment purposes.

---

## 🚀 Deployment

### Prepare for Production

1. Update `appsettings.Production.json`
2. Change JWT secret key
3. Configure CORS for production domains
4. Set up logging provider (Serilog, NLog)
5. Configure cloud storage for images
6. Set up CI/CD pipeline
7. Configure SSL certificates
8. Set up monitoring and health checks

---

## 📞 Support

For issues or questions:
1. Check Swagger documentation at `/swagger`
2. Review database script and stored procedures
3. Run unit and integration tests
4. Check application logs

---

**End of Documentation**