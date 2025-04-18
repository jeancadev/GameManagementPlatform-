@echo off
echo === Construyendo y ejecutando GameManagementPlatform ===
echo.

rem Construir la imagen individual (si no se usa docker-compose)
echo Construyendo imagen Docker...
docker build -t gamemanagement-api:latest .

rem Opción 1: Ejecutar solo la aplicación
echo Para ejecutar solo la aplicación, use:
echo docker run -p 5000:80 -p 5001:443 gamemanagement-api:latest
echo.

rem Opción 2: Ejecutar con docker-compose
echo Ejecutando con docker-compose (incluye SQL Server)...
docker-compose up -d

echo.
echo La aplicación está disponible en:
echo - http://localhost:5000
echo - https://localhost:5001
echo - Swagger UI: http://localhost:5000/swagger
echo.
echo Para detener los contenedores, ejecute: docker-compose down
pause 