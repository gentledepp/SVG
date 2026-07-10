using Moq;
using NUnit.Framework;
using Shouldly;
using Svg.Editor.Avalon.Forms.Dialog;
using Svg.Editor.Avalon.Forms.Services;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Svg.Editor.Core.Tests
{
    [TestFixture]
    public class PinInputServiceTests
    {
        private Mock<IUserInteraction> _userInteractionMock;
        private Mock<ILocalizationService> _localizationServiceMock;
        private PinInputService _service;

        private static readonly string[] DefaultOptions = { "Small", "Medium", "Large" };

        [SetUp]
        public void Setup()
        {
            SvgEditor.Init();

            _userInteractionMock = new Mock<IUserInteraction>();
            _localizationServiceMock = new Mock<ILocalizationService>();

            SvgEngine.Register<IUserInteraction>(() => _userInteractionMock.Object);
            SvgEngine.Register<ILocalizationService>(() => _localizationServiceMock.Object);

            // Localization just echoes back the key, so we can assert on it directly.
            _localizationServiceMock
                .Setup(x => x.GetString(It.IsAny<string>(), It.IsAny<CultureInfo>()))
                .Returns((string key, CultureInfo culture) => key);

            _service = new PinInputService();
        }

        private void SetupActionSheet(string returnValue)
        {
            _userInteractionMock
                .Setup(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .ReturnsAsync(returnValue);
        }

        [Test]
        public async Task GetUserInput_WhenActionSheetReturnsValidOptionName_ReturnsMatchingPinSize()
        {
            // Arrange
            SetupActionSheet("Medium");

            // Act
            var result = await _service.GetUserInput(DefaultOptions);

            // Assert
            result.ShouldBe(PinTool.PinSize.Medium);
        }

        [Test]
        public async Task GetUserInput_WhenActionSheetReturnsNull_ReturnsDefaultBasedOnOldSizeIndex()
        {
            // Arrange
            SetupActionSheet(null);

            // Act
            var result = await _service.GetUserInput(DefaultOptions, oldSizeIndex: 2);

            // Assert
            result.ShouldBe((PinTool.PinSize)2);
        }

        [Test]
        public async Task GetUserInput_WhenActionSheetReturnsUnparsableValue_FallsBackToDefault()
        {
            // Arrange
            SetupActionSheet("NotARealPinSize");

            // Act
            var result = await _service.GetUserInput(DefaultOptions, oldSizeIndex: 0);

            // Assert
            result.ShouldBe((PinTool.PinSize)0);
        }

        [Test]
        public async Task GetUserInput_WhenActionSheetReturnsEmptyString_FallsBackToDefault()
        {
            // Arrange
            SetupActionSheet(string.Empty);

            // Act
            var result = await _service.GetUserInput(DefaultOptions, oldSizeIndex: 1);

            // Assert
            result.ShouldBe((PinTool.PinSize)1);
        }

        [Test]
        public async Task GetUserInput_WithoutExplicitOldSizeIndex_DefaultsToIndexOne()
        {
            // Arrange
            SetupActionSheet(null);

            // Act
            var result = await _service.GetUserInput(DefaultOptions);

            // Assert
            result.ShouldBe((PinTool.PinSize)1);
        }

        [Test]
        public async Task GetUserInput_PassesLocalizedMessageAndCancelButton_ToActionSheet()
        {
            // Arrange
            string capturedMessage = null;
            string capturedCancelButton = null;
            bool capturedCancellable = false;

            _userInteractionMock
                .Setup(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .Callback<string, IEnumerable<string>, string, string, CancellationToken?, bool, int>(
                    (message, options, title, cancelButton, cancelToken, cancellable, defaultIndex) =>
                    {
                        capturedMessage = message;
                        capturedCancelButton = cancelButton;
                        capturedCancellable = cancellable;
                    })
                .ReturnsAsync("Medium");

            // Act
            await _service.GetUserInput(DefaultOptions);

            // Assert
            capturedMessage.ShouldBe("Svg.Editor.Global.Pin.Size.Selection");
            capturedCancelButton.ShouldBe("Svg.Editor.Global.Cancel");
            capturedCancellable.ShouldBeTrue();
        }

        [Test]
        public async Task GetUserInput_PassesGivenOptions_ConvertedToArray()
        {
            // Arrange
            var options = new List<string> { "Small", "Medium", "Large" };
            IEnumerable<string> capturedOptions = null;

            _userInteractionMock
                .Setup(x => x.ActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken?>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
                .Callback<string, IEnumerable<string>, string, string, CancellationToken?, bool, int>(
                    (message, opts, title, cancelButton, cancelToken, cancellable, defaultIndex) => capturedOptions = opts)
                .ReturnsAsync("Medium");

            // Act
            await _service.GetUserInput(options);

            // Assert
            capturedOptions.ShouldNotBeNull();
            capturedOptions.ShouldBeOfType<string[]>();
            capturedOptions.ShouldBe(options);
        }

        [Test]
        public async Task GetUserInput_CallsLocalizationService_ForSelectionPromptAndCancelButton()
        {
            // Arrange
            SetupActionSheet("Medium");

            // Act
            await _service.GetUserInput(DefaultOptions);

            // Assert
            _localizationServiceMock.Verify(
                x => x.GetString("Svg.Editor.Global.Pin.Size.Selection", It.IsAny<CultureInfo>()),
                Times.Once);
            _localizationServiceMock.Verify(
                x => x.GetString("Svg.Editor.Global.Cancel", It.IsAny<CultureInfo>()),
                Times.Once);
        }
    }
}