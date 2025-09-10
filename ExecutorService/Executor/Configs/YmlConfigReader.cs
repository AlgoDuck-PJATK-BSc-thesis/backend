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

    public static YMLConfig ReadExecutorYmlConfig()
    {
        return ReadYmlConfig<YMLConfig>("/app/ExecutorConfig.yml");
    }
}