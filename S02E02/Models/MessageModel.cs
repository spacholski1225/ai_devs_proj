﻿using System.Text.Json.Serialization;

namespace ai_devs_proj.S02E02.Models
{
    internal class MessageModel
    {
        [JsonPropertyName("role")] public string Role { get; set; }
        [JsonPropertyName("content")] public string Content { get; set; }
    }
}