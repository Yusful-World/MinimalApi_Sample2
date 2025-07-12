## User Authentication & Management API (Minimal API)
This repository contains a robust and efficient User Authentication and Management API built with ASP.NET Core Minimal APIs. It provides core functionalities for user registration, login, retrieval, updating, and deletion, leveraging modern .NET features like JWT authentication, ASP.NET Core Identity, Entity Framework Core with SQL Server, Redis for caching, and gRPC for high-performance communication.

### Project Overview
This API serves as a foundational service for managing user accounts within a microservices architecture. It's designed for speed and simplicity using ASP.NET Core Minimal APIs, making it ideal for providing authentication and user data to other services (e.g., a Menu Service, Order Service, etc.).

### Features
User Registration (/sign_up): Allows new users to create accounts.

User Login (/login): Authenticates users and issues JSON Web Tokens (JWTs).

### User Management:

Retrieve all users (/get_users).

Retrieve a specific user by ID (/get_user/{id}).

Update user details (/update_user/{id}).

Delete user accounts (/delete_user/{id}).

JWT Authentication: Securely authenticates API requests using Bearer tokens.

ASP.NET Core Identity: Robust user and role management system.

Entity Framework Core: ORM for interacting with a SQL Server database.

Redis Caching: Utilizes StackExchange.Redis for output caching on specific endpoints to improve performance.

gRPC Service: Exposes a gRPC endpoint (/GetUserDetails) for efficient inter-service communication.

Swagger/OpenAPI: Provides interactive API documentation and a testing interface.

### Technologies Used
ASP.NET Core Minimal APIs: For building lightweight and fast HTTP APIs.

C#: The primary programming language.

Entity Framework Core: For database interactions with SQL Server.

ASP.NET Core Identity: For user and role management.

JWT (JSON Web Tokens): For secure authentication.

StackExchange.Redis: For distributed caching.

gRPC: For high-performance remote procedure calls.

Swagger/Swashbuckle: For API documentation.

### Getting Started
Follow these steps to get the project up and running on your local machine.

Prerequisites
.NET SDK (Version 8.0 or later recommended)

SQL Server Instance: A running SQL Server instance (localdb, SQL Server Express, or a full instance).

Redis Instance: A running Redis server. This can be a local installation (e.g., via Docker or Windows Subsystem for Linux), or a cloud-hosted instance (e.g., Redis Cloud).

### Installation
Clone the repository:

git clone <https://github.com/Yusful-World/MinimalApi_Sample2>
cd MinimalApi_Sample2 # Or your project's root directory

Restore NuGet packages:

dotnet restore

### Configuration
The application uses appsettings.json and environment variables for configuration.

appsettings.json:
Create or update appsettings.json (and appsettings.Development.json for local development) in the project root.

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_sql_server_address;Database=YourAuthDb;User Id=your_user;Password=your_password;TrustServerCertificate=True;",
    "redis": "your_redis_host:your_redis_port,password=your_redis_password,ssl=False,abortConnect=False"
  },
  "JWT": {
    "SigningKey": "YourVerySecretAndLongSigningKeyThatIsAtLeast32CharactersLong",
    "Issuer": "YourAuthServiceIssuer",
    "Audience": "YourAuthServiceAudience"
  },
  "message": "Hello from Minimal API!"
}

ConnectionStrings:DefaultConnection: Replace with your SQL Server connection string.

ConnectionStrings:redis: Replace with your Redis connection string. For Redis Cloud, this will be provided in your instance details.

JWT:SigningKey: Crucial for security. This should be a long, complex, and securely stored key. Never hardcode this in production.

JWT:Issuer / JWT:Audience: Define these for JWT validation (though currently ValidateIssuer and ValidateAudience are false in Program.cs, it's good practice to define them).

message: An example configuration value.

### Database Setup
The application uses Entity Framework Core migrations to manage the database schema.

Apply Migrations:

dotnet ef database update

This command will create the database and apply all pending migrations, setting up the necessary tables for ASP.NET Core Identity and your User model.

Seed Roles (Optional but Recommended):
The UserRoleSeed.SeedRolesAsync method is called on application startup to ensure default roles (e.g., "Admin", "User") are present. You can customize this in UserRoleSeed.cs.

### Running the Application
Run in Development:

dotnet run

The application will typically start on http://localhost:5000 (HTTP) and https://localhost:7000 (HTTPS) by default.
The Swagger UI will be available at https://localhost:7000/swagger (or your configured HTTPS port).

### API Endpoints
The following endpoints are available:

Method  Path                Description     

GET     /                   Root endpoint, returns "User gRPC is running".
GET     /message            Returns a configured message.
GET     /get_users          Retrieves a list of all registered users.
GET     /get_user/{id}      Retrieves a specific user by their ID.
POST    /sign_up            Registers a new user account.
POST    /login              Authenticates a user and returns a JWT.
PATCH   /update_user/{id}   Updates an existing user's details.
DELETE  /delete_user/{id}   Deletes a user account.
gRPC    /GetUserDetails     gRPC service for retrieving user details (inter-service communication).

### Authentication & Authorization
This API uses JWT Bearer Token Authentication.

Obtain a Token:

Make a POST request to the /login endpoint with valid user credentials.

The response will include a JWT.

Use the Token in Swagger UI:

On the Swagger UI page (/swagger), click the "Authorize" button (usually a lock icon).

In the dialog, enter your JWT in the format: Bearer YOUR_JWT_TOKEN (e.g., Bearer eyJhbGciOiJIUzI1Ni...).

Click "Authorize".

Your subsequent requests from Swagger UI will include the JWT in the Authorization header.

Use the Token in API Requests:

For any protected endpoint, include the Authorization header in your HTTP requests:

Authorization: Bearer YOUR_JWT_TOKEN

### Caching
The API utilizes Redis for output caching to enhance performance for frequently accessed data.

AddStackExchangeRedisOutputCache: Configured to use Redis as the caching store. The connection string is pulled from builder.Configuration.GetConnectionString("redis").

CacheOutput(c => c.Expire(TimeSpan.FromSeconds(120)).Tag("Get-users")): The /get_users endpoint is cached for 120 seconds and tagged with "Get-users".

IOutputCacheStore.EvictByTagAsync("Get-users", default): The cache for "Get-users" is explicitly evicted after user creation (/sign_up) and update (/update_user/{id}) to ensure data freshness.

### gRPC Service
The API includes a gRPC service, GetUserDetails, intended for efficient communication with other services.

app.MapGrpcService<GetUserDetails>(); maps the gRPC service.

The actual implementation of GetUserDetails is found in .proto file definition.

### Contributing
Contributions are welcome! Please follow standard Git Flow practices:

Fork the repository.

Create a new branch (git checkout -b feature/YourFeatureName).

Make your changes.

Commit your changes (git commit -m 'FEAT: Add Your Feature').

Push to the branch (git push origin feature/YourFeatureName).

Open a Pull Request.