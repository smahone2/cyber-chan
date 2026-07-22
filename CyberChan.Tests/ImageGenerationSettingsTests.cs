using CyberChan.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Xunit;

namespace CyberChan.Tests;

public class ImageGenerationSettingsTests
{
    [Fact]
    public void CreateImageGenerationOptions_UsesLowQualityByDefault()
    {
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
        using HttpClient client = factory.CreateClient(ImageGenerationSettings.DefaultHttpClientName);
        using HttpClient injectedClient = provider.GetRequiredService<HttpClient>();

        Assert.Equal(TimeSpan.FromMinutes(5), client.Timeout);
        Assert.Equal(TimeSpan.FromMinutes(5), injectedClient.Timeout);
    }
}