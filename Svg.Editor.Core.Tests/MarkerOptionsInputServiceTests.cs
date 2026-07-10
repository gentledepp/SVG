using Moq;
using NUnit.Framework;
using Shouldly;
using Svg.Editor.Avalon.Forms.Dialog;
using Svg.Editor.Avalon.Forms.Services;
using Svg.Editor.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Editor.Core.Tests
{
    [TestFixture]
    public class MarkerOptionsInputServiceTests
    {
        private Mock<IUserInteraction> _userInteractionMock;
        private Mock<ILocalizationService> _localizationServiceMock;
        private MarkerOptionsInputService _service;

        [SetUp]
        public void Setup()
        {
            SvgEditor.Init();
            _userInteractionMock = new Mock<IUserInteraction>();
            _localizationServiceMock = new Mock<ILocalizationService>();
            SvgEngine.Register<IUserInteraction>(() => _userInteractionMock.Object);
            SvgEngine.Register<ILocalizationService>(() => _localizationServiceMock.Object);

            _service = new MarkerOptionsInputService();

            // Setup localization strings
            _localizationServiceMock
                .Setup(x => x.GetString(It.IsAny<string>(), It.IsAny<CultureInfo>()))
                .Returns((string key, CultureInfo culture) => key);
        }

        [Test]
        public async Task GetUserInput_WithValidSelections_ReturnsSelectedIndices()
        {
            // Arrange
            var markerStartOptions = new[] { "None", "Arrow", "Circle" };
            var markerEndOptions = new[] { "None", "Diamond", "Square" };

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .ReturnsAsync("Arrow")    // Index 1
                .ReturnsAsync("Square");  // Index 2

            // Act
            var result = await _service.GetUserInput(markerStartOptions, 0, markerEndOptions, 0);

            // Assert
            result.ShouldNotBeNull();
            result.Length.ShouldBe(2);
            result[0].ShouldBe(1);
            result[1].ShouldBe(2);
        }

        [Test]
        public async Task GetUserInput_WithStartMarkerCancellation_ReturnsOriginalIndices()
        {
            // Arrange
            var markerStartOptions = new[] { "None", "Arrow", "Circle" };
            var markerEndOptions = new[] { "None", "Diamond", "Square" };
            const int startSelected = 2;
            const int endSelected = 1;

            _userInteractionMock
                .Setup(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .ReturnsAsync((string)null); // User cancels

            // Act
            var result = await _service.GetUserInput(markerStartOptions, startSelected, markerEndOptions, endSelected);

            // Assert
            result.ShouldNotBeNull();
            result.Length.ShouldBe(2);
            result[0].ShouldBe(startSelected);
            result[1].ShouldBe(endSelected);
            _userInteractionMock.Verify(
                x => x.ActionSheetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken?>(), It.IsAny<bool>(), 2),
                Times.Once, "Should only call ActionSheetAsync once");
        }

        [Test]
        public async Task GetUserInput_WithEndMarkerCancellation_ReturnsOriginalIndices()
        {
            // Arrange
            var markerStartOptions = new[] { "None", "Arrow", "Circle" };
            var markerEndOptions = new[] { "None", "Diamond", "Square" };
            const int startSelected = 1;
            const int endSelected = 2;

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .ReturnsAsync("Arrow")     // User selects start
                .ReturnsAsync((string)null); // User cancels end

            // Act
            var result = await _service.GetUserInput(markerStartOptions, startSelected, markerEndOptions, endSelected);

            // Assert
            result.ShouldNotBeNull();
            result.Length.ShouldBe(2);
            result[0].ShouldBe(startSelected);
            result[1].ShouldBe(endSelected);
            _userInteractionMock.Verify(
                x => x.ActionSheetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken?>(), It.IsAny<bool>(), 1),
                Times.Exactly(1), "Should call ActionSheetAsync twice");
            _userInteractionMock.Verify(
                x => x.ActionSheetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken?>(), It.IsAny<bool>(), 2),
                Times.Exactly(1), "Should call ActionSheetAsync twice");
        }

        [Test]
        public async Task GetUserInput_WithFirstOptions_ReturnsZeroIndices()
        {
            // Arrange
            var markerStartOptions = new[] { "None", "Arrow" };
            var markerEndOptions = new[] { "None", "Diamond" };

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .ReturnsAsync("None")  // First option
                .ReturnsAsync("None"); // First option

            // Act
            var result = await _service.GetUserInput(markerStartOptions, 1, markerEndOptions, 1);

            // Assert
            result.ShouldNotBeNull();
            result[0].ShouldBe(0);
            result[1].ShouldBe(0);
        }

        [Test]
        public async Task GetUserInput_WithInvalidSelection_ReturnsOriginalIndices()
        {
            // Arrange
            var markerStartOptions = new[] { "None", "Arrow", "Circle" };
            var markerEndOptions = new[] { "None", "Diamond", "Square" };
            const int startSelected = 1;
            const int endSelected = 2;

            _userInteractionMock
                .Setup(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .ReturnsAsync("InvalidOption"); // Option not in list

            // Act
            var result = await _service.GetUserInput(markerStartOptions, startSelected, markerEndOptions, endSelected);

            // Assert
            result.ShouldNotBeNull();
            result[0].ShouldBe(startSelected);
            result[1].ShouldBe(endSelected);
        }

        [Test]
        public async Task GetUserInput_CallsActionSheetWithCorrectStartParameters()
        {
            // Arrange
            var markerStartOptions = new[] { "None", "Arrow" };
            var markerEndOptions = new[] { "None", "Diamond" };

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .ReturnsAsync("Arrow")
                .ReturnsAsync("Diamond");

            // Act
            await _service.GetUserInput(markerStartOptions, 0, markerEndOptions, 0);

            // Assert - Verify first call (start markers)
            _userInteractionMock.Verify(
                x => x.ActionSheetAsync(
                    "Svg.Editor.Marker.Options.Start",
                    It.Is<IEnumerable<string>>(options => options.SequenceEqual(markerStartOptions)),
                    It.IsAny<string>(),
                    "Svg.Editor.Global.Cancel",
                    It.IsAny<System.Threading.CancellationToken?>(),
                    true,
                    0),
                Times.Once);
        }

        [Test]
        public async Task GetUserInput_CallsActionSheetWithCorrectEndParameters()
        {
            // Arrange
            var markerStartOptions = new[] { "None", "Arrow" };
            var markerEndOptions = new[] { "None", "Diamond" };

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .ReturnsAsync("Arrow")
                .ReturnsAsync("Diamond");

            // Act
            await _service.GetUserInput(markerStartOptions, 0, markerEndOptions, 0);

            // Assert - Verify second call (end markers)
            _userInteractionMock.Verify(
                x => x.ActionSheetAsync(
                    "Svg.Editor.Marker.Options.End",
                    It.Is<IEnumerable<string>>(options => options.SequenceEqual(markerEndOptions)),
                    It.IsAny<string>(),
                    "Svg.Editor.Global.Cancel",
                    It.IsAny<System.Threading.CancellationToken?>(),
                    true,
                    0),
                Times.Once);
        }

        [Test]
        public async Task GetUserInput_WithLastOptions_ReturnsCorrectIndices()
        {
            // Arrange
            var markerStartOptions = new[] { "None", "Arrow", "Circle", "Triangle" };
            var markerEndOptions = new[] { "None", "Diamond", "Square", "Pentagon", "Hexagon" };

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .ReturnsAsync("Triangle")  // Index 3
                .ReturnsAsync("Hexagon");   // Index 4

            // Act
            var result = await _service.GetUserInput(markerStartOptions, 0, markerEndOptions, 0);

            // Assert
            result.ShouldNotBeNull();
            result[0].ShouldBe(3);
            result[1].ShouldBe(4);
        }

        [Test]
        public async Task GetUserInput_PreservesSelectedIndicesAcrossMultipleCalls()
        {
            // Arrange
            var markerStartOptions = new[] { "None", "Arrow", "Circle" };
            var markerEndOptions = new[] { "None", "Diamond", "Square" };
            const int startSelected = 2;
            const int endSelected = 1;

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .ReturnsAsync((string)null)  // Cancel first time
                .ReturnsAsync("Arrow")       // Select second time
                .ReturnsAsync("Square");     // Select third time

            // Act
            var result1 = await _service.GetUserInput(markerStartOptions, startSelected, markerEndOptions, endSelected);
            var result2 = await _service.GetUserInput(markerStartOptions, startSelected, markerEndOptions, endSelected);

            // Assert
            result1[0].ShouldBe(startSelected); // Should be preserved
            result1[1].ShouldBe(endSelected);
            result2[0].ShouldBe(1);
            result2[1].ShouldBe(2);
        }
    }
}