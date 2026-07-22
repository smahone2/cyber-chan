using System.Collections.Generic;

namespace CyberChan.Services
{
    /// <summary>
    /// Central source of truth for OpenAI model identifiers used by the bot,
    /// grouped by usefulness category. Update model strings here to migrate
    /// all call sites at once.
    ///
    /// Model IDs reflect the current frontier per the OpenAI models docs page
    /// (https://developers.openai.com/api/docs/models/all). Models the page
    /// marks as deprecated (e.g. gpt-5, gpt-image-1, dall-e-3, o3, o4-mini)
    /// have been removed. Dedicated reasoning models are no longer a separate
    /// category — the gpt-5.6-* frontier handles reasoning workloads.
    /// </summary>
    internal static class ModelCatalog
    {
        /// <summary>Current frontier general-purpose chat models (gpt-5.6 family).</summary>
        public static class Flagship
        {
            // Top-capability flagship — best quality, handles reasoning workloads.
            public const string Gpt56Sol = "gpt-5.6-sol";
            // Balanced flagship — good quality, faster / cheaper than Sol.
            public const string Gpt56Terra = "gpt-5.6-terra";
            // Cost-sensitive flagship — smallest / cheapest of the gpt-5.6 tier.
            public const string Gpt56Luna = "gpt-5.6-luna";
        }

        /// <summary>Prior-generation flagship chat models, kept as fallbacks.</summary>
        public static class LegacyFlagship
        {
            public const string Gpt55 = "gpt-5.5";
            public const string Gpt55Pro = "gpt-5.5-pro";
            public const string Gpt54 = "gpt-5.4";
            public const string Gpt54Pro = "gpt-5.4-pro";
            public const string Gpt54Mini = "gpt-5.4-mini";
            public const string Gpt54Nano = "gpt-5.4-nano";
        }

        /// <summary>Multimodal models (vision input, image generation).</summary>
        public static class Multimodal
        {
            // Frontier flagship also handles vision — reuse Terra for image analysis.
            public const string VisionChat = Flagship.Gpt56Terra;
            // Current image-generation model.
            public const string ImageGen = "gpt-image-2";
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
        /// Empty for now — gpt-image-2 returns bytes by default.
        /// </summary>
        public static readonly HashSet<string> Base64ImageModels = new();

        /// <summary>
        /// Chat models that use the "developer" role instead of "system"
        /// (historically the o-series reasoning models). Empty now that
        /// dedicated reasoning models are deprecated.
        /// </summary>
        public static readonly HashSet<string> ReasoningChatModels = new();
    }
}
