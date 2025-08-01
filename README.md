# PharmaLink

[![.NET 9](https://img.shields.io/badge/.NET-9.0-blueviolet)](https://dotnet.microsoft.com/) [![Angular](https://img.shields.io/badge/Frontend-Angular-red)](https://github.com/HosamAyoub/PharmaLink-Angular)

PharmaLink is a modern web platform that connects all pharmacies in one place, empowering users to easily find medicines, check real-time availability, and access comprehensive drug information. The platform leverages AI to suggest drug alternatives based on user history and pharmacy stock, ensuring users always have access to the medications they need. PharmaLink also provides a full suite of e-commerce features, including search, filtering, categories, and a seamless shopping experience.


## Table of Contents
- [Project Overview](#project-overview)
- [Features](#features)
- [System Architecture](#system-architecture)
- [Database Design (ERD)](#database-design-erd)
- [API Endpoints](#api-endpoints)
- [Frontend](#frontend)
- [Getting Started](#getting-started)
- [Contributing](#contributing)
- [License](#license)
- [API Usage Examples](#api-usage-examples)


## Project Overview
PharmaLink addresses the challenge of medicine accessibility by aggregating pharmacy inventories and providing users with up-to-date information on where to find their required drugs. The platform not only helps users locate medicines, especially during shortages, but also empowers them with detailed drug data and intelligent AI-driven suggestions for alternatives.


## Features
- **Unified Pharmacy Search:** Instantly find which pharmacies have your required medicine in stock, including during shortages.
- **Comprehensive Drug Information:** View active ingredients, side effects, usage instructions, and more for every drug.
- **AI-Powered Alternatives:** Receive smart suggestions for alternative drugs based on your medical history, preferences, and real-time pharmacy stock.
- **Personalized Experience:** Recommendations are tailored using user history and location for maximum relevance.
- **E-Commerce Essentials:** Search, filter, browse by categories, manage cart, and place orders.
- **Order Tracking:** Track your orders and view order history.
- **Favorites:** Save favorite drugs for quick access.
- **Secure Authentication:** Role-based access for users and pharmacies.
- **Admin Dashboard:** Manage drugs, pharmacies, users, and orders.
- **RESTful API:** Clean, well-documented endpoints for all core features.


## System Architecture
PharmaLink is built with a scalable, maintainable architecture:
- **Backend:** ASP.NET Core (.NET 9), Entity Framework Core, Identity for authentication and authorization.
- **Frontend:** Angular ([PharmaLink-Angular](https://github.com/HosamAyoub/PharmaLink-Angular))
- **Database:** Relational (see ERD below)
- **AI Integration:** For drug alternative suggestions (ongoing development)
- **RESTful API:** Clean separation of concerns, following best practices.

### Backend Structure
- **Controllers:** Handle HTTP requests and responses (e.g., `AccountController` for registration and login).
- **Services:** Business logic, including registration, login, role assignment, and AI suggestions (e.g., `AccountService`, `RoleService`).
- **Repositories:** Data access layer, abstracting database operations (e.g., `AccountRepository`, `RoleRepository`).
- **DTOs:** Data Transfer Objects for clean API contracts.
- **Identity:** User and role management.

### Key Backend Components
- **Account Management:** Registration, login, and user profile creation with role assignment.
- **Pharmacy & Drug Management:** CRUD operations for pharmacies and drugs, including stock management.
- **Order & Cart Management:** E-commerce features for placing and tracking orders.
- **Favorites & Requests:** Users can save favorite drugs and request out-of-stock medicines.
- **AI Suggestions:** (Planned) Integration with AI services to suggest alternatives based on user and stock data.


## Database Design (ERD)
The database is designed to support all core features and relationships:
- **Users:** Stores user information, roles, and authentication data.
- **Pharmacies:** Details about each pharmacy, including location and working hours.
- **Drugs:** Comprehensive drug information (name, active ingredient, side effects, etc.).
- **Pharmacy Stock:** Tracks which pharmacies have which drugs, including quantity and price.
- **Orders & Cart:** E-commerce functionality for purchasing and tracking medicines.
- **Favorites:** Users can save favorite drugs.
- **Requests:** For out-of-stock or special-order medicines.

*Refer to the ERD diagram for detailed relationships and attributes.*


## API Endpoints
PharmaLink exposes a rich set of RESTful endpoints, including but not limited to:
- **Authentication:** Register, login, and role management.
- **Pharmacy Management:** Add, update, and list pharmacies.
- **Drug Management:** Add, update, search, and get details for drugs.
- **Stock Management:** Update and query pharmacy stock.
- **Order Management:** Place orders, view order history, and manage cart.
- **Favorites:** Add/remove favorite drugs.
- **AI Suggestions:** Get alternative drug recommendations (planned/ongoing).

*See the endpoints diagram for a full list of available routes. Endpoints are organized by resource and follow RESTful conventions for clarity and maintainability.*


## Frontend
The frontend is built with Angular and provides a modern, responsive user experience.
- **Repository:** [PharmaLink-Angular](https://github.com/HosamAyoub/PharmaLink-Angular)
- **Features:** User registration/login, pharmacy and drug search, detailed drug pages, cart and order management, AI-powered suggestions, and more.
- **Screens:** Home, Search, Drug Details, Pharmacy List, Cart, Orders, Profile, Admin Dashboard, etc.
- **User Flows:** The UI is designed for both end-users and pharmacy admins, with dedicated screens for each role.


## Getting Started
### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js & Angular CLI](https://angular.io/guide/setup-local) (for frontend)
- SQL Server or compatible database

### Backend Setup
1. Clone this repository.
2. Configure your database connection in `appsettings.json`.
3. Run database migrations:
```
dotnet ef database update
```
4. Build and run the API:
```
dotnet run --project PharmaLink_API
```

### Frontend Setup
1. Clone the [PharmaLink-Angular](https://github.com/HosamAyoub/PharmaLink-Angular) repository.
2. Install dependencies:
```
npm install
```
3. Run the Angular app:
```
ng serve
```


## API Usage Examples

### Register a New User
**Endpoint:** `POST /api/Account/Register`

**Request Body:**
```json
{
  "userName": "john_doe",
  "email": "john@example.com",
  "passwordHash": "YourPassword123!",
  "confirmPassword": "YourPassword123!",
  "phoneNumber": "+1234567890",
  "patient": {
    "name": "John Doe",
    "gender": 0,
    "dateOfBirth": "1990-01-01",
    "country": "USA",
    "address": "123 Main St",
    "patientDiseases": "Diabetes",
    "patientDrugs": "Metformin"
  },
  "pharmacy": null
}
```

**Success Response:**
```json
{
  "message": "User registered successfully\njohn_doe\njohn@example.com"
}
```

**Failure Response:**
```json
{
  "errors": {
    "email": [
      "The Email field must be a valid email address.",
      "The Email field is already in use."
    ]
  }
}
```

### Login
**Endpoint:** `POST /api/Account/Login`

**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "YourPassword123!",
  "rememberMe": true
}
```

**Success Response:**
```json
{
  "token": "<JWT_TOKEN>",
  "expiration": "2024-12-31T23:59:59Z",
  "userName": "john_doe"
}
```

**Failure Response:**
```json
{
  "message": "Invalid email or password."
}
```


## Contributing
Contributions are welcome! Please open issues or submit pull requests for new features, bug fixes, or improvements. For major changes, please discuss them in an issue first.


## License
This project is licensed under the MIT License.


## Additional Details
- **ERD & Diagrams:** The included ERD and diagrams provide a comprehensive overview of the data model, backend structure, and user flows. These resources are invaluable for onboarding new developers and for understanding the relationships between entities.
- **Extensibility:** The backend is designed with extensibility in mind, making it easy to add new features such as notifications, advanced analytics, or integrations with external health systems.
- **Security:** All sensitive operations are protected by robust authentication and authorization mechanisms using ASP.NET Core Identity.
- **AI Integration:** The platform is architected to support advanced AI features, such as personalized drug suggestions and shortage prediction, leveraging user data and real-time stock information.


*PharmaLink - Making medicine accessible, transparent, and intelligent for everyone.*