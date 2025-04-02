# GameManagementPlatform

**GameManagementPlatform** es una plataforma de gesti�n dise�ada para administrar partidas multijugador, usuarios, salas de juego y estad�sticas en tiempo real. El proyecto est� construido sobre **ASP.NET Core** y **Entity Framework Core**, aplicando los principios de **Clean Architecture** y **SOLID**, y siguiendo las mejores pr�cticas de desarrollo de software. Actualmente, el proyecto se encuentra en construcci�n (alto porcentaje de completado), con algunos problemas conocidos en la interfaz Swagger y con planes de implementar un frontend en el futuro.

> **Nota:** Este proyecto est� en desarrollo y algunos endpoints pueden presentar problemas en la interfaz de Swagger. Sin embargo, la l�gica de negocio y la arquitectura est�n completamente implementadas, lo que permite apreciar la calidad del dise�o y la aplicaci�n de buenas pr�cticas de programaci�n.

---

## �ndice

1. [Arquitectura y Diagrama](#arquitectura-y-diagrama)
2. [Requisitos Previos](#requisitos-previos)
3. [C�mo Ejecutar el Proyecto](#c�mo-ejecutar-el-proyecto)
4. [Uso de la API y Ejemplos de Endpoints](#uso-de-la-api-y-ejemplos-de-endpoints)
5. [Historias de Impacto y Logros T�cnicos](#historias-de-impacto-y-logros-t�cnicos)
6. [Posibles Mejoras Futuras](#posibles-mejoras-futuras)
7. [Cr�ditos / Referencias](#cr�ditos--referencias)
8. [Contacto](#contacto)

---

## Arquitectura y Diagrama

El proyecto se organiza en varias capas siguiendo el patr�n **Clean Architecture**:

- **Domain:** Contiene las entidades y la l�gica de negocio.  
- **Application:** Define casos de uso, DTOs, interfaces de repositorio y orquesta la comunicaci�n entre capas.  
- **Infrastructure:** Implementa el acceso a datos a trav�s de Entity Framework Core, migraciones y repositorios concretos.  
- **WebAPI:** Expone los endpoints REST y se encarga de la presentaci�n de la API.

La arquitectura tambi�n contempla el uso de autenticaci�n JWT para un acceso seguro y un mecanismo de seeding que garantiza datos de ejemplo consistentes para demostraciones.

![Arquitectura del Proyecto](./docs/architecture.png)

*El diagrama muestra la Web API (ASP.NET Core / EF Core) y su conexi�n a la base de datos (SQL Server 2019), adem�s de la divisi�n interna en capas (Domain, Application, Infrastructure).*

---

## Requisitos Previos

- **.NET SDK 7.0**  
- **Visual Studio 2022** (Community Edition es suficiente)  
- **SQL Server** (para ejecutar la base de datos localmente, si se desea)  
- **Git** para el control de versiones

---

## C�mo Ejecutar el Proyecto

### Ejecuci�n Local

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

Obtener informaci�n de un usuario:

GET /api/users/{userId}

Obtener informaci�n de una sala de juego:

GET /api/rooms/{roomId}

Obtener informaci�n de una partida:

GET /api/games/{gameId}

---

### Historias de Impacto y Logros T�cnicos

- **Contenedorizaci�n y Arquitectura Limpia:**
Implement� una soluci�n basada en Clean Architecture, separando claramente las capas de dominio, aplicaci�n, infraestructura y presentaci�n (Web API), lo que facilita el mantenimiento y la escalabilidad.

- Resoluci�n de Problemas Complejos:**
Durante el desarrollo se solucionaron desaf�os como la configuraci�n de migraciones autom�ticas con EF Core, el seeding de datos para entornos de demo y la integraci�n de autenticaci�n JWT para seguridad.

- **Preparaci�n para Integraci�n Continua:**
El proyecto est� dise�ado para integrarse f�cilmente en pipelines de CI/CD (por ejemplo, usando GitHub Actions o Azure DevOps), lo que garantiza despliegues automatizados y pruebas consistentes.

- Buenas Pr�cticas de Programaci�n:**
Se aplicaron principios SOLID y patrones de dise�o para lograr un c�digo limpio, modular y testeable, lo que es fundamental en entornos de desarrollo a gran escala.

### Posibles Mejoras Futuras

- **Implementaci�n de Frontend:**
- **Persistencia de Datos:**
- **Optimizaci�n de Rendimiento:**
- **Automatizaci�n CI/CD:**

---

### Cr�ditos / Referencias

- **ASP.NET Core:** https://docs.microsoft.com/en-us/aspnet/core/
- **Entity Framework Core:** https://docs.microsoft.com/en-us/ef/core/
- **JWT Authentication in ASP.NET Core:** https://jasonwatmore.com/post/2019/10/11/aspnet-core-3-jwt-authentication-tutorial-with-example-api
- **Docker Documentation:** https://docs.docker.com/
- **Clean Architecture:** https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html

---

### Contacto

Para m�s informaci�n o colaboraci�n, puedes contactarme en jean.obandocortes@gmail.com