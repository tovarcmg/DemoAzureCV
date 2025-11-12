using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Collections.Generic;

namespace VisionDemo
{
    class Programm
    {
        // 👉 Reemplaza con tu endpoint y key del portal de Azure
        private static string endpoint = "";
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
                    foreach (var page in results.AnalyzeResult.ReadResults)
                    {
                        foreach (var line in page.Lines)
                        {
                            Console.WriteLine(line.Text);
                        }
                    }
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
