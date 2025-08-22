namespace SystemWarden.Dtos;

public class ContainerCreationRequestDto(int port, string mem, double cpus)
{
    public int Port { get; set; } = port;
    public string Mem { get; set; } = mem;
    public double Cpus { get; set; } = cpus;
}