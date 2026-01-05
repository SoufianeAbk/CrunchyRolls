CrunchyRolls 🍣
Een moderne, cross-platform sushi delivery applicatie gebouwd met .NET 9.0 MAUI, ASP.NET Core Web API, en offline-first architectuur met JWT authenticatie.

📋 Inhoudsopgave

- [Overzicht](-overzicht)
- [Architectuur](-architectuur)
- [Installatie](-installatie)
- [Functionaliteiten](-functionaliteiten)
- [API Endpoints](-api-endpoints)
- [Database Schema](-database-schema)
- [Authenticatie](-authenticatie)
- [Configuratie](-configuratie)

🎯 Overzicht

CrunchyRolls is een volledige e-commerce sushi delivery app met:

✅ Cross-platform MAUI (iOS, Android, Windows, macCatalyst)  
✅ ASP.NET Core REST API met Swagger/OpenAPI  
✅ SQLite Database met Entity Framework Core  
✅ JWT Authenticatie met BCrypt password hashing  
✅ Offline-First Architectuur met lokale cache  
✅ Clean Architecture (5 gescheiden projecten)  
✅ MVVM Pattern met CommunityToolkit.Mvvm  
✅ Repository Pattern voor data access  
✅ 25 mock producten in 5 categorieën  



🏗️ Architectuur

Project Structuur
CrunchyRolls.sln
│
├── 1️⃣ CrunchyRolls/                          🎨 MAUI Frontend
│   ├── Views/
│   │   ├── LoginPage.xaml                    JWT authenticatie
│   │   ├── ProductsPage.xaml                 Product browsing met category filter
│   │   ├── ProductDetailPage.xaml            Product details
│   │   ├── OrderPage.xaml                    Shopping cart & checkout
│   │   └── OrderHistoryPage.xaml             Order tracking
│   ├── Resources/ (Styles, Colors, Images)
│   ├── Platforms/ (Android, iOS, macCatalyst, Windows)
│   └── MauiProgram.cs (Dependency Injection)
│
├── 2️⃣ CrunchyRolls.Core/                     🧠 Business Logic
│   ├── Services/
│   │   ├── ApiService.cs                     HTTP client met JWT auth
│   │   ├── AuthService.cs                    Login/logout management
│   │   ├── TokenService.cs                   JWT token validation
│   │   ├── SecureStorageService.cs           Secure token storage
│   │   ├── HybridProductService.cs           Offline-first products
│   │   └── HybridOrderService.cs             Offline-first orders
│   ├── ViewModels/ (MVVM met CommunityToolkit)
│   ├── Data/
│   │   ├── LocalDbContext.cs                 SQLite voor MAUI
│   │   └── Repositories/ (Local cache repos)
│   ├── Converters/ (XAML value converters)
│   └── Authentication/ (JWT models & interfaces)
│
├── 3️⃣ CrunchyRolls.Models/                   📊 Shared Data Models
│   ├── Entities/
│   │   ├── User.cs                           User met BCrypt hash
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── Order.cs
│   │   └── OrderItem.cs
│   ├── Enums/
│   │   └── OrderStatus.cs                    Pending/Confirmed/InProgress/Delivered/Cancelled
│   └── DTOs/ (LoginRequest, LoginResponse, AuthUser)
│
├── 4️⃣ CrunchyRolls.Data/                     💾 Database Layer (SQLite)
│   ├── Context/
│   │   ├── ApplicationDbContext.cs           EF Core DbContext
│   │   └── ApplicationDbContextFactory.cs    Design-time factory
│   ├── Repositories/
│   │   ├── IUserRepository.cs
│   │   ├── UserRepository.cs                 BCrypt password handling
│   │   ├── ICategoryRepository.cs
│   │   ├── CategoryRepository.cs
│   │   ├── IProductRepository.cs
│   │   ├── ProductRepository.cs
│   │   ├── IOrderRepository.cs
│   │   └── OrderRepository.cs
│   ├── Seeders/
│   │   └── DataSeeder.cs                     4 users + 25 products
│   ├── Migrations/                           EF Core migrations
│   └── Extensions/ (DI configuration)
│
└── 5️⃣ CrunchyRolls.Api/                      🌐 ASP.NET Core REST API
    ├── Controllers/
    │   ├── AuthController.cs                 JWT login/refresh endpoints
    │   ├── CategoriesController.cs           Category CRUD
    │   ├── ProductsController.cs             Product CRUD
    │   └── OrdersController.cs               Order CRUD
    ├── Program.cs (API configuration, CORS, Swagger)
    ├── appsettings.json (JWT secrets, connection string)
    └── CrunchyRolls.db (SQLite database file)

