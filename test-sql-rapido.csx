using Microsoft.Data.SqlClient;
using System;

Console.WriteLine("==========================================");
Console.WriteLine(" TEST RAPIDO DE CONEXION SQL SERVER");
Console.WriteLine("==========================================");
Console.WriteLine();

var connectionString = "Server=192.168.88.14,56885;Database=db_AgoraERP_Core;User Id=usagora;Password=Sinnada123.**;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=false;Connection Timeout=3";

Console.WriteLine("Intentando conectar...");
Console.WriteLine("Servidor: 192.168.88.14:56885");
Console.WriteLine("Base de Datos: db_AgoraERP_Core");
Console.WriteLine();

try
{
    using var conn = new SqlConnection(connectionString);
    conn.Open();
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("✓ CONEXION EXITOSA!");
    Console.ResetColor();
    Console.WriteLine($"Version: {conn.ServerVersion}");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("✗ ERROR DE CONEXION");
    Console.ResetColor();
    Console.WriteLine($"Mensaje: {ex.Message}");
}
