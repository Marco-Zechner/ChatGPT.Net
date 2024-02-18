namespace ChatGPT.Net
{
    public static class GptModels
    {
        private static readonly Dictionary<string, GptModelInfo> _models = [];

        public static void AddModel(string name, float promptCostPer1kToken, int maxPromptToken, float generationCostPer1kToken, int maxGenerationToken, string description)
        {
            var model = new GptModelInfo(name, promptCostPer1kToken, maxPromptToken, generationCostPer1kToken, maxGenerationToken, description);
            _models[name] = model;
        }

        static GptModels()
        {
            AddModel("gpt-3.5-turbo-0125", 0.0005f, 16385, 0.0015f, 4096, "cheap");
            AddModel("gpt-4-1106-preview", 0.01f, 128000, 0.03f, 4096, "improved json and reproducible outputs");
            AddModel("gpt-4-0125-preview", 0.01f, 128000, 0.03f, 4096, "newest, less 'lazy'");
        }

        public static GptModelInfo GetModel(string name)
        {
            if (_models.TryGetValue(name, out var model))
                return model;
            
            throw new ArgumentException("Model not found.");
        }

        public static GptModelInfo[] GetAllModels()
        {
            return [.. _models.Values];
        }

    }

    public class GptModelInfo
    {
        public string Name { get; }
        public float PromptCostPer1kToken { get; }
        public int MaxPromptToken { get; }
        public float CompletionCostPer1kToken { get; }
        public int MaxGenerationToken { get; }
        public string Description { get; }

        public GptModelInfo(string name, float promptCostPer1kToken, int maxPromptToken, float generationCostPer1kToken, int maxGenerationToken, string description)
        {
            Name = name;
            PromptCostPer1kToken = promptCostPer1kToken;
            MaxPromptToken = maxPromptToken;
            CompletionCostPer1kToken = generationCostPer1kToken;
            MaxGenerationToken = maxGenerationToken;
            Description = description;
        }
    }
}