Project Dependencies

CrunchyRolls (MAUI)
    ↓ references
CrunchyRolls.Core (Services, ViewModels, Local Repos)
    ↓ references
CrunchyRolls.Models (Shared DTOs, Entities, Enums)

CrunchyRolls.Api (ASP.NET Core Web API)
    ↓ references
CrunchyRolls.Data (EF Core + Repositories)
    ↓ references
CrunchyRolls.Models

Data Flow

┌─────────────────────────┐
│   MAUI Frontend         │  Views (XAML) ←→ ViewModels (MVVM)
│   (CrunchyRolls)        │       ↓
└────────────┬────────────┘   Commands
             │                   ↓
             └──→ AuthService ──→ JWT Login
             │    HybridServices  ↓
             │         ↓       ApiService (HTTP + JWT)
             │    Local SQLite     ↓
             │    (Cache)      HTTP/JSON
             │                     ↓
┌─────────────────────────┐
│   ASP.NET Core API      │  Controllers → Repositories
│  (CrunchyRolls.Api)     │       ↓
└────────────┬────────────┘   LINQ to SQL
             │                   ↓
             └──→ EF Core ────→ SQLite Database
                  (Data)       (CrunchyRolls.db)

🛠️ Technische Stack

| Laag | Technologie | Details |
|------|-------------|---------|
| Frontend | .NET MAUI 9.0 | C# 12, XAML, Cross-platform |
| UI Pattern | MVVM | CommunityToolkit.Mvvm, ObservableProperty, RelayCommand |
| Business Logic | Services & ViewModels | Hybrid offline-first services |
| Authentication | JWT Tokens | HS256, BCrypt password hashing |
| Data Models | C# Classes | Entities, DTOs, Enums (Shared) |
| Backend API | ASP.NET Core 9.0 | REST JSON, Swagger/OpenAPI, CORS |
| Database | SQLite | Entity Framework Core 9.0, Code-First |
| Local Storage | SQLite (MAUI) | Offline cache met sync |
| HTTP Client | HttpClient | Async/await, JWT authorization header |
| Architecture | Clean Architecture | Repository Pattern, DI, Separation of Concerns |

NuGet Packages

API Project:
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.AspNetCore.Authentication.JwtBearer
- BCrypt.Net-Next (password hashing)
- Swashbuckle.AspNetCore (Swagger)

MAUI Project:
- CommunityToolkit.Mvvm
- Microsoft.EntityFrameworkCore.Sqlite
- System.IdentityModel.Tokens.Jwt

📦 Installatie

Vereisten

- .NET 9.0 SDK - https://dotnet.microsoft.com/download
- Visual Studio 2022 v17.14+ of VS Code
- Platform SDKs: Xcode (iOS), Android SDK (Android)

Setup (4 stappen)

1. Clone repository
bash
git clone https://github.com/SoufianeAbk/CrunchyRolls
cd CrunchyRolls

2. Restore dependencies
bash
dotnet restore

3. Start API (database wordt automatisch aangemaakt)
bash
cd CrunchyRolls.Api
dotnet run
API draait op: http://localhost:5000
Swagger UI: http://localhost:5000/swagger

Database `CrunchyRolls.db` wordt automatisch aangemaakt met seeded data.

