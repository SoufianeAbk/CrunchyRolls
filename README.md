CrunchyRolls 🍣
Een moderne, cross-platform sushi delivery applicatie gebouwd met .NET 9.0 MAUI, ASP.NET Core Web API, en offline-first architectuur met JWT authenticatie en volledige GDPR-compliance.

📋 Inhoudsopgave

- [Overzicht](#-overzicht)
- [Architectuur](#-architectuur)
- [Installatie](#-installatie)
- [Functionaliteiten](#-functionaliteiten)
- [GDPR Compliance](#-gdpr-compliance)
- [API Endpoints](#-api-endpoints)
- [Database Schema](#-database-schema)
- [Authenticatie](#-authenticatie)
- [Configuratie](#-configuratie)

🎯 Overzicht

CrunchyRolls is een volledige e-commerce sushi delivery app met:

✅ Cross-platform MAUI (iOS, Android, Windows, macCatalyst)  
✅ ASP.NET Core REST API met Swagger/OpenAPI  
✅ SQLite Database met Entity Framework Core  
✅ JWT Authenticatie met BCrypt password hashing  
✅ GDPR-Compliance met Privacy & Consent Management  
✅ Offline-First Architectuur met lokale cache  
✅ Clean Architecture (5 gescheiden projecten)  
✅ MVVM Pattern met CommunityToolkit.Mvvm  
✅ Repository Pattern voor data access  
✅ 25 mock producten in 5 categorieën  

Architectuur & Project Structuur
```
CrunchyRolls.sln
│
├── 1️⃣ CrunchyRolls/                          🎨 MAUI Frontend
│   ├── Views/
│   │   ├── LoginPage.xaml                    JWT authenticatie
│   │   ├── ConsentPage.xaml                  ⭐ GDPR consent screen
│   │   ├── PrivacyPage.xaml                  ⭐ Privacy & gegevensbeheer
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
│   │   ├── ConsentService.cs                 ⭐ GDPR consent tracking
│   │   ├── HybridProductService.cs           Offline-first products
│   │   └── HybridOrderService.cs             Offline-first orders
│   ├── ViewModels/ (MVVM met CommunityToolkit)
│   │   ├── ConsentViewModel.cs               ⭐ Consent management
│   │   └── PrivacyViewModel.cs               ⭐ Privacy dashboard
│   ├── Data/
│   │   ├── LocalDbContext.cs                 SQLite voor MAUI
│   │   └── Repositories/ (Local cache repos)
│   ├── Converters/ (XAML value converters)
│   └── Authentication/ (JWT models & interfaces)
│
├── 3️⃣ CrunchyRolls.Models/                   📊 Shared Data Models
│   ├── Entities/
│   │   ├── User.cs                           User met BCrypt hash
│   │   ├── UserConsent.cs                    ⭐ GDPR consent records
│   │   ├── ConsentType.cs                    ⭐ Consent categorieën
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
│   │   ├── IConsentRepository.cs             ⭐ GDPR consent data access
│   │   ├── ConsentRepository.cs              ⭐ Consent CRUD operations
│   │   ├── ICategoryRepository.cs
│   │   ├── CategoryRepository.cs
│   │   ├── IProductRepository.cs
│   │   ├── ProductRepository.cs
│   │   ├── IOrderRepository.cs
│   │   └── OrderRepository.cs
│   ├── Seeders/
│   │   ├── DataSeeder.cs                     4 users + 25 products
│   │   └── GdprSeeder.cs                     ⭐ GDPR consent types
│   ├── Migrations/                           EF Core migrations
│   └── Extensions/ (DI configuration)
│
└── 5️⃣ CrunchyRolls.Api/                      🌐 ASP.NET Core REST API
    ├── Controllers/
    │   ├── AuthController.cs                 JWT login/refresh endpoints
    │   ├── ConsentController.cs              ⭐ GDPR consent endpoints
    │   ├── CategoriesController.cs           Category CRUD
    │   ├── ProductsController.cs             Product CRUD
    │   └── OrdersController.cs               Order CRUD
    ├── Program.cs (API configuration, CORS, Swagger)
    ├── appsettings.json (JWT secrets, connection string)
    └── CrunchyRolls.db (SQLite database file)
```

GDPR Compliance & Functionaliteiten

Consent Management
- Granulaire toestemming: 5 consent types (Essentieel, Functioneel, Analytics, Marketing, Profiling)
- Opt-in mechanisme: Gebruikers moeten actief toestemming geven
- Consent tracking: Alle wijzigingen worden gelogd met timestamp
- Versioning: ConsentVersion tracking voor juridische compliance
- Withdrawal: Gebruikers kunnen toestemming altijd intrekken

Privacy Dashboard (PrivacyPage)
- Inzage recht (Art. 15 GDPR): Bekijk alle opgeslagen persoonlijke data
- Rectificatie recht (Art. 16 GDPR): Wijzig persoonlijke gegevens
- Wissen recht (Art. 17 GDPR): Verwijder account en alle data
- Data portability (Art. 20 GDPR): Export data in JSON formaat
- Consent overzicht: Huidige toestemmingen per categorie
- Dataverwerking transparantie: Welke data wordt verzameld en waarom

Juridische Basis
- Legitieme belangen: Essentiële cookies (Art. 6(1)(f) GDPR)
- Toestemming: Niet-essentiële tracking (Art. 6(1)(a) GDPR)
- Contractuele noodzaak: Order processing (Art. 6(1)(b) GDPR)

Consent Types

| Type | Beschrijving | Verplicht | Juridische Basis |
|------|--------------|-----------|------------------|
| Essential | Login, sessies, winkelwagen | ✅ Ja | Legitiem belang |
| Functional | Taalvoorkeuren, UI instellingen | ❌ Nee | Toestemming |
| Analytics | Gebruiksstatistieken, performance | ❌ Nee | Toestemming |
| Marketing | Aanbiedingen, nieuwsbrieven | ❌ Nee | Toestemming |
| Profiling | Gepersonaliseerde aanbevelingen | ❌ Nee | Toestemming |

User Flow

```
1. Registratie/Login
   └─> ConsentPage (eerste keer)
       ├─> Essentieel: Auto-enabled (disabled toggle)
       ├─> Functioneel/Analytics/Marketing/Profiling: Opt-in
       └─> "Opslaan" → ProductsPage

2. Privacy Beheer (Settings)
   └─> PrivacyPage
       ├─> Tab 1: Mijn Gegevens (view/edit)
       ├─> Tab 2: Toestemmingen (wijzig consent)
       ├─> Tab 3: Dataverwerking (transparantie)
       └─> Acties: Export Data / Verwijder Account
```

Database Schema (GDPR)

```
┌─────────────┐         ┌──────────────┐
│    Users    │────────<│ UserConsents │
│ Id          │   1:N   │ Id           │
│ Email       │         │ UserId       │ (FK)
│ FirstName   │         │ ConsentTypeId│ (FK)
│ LastName    │         │ IsGranted    │
│ CreatedDate │         │ GrantedDate  │
└─────────────┘         │ RevokedDate  │
                        │ ConsentVersion│
                        │ IpAddress    │
                        └──────┬───────┘
                               │
                        ┌──────▼───────┐
                        │ ConsentTypes │
                        │ Id           │
                        │ Name         │
                        │ Description  │
                        │ IsRequired   │
                        │ Category     │
                        └──────────────┘
```

API Endpoints (GDPR)

```http
# Consent Management
GET     /api/consent/user/{userId}              Alle consents van gebruiker
GET     /api/consent/types                      Alle consent types
POST    /api/consent                            Grant/revoke consent
PUT     /api/consent/{id}                       Update consent status
GET     /api/consent/user/{userId}/active       Alleen actieve consents

# Privacy & Data Rights
GET     /api/users/{id}/data                    Export persoonlijke data (GDPR Art. 20)
DELETE  /api/users/{id}                         Verwijder account (GDPR Art. 17)
PUT     /api/users/{id}                         Wijzig persoonlijke data (GDPR Art. 16)
GET     /api/users/{id}/consents                Consent geschiedenis
```

Consent Request Body:
```json
{
  "userId": 1,
  "consentTypeId": 3,
  "isGranted": true,
  "ipAddress": "192.168.1.100",
  "consentVersion": "1.0"
}
```

Consent Tracking Voorbeeld

```json
{
  "userId": 1,
  "consents": [
    {
      "consentType": "Essential",
      "isGranted": true,
      "grantedDate": "2025-01-19T10:00:00Z",
      "revokedDate": null,
      "consentVersion": "1.0",
      "ipAddress": "192.168.1.100"
    },
    {
      "consentType": "Marketing",
      "isGranted": false,
      "grantedDate": "2025-01-19T10:00:00Z",
      "revokedDate": "2025-01-19T14:30:00Z",
      "consentVersion": "1.0",
      "ipAddress": "192.168.1.100"
    }
  ]
}
```

Functionaliteiten, Authenticatie & Beveiliging

- JWT Token Authenticatie met HS256 algorithm
- BCrypt Password Hashing (work factor 12)
- Secure Token Storage (iOS Keychain / Android EncryptedSharedPreferences)
- Automatic Token Refresh (5 minuten voor expiry)
- Session Management met auto-logout bij token expiry

Producten & Categorieën

- 25 mock producten in 5 categorieën (Sushi, Ramen, Dranken, Desserts, Voorgerechten)
- Category filtering met horizontal scrolling chips
- Real-time zoekfunctionaliteit in naam/beschrijving
- Voorraad status met visual indicators (groen/rood)
- Product detail pagina met afbeelding, prijs, beschrijving
- Offline-first: Data cached lokaal in SQLite

Winkelwagen

- Add/Remove items met quantity control
- Real-time totaal berekening (quantity × unit price)
- Cart persistent in lokale database
- Clear cart functionaliteit
- Stock validation voor checkout

Order Management

- Bestellingen plaatsen met validatie (naam, email, adres)
- Unique order IDs (auto-increment)
- Status tracking: Pending → Confirmed → InProgress → Delivered → Cancelled
- Order cancellation (alleen niet-Delivered orders)
- Email notificaties (toekomstige feature)

Order History

- Alle bestellingen gesorteerd op datum (nieuwste eerst)
- Order statistieken: Totaal bestellingen & totaal besteed
- Status filtering per OrderStatus enum
- Order details met items en products
- Pull-to-refresh voor data sync
- Kleurgecodeerde status badges (Pending=Orange, Delivered=Green, etc.)

UI/UX Features

- Dark theme met gouden accenten (#FFD700)
- Responsive design (2-column grid op portrait)
- Touch-friendly controls (44x44 pt minimum)
- Pull-to-refresh op alle lijst views
- Loading indicators tijdens API calls
- Empty state messages wanneer geen data
- Toast/Alert dialogen voor user feedback
- Smooth animations (MAUI native)

Offline-First Architectuur

- HybridProductService: API → Local Cache → Empty
- HybridOrderService: API → Local Cache → Empty
- Automatic sync elke 60 minuten
- Manual refresh via pull-to-refresh
- Works offline met cached data

API Endpoints

Base URL: `http://localhost:5000/api` (Development)  
Swagger: `http://localhost:5000/swagger`

Authentication

```http
POST   /api/auth/login               Login met email/password → JWT token
POST   /api/auth/refresh             Refresh expired token
```

Consent (GDPR)

```http
GET     /api/consent/types                      Alle consent types
GET     /api/consent/user/{userId}              Alle consents van gebruiker
GET     /api/consent/user/{userId}/active       Actieve consents
POST    /api/consent                            Grant/revoke consent
PUT     /api/consent/{id}                       Update consent
```

Categories

```http
GET     /api/categories                       Alle categorieën
GET     /api/categories/{id}                  Categorie met producten
GET     /api/categories/search?name=sushi     Zoeken op naam
POST    /api/categories                       Create (body: Category)
PUT     /api/categories/{id}                  Update (body: Category)
DELETE  /api/categories/{id}                  Delete (cascade naar products)
```

Products

```http
GET     /api/products                         Alle producten
GET     /api/products/{id}                    Product detail met category
GET     /api/products/category/{categoryId}   Filter op categorie
GET     /api/products/search?term=roll        Zoeken in naam/beschrijving
GET     /api/products/instock                 Alleen producten op voorraad
POST    /api/products                         Create (body: Product)
PUT     /api/products/{id}                    Update (body: Product)
DELETE  /api/products/{id}                    Delete (OrderItems.ProductId → NULL)
```

Orders

```http
GET     /api/orders                           Alle orders
GET     /api/orders/{id}                      Order met items en products
GET     /api/orders/customer/{email}          Orders per klant email
GET     /api/orders/status/{status}           Filter op status (0-4)
GET     /api/orders/recent?count=10           Laatste N orders
GET     /api/orders/revenue                   Totale omzet (delivered orders)
POST    /api/orders                           Create (body: Order met OrderItems)
PUT     /api/orders/{id}/status               Update status (body: { status: 1 })
DELETE  /api/orders/{id}                      Delete (cascade naar OrderItems)
```

Database Schema, ERD (Entity Relationship Diagram)

```
┌─────────────┐         ┌──────────────┐
│    Users    │────────<│ UserConsents │ ⭐ GDPR
│ Id          │   1:N   │ Id           │
│ Email       │         │ UserId       │ (FK)
│ PasswordHash│         │ ConsentTypeId│ (FK)
│ FirstName   │         │ IsGranted    │
│ LastName    │         │ GrantedDate  │
│ Role        │         │ RevokedDate  │
│ IsActive    │         │ ConsentVersion
│ CreatedDate │         │ IpAddress    │
│ LastLogin   │         └──────┬───────┘
└─────────────┘                │
                        ┌──────▼───────┐
                        │ ConsentTypes │ ⭐ GDPR
                        │ Id           │
                        │ Name         │
                        │ Description  │
                        │ IsRequired   │
                        │ Category     │
                        └──────────────┘

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

ConsentTypes (Seeded Data)

| Id | Name | Description | IsRequired | Category |
|----|------|-------------|------------|----------|
| 1 | Essential | Noodzakelijke cookies voor basisfunctionaliteit | ✅ true | Security |
| 2 | Functional | Voorkeuren en personalisatie | ❌ false | Preferences |
| 3 | Analytics | Gebruiksstatistieken en performance | ❌ false | Analytics |
| 4 | Marketing | Marketing en communicatie | ❌ false | Marketing |
| 5 | Profiling | Gedragsanalyse en profielen | ❌ false | Personalization |

Vereisten

- .NET 9.0 SDK - https://dotnet.microsoft.com/download
- Visual Studio 2022 v17.14+ of VS Code
- Platform SDKs: Xcode (iOS), Android SDK (Android)

### Setup (4 stappen)

1. Clone repository
```bash
git clone https://github.com/SoufianeAbk/CrunchyRolls
cd CrunchyRolls
```

2. Restore dependencies
```bash
dotnet restore
```

3. Start API (database wordt automatisch aangemaakt)
```bash
cd CrunchyRolls.Api
dotnet run
# API draait op: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

Database `CrunchyRolls.db` wordt automatisch aangemaakt met seeded data (inclusief GDPR consent types).

4. Start MAUI Frontend (nieuw terminal venster)
```bash
cd CrunchyRolls

# Windows
dotnet run -f net9.0-windows10.0.19041.0

# Android (requires Android SDK)
dotnet run -f net9.0-android

# iOS (macOS only, requires Xcode)
dotnet run -f net9.0-ios

# macCatalyst (macOS only)
dotnet run -f net9.0-maccatalyst
```

5. Login met test account
```
Email: test@example.com
Password: Password123
```

Klaar! App connecteert automatisch met API, toont ConsentPage bij eerste gebruik, en cached data lokaal.

Seeded Test Data

Users (4 accounts)

| Email | Password | Role | Status |
|-------|----------|------|--------|
| test@example.com | Password123 | Customer | Active |
| admin@example.com | AdminPassword123 | Admin | Active |
| john@example.com | JohnPassword123 | Customer | Active |
| jane@example.com | JanePassword123 | Customer | Active |

Consent Types (5 GDPR categorieën)

1. Essential - Noodzakelijke cookies (verplicht)
2. Functional - Voorkeuren en instellingen
3. Analytics - Gebruiksstatistieken
4. Marketing - Aanbiedingen en communicatie
5. Profiling - Gedragsanalyse

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

Roadmap

GDPR & Privacy:
- [x] v1.0: Consent management systeem
- [x] v1.0: Privacy dashboard met data export
- [x] v1.0: Right to erasure (account deletion)
- [ ] v1.1: Cookie banner compliance
- [ ] v1.2: Data retention policies (auto-delete old data)
- [ ] v1.3: Audit log voor GDPR acties
- [ ] v1.4: Privacy policy versioning

Features:
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

GDPR Resources:
- GDPR Tekst: https://eur-lex.europa.eu/eli/reg/2016/679/oj
- Gegevensbeschermingsautoriteit (BE): https://www.gegevensbeschermingsautoriteit.be/
- GDPR Checklist: https://gdpr.eu/checklist/

**Made with ❤️ for privacy-conscious sushi lovers**  
**Licensed under MIT** | **GDPR Compliant** 🔒
