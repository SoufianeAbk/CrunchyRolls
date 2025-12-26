CrunchyRolls 🍣
Een moderne, cross-platform sushi delivery applicatie gebouwd met .NET MAUI, ontworpen voor iOS, Android, macCatalyst en Windows.
CrunchyRolls is een volledige e-commerce applicatie voor sushi delivery met een intuïtief design en geavanceerde features. De applicatie is opgebouwd uit twee projecten met een clean architecture pattern.

Projectstructuur
CrunchyRolls.sln
├── CrunchyRolls/                 # MAUI Frontend Application
│   ├── Views/                    # XAML Pages (UI)
│   ├── Resources/                # Styles, Colors, Fonts, Icons
│   ├── Platforms/                # Platform-specifieke code (Android, iOS, Windows, macCatalyst)
│   ├── App.xaml & App.xaml.cs   # Application entry point
│   ├── AppShell.xaml             # Shell navigation
│   └── MauiProgram.cs            # Dependency Injection setup
│
└── CrunchyRolls.Core/            # Shared Core Library
    ├── Models/                   # Data models (Product, Order, Category, OrderItem)
    ├── Services/                 # Business logic (ProductService, OrderService, ApiService)
    ├── ViewModels/               # MVVM ViewModels
    ├── Converters/               # Value converters (StatusToColor, StatusToText, etc.)
    └── Helpers/                  # Base classes (BaseViewModel)

✨ Functionaliteiten & 🛍️ Product Management

Productcatalogus met 25 mock producten verdeeld over 5 categorieën

Sushi rollen (California Roll, Dragon Roll, Rainbow Roll, etc.)
Ramen & noedelsoepen
Japanse dranken
Desserts (Mochi, Dorayaki, Taiyaki, etc.)
Voorgerechten (Gyoza, Takoyaki, Tempura)


Categorie filtering - Snel door categorieën navigeren
Zoekfunctionaliteit - Producten zoeken op naam en beschrijving
Voorraadstatus - Realtime zichtbaarheid van product beschikbaarheid

🛒 Shopping Cart

Toevoegen/verwijderen van producten aan/van winkelwagen
Hoeveelheid aanpassen met increment/decrement buttons
Automatische berekening van subtotaal en cartmtotaal
Winkelwagen beheer - Leegmaken of individuele items verwijderen
Persistente opslag - Cart items in geheugen behouden

📦 Order Management

Klantgegevens - Naam, email, bezorgadres invoer
Order aanmaken - Bestellingen plaatsen met validatie
Order ID generatie - Unieke ID's (1001+)
Voorraad integratie - Order items koppelen aan product stock
Status tracking - Order statussen: Pending, Processing, Shipped, Delivered, Cancelled

📊 Order History

Bestellingsgeschiedenis - Alle bestellingen bekijken (gesorteerd op datum)
Statistieken - Totaal aantal bestellingen en totaal besteed
Order details - Tik op bestelling voor volledige informatie
Order verwijdering - Bestellingen (behalve Delivered) uit geschiedenis verwijderen
Status visualisatie - Kleurgecodeerde status badges

🎨 User Interface

Dark theme - Premium dark mode design met gouden accenten
Responsive layout - Optimaal voor alle schermformaten
Hero sectie - Aantrekkelijke landing page
Value propositions - Visueel aantrekkelijke trust signals
Emoji decoraties - Speelse, moderne UI elements
Toast/Alert dialogen - User feedback systeem

🌍 Cross-platform Support

Android - Volledige ondersteuning
iOS - Native iOS integratie
macCatalyst - Mac app variant
Windows - UWP/WinUI support


🛠️ Technische Stack
Framework & Talen

.NET 9.0 - Latest .NET framework
C# 12 - Modern C# syntax
.NET MAUI - Cross-platform UI framework

Architecture Patterns

MVVM (Model-View-ViewModel) - Clean separation of concerns
Dependency Injection - Services registration in MauiProgram
Repository Pattern - Service layer abstraction (ProductService, OrderService)
Observer Pattern - INotifyPropertyChanged for data binding

Libraries & Packages

Microsoft.Maui.Controls - UI components
System.ComponentModel - MVVM foundation
System.Net.Http - API communication (ready for real backend)

Styling & Theming

XAML Resource Dictionaries - Centralized styling
AppThemeBinding - Light/dark mode support
Custom Converters - Value converters for data transformation

StatusToColorConverter - Order status → color
StatusToTextConverter - Order status → Dutch text
StatusToCancelVisibilityConverter - Visibility logic
InvertedBoolConverter - Boolean inversion
IsNotZeroConverter - Zero checking




📱 Pages & Navigation
Tab-based Shell Navigation
TabPageFunctionaliteit🏠 HomeMainPageLanding page met hero, speciale aanbiedingen, categorieën📦 ProductenProductsPageProductcatalogus met filtering en zoeken🛒 WinkelwagenOrderPageWinkelwagen beheer en checkout📋 BestellingenOrderHistoryPageOrder history en statistieken
Modal Navigation

