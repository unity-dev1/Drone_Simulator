using UnityEngine;
using User;



namespace DroneSimulator.API.Interfaces
{
    public interface IAPIModel
    {
        void OnReceivedUserData(UserProfile userProfile);
    }
}
