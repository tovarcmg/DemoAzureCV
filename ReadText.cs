using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Collections.Generic;

namespace VisionDemo
{
    class Program
    {
        // 👉 Reemplaza con tu endpoint y key del portal de Azure
        private static string endpoint = "https://pruebascv.cognitiveservices.azure.com/";
        private static string key = "";

        static async Task Main(string[] args)
        {
            var client = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(key))
            { Endpoint = endpoint };

            // Puedes elegir si usar una imagen local o remota:
            //string imageUrl = "https://aka.ms/azsdk/image-analysis/sample.jpg";
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "ine.jpg");

            Console.WriteLine("📖 Analizando texto en la imagen...");

            await ReadTextFromImage(client, imagePath);
        }

        static async Task ReadTextFromImage(ComputerVisionClient client, string imagePath)
        {
            // Lee texto desde una imagen local
            using (Stream imageStream = File.OpenRead(imagePath))
            {
                var readOp = await client.ReadInStreamAsync(imageStream);
                string operationId = readOp.OperationLocation.Split('/')[^1];

                ReadOperationResult results;
                do
                {
                    results = await client.GetReadResultAsync(Guid.Parse(operationId));
                    await Task.Delay(1000);
                }
                while (results.Status == OperationStatusCodes.Running ||
                       results.Status == OperationStatusCodes.NotStarted);

                if (results.Status == OperationStatusCodes.Succeeded)
                {
                    string nombre = "";
                    string sexo = "";
                    string domicilio = "";
                    string claveDeElector = "";
                    string curp = "";
                    string vigencia = "";
                    string fechaNacimiento = "";
                    string seccion = "";
                    string anioRegistro = "";

                    int nombreCount = 0;
                    int domicilioCount = 0;

                    foreach (var page in results.AnalyzeResult.ReadResults)
                    {
                        foreach (var line in page.Lines)
                        {
                            if (line.Text == "INE")
                                continue;
                            if (line.Text == "LINE")
                                continue;
                            if (line.Text == "INSTITUTO NACIONAL ELECTORAL")
                                continue;
                            if (line.Text == "MÉXICO")
                                continue;

                            if (line.Text == "SEXO M")
                                sexo = "Mujer";
                            else if (line.Text == "SEXO F")
                                sexo = "Hombre";
                            else if (line.Text == "NOMBRE")
                            {
                                nombreCount = 1;
                            }
                            else if (nombreCount == 1)
                            {
                                nombreCount = 2;
                                nombre += " " + line.Text;
                            }
                            else if (nombreCount == 2)
                            {
                                nombreCount = 3;
                                nombre += " " + line.Text;
                            }
                            else if (nombreCount == 3)
                            {
                                nombreCount = 4;
                                nombre += " " + line.Text;
                            }

                            if (line.Text == "DOMICILIO")
                            {
                                domicilioCount = 1;
                            }
                            else if (domicilioCount == 1)
                            {
                                if (!line.Text.ToLower().Contains("clave"))
                                    domicilio = domicilio + " " + line.Text;
                                else
                                    domicilioCount = 2;
                            }

                            if (line.Text.ToLower().Contains("clave"))
                                claveDeElector = line.Text.ToLower().Replace("clave de elector", "");

                            else if (line.Text.Length == 18)
                                curp = line.Text;

                            else if (line.Text.Length == 11)
                                vigencia = line.Text;

                            else if (line.Text.Length == 10 && line.Text.Contains("/"))
                                fechaNacimiento = line.Text;

                            //validar si es numerico y longitud 4
                            bool esValido = line.Text.Length == 4 && line.Text.All(char.IsDigit);

                            if (esValido)
                                seccion = line.Text;

                            else if (line.Text.Length == 7 && line.Text.Contains(" "))
                                anioRegistro = line.Text;
                        }
                    }

                    Console.WriteLine("Nombre: " + nombre.Trim());
                    Console.WriteLine("Sexo: " + sexo);
                    Console.WriteLine("Domicilio: " + domicilio.Trim());
                    Console.WriteLine("Clave de elector: " + claveDeElector.Trim().ToUpper());
                    Console.WriteLine("CURP: " + curp);
                    Console.WriteLine("Año de registro: " + anioRegistro);
                    Console.WriteLine("Fecha de nacimiento: " + fechaNacimiento);
                    Console.WriteLine("Sección: " + seccion);
                    Console.WriteLine("Vigencia: " + vigencia);
                }
                else
                {
                    Console.WriteLine("❌ No se pudo leer el texto.");
                }
            }

            Console.WriteLine("\n✅ Proceso completado.");
        }
    }
}
