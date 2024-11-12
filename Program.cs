using ai_devs_proj.S02E02;

class Program
{
    static async Task Main(string[] args)
    {
        var recognizer = new ImgRecognizer();
        Console.WriteLine(await recognizer.GetLocationBaseOnMapImg("C:\\Sources\\ai_devs_proj\\S02E02\\Imgs\\map1.jpg"));
        Console.WriteLine(await recognizer.GetLocationBaseOnMapImg("C:\\Sources\\ai_devs_proj\\S02E02\\Imgs\\map2.jpg"));
        Console.WriteLine(await recognizer.GetLocationBaseOnMapImg("C:\\Sources\\ai_devs_proj\\S02E02\\Imgs\\map3.jpg"));
        Console.WriteLine(await recognizer.GetLocationBaseOnMapImg("C:\\Sources\\ai_devs_proj\\S02E02\\Imgs\\map4.jpg"));
    }
}
