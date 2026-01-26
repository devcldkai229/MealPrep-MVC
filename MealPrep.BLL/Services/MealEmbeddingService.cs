using MealPrep.DAL.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MealPrep.BLL.Services
{
    public interface IMealEmbeddingService
    {
        Task<string?> GenerateEmbeddingAsync(Meal meal);
    }

    /// <summary>
    /// Service để generate vector embeddings cho Meal entities sử dụng AWS Bedrock Titan Embeddings v2
    /// </summary>
    public class MealEmbeddingService : IMealEmbeddingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MealEmbeddingService> _logger;

        public MealEmbeddingService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<MealEmbeddingService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Generate embedding cho meal theo spec:
        /// "Món: {Name}. Thành phần: {Ingredients}. Mô tả: {Description}. Dinh dưỡng: {Macros}."
        /// Macros format: "High Protein, Low Carb, 350kcal" hoặc tương tự
        /// </summary>
        public async Task<string?> GenerateEmbeddingAsync(Meal meal)
        {
            try
            {
                // Build descriptive text theo format yêu cầu
                var descriptionText = new StringBuilder();
                descriptionText.Append($"Món: {meal.Name}. ");
                
                // Parse Ingredients JSON nếu có
                if (!string.IsNullOrEmpty(meal.Ingredients))
                {
                    try
                    {
                        var ingredients = JsonSerializer.Deserialize<List<string>>(meal.Ingredients);
                        if (ingredients != null && ingredients.Any())
                        {
                            descriptionText.Append($"Thành phần: {string.Join(", ", ingredients)}. ");
                        }
                    }
                    catch
                    {
                        // Nếu parse lỗi, dùng raw string
                        descriptionText.Append($"Thành phần: {meal.Ingredients}. ");
                    }
                }
                
                if (!string.IsNullOrEmpty(meal.Description))
                {
                    descriptionText.Append($"Mô tả: {meal.Description}. ");
                }
                
                // Format macros với tags thông minh
                var macroTags = FormatMacroTags(meal);
                descriptionText.Append($"Dinh dưỡng: {macroTags}.");

                var textToEmbed = descriptionText.ToString();
                _logger.LogInformation("Generating embedding for meal {MealId}: {Text}", meal.Id, textToEmbed);

                // Call AWS Bedrock Titan Embeddings v2
                var embedding = await CallBedrockEmbeddingAsync(textToEmbed);
                
                if (embedding == null || !embedding.Any())
                {
                    _logger.LogWarning("Failed to generate embedding for meal {MealId}", meal.Id);
                    return null;
                }

                // Serialize to JSON string
                var embeddingJson = JsonSerializer.Serialize(embedding);
                _logger.LogInformation("Successfully generated embedding for meal {MealId} (dimension: {Dimension})", 
                    meal.Id, embedding.Count);
                
                return embeddingJson;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding for meal {MealId}", meal.Id);
                return null;
            }
        }

        /// <summary>
        /// Format macros thành dạng tags thông minh: "High Protein, Low Carb, 350kcal"
        /// </summary>
        private string FormatMacroTags(Meal meal)
        {
            var tags = new List<string>();
            
            // Protein tags
            if (meal.Protein >= 40)
                tags.Add("High Protein");
            else if (meal.Protein >= 25)
                tags.Add("Moderate Protein");
            else if (meal.Protein < 15)
                tags.Add("Low Protein");
            
            // Carb tags
            if (meal.Carbs < 30)
                tags.Add("Low Carb");
            else if (meal.Carbs >= 60)
                tags.Add("High Carb");
            
            // Fat tags
            if (meal.Fat < 10)
                tags.Add("Low Fat");
            else if (meal.Fat >= 25)
                tags.Add("High Fat");
            
            // Calories
            tags.Add($"{meal.Calories}kcal");
            
            // Nếu không có tags đặc biệt, thêm thông tin chi tiết
            if (tags.Count == 1) // Chỉ có calories
            {
                tags.Insert(0, $"P:{meal.Protein}g, C:{meal.Carbs}g, F:{meal.Fat}g");
            }
            
            return string.Join(", ", tags);
        }

        private async Task<List<float>?> CallBedrockEmbeddingAsync(string text)
        {
            // TODO: Implement AWS Bedrock API call
            // This requires AWS SDK for .NET (AWSSDK.BedrockRuntime)
            // For now, return null to indicate not implemented
            // 
            // Example implementation would be:
            // var bedrockClient = new AmazonBedrockRuntimeClient(region);
            // var request = new InvokeModelRequest
            // {
            //     ModelId = "amazon.titan-embed-text-v2:0",
            //     Body = JsonSerializer.Serialize(new { inputText = text, dimensions = 1024, normalize = true }),
            //     ContentType = "application/json"
            // };
            // var response = await bedrockClient.InvokeModelAsync(request);
            // var result = JsonSerializer.Deserialize<EmbeddingResponse>(response.Body);
            // return result?.embedding;
            
            _logger.LogWarning("Bedrock embedding API not yet implemented. Please install AWSSDK.BedrockRuntime and configure AWS credentials.");
            return null;
        }
    }
}
