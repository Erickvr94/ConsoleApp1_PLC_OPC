using System;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace ConsoleApp1_PLC_OPC
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var application = new ApplicationInstance
            {
                ApplicationName = "MyClient",
                ApplicationType = ApplicationType.Client
            };

            //Cargar configuración desde ClientConfig.xml
            ApplicationConfiguration config =
                await application.LoadApplicationConfiguration("ClientConfig.xml", false);

            // 2️⃣ IMPORTANTE:
            // En tu versión del SDK, NO SE USA CheckApplicationInstanceCertificate
            // La validación y generación del certificado se hace automáticamente
            // durante LoadApplicationConfiguration()

            // Agregar manejador de validación de certificados (opcional)
            config.CertificateValidator.CertificateValidation += (s, e) =>
            {
                Console.WriteLine($"[CERT] {e.Error.StatusCode} - {e.Certificate.Subject}");
                // Para pruebas puedes aceptar:
                // e.Accept = true;
            };

            //Endpoint del PLC S7-1200
            string endpointUrl = "opc.tcp://192.168.199.91:4840";

            EndpointDescription endpointDescription =
                CoreClientUtils.SelectEndpoint(config, endpointUrl, false, 15000);
            //                                      ^^^^^  ^^^^^^^^^^^  ^^^^^
            //                                      config  URL         sin seguridad


            EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(config);
            ConfiguredEndpoint endpoint =
                new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            //Crear sesión OPC UA (API obsoleta pero funcional)
            using (Session session = await Session.Create(
                config,
                endpoint,
                false,
                application.ApplicationName,
                60000,
                null,
                null))
            {
                Console.WriteLine("Conectado al PLC S7-1200 por OPC UA.");

                // 6Leer una variable 
                ushort opcnamespace = 5;
                ushort id = 5;
                NodeId nodeId = new NodeId(id, opcnamespace);   //(identifier, namespaceIndex)

                DataValue dv = session.ReadValue(nodeId);

                Console.WriteLine($"Valor leído (ns={opcnamespace};i={id}): {dv.Value}");


                session.Close();
                Console.WriteLine("Sesión cerrada.");
            }

            Console.WriteLine("Fin. Pulsa una tecla para salir...");
            Console.ReadKey();
        }
    }
}
