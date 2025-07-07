﻿
namespace AspnetcoreEx.KernelService.Skills;

using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

public class LightPlugin
{
    // Mock data for the lights
    private readonly List<LightModel> lights =
   [
      new LightModel { Id = 1, Name = "Table Lamp", IsOn = false },
      new LightModel { Id = 2, Name = "Porch light", IsOn = false },
      new LightModel { Id = 3, Name = "Chandelier", IsOn = true }
   ];

    [KernelFunction("get_lights")]
    [Description("Gets a list of lights and their current state")]
    public Task<List<LightModel>> GetLightsAsync()
    {
        return Task.FromResult(lights);
    }

    [KernelFunction("change_state")]
    [Description("Changes the state of the light")]
    public Task<LightModel?> ChangeStateAsync(int id, bool isOn)
    {
        var light = lights.FirstOrDefault(light => light.Id == id);

        if (light == null)
        {
            return Task.FromResult<LightModel?>(null);
        }

        // Update the light with the new state
        light.IsOn = isOn;

        return Task.FromResult<LightModel?>(light);
    }
}

public class LightModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("is_on")]
    public bool? IsOn { get; set; }
}