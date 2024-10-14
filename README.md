# Link to GitHub-Repository
https://github.com/alstadler/swen1_mtcg

# Protocol for Intermediate Hand-In

# 1. Design Decisions

1.1 Use of HTTPListener: In order to avoid using frameworks or external HTTP libraries, this project uses the built-in HttpListener class to handle manual HTTP requests and responds

1.2 Routing: The server handles routing based on HTTP methods and request URLs (/users for registration and /sessions for login). Simple conditional (if-else) checks are used to map requests to the correct controller.

1.3 Token-Based Authentications: For secure login, the server generates tokens upon successful login. For this, the SHA256 hashing algorithm is used in the TokenManager class, which combines username and current timestamp to ensure a unique token is created.

1.4 Memory-Based User Management: Users are stored inside an in-memory dictionary. The object stores the username, password, token as well as other attributes.

1.5 Layered Architecture: The project seperates components logically inside a structured project-architecture - Controllers(handle API-requests, delegate business logic), Services(contain business logic e.g. user registration and login), Models (represent application data e.g. user, card), DTOs(used for deserializing and validation of API request data), Utilities(handle token generation) as well as Interfaces.


    ------------------------------------------


# 2. Class Structure

# 2.1 Controllers
UserController.cs: Responsible for handling requests related to user registration and login. It delegates tasks to UserService for operations.

# 2.2 Services
Userservice.cs: Contains business logic related to user management. It manages user registration, authentication and token management.

# 2.3 Models
User.cs: Holds information for the user in the system. Includes attributes as Username, Password, Token, Coins and CardStack.

Card.cs: Base class for all card types in the game.

MonsterCard.cs and SpellCard.cs: Extensions for the Card class and specifies behaviour of monsters and spells, respectively.

# 2.4 DTOs
RegistrationRequest.cs and LoginRequest.cs: Simple Data Transfer Objects (DTOs) responsible for deserializing incoming JSON payloads from the client.

# 2.5 Utilities
TokenManager.cs: Responsible for generating the authentication tokens for users upon successful login.

    ------------------------------------------


# 3. API Endpoints

# 3.1 POST /users - Register a User 

Request/Payload: { "Username": "<username>", "Password": "<password>" }

Responses
Success: HTTP 201 Created (user registered)
Failure: HTTP 409 Conflict (user already exists)

# 3.2 POST /sessions - User Login
Request/Payload: { "Username": "<username>", "Password": "<password>" }

Responses
Success: HTTP 200 OK (with JSON object containing the token)
Failure: HTTP 401 Unauthorized (incorrect credentials)

