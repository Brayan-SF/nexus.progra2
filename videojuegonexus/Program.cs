using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NexusSistema
{
    // ==========================================
    // 🧬 1. CLASE EXPLORADOR (POO)
    // ==========================================
    public class Explorador
    {
        private string _nombre;
        private int _nivelEnergia;
        private bool _tieneTarjetaAcceso;
        private int _posicionX;
        private int _posicionY;

        public string Nombre
        {
            get => _nombre;
            private set => _nombre = string.IsNullOrWhiteSpace(value) ? "Explorador Desconocido" : value;
        }

        public int NivelEnergia
        {
            get => _nivelEnergia;
            private set
            {
                if (value > 100) _nivelEnergia = 100;
                else if (value < 0) _nivelEnergia = 0;
                else _nivelEnergia = value;
            }
        }

        public bool TieneTarjetaAcceso => _tieneTarjetaAcceso;
        public int PosicionX => _posicionX;
        public int PosicionY => _posicionY;
        public bool EstaConVida => _nivelEnergia > 0;

        public Explorador(string nombre, int energiaInicial, bool tieneTarjeta)
        {
            Nombre = nombre;
            NivelEnergia = energiaInicial;
            _tieneTarjetaAcceso = tieneTarjeta;
            _posicionX = 0;
            _posicionY = 0;
        }

        public string ValidarIngresoSector()
        {
            if (!EstaConVida)
                return $"[ALERTA POO]: {Nombre} está fuera de combate. No puede operar.";

            if (NivelEnergia >= 50 && _tieneTarjetaAcceso)
                return $"[NEXUS ACCESO]: {Nombre} ha ingresado a la Zona de Exploración. Energía restante: {NivelEnergia}%.";
            else if (NivelEnergia < 50 && _tieneTarjetaAcceso)
                return $"[ALERTA ENERGÍA]: {Nombre} posee tarjeta válida pero energía insuficiente ({NivelEnergia}% < 50%).";
            else
                return $"[DENEGADO]: {Nombre} no posee tarjeta de acceso válida.";
        }

        public string RecibirDaño(int cantidad)
        {
            NivelEnergia -= cantidad;
            if (!EstaConVida)
                return $"[CRÍTICO]: ¡{Nombre} ha recibido {cantidad} de daño y su sistema ha colapsado!";

            return $"[IMPACTO]: {Nombre} sufrió {cantidad} de daño. Energía actual: {NivelEnergia}%.";
        }

        public string RecargarEnergia(int cantidad)
        {
            NivelEnergia += cantidad;
            return $"[RECARGA]: {Nombre} ha restaurado su energía a {NivelEnergia}%.";
        }

        public string Mover(int deltaX, int deltaY)
        {
            if (NivelEnergia < 5)
                return $"[INSUFICIENTE]: {Nombre} no tiene suficiente energía (mínimo 5%) para desplazarse.";

            _posicionX += deltaX;
            _posicionY += deltaY;
            NivelEnergia -= 5;

            return $"[MOVIMIENTO]: {Nombre} se movió a la posición ({_posicionX}, {_posicionY}). Energía restante: {NivelEnergia}%.";
        }

        public string ObtenerEstado()
        {
            string estadoVida = EstaConVida ? "OPERATIVO" : "CRÍTICO";
            return $"[ESTADO]: '{Nombre}' | Estado: {estadoVida} | Energía: {NivelEnergia}% | Tarjeta: {_tieneTarjetaAcceso} | Posición: ({_posicionX},{_posicionY})";
        }
    }

    // ==========================================
    // 📡 2. SERVIDOR (CONTIENE ProcesarComandoPOO)
    // ==========================================
    public class NexusServidor
    {
        private static Explorador exploradorAlfa = new Explorador("Alfa-01", 80, true);
        private static Explorador exploradorBeta = new Explorador("Beta-02", 40, false);

        public static void Iniciar()
        {
            Console.WriteLine("\n=== NEXUS SERVIDOR POO: OPERACIÓN IRIS ===");
            Console.WriteLine("Instanciando entidades en la memoria del sistema...");
            Console.WriteLine(exploradorAlfa.ObtenerEstado());
            Console.WriteLine(exploradorBeta.ObtenerEstado());
            Console.WriteLine("----------------------------------------------------\n");

            IPAddress ipLocal = IPAddress.Any;
            int puerto = 5000;
            TcpListener servidor = new TcpListener(ipLocal, puerto);

            try
            {
                servidor.Start();
                Console.WriteLine($"[SERVIDOR]: Escuchando en el puerto {puerto}. Esperando cliente...");

                using (TcpClient cliente = servidor.AcceptTcpClient())
                {
                    IPEndPoint endpoint = (IPEndPoint)cliente.Client.RemoteEndPoint;
                    Console.WriteLine($"[SERVIDOR]: ¡CONEXIÓN ESTABLECIDA desde {endpoint.Address}!");

                    using (NetworkStream stream = cliente.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        bool sesionActiva = true;

                        while (sesionActiva)
                        {
                            int bytesLeidos = stream.Read(buffer, 0, buffer.Length);
                            if (bytesLeidos == 0) break;

                            string mensajeRecibido = Encoding.UTF8.GetString(buffer, 0, bytesLeidos).Trim();
                            Console.WriteLine($"\n[COMANDO RECIBIDO]: '{mensajeRecibido}'");

                            if (mensajeRecibido.Equals("DESCONECTAR", StringComparison.OrdinalIgnoreCase))
                            {
                                byte[] bytesDesconexion = Encoding.UTF8.GetBytes("[NEXUS]: Sesión cerrada correctamente.");
                                stream.Write(bytesDesconexion, 0, bytesDesconexion.Length);
                                sesionActiva = false;
                                continue;
                            }

                            // Llamada al método dentro de la misma clase NexusServidor
                            string respuesta = ProcesarComandoPOO(mensajeRecibido);
                            byte[] bytesRespuesta = Encoding.UTF8.GetBytes(respuesta);
                            stream.Write(bytesRespuesta, 0, bytesRespuesta.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR SERVIDOR]: {ex.Message}");
            }
            finally
            {
                servidor.Stop();
                Console.WriteLine("[SERVIDOR]: Detenido.");
            }
        }

        // Método auxiliar dentro de NexusServidor
        private static string ProcesarComandoPOO(string comando)
        {
            string[] partes = comando.Split(' ');
            string accion = partes[0].ToUpper();

            Explorador objetivo = exploradorAlfa;
            if (partes.Length > 1 && partes[1].ToUpper() == "BETA")
            {
                objetivo = exploradorBeta;
            }

            switch (accion)
            {
                case "ESTADO":
                    return $"{exploradorAlfa.ObtenerEstado()}\n{exploradorBeta.ObtenerEstado()}";

                case "INGRESAR":
                    return objetivo.ValidarIngresoSector();

                case "DAÑO":
                    if (partes.Length >= 3 && int.TryParse(partes[2], out int ptsDaño))
                        return objetivo.RecibirDaño(ptsDaño);
                    return "[SINTAXIS]: DAÑO <ALFA|BETA> <puntos>";

                case "RECARGAR":
                    if (partes.Length >= 3 && int.TryParse(partes[2], out int ptsRecarga))
                        return objetivo.RecargarEnergia(ptsRecarga);
                    return "[SINTAXIS]: RECARGAR <ALFA|BETA> <puntos>";

                case "MOVER":
                    if (partes.Length >= 4 && int.TryParse(partes[2], out int dx) && int.TryParse(partes[3], out int dy))
                        return objetivo.Mover(dx, dy);
                    return "[SINTAXIS]: MOVER <ALFA|BETA> <deltaX> <deltaY>";

                case "AYUDA":
                    return "[MANUAL]: Comandos -> ESTADO | INGRESAR <ALFA|BETA> | DAÑO <ALFA|BETA> <pts> | RECARGAR <ALFA|BETA> <pts> | MOVER <ALFA|BETA> <dx> <dy> | DESCONECTAR";

                default:
                    return $"[ERROR]: Comando '{comando}' no reconocido. Escriba AYUDA.";
            }
        }
    }

    // ==========================================
    // 💻 3. CLIENTE
    // ==========================================
    public class NexusCliente
    {
        public static void Iniciar()
        {
            Console.WriteLine("\n=== NEXUS CLIENTE: EXPLORADOR DE CAMPO ===");
            Console.Write("Ingrese la IP del Servidor (presione ENTER para 172.22.160.1): ");
            string ipServidor = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ipServidor)) ipServidor = "172.22.160.1";

            try
            {
                using (TcpClient cliente = new TcpClient(ipServidor, 5000))
                using (NetworkStream stream = cliente.GetStream())
                {
                    Console.WriteLine("[NIVEL 1]: ¡CONEXIÓN ESTABLECIDA CON ÉXITO!\n");
                    Console.WriteLine("Prueba comandos como: 'ESTADO', 'INGRESAR ALFA', 'DAÑO ALFA 30', 'MOVER BETA 1 0', 'DESCONECTAR'\n");

                    byte[] bufferRecibo = new byte[1024];
                    bool sesionActiva = true;

                    while (sesionActiva)
                    {
                        Console.Write("Explorador-POO> ");
                        string comando = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(comando)) continue;

                        byte[] bytesEnviar = Encoding.UTF8.GetBytes(comando);
                        stream.Write(bytesEnviar, 0, bytesEnviar.Length);

                        int bytesLeidos = stream.Read(bufferRecibo, 0, bufferRecibo.Length);
                        string respuesta = Encoding.UTF8.GetString(bufferRecibo, 0, bytesLeidos);

                        Console.WriteLine($"< {respuesta}\n");

                        if (comando.Trim().Equals("DESCONECTAR", StringComparison.OrdinalIgnoreCase))
                        {
                            sesionActiva = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR CLIENTE]: No se pudo conectar -> {ex.Message}");
            }
        }
    }

    // ==========================================
    // 🚀 4. PUNTO DE ENTRADA (RESUELVE CS5001)
    // ==========================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "NEXUS SYSTEM POO + SOCKETS";
            Console.WriteLine("=================================");
            Console.WriteLine("    SISTEMA DE CONTROL NEXUS     ");
            Console.WriteLine("=================================");
            Console.WriteLine("1. Ejecutar como SERVIDOR");
            Console.WriteLine("2. Ejecutar como CLIENTE");
            Console.Write("Seleccione una opción (1 o 2): ");

            string opcion = Console.ReadLine();

            if (opcion == "1")
            {
                NexusServidor.Iniciar();
            }
            else if (opcion == "2")
            {
                NexusCliente.Iniciar();
            }
            else
            {
                Console.WriteLine("Opción no válida. Reinicie el programa.");
            }
        }
    }
}