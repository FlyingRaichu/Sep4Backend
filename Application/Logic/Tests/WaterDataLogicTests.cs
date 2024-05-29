using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Logic;
using Application.ServiceInterfaces;
using DatabaseInterfacing.Domain.DTOs;
using IoTInterfacing.Interfaces;
using Moq;
using Xunit;

namespace YourNamespace.Tests
{
    public class WaterDataLogicTests
    {
        [Fact]
        public async Task CheckWaterTemperatureAsync_NormalTemperature_ReturnsNormalStatus()
        {
            var value = 20; 

            var jsonString = $@"
                {{
                    ""name"": ""monitoring_results"",
                    ""readings"": [
                        {{
                            ""waterTemperature"": {value}
                        }}
                    ]
                }}";

            var connectionControllerMock = new Mock<IConnectionController>();
            connectionControllerMock.Setup(c => c.SendRequestToArduinoAsync(It.IsAny<string>())).ReturnsAsync(jsonString);

            var configurationServiceMock = new Mock<IThresholdConfigurationService>();
            configurationServiceMock.Setup(c => c.GetConfigurationAsync()).ReturnsAsync(new ThresholdConfigurationDto
            {
                Thresholds = new List<ThresholdDto>
                {
                    new() { Type = "waterTemperature", Min = 10, Max = 30, WarningMin = 15, WarningMax = 25 }
                }
            });
            var waterDataLogic = new WaterDataLogic(connectionControllerMock.Object, configurationServiceMock.Object, null);
            var result = await waterDataLogic.CheckWaterTemperatureAsync();
            Assert.Equal("Normal", result.Status);
            value = 12;
            var result2 = await waterDataLogic.CheckWaterTemperatureAsync();
            Assert.Equal("Warning", result.Status);
            value = 27;
            var result3 = await waterDataLogic.CheckWaterTemperatureAsync();
            Assert.Equal("Warning", result.Status);
            value = 31;
            var result4 = await waterDataLogic.CheckWaterTemperatureAsync();
            Assert.Equal("Danger", result.Status);
            value = 5;
            var result5 = await waterDataLogic.CheckWaterTemperatureAsync();
            Assert.Equal("Danger", result.Status);
        }
        
        /*More methods can be added to check, however, due to the similar structure of all the other methods
        more test are excluded for now. */
        
    }
}