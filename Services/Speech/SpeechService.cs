using Whisper.net;
using System.Text;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace RAG_Code_Base.Services.Speech;

public class SpeechService : IDisposable
{
    private readonly WhisperFactory _factory;
    private readonly WhisperProcessor _processor;
    private readonly ILogger<SpeechService> _logger;

    public SpeechService(ILogger<SpeechService> logger)
    {
        _logger = logger;
        
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "LM", "ggml-base.bin");
        
        _factory = WhisperFactory.FromPath(modelPath);
        _processor = _factory.CreateBuilder()
            .WithLanguage("ru")
            .WithTemperature(0.0f)
            .Build();
        
        _logger.LogInformation("SpeechService инициализирован (Whisper.net)");
    }

    public async Task<string> RecognizeAsync(IFormFile audio)
    {
        var tempInput = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        var tempOutput = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");

        try
        {
            await using (var fs = File.Create(tempInput))
                await audio.CopyToAsync(fs);

            using (var reader = new AudioFileReader(tempInput))
            {
                ISampleProvider sample = reader;

                if (reader.WaveFormat.Channels == 2)
                {
                    sample = new StereoToMonoSampleProvider(sample)
                    {
                        LeftVolume = 0.5f,
                        RightVolume = 0.5f
                    };
                }

                sample = new WdlResamplingSampleProvider(sample, 16000);
                
                WaveFileWriter.CreateWaveFile16(tempOutput, sample);
            }
            
            await using var wavStream = File.OpenRead(tempOutput);
            
            var result = new StringBuilder();
            
            await foreach (var segment in _processor.ProcessAsync(wavStream))
                result.Append(segment.Text);

            return result.ToString().Trim();
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
    }

    public void Dispose()
    {
        _processor.Dispose();
        _factory.Dispose();
    }
}