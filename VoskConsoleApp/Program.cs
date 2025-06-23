using System.Diagnostics;
using System.Text;
using System.Text.Json;
using NAudio.Wave;
using Vosk;

var oggPath = args.Length > 0 ? args[0] : @"...path to ogg";
Console.WriteLine(oggPath);
var wavPath = "...";

ConvertOggToWav(oggPath, wavPath);
RecognizeSpeech(wavPath);

static void ConvertOggToWav(string oggPath, string wavPath)
{
    const string ffmpeg = "ffmpeg";
    var arguments = $"-i \"{oggPath}\" -ar 16000 -ac 1 -f wav \"{wavPath}\" -y";

    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = ffmpeg,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };
    process.Start();
    process.WaitForExit();
}

static void RecognizeSpeech(string wavPath)
{
    var builder = new StringBuilder();
    Vosk.Vosk.SetLogLevel(0);
    var model = new Model("...path");
    
    using var waveReader = new WaveFileReader(wavPath);
    using var rec = new VoskRecognizer(model, 16000.0f);

    var buffer = new byte[4096];
    while (true)
    {
        int bytesRead = waveReader.Read(buffer, 0, buffer.Length);
        if (bytesRead == 0)
            break;

        if (rec.AcceptWaveform(buffer, bytesRead))
            builder.Append(JsonDocument.Parse(rec.Result()).RootElement.GetProperty("text").GetString() + " ");
    }

    Console.WriteLine(builder.ToString());
}