using System.Threading.Tasks;
using DroneSimulator.API.Models;

namespace DroneSimulator.API.Interfaces
{
    public interface IScenarioService
    {
        Task<StartScenarioResponse> StartScenarioAsync(string locationName, string scenarioName, string droneName);
        Task<EndScenarioResponse> EndScenarioAsync(string locationName, string scenarioName, string droneName);
    }
} 