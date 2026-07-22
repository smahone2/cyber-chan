using System.Collections.Generic;

namespace CyberChan.Services
{
    /// <summary>
    /// Central source of truth for OpenAI model identifiers used by the bot,
    /// grouped by usefulness category. Update model strings here to migrate
    /// all call sites at once.
    /// </summary>
    internal static class ModelCatalog
    {
        // General-purpose / flagship
        public static class Flagship
        {
            public const string Gpt4o = "gpt-4o";
            public const string Gpt41 = "gpt-4.1";
            // Latest flagship: keep in sync with Commands.cs aliases
            public const string Gpt5 = "gpt-5";
        }

        // Reasoning-heavy
        public static class Reasoning
        {
            public const string O1 = "o1";
            public const string O1Mini = "o1-mini";
            public const string O3 = "o3";
            public const string O4Mini = "o4-mini";
        }

        // Cost-efficient / fast
        public static class CostEfficient
        {
            public const string Gpt4oMini = "gpt-4o-mini";
            public const string Gpt41Mini = "gpt-4.1-mini";
            public const string Gpt41Nano = "gpt-4.1-nano";
            // Legacy alias retained for compatibility with the !gpt3/!chatgpt commands.
            [System.Obsolete("Legacy chat model; prefer Gpt4oMini or Gpt41Mini.")]
            public const string Gpt35Turbo = "gpt-3.5-turbo";
        }

        // Multimodal (vision + image gen)
        public static class Multimodal
        {
            public const string VisionChat = "gpt-4o";
            public const string DallE2 = "dall-e-2";
            public const string DallE3 = "dall-e-3";
            public const string GptImage1 = "gpt-image-1";
        }

        // Embeddings
        public static class Embeddings
        {
            public const string TextEmbedding3Small = "text-embedding-3-small";
            public const string TextEmbedding3Large = "text-embedding-3-large";
        }

        // Moderation
        public const string Moderation = "omni-moderation-latest";

        /// <summary>
        /// Image models that support base64 response payloads (used to pick response format).
        /// </summary>
        public static readonly HashSet<string> Base64ImageModels = new()
        {
            Multimodal.DallE2,
            Multimodal.DallE3,
        };

        /// <summary>
        /// Chat models that only support the "developer"/user role scheme (reasoning models
        /// reject the legacy "system" role in favor of "developer" messages).
        /// </summary>
        public static readonly HashSet<string> ReasoningChatModels = new()
        {
            Reasoning.O1,
            Reasoning.O1Mini,
            Reasoning.O3,
            Reasoning.O4Mini,
        };
    }
}
