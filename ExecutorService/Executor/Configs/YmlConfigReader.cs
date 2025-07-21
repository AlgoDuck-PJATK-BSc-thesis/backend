using YamlDotNet.Serialization;

namespace ExecutorService.Executor.Configs;

public static class YmlConfigReader
{
    public static T ReadYmlConfig<T>(string path)
    {
        var yamlConfig = File.ReadAllText(path);
        
        var deserializer = new DeserializerBuilder()
            .Build();
        return deserializer.Deserialize<T>(yamlConfig);
    }

    public static Config ReadExecutorYmlConfig()
    {
        return ReadYmlConfig<Config>("/app/ExecutorConfig.yml");
    }
}