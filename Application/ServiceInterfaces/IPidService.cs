namespace Application.ServiceInterfaces;

public interface IPidService
{
    int Compute(float currentFlowRate, double deltaTimeInSeconds);
}