using ai_devs_proj.ApiHelpers;
using System.Text.Json;

namespace ai_devs_proj.S03E02
{
    internal class Finisher
    {
        internal async Task Finish()
        {
            var folderPath = "C:\\Sources\\ai_devs_proj\\S03E02\\Raports\\";
            var documents = new Dictionary<string, string>();
            foreach (var file in Directory.GetFiles(folderPath, "*.txt"))
            {
                documents.Add(Guid.NewGuid().ToString(), $"{Path.GetFileNameWithoutExtension(file)}:{await File.ReadAllTextAsync(file)}");
            }

            var openAiEmbedding = new Embedding();
            var embeddedDocuments = await openAiEmbedding.GetEmbeddingsAsync(documents);

            var qdrantClient = new QdrantClient();
            await qdrantClient.CreateCollectionAsync("raporty", embeddedDocuments.First().Value.Length);

            var documentsWithEmbeddings = embeddedDocuments.Select(entry =>
            (
                id: entry.Key,
                vector: entry.Value,
                text: documents[entry.Key]
            )).ToList();

            await qdrantClient.UpsertDocumentsAsync("raporty", documentsWithEmbeddings);

            Console.WriteLine("Zakończono indeksowanie raportów w Qdrant.");

            var question = "W raporcie, z którego dnia znajduje się wzmianka o kradzieży prototypu broni?";

            var embedding = await openAiEmbedding.GetEmbeddingsAsync(new Dictionary<string, string>
            {
                { "question", question }
            });


            var response = await qdrantClient.QueryDatabaseAsync(embedding["question"], "raporty");

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(response);

            var foundUUID = jsonResponse.GetProperty("result").EnumerateArray().First().GetProperty("id").GetString();

            var raport = documents[foundUUID];

            var raportName = raport.Split(":")[0].Replace('_', '-');

            var aiDevsResponse = await ApiHelper.PostCompletedTask("wektory", raportName);

            Console.WriteLine(await aiDevsResponse.Content.ReadAsStringAsync());
        }
    }
}
