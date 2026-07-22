using OpenAI.Images;
using System;

namespace CyberChan.Services
{
    internal enum ImageGenerationQuality
    {
        LowQuality,
        MediumQuality,
        HighQuality
    }

    internal static class ImageGenerationSettings
    {
        internal static readonly TimeSpan NetworkTimeout = TimeSpan.FromMinutes(5);

        internal static bool TryParseRequest(string seed, out string promptStyle, out ImageGenerationQuality quality, out string validationError)
        {
            promptStyle = string.Empty;
            quality = ImageGenerationQuality.LowQuality;
            validationError = string.Empty;

            if (string.IsNullOrWhiteSpace(seed))
            {
                return true;
            }

            var qualitySpecified = false;

            foreach (var token in seed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (IsPromptStyle(token))
                {
                    promptStyle = token;
                    continue;
                }

                if (TryParseQuality(token, out var parsedQuality))
                {
                    if (qualitySpecified)
                    {
                        validationError = "Only one image quality may be specified. Supported values: LowQuality, MediumQuality, HighQuality.";
                        return false;
                    }

                    quality = parsedQuality;
                    qualitySpecified = true;
                    continue;
                }

                if (string.IsNullOrEmpty(promptStyle))
                {
                    promptStyle = token;
                    continue;
                }

                validationError = $"Invalid image quality '{token}'. Supported values: LowQuality, MediumQuality, HighQuality.";
                return false;
            }

            return true;
        }

        internal static bool TryParseQuality(string value, out ImageGenerationQuality quality)
        {
            if (value.Equals(nameof(ImageGenerationQuality.LowQuality), StringComparison.OrdinalIgnoreCase))
            {
                quality = ImageGenerationQuality.LowQuality;
                return true;
            }

            if (value.Equals(nameof(ImageGenerationQuality.MediumQuality), StringComparison.OrdinalIgnoreCase))
            {
                quality = ImageGenerationQuality.MediumQuality;
                return true;
            }

            if (value.Equals(nameof(ImageGenerationQuality.HighQuality), StringComparison.OrdinalIgnoreCase))
            {
                quality = ImageGenerationQuality.HighQuality;
                return true;
            }

            quality = ImageGenerationQuality.LowQuality;
            return false;
        }

        internal static GeneratedImageQuality ToSdkQuality(ImageGenerationQuality quality) => quality switch
        {
            ImageGenerationQuality.LowQuality => GeneratedImageQuality.LowQuality,
            ImageGenerationQuality.MediumQuality => GeneratedImageQuality.MediumQuality,
            _ => GeneratedImageQuality.HighQuality,
        };

        private static bool IsPromptStyle(string value) =>
            value.Equals("simple", StringComparison.OrdinalIgnoreCase)
            || value.Equals("detailed", StringComparison.OrdinalIgnoreCase);
    }
}
