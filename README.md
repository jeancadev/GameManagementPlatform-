# GameManagementPlatform

**GameManagementPlatform** is a robust management platform designed to administer multiplayer games, users, game rooms, and real-time statistics. The project is built on **ASP.NET Core 8.0** and **Entity Framework Core**, rigorously implementing the principles of **Clean Architecture** and **SOLID**, and following best practices for enterprise software development. The platform offers a complete RESTful API with JWT authentication, interactive documentation via Swagger, and real-time communication through SignalR.

---

## Table of Contents

1. [Architecture and Diagram](#architecture-and-diagram)
2. [Main Features](#main-features)
3. [Technologies Used](#technologies-used)
4. [Prerequisites](#prerequisites)
5. [How to Run the Project](#how-to-run-the-project)
6. [API Usage and Endpoint Examples](#api-usage-and-endpoint-examples)
7. [Authentication and Security](#authentication-and-security)
8. [Impact Stories and Technical Achievements](#impact-stories-and-technical-achievements)
9. [Potential Future Improvements](#potential-future-improvements)
10. [Credits / References](#credits--references)
11. [Contact](#contact)

---

## Architecture and Diagram

The project rigorously implements the **Clean Architecture** pattern, organizing into clearly defined layers with specific responsibilities:

- **Domain Layer:** Contains the fundamental business entities (User, Game, Room, etc.), business rules, enumerations, exceptions, and domain contracts. This layer has no external dependencies.

- **Application Layer:** Orchestrates application flows through services, implements use cases, defines DTOs, repository interfaces, and infrastructure services. It contains all application logic and acts as a mediator between the presentation and domain layers.

- **Infrastructure Layer:** Provides concrete implementations of the interfaces defined in the application layer. Includes data access via Entity Framework Core, migrations, configurations, repositories, external services, and transaction management.

- **WebAPI Layer:** Exposes the application's functionality through RESTful endpoints. It handles middleware configuration, JWT authentication, Swagger, request validation, and response serialization.

This architecture ensures high cohesion, low coupling, testability, and ease of maintenance.

![Project Architecture](./docs/clean-architecture-diagram.svg)

*The diagram illustrates the Web API (ASP.NET Core / EF Core) and its connection to the database (SQL Server), highlighting the clear separation between layers according to Clean Architecture principles.*

---

## Main Features

- **Complete User Management:** Registration, authentication, profiles, and roles.
- **Room System:** Creation, configuration, and management of game rooms.
- **Game Management:** Creation, tracking, and completion of games.
- **Real-Time Statistics:** Tracking of metrics and game events.
- **Real-Time Communication:** Implementation of SignalR for notifications.
- **Robust Security:** JWT authentication, token validation, and endpoint protection.
- **Documented API:** Complete Swagger interface with examples and descriptions.
- **Detailed Logging:** Logging system for debugging and auditing.
- **Transaction Control:** Consistent handling of database operations.
- **Data Seeding:** Initial data for development and demonstration.
- **Rate Limiting:** Protection against API abuse.

---

## Technologies Used

- **Backend:**
  - ASP.NET Core 8.0
  - Entity Framework Core 8.0
  - SQL Server
  - SignalR for real-time communication
  
- **Security:**
  - JWT (JSON Web Tokens)
  - HTTPS
  - Request validation with FluentValidation
  
- **API Documentation:**
  - Swagger/OpenAPI
  
- **Patterns and Practices:**
  - Clean Architecture
  - SOLID
  - Repository Pattern
  - Dependency Injection
  - Unit of Work
  - CQRS (partially implemented)
  
- **Infrastructure:**
  - Capability for deployment in Docker containers

---

## Prerequisites

- **.NET SDK 8.0** or higher
- **Visual Studio 2022** (Community Edition is sufficient) or **VS Code** with C# extensions
- **SQL Server** (to run the database locally)
- **Git** for version control

---

## How to Run the Project

### Local Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/jeancadev/GameManagementPlatform.git
   cd GameManagementPlatform
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

3. **Configure the database:**
   - Update the connection string in `appsettings.json` according to your environment
   - Run migrations to create the database:
   ```bash
   dotnet ef database update --project GameManagement.Infrastructure --startup-project GameManagement.API
   ```

4. **Run the application:**
   ```bash
   dotnet run --project GameManagement.API
   ```

5. **Access the API documentation:**
   - Open your browser and visit: `https://localhost:5001/swagger`

### Using Docker (Optional)

1. **Build the Docker image:**
   ```bash
   docker build -t gamemanagement-api:latest .
   ```

2. **Run the container (API only):**
   ```bash
   docker run -p 5000:80 -p 5001:443 gamemanagement-api:latest
   ```

3. **Or run using Docker Compose (includes SQL Server):**
   ```bash
   docker-compose up -d
   ```

4. **Verify that the containers are running:**
   ```bash
   docker ps
   ```

5. **To stop the containers:**
   ```bash
   docker-compose down
   ```

6. **Using automation scripts:**
   - On Windows: run `build-and-run.bat`
   - On Linux/Mac: run `./build-and-run.sh` (may require `chmod +x build-and-run.sh` permissions)

The application will be available at:
- http://localhost:5000
- https://localhost:5001
- Swagger UI: http://localhost:5000/swagger

---

## API Usage and Endpoint Examples

The API offers a wide range of endpoints to manage all system entities. Below are examples of the most common operations:

### Authentication

**User Registration:**
```http
POST /api/authentication/register
Content-Type: application/json

{
  "username": "newuser",
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Login:**
```http
POST /api/authentication/login
Content-Type: application/json

{
  "username": "newuser",
  "password": "SecurePassword123!"
}
```

### Room Management

**Create a game room:**
```http
POST /api/rooms
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Strategy Room",
  "description": "Room for strategy games",
  "maxPlayers": 4,
  "isPrivate": false
}
```

**Join a room:**
```http
POST /api/rooms/{roomId}/join
Authorization: Bearer [token]
```

**List available rooms:**
```http
GET /api/rooms
Authorization: Bearer [token]
```

### Game Management

**Start a game:**
```http
POST /api/games
Authorization: Bearer [token]
Content-Type: application/json

{
  "roomId": "1",
  "gameType": "Poker",
  "configuration": {
    "initialChips": 1000,
    "blindLevels": 15
  }
}
```

**Get game details:**
```http
GET /api/games/{gameId}
Authorization: Bearer [token]
```

**End a game:**
```http
POST /api/games/{gameId}/end
Authorization: Bearer [token]
Content-Type: application/json

{
  "winnerId": "5",
  "finalScores": [
    {"userId": "5", "score": 1500},
    {"userId": "2", "score": 800}
  ]
}
```

### Statistics and Profiles

**Get user profile:**
```http
GET /api/users/{userId}/profile
Authorization: Bearer [token]
```

**Get game statistics:**
```http
GET /api/statistics/user/{userId}
Authorization: Bearer [token]
```

---

## Authentication and Security

The platform implements a robust authentication system based on JWT (JSON Web Tokens):

- **Secure token generation:** Tokens are generated using secure cryptographic algorithms (HMAC SHA-256).
- **Custom claims:** Each token includes user information (ID, username) and security metadata.
- **Full validation:** Issuer, audience, signature, and expiration time are validated.
- **Token renewal:** Mechanisms are implemented to update expired tokens without requiring re-authentication.
- **Endpoint protection:** All sensitive endpoints are protected using authorization policies.
- **Secure password storage:** Passwords are stored as hashes using secure algorithms (HMACSHA512).

To use a protected endpoint, include the token in the authorization header:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Impact Stories and Technical Achievements

### Clean and Scalable Architecture
The rigorous implementation of Clean Architecture has created a modular system where each component has a clearly defined responsibility. This separation allows for horizontal scaling of specific parts of the system as needed, facilitating updates and maintenance.

### Performance Optimization
The platform implements advanced strategies to optimize performance, including:
- Efficient database query implementation
- Design patterns to reduce server load
- Use of cache where appropriate
- Pagination and filtering for large datasets

### Integration of Robust JWT Authentication
The system implements a secure authentication mechanism using JWT, allowing the protection of critical endpoints while maintaining scalability. The implementation includes detailed error handling, security event logging, and flexible configuration.

### Advanced Design Patterns
Multiple design and architectural patterns have been applied to solve specific problems:
- Repository Pattern to abstract data access
- Mediator Pattern to decouple components
- Factory Pattern for creating complex entities
- Unit of Work to ensure atomic transactions

---

## Potential Future Improvements

### Interactive Frontend
Development of a modern user interface using frameworks like React, Angular, or Vue.js, which consumes the existing RESTful API and implements real-time communication via SignalR.

### Horizontal Scalability
Preparing the infrastructure to support deployment across multiple instances, implementing distributed caching and shared sessions.

### Analytics and Telemetry
Integration of analysis tools to monitor application performance, user behavior, and business metrics.

### Implementation of Automated Tests
Expansion of test coverage with:
- Unit tests for business logic
- Integration tests for complete flows
- Performance tests for critical endpoints

### Microservices
Evolution towards a microservices architecture for specific components requiring independent scalability.

---

## Credits / References

- **ASP.NET Core Documentation:** https://docs.microsoft.com/en-us/aspnet/core/
- **Entity Framework Core Documentation:** https://docs.microsoft.com/en-us/ef/core/
- **JWT Authentication in ASP.NET Core:** https://learn.microsoft.com/en-us/aspnet/core/security/authentication/
- **SignalR Documentation:** https://learn.microsoft.com/en-us/aspnet/core/signalr/
- **Clean Architecture:** https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- **SOLID Principles:** https://en.wikipedia.org/wiki/SOLID
- **Microsoft Identity Documentation:** https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity

---

## Contact

For more information, technical inquiries, or professional collaboration, you can contact me at:

- **Email:** jean.obandocortes@gmail.com
- **LinkedIn:** https://www.linkedin.com/in/jeancarlosobando/
- **GitHub:** https://github.com/jeancadev
