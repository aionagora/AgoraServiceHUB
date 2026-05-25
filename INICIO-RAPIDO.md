# 🚀 INICIO RÁPIDO - AgoraHub360 ERP

## ⚡ 3 Pasos para iniciar:

### 1️⃣ Verificar SQL Server
```cmd
0-test-sqlserver.bat
```

### 2️⃣ Iniciar API (Terminal 1)
```cmd
1-iniciar-api.bat
```

### 3️⃣ Iniciar Blazor (Terminal 2)
```cmd
2-iniciar-blazor.bat
```

### 4️⃣ Abrir navegador
```
https://localhost:5002
```

---

## 📁 Documentación Completa

- **[INSTRUCCIONES-COMPLETAS.md](./INSTRUCCIONES-COMPLETAS.md)** - Guía completa de solución de problemas
- **[GUIA-DIAGNOSTICO.txt](./GUIA-DIAGNOSTICO.txt)** - Comandos PowerShell de diagnóstico

---

## ⚙️ Configuración

| Componente | URL |
|------------|-----|
| API | https://localhost:7001/ |
| Blazor | https://localhost:5002/ |
| SQL Server | 192.168.88.14:56885 |
| Base de Datos | db_AgoraERP_Core |

---

## ❗ Problemas Comunes

### API no inicia
- Verifica que SQL Server esté accesible (`0-test-sqlserver.bat`)
- Revisa errores en la terminal de la API

### Blazor no conecta con API
- Asegúrate de que la API esté corriendo primero
- Verifica que no haya errores CORS en la consola del navegador (F12)

### No redirige al login
- Abre DevTools (F12) → Console
- Busca: `🔧 API Base URL configurada: https://localhost:7001/`
- Si no aparece, hay un problema de configuración

---

## 📞 Soporte

Si sigues teniendo problemas, abre **INSTRUCCIONES-COMPLETAS.md** para diagnóstico detallado.
