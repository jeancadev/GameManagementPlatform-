# GameManagementPlatform

**GameManagementPlatform** es una plataforma robusta de gestión diseñada para administrar partidas multijugador, usuarios, salas de juego y estadísticas en tiempo real. El proyecto está construido sobre **ASP.NET Core 8.0** y **Entity Framework Core**, implementando rigurosamente los principios de **Clean Architecture** y **SOLID**, y siguiendo las mejores prácticas de desarrollo de software empresarial. La plataforma ofrece una API RESTful completa con autenticación JWT, documentación interactiva mediante Swagger y comunicación en tiempo real a través de SignalR.

---

## Índice

1. [Arquitectura y Diagrama](#arquitectura-y-diagrama)
2. [Características Principales](#características-principales)
3. [Tecnologías Utilizadas](#tecnologías-utilizadas)
4. [Requisitos Previos](#requisitos-previos)
5. [Cómo Ejecutar el Proyecto](#cómo-ejecutar-el-proyecto)
6. [Uso de la API y Ejemplos de Endpoints](#uso-de-la-api-y-ejemplos-de-endpoints)
7. [Autenticación y Seguridad](#autenticación-y-seguridad)
8. [Historias de Impacto y Logros Técnicos](#historias-de-impacto-y-logros-técnicos)
9. [Posibles Mejoras Futuras](#posibles-mejoras-futuras)
10. [Créditos / Referencias](#créditos--referencias)
11. [Contacto](#contacto)

---

## Arquitectura y Diagrama

El proyecto implementa rigurosamente el patrón **Clean Architecture**, organizándose en capas claramente definidas y con responsabilidades específicas:

- **Domain Layer:** Contiene las entidades fundamentales del negocio (User, Game, Room, etc.), reglas de negocio, enumeraciones, excepciones y contratos de dominio. Esta capa no tiene dependencias externas.

- **Application Layer:** Orquesta los flujos de aplicación a través de servicios, implementa casos de uso, define DTOs, interfaces de repositorio y servicios de infraestructura. Contiene toda la lógica de aplicación y actúa como mediador entre la capa de presentación y la de dominio.

- **Infrastructure Layer:** Proporciona implementaciones concretas de las interfaces definidas en la capa de aplicación. Incluye acceso a datos a través de Entity Framework Core, migraciones, configuraciones, repositorios, servicios externos y manejo de transacciones.

- **WebAPI Layer:** Expone la funcionalidad de la aplicación a través de endpoints RESTful. Se encarga de la configuración de middleware, autenticación JWT, Swagger, validación de solicitudes y serialización de respuestas.

Esta arquitectura garantiza alta cohesión, bajo acoplamiento, testabilidad y facilidad de mantenimiento.

![Arquitectura del Proyecto](./docs/architecture.png)

*El diagrama ilustra la Web API (ASP.NET Core / EF Core) y su conexión a la base de datos (SQL Server), destacando la clara separación entre capas conforme a los principios de Clean Architecture.*

---

## Características Principales

- **Gestión Completa de Usuarios:** Registro, autenticación, perfiles y roles.
- **Sistema de Salas:** Creación, configuración y gestión de salas de juego.
- **Administración de Partidas:** Creación, seguimiento y finalización de partidas.
- **Estadísticas en Tiempo Real:** Seguimiento de métricas y eventos de juego.
- **Comunicación en Tiempo Real:** Implementación de SignalR para notificaciones.
- **Seguridad Robusta:** Autenticación JWT, validación de tokens y protección de endpoints.
- **API Documentada:** Interfaz Swagger completa con ejemplos y descripciones.
- **Logging Detallado:** Sistema de registro para depuración y auditoría.
- **Control de Transacciones:** Manejo consistente de operaciones en la base de datos.
- **Seeding de Datos:** Datos iniciales para desarrollo y demostración.
- **Rate Limiting:** Protección contra abuso de la API.

---

## Tecnologías Utilizadas

- **Backend:**
  - ASP.NET Core 8.0
  - Entity Framework Core 8.0
  - SQL Server
  - SignalR para comunicación en tiempo real
  
- **Seguridad:**
  - JWT (JSON Web Tokens)
  - HTTPS
  - Validación de solicitudes con FluentValidation
  
- **Documentación API:**
  - Swagger/OpenAPI
  
- **Patrones y Prácticas:**
  - Clean Architecture
  - SOLID
  - Repository Pattern
  - Dependency Injection
  - Unit of Work
  - CQRS (parcialmente implementado)
  
- **Infraestructura:**
  - Capacidad para despliegue en contenedores Docker

---

## Requisitos Previos

- **.NET SDK 8.0** o superior
- **Visual Studio 2022** (Community Edition es suficiente) o **VS Code** con extensiones C#
- **SQL Server** (para ejecutar la base de datos localmente)
- **Git** para control de versiones

---

## Cómo Ejecutar el Proyecto

### Configuración Local

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/jeancadev/GameManagementPlatform.git
   cd GameManagementPlatform
   ```

2. **Restaurar paquetes NuGet:**
   ```bash
   dotnet restore
   ```

3. **Configurar la base de datos:**
   - Actualiza la cadena de conexión en `appsettings.json` según tu entorno
   - Ejecuta las migraciones para crear la base de datos:
   ```bash
   dotnet ef database update --project GameManagement.Infrastructure --startup-project GameManagement.API
   ```

4. **Ejecutar la aplicación:**
   ```bash
   dotnet run --project GameManagement.API
   ```

5. **Acceder a la documentación de la API:**
   - Abre tu navegador y visita: `https://localhost:5001/swagger`

### Usando Docker (Opcional)

1. **Construir la imagen Docker:**
   ```bash
   docker build -t gamemanagement:latest .
   ```

2. **Ejecutar el contenedor:**
   ```bash
   docker run -p 5000:80 -p 5001:443 gamemanagement:latest
   ```

---

## Uso de la API y Ejemplos de Endpoints

La API ofrece una amplia gama de endpoints para gestionar todas las entidades del sistema. A continuación se presentan ejemplos de las operaciones más comunes:

### Autenticación

**Registro de usuario:**
```http
POST /api/authentication/register
Content-Type: application/json

{
  "username": "newuser",
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Inicio de sesión:**
```http
POST /api/authentication/login
Content-Type: application/json

{
  "username": "newuser",
  "password": "SecurePassword123!"
}
```

### Gestión de Salas

**Crear una sala de juego:**
```http
POST /api/rooms
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Sala de Estrategia",
  "description": "Sala para juegos de estrategia",
  "maxPlayers": 4,
  "isPrivate": false
}
```

**Unirse a una sala:**
```http
POST /api/rooms/{roomId}/join
Authorization: Bearer [token]
```

**Listar salas disponibles:**
```http
GET /api/rooms
Authorization: Bearer [token]
```

### Gestión de Partidas

**Iniciar una partida:**
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

**Obtener detalles de una partida:**
```http
GET /api/games/{gameId}
Authorization: Bearer [token]
```

**Finalizar una partida:**
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

### Estadísticas y Perfiles

**Obtener perfil de usuario:**
```http
GET /api/users/{userId}/profile
Authorization: Bearer [token]
```

**Obtener estadísticas de juego:**
```http
GET /api/statistics/user/{userId}
Authorization: Bearer [token]
```

---

## Autenticación y Seguridad

La plataforma implementa un sistema de autenticación robusto basado en JWT (JSON Web Tokens):

- **Generación segura de tokens:** Los tokens se generan utilizando algoritmos criptográficos seguros (HMAC SHA-256).
- **Claims personalizados:** Cada token incluye información sobre el usuario (ID, nombre de usuario) y metadatos de seguridad.
- **Validación completa:** Se valida el emisor, audiencia, firma y tiempo de expiración.
- **Renovación de tokens:** Implementación de mecanismos para actualizar tokens expirados sin requerir nueva autenticación.
- **Protección de endpoints:** Todos los endpoints sensibles están protegidos mediante políticas de autorización.
- **Almacenamiento seguro de contraseñas:** Las contraseñas se almacenan como hashes utilizando algoritmos seguros (HMACSHA512).

Para utilizar un endpoint protegido, incluye el token en el encabezado de autorización:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Historias de Impacto y Logros Técnicos

### Arquitectura Limpia y Escalable
La implementación rigurosa de Clean Architecture ha creado un sistema modular donde cada componente tiene una responsabilidad claramente definida. Esta separación permite escalar horizontalmente partes específicas del sistema según las necesidades, facilitando actualizaciones y mantenimiento.

### Optimización de Rendimiento
La plataforma implementa estrategias avanzadas para optimizar el rendimiento, incluyendo:
- Implementación eficiente de consultas a la base de datos
- Patrones de diseño para reducir carga en el servidor
- Uso de caché donde es apropiado
- Paginación y filtrado para conjuntos de datos grandes

### Integración de Autenticación JWT Robusta
El sistema implementa un mecanismo de autenticación seguro utilizando JWT, permitiendo la protección de endpoints críticos mientras mantiene la escalabilidad. La implementación incluye manejo de errores detallado, registro de eventos de seguridad y configuración flexible.

### Patrones de Diseño Avanzados
Se han aplicado múltiples patrones de diseño y arquitectónicos para resolver problemas específicos:
- Repository Pattern para abstraer el acceso a datos
- Mediator Pattern para desacoplar componentes
- Factory Pattern para la creación de entidades complejas
- Unit of Work para garantizar transacciones atómicas

---

## Posibles Mejoras Futuras

### Frontend Interactivo
Desarrollo de una interfaz de usuario moderna utilizando frameworks como React, Angular o Vue.js, que consuma la API RESTful existente e implemente comunicación en tiempo real mediante SignalR.

### Escalabilidad Horizontal
Preparación de la infraestructura para soportar despliegue en múltiples instancias, implementando caching distribuido y sesiones compartidas.

### Analytics y Telemetría
Integración de herramientas de análisis para monitorear el rendimiento de la aplicación, comportamiento de usuarios y métricas de negocio.

### Implementación de Tests Automatizados
Expansión de la cobertura de pruebas con:
- Pruebas unitarias para lógica de negocio
- Pruebas de integración para flujos completos
- Pruebas de rendimiento para endpoints críticos

### Microservicios
Evolución hacia una arquitectura de microservicios para componentes específicos que requieran escalabilidad independiente.

---

## Créditos / Referencias

- **ASP.NET Core Documentation:** https://docs.microsoft.com/en-us/aspnet/core/
- **Entity Framework Core Documentation:** https://docs.microsoft.com/en-us/ef/core/
- **JWT Authentication in ASP.NET Core:** https://learn.microsoft.com/en-us/aspnet/core/security/authentication/
- **SignalR Documentation:** https://learn.microsoft.com/en-us/aspnet/core/signalr/
- **Clean Architecture:** https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- **SOLID Principles:** https://en.wikipedia.org/wiki/SOLID
- **Microsoft Identity Documentation:** https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity

---

## Contacto

Para más información, consultas técnicas o colaboración profesional, puede contactarme en:

- **Email:** jean.obandocortes@gmail.com
- **LinkedIn:** https://www.linkedin.com/in/jeancarlosobando/
- **GitHub:** [https://github.com/jeancadev]