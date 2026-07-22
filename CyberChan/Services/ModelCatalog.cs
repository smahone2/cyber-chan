using System.Collections.Generic;

namespace CyberChan.Services
{
    /// <summary>
    /// Central source of truth for OpenAI model identifiers used by the bot,
    /// grouped by usefulness category. Update model strings here to migrate
    /// all call sites at once. Legacy / superseded models have been removed.
    /// </summary>
    internal static class ModelCatalog
    {
        /// <summary>General-purpose flagship chat models.</summary>
        public static class Flagship
        {
            // Newest general-purpose flagship.
            public const string Gpt5 = "gpt-5";
            // Prior-generation flagship, kept as a fallback for callers that need it.
            public const string Gpt41 = "gpt-4.1";
        }

        /// <summary>Reasoning-heavy models (for tough problems, math, planning).</summary>
        public static class Reasoning
        {
            // Newest small-but-fast reasoning model.
            public const string O4Mini = "o4-mini";
            // Highest-capability reasoning model.
            public const string O3 = "o3";
        }

        /// <summary>Cost-efficient / fast chat models.</summary>
        public static class Fast
        {
            // Newest cost-efficient siblings of gpt-5.
            public const string Gpt5Mini = "gpt-5-mini";
            public const string Gpt5Nano = "gpt-5-nano";
            // Prior-generation cost-efficient fallbacks.
            public const string Gpt41Mini = "gpt-4.1-mini";
            public const string Gpt41Nano = "gpt-4.1-nano";
        }

        /// <summary>Multimodal models (vision input, image generation).</summary>
        public static class Multimodal
        {
            // Use gpt-4.1 for vision analysis (broad availability, strong vision).
            public const string VisionChat = "gpt-4.1";
            // Preferred image-generation model.
            public const string ImageGen = "gpt-image-1";
            // Legacy DALL-E kept for the !dalle3 alias.
            public const string DallE3 = "dall-e-3";
        }

        /// <summary>Text embedding models.</summary>
        public static class Embeddings
        {
            public const string Small = "text-embedding-3-small";
            public const string Large = "text-embedding-3-large";
        }

        /// <summary>Moderation model used for input safety checks.</summary>
        public const string Moderation = "omni-moderation-latest";

        /// <summary>
        /// Image models that support base64 response payloads and therefore need
        /// an explicit <c>ResponseFormat = Bytes</c> to receive raw image data.
        /// </summary>
        public static readonly HashSet<string> Base64ImageModels = new()
        {
            Multimodal.DallE3,
        };

        /// <summary>
        /// Chat models that use the "developer" role instead of "system" (reasoning models).
        /// </summary>
        public static readonly HashSet<string> ReasoningChatModels = new()
        {
            Reasoning.O3,
            Reasoning.O4Mini,
        };
    }
}