4. Start MAUI Frontend (nieuw terminal venster)
bash
cd CrunchyRolls

Windows
dotnet run -f net9.0-windows10.0.19041.0

Android (requires Android SDK)
dotnet run -f net9.0-android

iOS (macOS only, requires Xcode)
dotnet run -f net9.0-ios

macCatalyst (macOS only)
dotnet run -f net9.0-maccatalyst

5. Login met test account
Email: test@example.com
Password: Password123

✅ Klaar! App connecteert automatisch met API en cached data lokaal.

🎯 Functionaliteiten

🔐 Authenticatie & Beveiliging

- JWT Token Authenticatie met HS256 algorithm
- BCrypt Password Hashing (work factor 12)
- Secure Token Storage (iOS Keychain / Android EncryptedSharedPreferences)
- Automatic Token Refresh (5 minuten voor expiry)
- Session Management met auto-logout bij token expiry

🛍️ Producten & Categorieën

- 25 mock producten in 5 categorieën (Sushi, Ramen, Dranken, Desserts, Voorgerechten)
- Category filtering met horizontal scrolling chips
- Real-time zoekfunctionaliteit in naam/beschrijving
- Voorraad status met visual indicators (groen/rood)
- Product detail pagina met afbeelding, prijs, beschrijving
- Offline-first: Data cached lokaal in SQLite

🛒 Winkelwagen

- Add/Remove items met quantity control
- Real-time totaal berekening (quantity × unit price)
- Cart persistent in lokale database
- Clear cart functionaliteit
- Stock validation voor checkout

📦 Order Management

- Bestellingen plaatsen met validatie (naam, email, adres)
- Unique order IDs (auto-increment)
- Status tracking: Pending → Confirmed → InProgress → Delivered → Cancelled
- Order cancellation (alleen niet-Delivered orders)
- Email notificaties (toekomstige feature)

📊 Order History

- Alle bestellingen gesorteerd op datum (nieuwste eerst)
- Order statistieken: Totaal bestellingen & totaal besteed
- Status filtering per OrderStatus enum
- Order details met items en products
- Pull-to-refresh voor data sync
- Kleurgecodeerde status badges (Pending=Orange, Delivered=Green, etc.)

🎨 UI/UX Features

- Dark theme met gouden accenten (#FFD700)
- Responsive design (2-column grid op portrait)
- Touch-friendly controls (44x44 pt minimum)
- Pull-to-refresh op alle lijst views
- Loading indicators tijdens API calls
- Empty state messages wanneer geen data
- Toast/Alert dialogen voor user feedback
- Smooth animations (MAUI native)

🔄 Offline-First Architectuur

- HybridProductService: API → Local Cache → Empty
- HybridOrderService: API → Local Cache → Empty
- Automatic sync elke 60 minuten
- Manual refresh via pull-to-refresh
- Works offline met cached data

🌐 API Endpoints

Base URL: http://localhost:5000/api (Development)  
Swagger: http://localhost:5000/swagger

Authentication

http
POST   /api/auth/login               Login met email/password → JWT token
POST   /api/auth/refresh             Refresh expired token


Login Request:
json
{
  "email": "test@example.com",
  "password": "Password123"
}

Login Response:
json
{
  "success": true,
  "message": "Inloggen succesvol",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "test@example.com",
    "firstName": "Test",
    "lastName": "User",
    "role": "Customer"
  }
}

Categories

http
GET     /api/categories                       Alle categorieën
GET     /api/categories/{id}                  Categorie met producten
GET     /api/categories/search?name=sushi     Zoeken op naam
POST    /api/categories                       Create (body: Category)
PUT     /api/categories/{id}                  Update (body: Category)
DELETE  /api/categories/{id}                  Delete (cascade naar products)

Products

