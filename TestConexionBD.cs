using System;
using Microsoft.Data.SqlClient;

class TestConexionBD
{
    static void Main()
    {
        Console.WriteLine("==========================================");
        Console.WriteLine(" TEST DE CONEXION A BASE DE DATOS");
        Console.WriteLine("==========================================");
        Console.WriteLine();

        string connectionString = "Server=192.168.88.14,56885;Database=db_AgoraERP_Core;User Id=usagora;Password=Sinnada123.**;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=false;Connection Timeout=5";

        Console.WriteLine("Servidor: 192.168.88.14,56885");
        Console.WriteLine("Base de Datos: db_AgoraERP_Core");
        Console.WriteLine("Usuario: usagora");
        Console.WriteLine();
        Console.WriteLine("Intentando conectar...");
        Console.WriteLine();

        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ CONEXION EXITOSA a la base de datos");
                Console.ResetColor();
                Console.WriteLine($"  Version de SQL Server: {connection.ServerVersion}");
                Console.WriteLine($"  Base de Datos: {connection.Database}");
                
                // Probar una consulta simple
                using (var command = new SqlCommand("SELECT COUNT(*) FROM sys.tables", connection))
                {
                    var result = command.ExecuteScalar();
                    Console.WriteLine($"  Numero de tablas: {result}");
                }
            }
        }
        catch (SqlException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ ERROR DE CONEXION");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"Mensaje: {ex.Message}");
            Console.WriteLine($"Numero de Error: {ex.Number}");
            Console.WriteLine();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Posibles soluciones:");
            Console.ResetColor();
            
            if (ex.Number == -1 || ex.Number == 258 || ex.Number == 2)
            {
                Console.WriteLine("  - Verifica que SQL Server este corriendo en 192.168.88.14");
                Console.WriteLine("  - Verifica que el puerto 56885 este abierto en el firewall");
                Console.WriteLine("  - Verifica que SQL Server acepte conexiones TCP/IP");
            }
            else if (ex.Number == 18456)
            {
                Console.WriteLine("  - Usuario o contraseña incorrectos");
                Console.WriteLine("  - Verifica las credenciales: usagora / Sinnada123.**");
            }
            else if (ex.Number == 4060)
            {
                Console.WriteLine("  - La base de datos 'db_AgoraERP_Core' no existe");
                Console.WriteLine("  - Crea la base de datos o verifica el nombre");
            }
            else
            {
                Console.WriteLine("  - Revisa la configuracion de SQL Server");
                Console.WriteLine("  - Verifica los permisos del usuario");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ ERROR INESPERADO");
            Console.ResetColor();
            Console.WriteLine($"Mensaje: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("==========================================");
    }
}
