using System;
using UnityEngine;

namespace DroneSimulator.API.Models
{
    [Serializable]
    public class StartScenarioRequest
    {
        public string location_name;
        public string scenario_name;
        public string drone_name;
    }

    [Serializable]
    public class EndScenarioRequest
    {
        public string location_name;
        public string scenario_name;
        public string drone_name;
    }

    [Serializable]
    public class ScenarioData
    {
        public int id;
        public int user;
        public string user_email;
        public string username;
        public string location_name;
        public string scenario_name;
        public string drone_name;
        public string start_time;
        public string end_time;
        public string duration;
        public string duration_seconds;
        public bool is_active;
        public string created_at;
        public string updated_at;
    }

    [Serializable]
    public class StartScenarioResponse
    {
        public string message;
        public ScenarioData data;
    }

    [Serializable]
    public class EndScenarioResponse
    {
        public string message;
        public ScenarioData data;
    }
} 