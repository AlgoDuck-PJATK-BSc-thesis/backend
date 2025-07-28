namespace DockerWarden.Dtos;

public class ContainerCreationResponseDto(string containerId)
{
    public string ContainerId { get; set; } = containerId;
}