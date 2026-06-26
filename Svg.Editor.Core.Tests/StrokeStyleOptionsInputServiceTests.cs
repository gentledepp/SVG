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
    public class StrokeStyleOptionsInputServiceTests
    {
        private Mock<IUserInteraction> _userInteractionMock;
        private Mock<ILocalizationService> _localizationServiceMock;
        private StrokeStyleOptionsInputService _service;

        [SetUp]
        public void Setup()
        {
            SvgEditor.Init();
            _userInteractionMock = new Mock<IUserInteraction>();
            _localizationServiceMock = new Mock<ILocalizationService>();
            SvgEngine.Register<IUserInteraction>(() => _userInteractionMock.Object);
            SvgEngine.Register<ILocalizationService>(() => _localizationServiceMock.Object);

            _service = new StrokeStyleOptionsInputService();

            // Setup localization strings
            _localizationServiceMock
                .Setup(x => x.GetString(It.IsAny<string>(), It.IsAny<CultureInfo>()))
                .Returns((string key, CultureInfo culture) => key);
        }

        [Test]
        public async Task GetUserInput_WithValidSelections_ReturnsStrokeStyleOptions()
        {
            // Arrange
            var strokeDashOptions = new[] { "solid", "dashed", "dotted" };
            var strokeWidthOptions = new[] { "thin", "medium", "thick" };
            const string title = "Choose Stroke Style";

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>()))
                .ReturnsAsync("dashed")  // User selects "dashed" (index 1)
                .ReturnsAsync("thick");   // User selects "thick" (index 2)

            // Act
            var result = await _service.GetUserInput(title, strokeDashOptions, 0, strokeWidthOptions, 0);

            // Assert
            result.ShouldNotBeNull();
            result.StrokeDashIndex.ShouldBe(1);
            result.StrokeWidthIndex.ShouldBe(2);
        }

        [Test]
        public async Task GetUserInput_WithDashCancellation_ReturnsNull()
        {
            // Arrange
            var strokeDashOptions = new[] { "solid", "dashed", "dotted" };
            var strokeWidthOptions = new[] { "thin", "medium", "thick" };

            _userInteractionMock
                .Setup(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((string)null); // User cancels

            // Act
            var result = await _service.GetUserInput("Test", strokeDashOptions, 0, strokeWidthOptions, 0);

            // Assert
            result.ShouldBeNull();
            _userInteractionMock.Verify(
                x => x.ActionSheetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken?>(), It.IsAny<bool>()),
                Times.Once, "Should only call ActionSheetAsync once for dash options");
        }

        [Test]
        public async Task GetUserInput_WithWidthCancellation_ReturnsNull()
        {
            // Arrange
            var strokeDashOptions = new[] { "solid", "dashed", "dotted" };
            var strokeWidthOptions = new[] { "thin", "medium", "thick" };

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>()))
                .ReturnsAsync("dashed")  // User selects dash
                .ReturnsAsync((string)null); // User cancels width

            // Act
            var result = await _service.GetUserInput("Test", strokeDashOptions, 0, strokeWidthOptions, 0);

            // Assert
            result.ShouldBeNull();
            _userInteractionMock.Verify(
                x => x.ActionSheetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken?>(), It.IsAny<bool>()),
                Times.Exactly(2), "Should call ActionSheetAsync twice");
        }

        [Test]
        public async Task GetUserInput_WithFirstOption_ReturnsZeroIndices()
        {
            // Arrange
            var strokeDashOptions = new[] { "solid", "dashed", "dotted" };
            var strokeWidthOptions = new[] { "thin", "medium", "thick" };

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>()))
                .ReturnsAsync("solid")  // First option
                .ReturnsAsync("thin");   // First option

            // Act
            var result = await _service.GetUserInput("Test", strokeDashOptions, 0, strokeWidthOptions, 0);

            // Assert
            result.ShouldNotBeNull();
            result.StrokeDashIndex.ShouldBe(0);
            result.StrokeWidthIndex.ShouldBe(0);
        }

        [Test]
        public async Task GetUserInput_CallsActionSheetWithCorrectParameters()
        {
            // Arrange
            var strokeDashOptions = new[] { "solid", "dashed" };
            var strokeWidthOptions = new[] { "thin", "thick" };
            const string title = "Stroke Options";

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>()))
                .ReturnsAsync("dashed")
                .ReturnsAsync("thick");

            // Act
            await _service.GetUserInput(title, strokeDashOptions, 0, strokeWidthOptions, 0);

            // Assert
            _userInteractionMock.Verify(
                x => x.ActionSheetAsync(
                    title,
                    It.Is<IEnumerable<string>>(options => options.SequenceEqual(strokeDashOptions)),
                    It.IsAny<string>(),
                    "Svg.Editor.Global.Cancel",
                    It.IsAny<System.Threading.CancellationToken?>(),
                    true),
                Times.Once);

            _userInteractionMock.Verify(
                x => x.ActionSheetAsync(
                    title,
                    It.Is<IEnumerable<string>>(options => options.SequenceEqual(strokeWidthOptions)),
                    It.IsAny<string>(),
                    "Svg.Editor.Global.Cancel",
                    It.IsAny<System.Threading.CancellationToken?>(),
                    true),
                Times.Once);
        }

        [Test]
        public async Task GetUserInput_WithLastOption_ReturnsCorrectIndices()
        {
            // Arrange
            var strokeDashOptions = new[] { "solid", "dashed", "dotted", "dashdot" };
            var strokeWidthOptions = new[] { "1px", "2px", "3px", "4px", "5px" };

            _userInteractionMock
                .SetupSequence(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Threading.CancellationToken?>(),
                    It.IsAny<bool>()))
                .ReturnsAsync("dashdot")  // Index 3
                .ReturnsAsync("5px");      // Index 4

            // Act
            var result = await _service.GetUserInput("Test", strokeDashOptions, 0, strokeWidthOptions, 0);

            // Assert
            result.ShouldNotBeNull();
            result.StrokeDashIndex.ShouldBe(3);
            result.StrokeWidthIndex.ShouldBe(4);
        }
    }
}