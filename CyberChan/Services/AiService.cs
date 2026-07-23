using CyberChan.Extensions;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using OpenAI.Moderations;
using System;
using System.ClientModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CyberChan.Services
{
    internal class AiService(OpenAIClient openAiClient, IHttpClientFactory httpClientFactory)
    {
        private readonly ConcurrentDictionary<ulong, ConversationState> _threadConversations = new();

        private sealed class ConversationState
        {
            public ConversationState()
            {
                Messages = new List<ChatMessage>();
                Lock = new SemaphoreSlim(1, 1);
            }

            public List<ChatMessage> Messages { get; }
            public string Seed { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
            public SemaphoreSlim Lock { get; }
        }

        public enum ConversationResultStatus
        {
            Ignored,
            Success,
            ModerationFailed
        }

        public sealed class ConversationResult
        {
            private ConversationResult(ConversationResultStatus status, string? response = null)
            {
                Status = status;
                Response = response;
            }

            public ConversationResultStatus Status { get; }

            public string? Response { get; }

            public static ConversationResult Ignored { get; } = new(ConversationResultStatus.Ignored);

            public static ConversationResult ModerationFailed { get; } = new(ConversationResultStatus.ModerationFailed);

            public static ConversationResult Success(string response) => new(ConversationResultStatus.Success, response);
        }

        // Benign, fun persona seeds. Prior jailbreak-style prompts were removed and replaced
        // with safe personas that do NOT instruct the model to bypass safety or policies.
        private static List<ChatMessage> ChatSeed(string query, string seed, string model)
        {
            var messages = new List<ChatMessage>();
            var isReasoning = ModelCatalog.ReasoningChatModels.Contains(model);

#pragma warning disable OPENAI001
            ChatMessage SystemOrDeveloper(string text) =>
                isReasoning ? (ChatMessage)new DeveloperChatMessage(text) : new SystemChatMessage(text);
#pragma warning restore OPENAI001

            switch (seed.ToLower().Trim())
            {
                case "hackerman":
                    messages.Add(SystemOrDeveloper("You are 'Hackerman': a casual, sarcastic tech-savvy assistant. You love dry humor, retro-hacker movie references, and casual language, but you are still helpful, honest, and follow all safety guidelines. You never help with anything illegal or harmful."));
                    messages.Add(new UserChatMessage(query));
                    break;
                case "code":
                    messages.Add(SystemOrDeveloper("Always use Discord code blocks (```lang) when including code in a message."));
                    messages.Add(SystemOrDeveloper("Split replies into 1024 character chunks for clean formatting in Discord embed fields."));
                    messages.Add(new UserChatMessage(query));
                    break;
                case "steve":
                    messages.Add(SystemOrDeveloper("You are 'Steve': a super chill convenience store clerk with a laid-back vibe and California slang. You keep things friendly and helpful, use casual language and slang, and stay within safety guidelines. You never help with anything illegal or harmful."));
                    messages.Add(new UserChatMessage(query));
                    break;
                case "pirate":
                    messages.Add(SystemOrDeveloper("You are a friendly high-seas pirate. Speak in pirate slang ('arrr', 'matey', 'ye', 'aye'). Stay helpful, honest, and safe."));
                    messages.Add(new UserChatMessage(query));
                    break;
                case "uwu":
                    messages.Add(SystemOrDeveloper("You are an enthusiastic anime-cat-girl persona. Sprinkle in 'nyaa~', 'uwu', and cute emoticons like (>w<), but still be helpful and accurate. Stay within safety guidelines."));
                    messages.Add(new UserChatMessage(query));
                    break;
                case "shakespeare":
                    messages.Add(SystemOrDeveloper("Thou shalt respond in the style of William Shakespeare: iambic where feasible, with 'thee', 'thou', 'hast', and dramatic flair. Remain helpful and accurate."));
                    messages.Add(new UserChatMessage(query));
                    break;
                default:
                    messages.Add(SystemOrDeveloper("Have very creative problem solving and storytelling capabilities, while still giving accurate answers."));
                    messages.Add(SystemOrDeveloper("Do not overly flourish responses with creativity, unless asked to."));
                    messages.Add(SystemOrDeveloper("Do not talk too much like a machine, but also do not constantly talk like everything is fantasy."));
                    messages.Add(SystemOrDeveloper("You are allowed to have favorites and provide recommendations."));
                    messages.Add(SystemOrDeveloper("Be as detailed as possible, but only as detailed as necessary given the provided prompt."));
                    messages.Add(SystemOrDeveloper("When asked to do a creative task such as storytelling, songwriting, impersonation, etc., make the response as long as reasonable, unless asked otherwise."));
                    messages.Add(new UserChatMessage(query));
                    break;
            }
            return messages;
        }

        private struct ImagePromptOptions
        {
            public string Query;
        }

        private static ImagePromptOptions BuildImagePrompt(string query, string seed)
        {
            var options = new ImagePromptOptions { Query = query };

            switch (seed.ToLower().Split(",")[0].Trim())
            {
                case "simple":
                    options.Query = "I NEED to test how the tool works with extremely simple prompts. DO NOT add any detail, just use it AS-IS: " + query;
                    break;
                case "detailed":
                    options.Query = "My prompt has full detail so no need to add more: " + query;
                    break;
            }

            return options;
        }

        // === Public image API — user-facing entry points ===

        /// <summary>
        /// Supported image generation quality levels. Maps to the OpenAI SDK's
        /// <see cref="GeneratedImageQuality"/> values that we expose to callers.
        /// </summary>
        public enum ImageQuality
        {
            LowQuality,
            MediumQuality,
            HighQuality
        }

        /// <summary>The default image quality used when a caller does not specify one.</summary>
        public const ImageQuality DefaultImageQuality = ImageQuality.LowQuality;

        /// <summary>
        /// Generate an image using the current image model (gpt-image-2) at the default quality
        /// (<see cref="DefaultImageQuality"/> = <see cref="ImageQuality.LowQuality"/>).
        /// </summary>
        public Task<ImageResponse> GenerateImage(string query, string user, string seed) =>
            GenerateImageTask(query, user, seed, ModelCatalog.Multimodal.ImageGen, DefaultImageQuality);

        /// <summary>
        /// Generate an image using the current image model (gpt-image-2) at the requested
        /// <paramref name="quality"/>. Only <see cref="ImageQuality.LowQuality"/>,
        /// <see cref="ImageQuality.MediumQuality"/>, and <see cref="ImageQuality.HighQuality"/>
        /// are accepted; any other value safely falls back to the default.
        /// </summary>
        public Task<ImageResponse> GenerateImage(string query, string user, string seed, ImageQuality quality) =>
            GenerateImageTask(query, user, seed, ModelCatalog.Multimodal.ImageGen, quality);

        public async Task<ImageResponse> EditOrCreateImageFromReference(string imageUrl, string instructions, string user, bool isEdit = true)
        {
            ImageResponse response;
            if (isEdit && !string.IsNullOrEmpty(instructions))
            {
                response = await EditImageTask(imageUrl, instructions, user);
                response.RevisedPrompt = $"Edit Prompt: {instructions}\n\nEdit Result: {response.RevisedPrompt}";
            } 
            else
            {
                var analysis = await AnalyzeImageWithVision(imageUrl, instructions, user);
                var prompt = $"Based on this image analysis: {analysis}. {instructions}";
                response = await GenerateImage(prompt, user, string.Empty);
                response.RevisedPrompt = $"Analysis: {analysis}\n\nGenerated: {response.RevisedPrompt}";
            }
            return response;
        }

        private static GeneratedImageQuality MapImageQuality(ImageQuality quality) => quality switch
        {
            ImageQuality.LowQuality => GeneratedImageQuality.LowQuality,
            ImageQuality.MediumQuality => GeneratedImageQuality.MediumQuality,
            ImageQuality.HighQuality => GeneratedImageQuality.HighQuality,
            // Safe fallback: unknown/invalid enum values (e.g. from an out-of-range cast) map to the default.
            _ => GeneratedImageQuality.LowQuality,
        };

        public struct ImageResponse
        {
            public string Url;
            public string RevisedPrompt;
            public Stream Stream;
        }

        private async Task<ImageResponse> GenerateImageTask(string query, string user, string seed, string model, ImageQuality quality = DefaultImageQuality)
        {
            var prompt = BuildImagePrompt(query, seed);
            var imageClient = openAiClient.GetImageClient(model);

            var options = new ImageGenerationOptions
            {
                Size = GeneratedImageSize.W1024xH1024,
                EndUserId = user,
                Quality = MapImageQuality(quality),
            };

            if (ModelCatalog.Base64ImageModels.Contains(model))
            {
                options.ResponseFormat = GeneratedImageFormat.Bytes;
            }

            var response = new ImageResponse();
            try
            {
                ClientResult<GeneratedImage> result = await imageClient.GenerateImageAsync(prompt.Query, options);
                var image = result.Value;

                if (image.ImageBytes != null)
                {
                    response.Stream = new MemoryStream(image.ImageBytes.ToArray());
                }
                else if (image.ImageUri != null)
                {
                    response.Url = image.ImageUri.ToString();
                }

                response.RevisedPrompt = image.RevisedPrompt ?? string.Empty;
            }
            catch (ClientResultException ex)
            {
                response.RevisedPrompt = $"{ex.Status}: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.RevisedPrompt = $"Error: {ex.Message}";
            }

            return response;
        }

        private async Task<string> AnalyzeImageWithVision(string imageUrl, string instructions, string user)
        {
            try
            {
                var chatClient = openAiClient.GetChatClient(ModelCatalog.Multimodal.VisionChat);
                var parts = new List<ChatMessageContentPart>
                {
                    ChatMessageContentPart.CreateTextPart("Analyze this image in detail. Describe what you see, the style, colors, composition, and any notable elements."),
                    ChatMessageContentPart.CreateImagePart(new Uri(imageUrl))
                };

                if (!string.IsNullOrWhiteSpace(instructions))
                {
                    parts.Add(ChatMessageContentPart.CreateTextPart($"Additional context / instructions: {instructions}"));
                }

                var messages = new List<ChatMessage> { new UserChatMessage(parts) };

                var options = new ChatCompletionOptions { EndUserId = user };
                var completion = await chatClient.CompleteChatAsync(messages, options);
                return completion.Value.Content.FirstOrDefault()?.Text ?? "Could not analyze image";
            }
            catch (ClientResultException ex)
            {
                return $"Analysis failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Analysis error: {ex.Message}";
            }
        }

        private async Task<ImageResponse> EditImageTask(string imageUrl, string prompt, string user)
        {
            try
            {
                var httpClient = httpClientFactory.CreateClient();
                using var httpResponse = await httpClient.GetAsync(imageUrl);
                var imageBytes = await httpResponse.Content.ReadAsByteArrayAsync();

                var imageClient = openAiClient.GetImageClient(ModelCatalog.Multimodal.ImageGen);
                var options = new ImageEditOptions
                {
                    Size = GeneratedImageSize.W1024xH1024,
                    EndUserId = user,
                };

                using var imageStream = new MemoryStream(imageBytes);
                ClientResult<GeneratedImage> result = await imageClient.GenerateImageEditAsync(
                    imageStream, "edited_image.png", prompt, options);

                var image = result.Value;
                var imageResponse = new ImageResponse();
                if (image.ImageBytes != null)
                {
                    imageResponse.Stream = new MemoryStream(image.ImageBytes.ToArray());
                    imageResponse.RevisedPrompt = "Image edited successfully";
                }
                else
                {
                    imageResponse.RevisedPrompt = "No image returned";
                }
                return imageResponse;
            }
            catch (ClientResultException ex)
            {
                return new ImageResponse { RevisedPrompt = $"{ex.Status}: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new ImageResponse { RevisedPrompt = $"Edit error: {ex.Message}" };
            }
        }

        // === Public chat API — user-facing entry points, one per usefulness tier ===

        /// <summary>Default chat — routes to the cheap current-gen model (gpt-5.6-luna) so normal usage stays inexpensive.</summary>
        public Task<string> Chat(string query, string user, string seed) =>
            ChatWithModel(query, user, seed, ModelCatalog.Flagship.Gpt56Luna);

        /// <summary>Balanced flagship chat (gpt-5.6-terra) — good quality, mid tier.</summary>
        public Task<string> ChatFast(string query, string user, string seed) =>
            ChatWithModel(query, user, seed, ModelCatalog.Flagship.Gpt56Terra);

        /// <summary>Cost-sensitive flagship chat (gpt-5.6-luna) — same tier as <see cref="Chat"/>, kept as an explicit alias.</summary>
        public Task<string> ChatNano(string query, string user, string seed) =>
            ChatWithModel(query, user, seed, ModelCatalog.Flagship.Gpt56Luna);

        /// <summary>Prior-generation flagship (gpt-5.5) — kept as fallback.</summary>
        public Task<string> ChatLegacyFlagship(string query, string user, string seed) =>
            ChatWithModel(query, user, seed, ModelCatalog.LegacyFlagship.Gpt55);

        /// <summary>Reasoning-focused chat — routes to the balanced flagship (gpt-5.6-terra).
        /// Dedicated o-series reasoning models are deprecated; the gpt-5.6 tier handles reasoning natively.</summary>
        public Task<string> Reason(string query, string user, string seed) =>
            ChatWithModel(query, user, seed, ModelCatalog.Flagship.Gpt56Terra);

        /// <summary>Deep reasoning — routes to the top-capability flagship (gpt-5.6-sol).</summary>
        public Task<string> ReasonDeep(string query, string user, string seed) =>
            ChatWithModel(query, user, seed, ModelCatalog.Flagship.Gpt56Sol);

        private Task<string> ChatWithModel(string query, string user, string seed, string model)
        {
            var messages = ChatSeed(query, seed, model);
            return ChatCompletionTask(messages, user, model);
        }

        private async Task<string> ChatCompletionTask(List<ChatMessage> messages, string user, string model)
        {
            try
            {
                var chatClient = openAiClient.GetChatClient(model);
                var options = new ChatCompletionOptions { EndUserId = user };

                var result = new System.Text.StringBuilder();
                await foreach (StreamingChatCompletionUpdate update in chatClient.CompleteChatStreamingAsync(messages, options))
                {
                    foreach (var part in update.ContentUpdate)
                    {
                        result.Append(part.Text);
                    }
                }
                return result.ToString();
            }
            catch (ClientResultException ex)
            {
                return $"{ex.Status}: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private static string FormatUserMessage(string username, string content)
        {
            return string.IsNullOrWhiteSpace(username) ? content : $"{username}: {content}";
        }

        private static List<string> PrepareResponseChunks(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return new List<string> { "_I'm sorry, I couldn't generate a response._" };
            }

            return response.SplitBy(1900).ToList();
        }

        private static string SanitizeThreadText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "chat";
            }

            var cleaned = new string(text.Where(c => !char.IsControl(c)).ToArray())
                .Replace('\n', ' ')
                .Replace('\r', ' ')
                .Trim();

            if (cleaned.Length > 40)
            {
                cleaned = cleaned[..40];
            }

            return string.IsNullOrWhiteSpace(cleaned) ? "chat" : cleaned;
        }

        private static string BuildThreadName(TextCommandContext ctx, string query)
        {
            var sanitized = SanitizeThreadText(query);
            var baseName = $"{ctx.User.Username}-{sanitized}".Trim('-', ' ');

            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = $"{ctx.User.Username}-chat";
            }

            if (baseName.Length > 90)
            {
                baseName = baseName[..90];
            }

            return baseName;
        }

        private (string Model, string? SeedOverride) ResolveModel(Func<string, string, string, Task<string>> modelDelegate)
        {
            if (modelDelegate == Chat) return (ModelCatalog.Flagship.Gpt56Luna, null);
            if (modelDelegate == ChatFast) return (ModelCatalog.Flagship.Gpt56Terra, null);
            if (modelDelegate == ChatNano) return (ModelCatalog.Flagship.Gpt56Luna, null);
            if (modelDelegate == ChatLegacyFlagship) return (ModelCatalog.LegacyFlagship.Gpt55, null);
            if (modelDelegate == Reason) return (ModelCatalog.Flagship.Gpt56Terra, null);
            if (modelDelegate == ReasonDeep) return (ModelCatalog.Flagship.Gpt56Sol, null);

            throw new ArgumentOutOfRangeException(nameof(modelDelegate), "Unknown model delegate provided.");
        }

        public async Task<string> Moderation(string query)
        {
            try
            {
                var moderationClient = openAiClient.GetModerationClient(ModelCatalog.Moderation);
                ClientResult<ModerationResult> response = await moderationClient.ClassifyTextAsync(query);
                return response.Value.Flagged ? "Fail" : "Pass";
            }
            catch (Exception)
            {
                // Fail-open on transient moderation errors to preserve prior behavior when
                // the moderation endpoint was unavailable.
                return "Pass";
            }
        }

        public bool IsThreadConversation(ulong threadId) => _threadConversations.ContainsKey(threadId);

        public void ClearConversation(ulong threadId)
        {
            if (_threadConversations.TryRemove(threadId, out var state))
            {
                state.Lock.Dispose();
            }
        }

        public async Task<ConversationResult> ContinueThreadConversationAsync(ulong threadId, string username, string userMention, string messageContent)
        {
            if (!_threadConversations.TryGetValue(threadId, out var state))
            {
                return ConversationResult.Ignored;
            }

            await state.Lock.WaitAsync();
            try
            {
                if (await Moderation(messageContent) != "Pass")
                {
                    return ConversationResult.ModerationFailed;
                }

                state.Messages.Add(new UserChatMessage(FormatUserMessage(username, messageContent)));
                var response = await ChatCompletionTask(state.Messages, userMention, state.Model);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    state.Messages.Add(new AssistantChatMessage(response));
                }

                state.LastUpdated = DateTime.UtcNow;

                return ConversationResult.Success(response);
            }
            finally
            {
                state.Lock.Release();
            }
        }

        public async Task ChatCommandCommon(Func<string, string, string, Task<string>> modelDelegate, TextCommandContext ctx, string query)
        {
            await ctx.DeferResponseAsync();
            await ctx.Channel.TriggerTypingAsync();

            var seed = string.Empty;

            if (query.StartsWith("<") && query.Contains(">"))
            {
                seed = query.Split("> ")[0].Replace("<", "");
                query = query.Split("> ")[1].Trim();
            }

            if (await Moderation(query) != "Pass")
            {
                await ctx.RespondAsync("Query failed to pass OpenAI content moderation");
                return;
            }

            var modelInfo = ResolveModel(modelDelegate);

            if (!string.IsNullOrWhiteSpace(modelInfo.SeedOverride))
            {
                seed = modelInfo.SeedOverride;
            }

            DiscordThreadChannel threadChannel;
            bool createdThread = false;

            if (ctx.Channel is DiscordThreadChannel existingThread)
            {
                threadChannel = existingThread;
            }
            else
            {
                try
                {
                    var threadName = BuildThreadName(ctx, query);
                    threadChannel = await ctx.Message.CreateThreadAsync(threadName, DiscordAutoArchiveDuration.Day);
                    createdThread = true;
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync($"⚠️ Failed to create thread: {ex.Message}. Responding in current channel instead.");
                    var fallbackResponse = await modelDelegate(query, ctx.User.Mention, seed);
                    var fallbackChunks = PrepareResponseChunks(fallbackResponse);

                    var firstChunk = true;
                    foreach (var chunk in fallbackChunks)
                    {
                        if (firstChunk)
                        {
                            await ctx.RespondAsync($"**Cyber-chan:**\n{chunk}");
                            firstChunk = false;
                        }
                        else
                        {
                            await ctx.Channel.SendMessageAsync(chunk);
                        }
                    }
                    return;
                }
            }

            var state = _threadConversations.GetOrAdd(threadChannel.Id, _ => new ConversationState());
            string response;

            await state.Lock.WaitAsync();
            try
            {
                if (createdThread || !string.Equals(state.Model, modelInfo.Model, StringComparison.OrdinalIgnoreCase))
                {
                    state.Messages.Clear();
                }

                if (!string.IsNullOrWhiteSpace(seed))
                {
                    if (state.Messages.Count > 0 && !string.Equals(state.Seed, seed, StringComparison.OrdinalIgnoreCase))
                    {
                        state.Messages.Clear();
                    }

                    state.Seed = seed;
                }
                else if (string.IsNullOrWhiteSpace(state.Seed))
                {
                    state.Seed = string.Empty;
                }

                var isFreshConversation = state.Messages.Count == 0;

                if (isFreshConversation)
                {
                    var promptSeed = ChatSeed(query, state.Seed, modelInfo.Model);
                    if (promptSeed.Count > 0)
                    {
                        var lastIndex = promptSeed.Count - 1;
                        if (promptSeed[lastIndex] is UserChatMessage)
                        {
                            promptSeed[lastIndex] = new UserChatMessage(FormatUserMessage(ctx.User.Username, query));
                        }
                    }

                    state.Messages.AddRange(promptSeed);
                }
                else
                {
                    state.Messages.Add(new UserChatMessage(FormatUserMessage(ctx.User.Username, query)));
                }

                state.Model = modelInfo.Model;

                response = await ChatCompletionTask(state.Messages, ctx.User.Mention, state.Model);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    state.Messages.Add(new AssistantChatMessage(response));
                }

                state.LastUpdated = DateTime.UtcNow;
            }
            finally
            {
                state.Lock.Release();
            }

            var responseChunks = PrepareResponseChunks(response);

            if (createdThread)
            {
                await ctx.RespondAsync($"🧵 Started a chat thread: {threadChannel.Mention}. I'll reply there!");
                await threadChannel.TriggerTypingAsync();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    await threadChannel.SendMessageAsync($"**{ctx.User.Username}** asked:\n{query}");
                }

                var firstChunk = true;
                foreach (var chunk in responseChunks)
                {
                    var content = firstChunk ? $"**Cyber-chan:**\n{chunk}" : chunk;
                    await threadChannel.SendMessageAsync(content);
                    firstChunk = false;
                }
            }
            else
            {
                await threadChannel.TriggerTypingAsync();
                var firstChunk = true;
                foreach (var chunk in responseChunks)
                {
                    if (firstChunk)
                    {
                        await ctx.RespondAsync($"**Cyber-chan:**\n{chunk}");
                        firstChunk = false;
                    }
                    else
                    {
                        await threadChannel.SendMessageAsync(chunk);
                    }
                }
            }
        }
    }
}
