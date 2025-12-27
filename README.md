CrunchyRolls 🍣
Een moderne, cross-platform sushi delivery applicatie gebouwd met .NET 9.0 en volledige integratie tussen frontend, backend en database.

📋 Inhoudsopgave

Overzicht
5 Projecten Architectuur
Installatie
Functionaliteiten
API Endpoints


🎯 Overzicht
CrunchyRolls is een volledige e-commerce sushi delivery app met:
✅ Cross-platform MAUI (iOS, Android, Windows, macCatalyst)
✅ ASP.NET Core REST API met Swagger
✅ SQL Server database met EF Core
✅ Clean Architecture (5 gescheiden projecten)
✅ 25 mock producten in 5 categorieën
✅ MVVM + Repository pattern

🏗️ 5 Projecten Architectuur
CrunchyRolls.sln
│
├── 1️⃣ CrunchyRolls/                       🎨 MAUI Frontend
│   ├── Views/ (MainPage, ProductsPage, OrderPage, OrderHistoryPage, ProductDetailPage)
│   ├── Resources/ (Styles, Colors, Images)
│   ├── Platforms/ (Android, iOS, MacCatalyst, Windows)
│   └── MauiProgram.cs (Dependency Injection)
│
├── 2️⃣ CrunchyRolls.Core/                  🧠 Business Logic
│   ├── Services/ (ApiService, ProductService, OrderService)
│   ├── ViewModels/ (ProductsViewModel, OrderViewModel, OrderHistoryViewModel)
│   ├── Converters/ (StatusToColor, StatusToText, ImageSourceConverter)
│   └── Helpers/ (BaseViewModel)
│
├── 3️⃣ CrunchyRolls.Models/                📊 Shared Data Models
│   ├── Entities/ (Category, Product, Order, OrderItem)
│   ├── Enums/ (OrderStatus: Pending, Processing, Shipped, Delivered, Cancelled)
│   └── DTOs/ (CategoryDto, ProductDto, OrderDto, OrderItemDto)
│
├── 4️⃣ CrunchyRolls.Data/                  💾 Database Layer (SQL Server)
│   ├── Context/ (ApplicationDbContext - EF Core)
│   ├── Repositories/ (IRepository, CategoryRepository, ProductRepository, OrderRepository)
│   ├── Seeders/ (DataSeeder - 25 mock producten)
│   └── Extensions/ (ServiceCollectionExtensions - DI setup)
│
└── 5️⃣ CrunchyRolls.Api/                   🌐 ASP.NET Core REST API
    ├── Controllers/ (CategoriesController, ProductsController, OrdersController)
    ├── Program.cs (API configuration, Swagger/OpenAPI)
    └── appsettings.json (Connection string, Logging)
Project Relaties
CrunchyRolls (MAUI)
    ↓ references
CrunchyRolls.Core (Services & ViewModels)
    ↓ references
CrunchyRolls.Models (Shared DTOs & Entities)
    
CrunchyRolls.Api (ASP.NET Core)
    ↓ references
CrunchyRolls.Data (EF Core + SQL Server)
    ↓ references
CrunchyRolls.Models

🛠️ Technische Stack
LaagTechnologieDetailsFrontend.NET MAUIC# 12, XAML, Cross-platform (iOS, Android, Windows, macCatalyst)Business LogicServices & ViewModelsMVVM Pattern, Dependency InjectionData ModelsC# ClassesEntities, DTOs, Enums (Shared across all projects)Backend APIASP.NET Core 9.0REST JSON, Swagger/OpenAPI, CORSDatabaseSQL Server LocalDBEntity Framework Core 9.0, Code-First MigrationsArchitectureClean ArchitectureRepository Pattern, Separation of ConcernsHTTP ClientHttpClientApiService for API communication

📦 Installatie
Vereisten

.NET 9.0 SDK https://dotnet.microsoft.com/download
Visual Studio 2022 v17.14+ of VS Code
SQL Server LocalDB (installeert met Visual Studio)
Platform SDKs: Xcode (iOS), Android SDK (Android)

Setup (5 stappen)
bash 1. Clone repository
git clone <https://github.com/SoufianeAbk/CrunchyRolls>
cd CrunchyRolls

 2. Restore dependencies
dotnet restore

 3. Database wordt automatisch aangemaakt bij API start
cd CrunchyRolls.Api
dotnet run
 Swagger UI: http://localhost:5000/swagger

 4. MAUI Frontend starten (ander terminal)
cd CrunchyRolls
dotnet run -f net9.0-windows10.0.19041.0  # Windows

dotnet run -f net9.0-android              # Android

dotnet run -f net9.0-ios                  # iOS (macOS only)

 5. Klaar! 🎉
Opmerking: API en MAUI draait op verschillende poorten. API haalt mock data totdat je API connection inschakelt.

🏗️ Architectuur
┌─────────────────────────┐
│   MAUI Frontend         │  Views (XAML) ←→ ViewModels (MVVM)
│   (CrunchyRolls)        │       ↓
└────────────┬────────────┘   Commands
             │                   ↓
             └──→ Services ──→ ApiService
                  (Core)         ↓
                                HTTP
                                 ↓
┌─────────────────────────┐
│   ASP.NET Core API      │  Controllers → Repositories
│  (CrunchyRolls.Api)     │       ↓
└────────────┬────────────┘   LINQ to SQL
             │                   ↓
             └──→ EF Core ────→ SQL Server
                  (Data)      Database
Data Flow
User Action → View → ViewModel → Service → API Controller → Repository → Database
Database