http
GET     /api/products                         Alle producten
GET     /api/products/{id}                    Product detail met category
GET     /api/products/category/{categoryId}   Filter op categorie
GET     /api/products/search?term=roll        Zoeken in naam/beschrijving
GET     /api/products/instock                 Alleen producten op voorraad
POST    /api/products                         Create (body: Product)
PUT     /api/products/{id}                    Update (body: Product)
DELETE  /api/products/{id}                    Delete (OrderItems.ProductId → NULL)

Orders

http
GET     /api/orders                           Alle orders
GET     /api/orders/{id}                      Order met items en products
GET     /api/orders/customer/{email}          Orders per klant email
GET     /api/orders/status/{status}           Filter op status (0-4)
GET     /api/orders/recent?count=10           Laatste N orders
GET     /api/orders/revenue                   Totale omzet (delivered orders)
POST    /api/orders                           Create (body: Order met OrderItems)
PUT     /api/orders/{id}/status               Update status (body: { status: 1 })
DELETE  /api/orders/{id}                      Delete (cascade naar OrderItems)

Create Order Request:
json
{
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "deliveryAddress": "123 Main Street, Brussels",
  "status": 0,
  "orderDate": "2025-01-05T12:00:00Z",
  "orderItems": [
{
"productId": 1,
"quantity": 2,
"unitPrice": 8.50
}
]
}

💾 Database Schema

ERD (Entity Relationship Diagram)

```
┌─────────────┐
│    Users    │
│ Id          │
│ Email       │ (Unique)
│ PasswordHash│ (BCrypt)
│ FirstName   │
│ LastName    │
│ Role        │
│ IsActive    │
│ CreatedDate │
│ LastLogin   │
└─────────────┘

┌─────────────┐         ┌──────────────┐
│ Categories  │────────<│   Products   │
│ Id          │   1:N   │ Id           │
│ Name        │         │ Name         │
│ Description │         │ Description  │
└─────────────┘         │ Price        │
                        │ CategoryId   │ (FK)
                        │ StockQuantity│
                        │ ImageUrl     │
                        └──────┬───────┘
                               │
                               │ N:M via
                               ▼
┌─────────────┐         ┌──────────────┐
│   Orders    │────────<│  OrderItems  │
│ Id          │   1:N   │ Id           │
│ OrderDate   │         │ OrderId      │ (FK)
│ CustomerName│         │ ProductId    │ (FK, nullable)
│ CustomerEmail│        │ Quantity     │
│ DeliveryAddr│         │ UnitPrice    │
│ Status      │         └──────────────┘
│ TotalAmount │ (computed)
└─────────────┘
```

Table Details

Users (Authenticatie)
- Id (PK, int, auto-increment)
- Email (string, unique index, max 300)
- PasswordHash (string, BCrypt, max 255)
- FirstName, LastName (string, max 100)
- Role (string, max 50, default: "Customer")
- IsActive (bool, default: true)
- CreatedDate (DateTime, default: UtcNow)
- LastLogin (DateTime?, nullable)

Categories → Products (1:N, Cascade Delete)
- Id (PK), Name (max 100), Description (max 500)

Products
- Id (PK), Name (max 200), Description (max 1000)
- Price (decimal 18,2), CategoryId (FK → Categories)
- StockQuantity (int), ImageUrl (max 500)
- Computed: IsInStock → StockQuantity > 0

Orders → OrderItems (1:N, Cascade Delete)
- Id (PK), OrderDate (DateTime, indexed)
- CustomerName (max 200), CustomerEmail (max 300)
- DeliveryAddress (max 500)
- Status (OrderStatus enum → int, indexed)
- Computed: TotalAmount → Sum of OrderItems

OrderItems
- Id (PK), OrderId (FK → Orders, cascade)
- ProductId (FK → Products, set null on delete)
- Quantity (int), UnitPrice (decimal 18,2)

🔐 Authenticatie

