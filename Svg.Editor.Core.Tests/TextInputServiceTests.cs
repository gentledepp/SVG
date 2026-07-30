using Moq;
using NUnit.Framework;
using Shouldly;
using Svg.Editor.Avalon.Forms.Dialog;
using Svg.Editor.Avalon.Forms.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Svg.Editor.Core.Tests
{
    [TestFixture]
    public class TextInputServiceTests
    {
        private Mock<IUserInteraction> _userInteractionMock;
        private TextInputService _service;

        [SetUp]
        public void Setup()
        {
            SvgEditor.Init();
            _userInteractionMock = new Mock<IUserInteraction>();
            SvgEngine.Register<IUserInteraction>(() => _userInteractionMock.Object);

            _service = new TextInputService();
        }

        [Test]
        public async Task GetUserInput_WithConfirmedResult_ReturnsEnteredTextAndSelectedIndex()
        {
            // Arrange
            var sizeOptions = new[] { "Small", "Medium", "Large" };

            _userInteractionMock
                .Setup(x => x.InputWithOptionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new InputWithOptionsResponse(true, "Hello world", "Large", 2));

            // Act
            var result = await _service.GetUserInput("My Title", "initial", sizeOptions, textSizeSelected: 0);

            // Assert
            result.ShouldNotBeNull();
            result.Text.ShouldBe("Hello world");
            result.FontSizeIndex.ShouldBe(2);
            result.LineHeight.ShouldBe(1.25f);
        }

        [Test]
        public async Task GetUserInput_WithCancelledResult_ReturnsOriginalTextAndSelectedIndex()
        {
            // Arrange
            var sizeOptions = new[] { "Small", "Medium", "Large" };
            const string originalText = "initial text";
            const int originalIndex = 1;

            _userInteractionMock
                .Setup(x => x.InputWithOptionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new InputWithOptionsResponse(false, null, null, -1));

            // Act
            var result = await _service.GetUserInput("My Title", originalText, sizeOptions, originalIndex);

            // Assert
            result.ShouldNotBeNull();
            result.Text.ShouldBe(originalText);
            result.FontSizeIndex.ShouldBe(originalIndex);
            result.LineHeight.ShouldBe(1.25f);
        }

        [Test]
        public async Task GetUserInput_WithTextExceedingMaxLength_TruncatesText()
        {
            // Arrange
            _userInteractionMock
                .Setup(x => x.InputWithOptionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new InputWithOptionsResponse(true, "This is way too long", null, -1));

            // Act
            var result = await _service.GetUserInput("Title", "initial", null, 0, maxTextLength: 5);

            // Assert
            result.Text.ShouldBe("This ");
            result.Text.Length.ShouldBe(5);
        }

        [Test]
        public async Task GetUserInput_WithMaxTextLengthDisabled_DoesNotTruncateText()
        {
            // Arrange
            const string longText = "This is a fairly long piece of text";

            _userInteractionMock
                .Setup(x => x.InputWithOptionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new InputWithOptionsResponse(true, longText, null, -1));

            // Act
            var result = await _service.GetUserInput("Title", "initial", null, 0, maxTextLength: -1);

            // Assert
            result.Text.ShouldBe(longText);
        }

        [Test]
        public async Task GetUserInput_WithTextShorterThanMaxLength_DoesNotTruncateText()
        {
            // Arrange
            const string shortText = "Hi";

            _userInteractionMock
                .Setup(x => x.InputWithOptionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new InputWithOptionsResponse(true, shortText, null, -1));

            // Act
            var result = await _service.GetUserInput("Title", "initial", null, 0, maxTextLength: 10);

            // Assert
            result.Text.ShouldBe(shortText);
        }

        [Test]
        public async Task GetUserInput_WithNegativeSelectedIndex_FallsBackToTextSizeSelected()
        {
            // Arrange
            const int fallbackIndex = 3;

            _userInteractionMock
                .Setup(x => x.InputWithOptionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new InputWithOptionsResponse(true, "some text", null, -1));

            // Act
            var result = await _service.GetUserInput("Title", "initial", new[] { "A", "B" }, fallbackIndex);

            // Assert
            result.FontSizeIndex.ShouldBe(fallbackIndex);
        }

        [Test]
        public async Task GetUserInput_WithNullResultText_ReturnsEmptyString()
        {
            // Arrange
            _userInteractionMock
                .Setup(x => x.InputWithOptionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new InputWithOptionsResponse(true, null, null, 0));

            // Act
            var result = await _service.GetUserInput("Title", "initial", null, 0);

            // Assert
            result.Text.ShouldBe(string.Empty);
        }

        [Test]
        public async Task GetUserInput_WithNullTextSizeOptions_PassesEmptyListToInteractionService()
        {
            // Arrange
            _userInteractionMock
                .Setup(x => x.InputWithOptionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new InputWithOptionsResponse(true, "text", null, -1));

            // Act
            await _service.GetUserInput("Title", "initial", null, 0);

            // Assert
            _userInteractionMock.Verify(
                x => x.InputWithOptionsAsync(
                    It.IsAny<string>(),
                    It.Is<IEnumerable<string>>(o => !o.Any()),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()),
                Times.Once);
        }

        [Test]
        public async Task GetUserInput_CallsInputWithOptionsAsync_WithCorrectParameters()
        {
            // Arrange
            var sizeOptions = new[] { "Small", "Medium", "Large" };
            const string title = "Edit Text";
            const string initialText = "hello";
            const int selected = 1;

            _userInteractionMock
                .Setup(x => x.InputWithOptionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new InputWithOptionsResponse(true, initialText, "Medium", selected));

            // Act
            await _service.GetUserInput(title, initialText, sizeOptions, selected);

            // Assert
            _userInteractionMock.Verify(
                x => x.InputWithOptionsAsync(
                    "Text edit",
                    It.Is<IEnumerable<string>>(o => o.SequenceEqual(sizeOptions)),
                    title,
                    "Ok",
                    "Cancel",
                    initialText,
                    "Enter text",
                    selected,
                    It.IsAny<bool>()),
                Times.Once);
        }

        [Test]
        public async Task GetUserInput_WhenCancelled_DoesNotTruncateOriginalTextValue()
        {
            // Arrange
            const string originalText = "This is longer than the max length";

            _userInteractionMock
                .Setup(x => x.InputWithOptionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new InputWithOptionsResponse(false, null, null, -1));

            // Act
            var result = await _service.GetUserInput("Title", originalText, null, 0, maxTextLength: 5);

            // Assert - cancellation path returns textValue untouched, even beyond maxTextLength
            result.Text.ShouldBe(originalText);
        }
    }
}
