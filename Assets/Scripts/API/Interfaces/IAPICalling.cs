using UnityEngine;


namespace DroneSimulator.API.Interfaces
{
    public interface IAPICalling
    {
        void OnAPICall(bool isSuccess, string username);
    }
}