JWT Token Flow
┌──────────────┐                    ┌──────────────┐
│  MAUI App    │                    │   Web API    │
│              │  1. POST /login    │              │
│  LoginPage   │───────────────────>│ AuthController
│              │ {email, password}  │              │
│              │                    │              │
│              │  2. Verify BCrypt  │  UserRepo    │
│              │<───────────────────│              │
│              │                    │              │
│              │  3. Generate JWT   │              │
│              │<───────────────────│              │
│ AuthService  │ {token, user}      │              │
│              │                    │              │
│              │  4. Store Secure   │              │
│SecureStorage │                    │              │
│              │                    │              │
│              │  5. All API Calls  │              │
│  ApiService  │───────────────────>│              │
│              │ Authorization:     │              │
│              │ Bearer <token>     │              │
└──────────────┘                    └──────────────┘

Token Details

JWT Claims:
json
{
  "email": "test@example.com",
  "given_name": "Test",
  "family_name": "User",
  "role": "Customer",
  "sub": "1",
  "jti": "unique-guid",
  "iss": "CrunchyRolls",
  "aud": "CrunchyRollsApp",
  "exp": 1704123456
}

Token Expiry: 60 minuten (configurable)  
Auto-Refresh: 5 minuten voor expiry  
Algorithm: HS256 (HMAC SHA256)  
Storage: iOS Keychain / Android EncryptedSharedPreferences

⚙️ Configuratie

API Configuration (appsettings.json)
json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=CrunchyRolls.db"
  },
  "Jwt": {
    "Secret": "VeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong!@#",
    "Issuer": "CrunchyRolls",
    "Audience": "CrunchyRollsApp",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}

MAUI API Connection (ApiService.cs)

csharp
// Development
private const string BaseUrl = "http://localhost:5000/api";

// Production
private const string BaseUrl = "https://your-api-domain.com/api";

MAUI Local Database (LocalDbContext.cs)

csharp
// SQLite database path
var dbPath = Path.Combine(FileSystem.AppDataDirectory, "CrunchyRollsLocal.db");

📝 Seeded Test Data

Users (4 accounts)

| Email | Password | Role | Status |
|-------|----------|------|--------|
| test@example.com | Password123 | Customer | Active |
| admin@example.com | AdminPassword123 | Admin | Active |
| john@example.com | JohnPassword123 | Customer | Active |
| jane@example.com | JanePassword123 | Customer | Active |

Categories (5)

1. Sushi - Verse sushi rollen en nigiri
2. Ramen - Warme Japanse noedelsoepen
3. Dranken - Frisdranken, thee en meer
4. Desserts - Zoete Japanse lekkernijen
5. Voorgerechten - Kleine hapjes en starters

Products (25 total, 5 per category)

Sushi:
- California Roll (€8.50, stock: 15)
- Salmon Nigiri (€6.75, stock: 20)
- Tuna Roll (€9.00, **out of stock**)
- Dragon Roll (€12.50, stock: 8)
- Rainbow Roll (€14.00, stock: 10)

Ramen:
- Shoyu Ramen (€12.50, stock: 10)
- Miso Ramen (€13.00, stock: 8)
- Tonkotsu Ramen (€14.50, stock: 12)
- Spicy Ramen (€13.50, **out of stock**)
- Vegetarische Ramen (€11.50, stock: 15)

Dranken:
- Groene Thee (€2.50, stock: 30)
- Ramune (€3.00, stock: 25)
- Sake (€8.50, stock: 18)
- Matcha Latte (€4.50, stock: 20)
- Yuzu Limonade (€3.50, stock: 22)

Desserts:
- Mochi (€4.50, stock: 12)
- Dorayaki (€3.75, stock: 18)
- Taiyaki (€4.00, stock: 14)
- Matcha Ice Cream (€5.50, **out of stock**)
- Anmitsu (€6.00, stock: 10)

Voorgerechten:
- Edamame (€4.00, stock: 25)
- Gyoza (€6.50, stock: 20)
- Takoyaki (€7.00, stock: 15)
- Tempura Mix (€8.50, **out of stock**)
- Yakitori (€7.50, stock: 18)

