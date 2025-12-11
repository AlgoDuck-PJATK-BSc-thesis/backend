using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentAssertions;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Utils;

public sealed class ChatModerationSettingsTests
{
    [Fact]
    public void CanCreateInstance()
    {
        var settings = new ChatModerationSettings();
        settings.Should().NotBeNull();
    }

    [Fact]
    public void HasAtLeastOneConfigurableProperty()
    {
        var settings = new ChatModerationSettings();
        var properties = settings.GetType().GetProperties();
        properties.Should().NotBeEmpty();
    }

    [Fact]
    public void Properties_AreReadableAndWritableWhenSettable()
    {
        var settings = new ChatModerationSettings();
        var properties = settings
            .GetType()
            .GetProperties()
            .Where(p => p.CanWrite)
            .ToArray();

        foreach (var property in properties)
        {
            object valueToAssign;
            if (property.PropertyType == typeof(bool))
            {
                valueToAssign = true;
            }
            else if (property.PropertyType == typeof(int))
            {
                valueToAssign = 1;
            }
            else if (property.PropertyType == typeof(string))
            {
                valueToAssign = "test";
            }
            else if (property.PropertyType.IsArray)
            {
                valueToAssign = Array.CreateInstance(property.PropertyType.GetElementType()!, 0);
            }
            else
            {
                continue;
            }

            property.SetValue(settings, valueToAssign);
            var readBack = property.GetValue(settings);
            readBack.Should().Be(valueToAssign);
        }
    }
}