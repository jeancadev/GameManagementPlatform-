# GameManagementPlatform

**GameManagementPlatform** es una plataforma de gestión diseñada para administrar partidas multijugador, usuarios, salas de juego y estadísticas en tiempo real. El proyecto está construido sobre **ASP.NET Core** y **Entity Framework Core**, aplicando los principios de **Clean Architecture** y **SOLID**, y siguiendo las mejores prácticas de desarrollo de software. Actualmente, el proyecto se encuentra en construcción (alto porcentaje de completado), con algunos problemas conocidos en la interfaz Swagger y con planes de implementar un frontend en el futuro.

> **Nota:** Este proyecto está en desarrollo y algunos endpoints pueden presentar problemas en la interfaz de Swagger. Sin embargo, la lógica de negocio y la arquitectura están completamente implementadas, lo que permite apreciar la calidad del diseño y la aplicación de buenas prácticas de programación.

---

## Índice

1. [Arquitectura y Diagrama](#arquitectura-y-diagrama)
2. [Requisitos Previos](#requisitos-previos)
3. [Cómo Ejecutar el Proyecto](#cómo-ejecutar-el-proyecto)
4. [Uso de la API y Ejemplos de Endpoints](#uso-de-la-api-y-ejemplos-de-endpoints)
5. [Historias de Impacto y Logros Técnicos](#historias-de-impacto-y-logros-técnicos)
6. [Posibles Mejoras Futuras](#posibles-mejoras-futuras)
7. [Créditos / Referencias](#créditos--referencias)
8. [Contacto](#contacto)

---

## Arquitectura y Diagrama

El proyecto se organiza en varias capas siguiendo el patrón **Clean Architecture**:

- **Domain:** Contiene las entidades y la lógica de negocio.  
- **Application:** Define casos de uso, DTOs, interfaces de repositorio y orquesta la comunicación entre capas.  
- **Infrastructure:** Implementa el acceso a datos a través de Entity Framework Core, migraciones y repositorios concretos.  
- **WebAPI:** Expone los endpoints REST y se encarga de la presentación de la API.

La arquitectura también contempla el uso de autenticación JWT para un acceso seguro y un mecanismo de seeding que garantiza datos de ejemplo consistentes para demostraciones.

![Arquitectura del Proyecto](./docs/architecture.png)

*El diagrama muestra la Web API (ASP.NET Core / EF Core) y su conexión a la base de datos (SQL Server 2019), además de la división interna en capas (Domain, Application, Infrastructure).*

---

## Requisitos Previos

- **.NET SDK 7.0**  
- **Visual Studio 2022** (Community Edition es suficiente)  
- **SQL Server** (para ejecutar la base de datos localmente, si se desea)  
- **Git** para el control de versiones

---

## Cómo Ejecutar el Proyecto

### Ejecución Local

1. **Clona el repositorio:**
   ```bash
   git clone https://github.com/tuusuario/GameManagementPlatform.git
   cd GameManagementPlatform

### Uso de la API y Ejemplos de Endpoints

Crear una sala de juego:

POST /api/rooms
Content-Type: application/json
{
  "name": "Sala de Prueba",
  "maxPlayers": 4
}

Unirse a una sala de juego:

POST /api/rooms/{roomId}/join
Content-Type: application/json
{
  "userId": "1"
}

Iniciar una partida:

POST /api/games
Content-Type: application/json
{
  "roomId": "1",
  "gameType": "Poker"
}

Crear un usuario:

POST /api/users

Obtener información de un usuario:

GET /api/users/{userId}

Obtener información de una sala de juego:

GET /api/rooms/{roomId}

Obtener información de una partida:

GET /api/games/{gameId}

---

### Historias de Impacto y Logros Técnicos

- **Contenedorización y Arquitectura Limpia:**
Implementé una solución basada en Clean Architecture, separando claramente las capas de dominio, aplicación, infraestructura y presentación (Web API), lo que facilita el mantenimiento y la escalabilidad.

- Resolución de Problemas Complejos:**
Durante el desarrollo se solucionaron desafíos como la configuración de migraciones automáticas con EF Core, el seeding de datos para entornos de demo y la integración de autenticación JWT para seguridad.

- **Preparación para Integración Continua:**
El proyecto está diseñado para integrarse fácilmente en pipelines de CI/CD (por ejemplo, usando GitHub Actions o Azure DevOps), lo que garantiza despliegues automatizados y pruebas consistentes.

- Buenas Prácticas de Programación:**
Se aplicaron principios SOLID y patrones de diseño para lograr un código limpio, modular y testeable, lo que es fundamental en entornos de desarrollo a gran escala.

### Posibles Mejoras Futuras

- **Implementación de Frontend:**
- **Persistencia de Datos:**
- **Optimización de Rendimiento:**
- **Automatización CI/CD:**

---

### Créditos / Referencias

- **ASP.NET Core:** https://docs.microsoft.com/en-us/aspnet/core/
- **Entity Framework Core:** https://docs.microsoft.com/en-us/ef/core/
- **JWT Authentication in ASP.NET Core:** https://jasonwatmore.com/post/2019/10/11/aspnet-core-3-jwt-authentication-tutorial-with-example-api
- **Docker Documentation:** https://docs.docker.com/
- **Clean Architecture:** https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html

---

### Contacto

Para más información o colaboración, puedes contactarme en jean.obandocortes@gmail.com