🚀 Quick Start Commands

bash
Complete setup
git clone https://github.com/SoufianeAbk/CrunchyRolls
cd CrunchyRolls
dotnet restore

Start API
cd CrunchyRolls.Api
dotnet run
API: http://localhost:5000
Swagger: http://localhost:5000/swagger

Start MAUI (nieuw terminal)
cd CrunchyRolls
dotnet run -f net9.0-windows10.0.19041.0

Database migrations (indien nodig)
cd CrunchyRolls.Api
dotnet ef database update

Clean build
dotnet clean && dotnet restore && dotnet build

🔄 Offline-First Features

Hybrid Services

HybridProductService:
1. Probeer API eerst (fresh data)
2. Cache result in local SQLite
3. Fallback naar cache bij failure
4. Auto-sync elke 60 minuten

HybridOrderService:
1. API voor create/update/delete
2. Local cache voor read operations
3. Optimistic UI updates
4. Background sync queue

Local SQLite Schema
SQL
MAUI Local Database (CrunchyRollsLocal.db)
CREATE TABLE LocalProducts (
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    Price REAL NOT NULL,
    CategoryId INTEGER,
    StockQuantity INTEGER,
    ImageUrl TEXT,
    LastSynced TEXT
);

CREATE TABLE LocalOrders (
    Id INTEGER PRIMARY KEY,
    OrderDate TEXT,
    CustomerName TEXT,
    CustomerEmail TEXT,
    Status INTEGER,
    TotalAmount REAL,
    SyncStatus INTEGER  -- 0=Synced, 1=Pending, 2=Failed
);

🎓 Development Notes

MVVM Pattern with CommunityToolkit

csharp
// ViewModel
public partial class ProductsViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Product> products = new();
    
    [ObservableProperty]
    private string searchText = string.Empty;
    
    [RelayCommand]
    private async Task LoadData()
    {
        var data = await _productService.GetProductsAsync();
        Products = new ObservableCollection<Product>(data);
    }
    
    partial void OnSearchTextChanged(string value)
    {
        FilterProducts();
    }
}

xml
<!-- XAML View -->
<ContentPage x:DataType="viewmodels:ProductsViewModel">
    <SearchBar Text="{Binding SearchText}"/>
    <CollectionView ItemsSource="{Binding Products}"/>
    <Button Command="{Binding LoadDataCommand}"/>
</ContentPage>

Error Handling Strategy
csharp
// All API calls return default on error (no crashes)
public async Task<T?> GetAsync<T>(string endpoint)
{
    try
    {
        var response = await _httpClient.GetAsync(url);
        return JsonSerializer.Deserialize<T>(content);
    }
    catch (HttpRequestException ex)
    {
        Debug.WriteLine($"Network error: {ex.Message}");
        return default;  // No crash
    }
    catch (TaskCanceledException ex)
    {
        Debug.WriteLine($"Timeout: {ex.Message}");
        return default;
    }
}

📚 Learning Resources

- MVVM Pattern: https://learn.microsoft.com/en-us/dotnet/maui/
- EF Core: https://learn.microsoft.com/en-us/ef/core/
- ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/
- .NET MAUI: https://github.com/dotnet/maui
- JWT Authentication: https://jwt.io/
- CommunityToolkit.Mvvm: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/

🗺️ Roadmap

- [ ] v1.1: Real product images van API
- [ ] v1.2: Push notifications voor order updates
- [ ] v1.3: Background sync service
- [ ] v1.4: Payment integration (Stripe/PayPal)
- [ ] v1.5: Order rating & review systeem
- [ ] v2.0: Real-time order tracking met SignalR

 Support

AI conversation logs:
- https://chatgpt.com/c/6941be9a-0c3c-8325-9139-61eb49ad471a
- https://chatgpt.com/c/69404b79-7b48-832c-80c1-6b937b394a61
- https://chatgpt.com/c/6919f1d1-6114-8327-aec2-1e7c3123015c