SQL Server LocalDB (Development)
4 Main Tables: Categories, Products, Orders, OrderItems
Relationships: Category ← Products, Orders → OrderItems ← Products
Automatic Seeding: 25 mock products (5 categories × 5 products)


🎯 Functionaliteiten
🛍️ Producten & Categorieën

25 mock producten in 5 categorieën (Sushi, Ramen, Dranken, Desserts, Voorgerechten)
Categorie filtering & zoekfunctionaliteit
Realtime voorraad status
Product detail pagina met afbeelding, prijs, beschrijving

🛒 Winkelwagen

Add/Remove items, update quantity
Automatische totaal berekening
Cart persistent in geheugen
Clear cart functionaliteit

📦 Order Management

Bestellingen plaatsen met validatie (naam, email, adres)
Unique order IDs (1001+)
Status tracking: Pending → Processing → Shipped → Delivered
Order verwijdering (niet Delivered status)

📊 Order History

Alle bestellingen gesorteerd op datum
Statistieken: Totaal bestellingen & totaal besteed
Order details op tik
Kleurgecodeerde status badges
Pull-to-refresh

🎨 UI/UX

Dark theme met gouden accenten
Responsive design (iOS, Android, Windows)
Hero section, trust signals, emoji decoraties
Toast/Alert dialogen voor feedback


🌐 API Endpoints
Base URL: http://localhost:5000/api (Development)
Swagger: http://localhost:5000/swagger
Categories
httpGET     /api/categories                      # Alle categorieën
GET     /api/categories/{id}                 # Met producten
GET     /api/categories/search?name=         # Zoeken
POST    /api/categories                      # Create
PUT     /api/categories/{id}                 # Update
DELETE  /api/categories/{id}                 # Delete (cascade)
Products
httpGET     /api/products                        # Alle producten
GET     /api/products/{id}                   # Product detail
GET     /api/products/category/{categoryId}  # Per categorie
GET     /api/products/search?term=           # Zoeken
GET     /api/products/instock                # Alleen beschikbaar
POST    /api/products                        # Create
PUT     /api/products/{id}                   # Update
DELETE  /api/products/{id}                   # Delete
Orders
httpGET     /api/orders                          # Alle orders
GET     /api/orders/{id}                     # Met items
GET     /api/orders/customer/{email}         # Per klant
GET     /api/orders/status/{status}          # Per status
GET     /api/orders/recent?count=10          # Recente
GET     /api/orders/revenue                  # Totale inkomsten
POST    /api/orders                          # Create
PUT     /api/orders/{id}/status              # Update status
DELETE  /api/orders/{id}                     # Delete

💾 Database Schema
sqlCategories        Products          Orders           OrderItems
│ Id              │ Id              │ Id             │ Id
│ Name            │ Name            │ OrderDate      │ OrderId (FK)
│ Description     │ Price           │ CustomerName   │ ProductId (FK)
└──────────────────│ CategoryId (FK) │ CustomerEmail  │ Quantity
                  │ StockQuantity   │ DeliveryAddr   │ UnitPrice
                  │                 │ Status (Enum)  └─────────
                  └─────────────────└────────────────
Relationships: Category ← Products, Orders → OrderItems → Products

🚀 Quick Start Commands
bash# Complete setup
git clone <https://github.com/SoufianeAbk/CrunchyRolls> && cd CrunchyRolls
dotnet restore
cd CrunchyRolls.Api && dotnet run &        # API in background
cd ../CrunchyRolls && dotnet run -f net9.0-windows10.0.19041.0

# Database only
cd CrunchyRolls.Api && dotnet ef database update --startup-project .

# Clean build
dotnet clean && dotnet restore && dotnet build

📝 Configuration
API Connection (CrunchyRolls.Core/Services/ApiService.cs)
csharp// Dev
_baseUrl = "http://localhost:5000/api";

Production
_baseUrl = "https://your-api-domain.com/api";
Database Connection (CrunchyRolls.Api/appsettings.json)
json{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=CrunchyRolls;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}

💡 Features & Data
25 Mock Products (Seeded Automatically)

Sushi (5): California Roll, Salmon Nigiri, Tuna Roll, Dragon Roll, Rainbow Roll
Ramen (5): Shoyu, Miso, Tonkotsu, Spicy, Vegetarian
Dranken (5): Green Tea, Ramune, Sake, Matcha Latte, Yuzu Limonade
Desserts (5): Mochi, Dorayaki, Taiyaki, Matcha Ice Cream, Anmitsu
Voorgerechten (5): Edamame, Gyoza, Takoyaki, Tempura Mix, Yakitori

Order Status Enum

Pending (0) - Zojuist geplaatst
Processing (1) - In keuken
Shipped (2) - Onderweg
Delivered (3) - Afgeleverd
Cancelled (4) - Geannuleerd


🔄 Switching to Real API

Uncomment API calls in CrunchyRolls.Core/Services/OrderService.cs
Set _useMockData = false in ProductService.cs
Update API base URL in ApiService.cs
Ensure API is running

🎓 Learning Resources

MVVM Pattern: https://learn.microsoft.com/en-us/dotnet/maui/
EF Core: https://learn.microsoft.com/en-us/ef/core/
ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/
.NET MAUI: https://github.com/dotnet/maui

📧 Support & AI
https://chatgpt.com/c/6941be9a-0c3c-8325-9139-61eb49ad471a, https://chatgpt.com/c/69404b79-7b48-832c-80c1-6b937b394a61, https://chatgpt.com/c/6919f1d1-6114-8327-aec2-1e7c3123015c.
