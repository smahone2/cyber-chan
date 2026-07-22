using CyberChan.Services;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Images;
using System;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace CyberChan.Tests;

public class UnitTest1
{
    [Fact]
    public void GenerateImage_UsesLowQualityByDefault()
    {
        var method = typeof(AiService).GetMethod(nameof(AiService.GenerateImage), [typeof(string), typeof(string), typeof(string), typeof(ImageGenerationQuality)]);

        Assert.NotNull(method);

        var qualityParameter = method!.GetParameters().Single(parameter => parameter.Name == "quality");

        Assert.True(qualityParameter.IsOptional);
        Assert.Equal(ImageGenerationQuality.LowQuality, qualityParameter.DefaultValue);

        var options = AiService.CreateImageGenerationOptions("@user", ModelCatalog.Multimodal.ImageGen);

        Assert.Equal("low", options.Quality.ToString());
    }

    [Theory]
    [InlineData(nameof(ImageGenerationQuality.LowQuality), "low")]
    [InlineData(nameof(ImageGenerationQuality.MediumQuality), "medium")]
    [InlineData(nameof(ImageGenerationQuality.HighQuality), "high")]
    public void CreateImageGenerationOptions_UsesExplicitQuality(string qualityName, string expectedQuality)
    {
        var quality = Enum.Parse<ImageGenerationQuality>(qualityName);
        var options = AiService.CreateImageGenerationOptions("@user", ModelCatalog.Multimodal.ImageGen, quality);

        Assert.Equal(expectedQuality, options.Quality.ToString());
    }

    [Fact]
    public void TryParseRequest_RejectsInvalidQuality()
    {
        var result = ImageGenerationSettings.TryParseRequest("simple, UltraQuality", out _, out _, out var validationError);

        Assert.False(result);
        Assert.Contains("UltraQuality", validationError);
        Assert.Contains("LowQuality, MediumQuality, HighQuality", validationError);
    }

    [Fact]
    public void AddCyberChanHttpClient_SetsTimeoutToFiveMinutes()
    {
        var services = new ServiceCollection();
        services.AddCyberChanHttpClient();

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        using HttpClient client = factory.CreateClient();

        Assert.Equal(TimeSpan.FromMinutes(5), client.Timeout);
    }
}