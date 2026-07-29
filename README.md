# MaizWareAPI

MaizWareAPI es el backend REST de MaizWare, desarrollado en C# con ASP.NET Core y Entity Framework Core. Expone los servicios necesarios para autenticar usuarios, consultar perfiles, registrar estados de animo, almacenar conversaciones del asistente IA y conectar la app movil Expo con SQL Server.

## Tecnologias

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server Express
- SQL Server Management Studio
- Microsoft Visual Studio
- OpenAPI
- CORS para consumo desde Expo

## Estructura del Proyecto

```text
MaizWareAPI/
  Controllers/
    AuthController.cs
    UsersController.cs
    MoodEntriesController.cs
    AiChatController.cs

  DTOs/
    ApiDtos.cs

  Models/
    MaizWareContext.cs
    User.cs
    UserProfile.cs
    Role.cs
    UserRole.cs
    Emotion.cs
    MoodEntry.cs
    AiConversation.cs
    AiMessage.cs

  Program.cs
  appsettings.json
  MaizWareAPI.csproj
```

## Base de Datos

La API trabaja con la base de datos relacional `MaizWare` en SQL Server.

Cadena de conexion local:

```text
Server=.\SQLEXPRESS;Database=MaizWare;Trusted_Connection=True;TrustServerCertificate=True;
```

> La conexion usa autenticacion integrada de Windows, por lo que no se guardan usuarios ni contrasenas de SQL Server dentro del proyecto.

## Endpoints

### Autenticacion

```http
POST /api/auth/login
POST /api/auth/register
```

### Usuarios y Roles

```http
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
GET    /api/users/roles
POST   /api/users/{id}/roles
```

### Estados de Animo

```http
GET    /api/moodentries/emotions
GET    /api/moodentries/user/{userId}
GET    /api/moodentries/{id}
POST   /api/moodentries
PUT    /api/moodentries/{id}
DELETE /api/moodentries/{id}
```

### Chat IA

```http
GET    /api/ai-chat/user/{userId}/conversations
GET    /api/ai-chat/conversations/{conversationId}
POST   /api/ai-chat/conversations
POST   /api/ai-chat/conversations/{conversationId}/messages
PUT    /api/ai-chat/conversations/{conversationId}/close
DELETE /api/ai-chat/conversations/{conversationId}
```

## Ejecucion Local

1. Abrir la solucion `MaizWareAPI.slnx` en Microsoft Visual Studio.
2. Confirmar que SQL Server Express este activo.
3. Verificar que exista la base de datos `MaizWare`.
4. Ejecutar el proyecto con el perfil `https`.

URLs locales:

```text
https://localhost:7127
http://localhost:5042
```

La pagina raiz de la API muestra una vista simple con los endpoints principales y enlace al JSON de OpenAPI.

## Integracion con MaizWareApp

La app Expo consume esta API desde el cliente centralizado:

```text
MaizWareApp/src/services/apiClient.ts
```

Rutas esperadas:

```text
Web:     https://localhost:7127/api
Android: https://10.0.2.2:7127/api
```

## Validacion

Comando de compilacion:

```bash
dotnet build
```

Estado actual:

- API compila correctamente.
- Entity Framework Core esta configurado con SQL Server.
- CORS esta abierto para desarrollo con Expo.
- Controladores RESTful listos para autenticacion, usuarios, estados de animo y chat IA.
- No se incluyen carpetas generadas como `bin`, `obj` o `.vs` en el repositorio.

## Nota Tecnica

Durante la compilacion puede aparecer una advertencia `NU1903` asociada a `Microsoft.OpenApi` como dependencia transitoria. El proyecto se mantiene compilable con `Microsoft.AspNetCore.OpenApi` y listo para desarrollo local.
