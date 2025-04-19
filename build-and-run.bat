@echo off
echo === Construyendo y ejecutando GameManagementPlatform ===
echo.

rem Detener contenedores anteriores si existen
echo Deteniendo contenedores anteriores...
docker-compose down

rem Construir la imagen Docker
echo Construyendo imagen Docker...
docker-compose build

rem Ejecutar con docker-compose
echo Ejecutando con docker-compose...
docker-compose up -d

echo.
echo La aplicación está disponible en:
echo - http://localhost:5000
echo - http://localhost:5000/swagger
echo - Swagger UI: http://localhost:5000/swagger/index.html
echo.
echo Para ver los logs de la aplicación: docker logs -f gamemanagement-api
echo Para detener los contenedores, ejecute: docker-compose down
pause 