using System;

namespace DroneSimulator.API.Models
{
    [Serializable]
    public class APIResponse<T>
    {
        public bool success;
        public string message;
        public T data;
        public string error;
    }
} 