ProductDetailPage - Product details, hoeveelheid selector, add-to-cart


🏗️ Services & ViewModels
Services (Core Business Logic)
ProductService
csharp- GetCategoriesAsync() → List<Category>
- GetProductsAsync() → List<Product>
- GetProductsByCategoryAsync(categoryId) → List<Product>
- GetProductByIdAsync(productId) → Product
- Implementeert mock data (25 producten, 5 categorieën)
OrderService
csharp- AddToCart(product, quantity)
- RemoveFromCart(productId)
- UpdateQuantity(productId, quantity)
- GetCartItems() → List<OrderItem>
- ClearCart()
- CreateOrderAsync(name, email, address) → Order
- GetOrderHistoryAsync() → List<Order>
- UpdateOrderStatusAsync(orderId, status)
- CancelOrderAsync(orderId)
- DeleteOrderAsync(orderId)
- GetTotalOrdersCount(), GetTotalSpent(), GetRecentOrders()
ApiService
csharp- GetAsync<T>(endpoint)
- PostAsync<TRequest, TResponse>(endpoint, data)
- PutAsync<T>(endpoint, data)
- DeleteAsync(endpoint)
- Klaar voor echte API integratie
ViewModels
ProductsViewModel

Producten & categorieën laden
Filtering (categorie + zoekterm)
Product detail navigatie
Add-to-cart directe functie

ProductDetailViewModel

Product details display
Hoeveelheid beheer (increment/decrement)
Stock validatie
Add-to-cart met feedback

OrderViewModel

Winkelwagen management
Item verwijdering
Hoeveelheid aanpassing
Klantgegevens validatie
Order placement met error handling

OrderHistoryViewModel

Order history laden
Statistieken berekening
Order detail view
Order verwijdering (DeleteOrderAsync)
Refresh functionaliteit


🎯 Data Models
Category
csharppublic class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Product> Products { get; set; }
}
Product
csharppublic class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int StockQuantity { get; set; }
    public bool IsInStock => StockQuantity > 0;
}
Order & OrderItem
csharppublic class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string DeliveryAddress { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> OrderItems { get; set; }
    public decimal TotalAmount => OrderItems.Sum(item => item.SubTotal);
}

public enum OrderStatus { Pending, Processing, Shipped, Delivered, Cancelled }

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal => Quantity * UnitPrice;
}

🚀 Getting Started
Vereisten

.NET 9.0 SDK of hoger
Visual Studio 2022 (v17.14+) of Visual Studio Code
Platform-specifieke requirements (Xcode voor iOS, Android SDK voor Android)

Installatie
Repository klonen

bash git clone <repository-url>
cd CrunchyRolls

Dependencies herstellen

bash   dotnet restore
Project openen

bash   Visual Studio
   start CrunchyRolls.sln
   
   Of via commandline
   dotnet build

Applicatie starten

bash Windows
   dotnet run -f net9.0-windows10.0.19041.0
   
   Android
   dotnet run -f net9.0-android
   
   iOS (macOS alleen)
   dotnet run -f net9.0-ios

📊 Mock Data
De applicatie bevat 25 mock producten:
Categorieën

Sushi (5 producten) - €6,75 - €14,00
Ramen (5 producten) - €11,50 - €14,50
Dranken (5 producten) - €2,50 - €8,50
Desserts (5 producten) - €3,75 - €6,00
Voorgerechten (5 producten) - €4,00 - €8,50

Enkele items uit voorraad ter demonstratie (Tuna Roll, Spicy Ramen, Tempura Mix, Matcha Ice Cream).

🔄 API Integratie
Het systeem is voorbereikt voor echte API integratie:

Pas ApiService base URL aan in ApiService.cs:
csharp   _baseUrl = "https://your-api-url.com/api";
Wijzig useMockData vlag in ProductService.cs:
csharp   private readonly bool _useMockData = false; // Zet op false
Activeer API calls in OrderService.cs:
csharp   // Uncomment API calls
   var createdOrder = await _apiService.PostAsync<Order, Order>("orders", order);

💡 Toekomstige Uitbreidingen

 Echte database integratie (SQL Server / PostgreSQL)
 User authentication (Login/Register)
 Payment gateway integratie (Stripe, PayPal)
 Real-time order tracking met maps
 Push notifications
 Favorieten/Wishlist systeem
 Coupon & discount codes
 User reviews & ratings
 Admin dashboard
 Multilingual support (Dutch, English, etc.)

AI Hulp 

https://chatgpt.com/c/6941be9a-0c3c-8325-9139-61eb49ad471a,
https://chatgpt.com/c/69404b79-7b48-832c-80c1-6b937b394a61,
https://chatgpt.com/c/6919f1d1-6114-8327-aec2-1e7c3123015c,  
