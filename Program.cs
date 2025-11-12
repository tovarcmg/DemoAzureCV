using System;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace VisionDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 🧠 Reemplaza con tus datos de Azure
            string endpoint = "";
            string key = "";

            // Crear cliente del servicio
            var credentials = new ApiKeyServiceClientCredentials(key);
            var client = new ComputerVisionClient(credentials)
            {
                Endpoint = endpoint
            };

            // Imagen a analizar (puede ser una URL pública o archivo local)
            string imageUrl = "https://portal.vision.cognitive.azure.com/dist/assets/portraitProcessing1-DO8kpz0b.png";

            Console.WriteLine("Analizando imagen...\n");

            // Elegir las características que queremos analizar
            var features = new VisualFeatureTypes[] {
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Objects
            };

            // Analizar la imagen
            var result = await client.AnalyzeImageAsync(imageUrl, new List<VisualFeatureTypes?>()
{
    VisualFeatureTypes.Categories,
    VisualFeatureTypes.Description,
    VisualFeatureTypes.Tags,
    VisualFeatureTypes.Objects
});



            // Mostrar descripción
            Console.WriteLine($"Descripción: {result.Description.Captions[0].Text} (confianza {result.Description.Captions[0].Confidence:P1})");

            // Mostrar etiquetas
            Console.WriteLine("\nEtiquetas:");
            foreach (var tag in result.Tags)
                Console.WriteLine($" - {tag.Name} ({tag.Confidence:P1})");

            // Mostrar objetos
            Console.WriteLine("\nObjetos detectados:");
            foreach (var obj in result.Objects)
                Console.WriteLine($" - {obj.ObjectProperty} ({obj.Confidence:P1})");
        }
    }
}
