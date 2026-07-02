using NUnit.Framework;
using Shouldly;
using Svg.Editor.Avalon.Forms.Dialog.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace Svg.Editor.Core.Tests.Forms
{
    [TestFixture]
    public class TextOptionsDialogResultViewModelTests
    {
        private TextOptionsDialogResultViewModel _vm = null!;

        [SetUp]
        public void SetUp()
        {
            _vm = new TextOptionsDialogResultViewModel();
        }

        [Test]
        public void GetResult_WithValidSelectedIndex_ReturnsMatchingOptionAndIndex()
        {
            _vm.UserInput = "user text";
            _vm.Options = new List<string> { "Alpha", "Beta", "Gamma" };
            _vm.SelectedIndex = 1;

            var result = _vm.GetResult();

            result.ShouldNotBeNull();
            result!.Text.ShouldBe("user text");
            result.SelectedOption.ShouldBe("Beta");
            result.SelectedIndex.ShouldBe(1);
        }

        [Test]
        public void GetResult_WithNegativeSelectedIndex_ReturnsNullSelectedOption()
        {
            _vm.Options = new List<string> { "Alpha", "Beta" };
            _vm.SelectedIndex = -1;

            var result = _vm.GetResult();

            result.ShouldNotBeNull();
            result!.SelectedOption.ShouldBeNull();
            result.SelectedIndex.ShouldBe(-1);
        }

        [Test]
        public void GetResult_WithSelectedIndexGreaterThanOrEqualToCount_ReturnsNullSelectedOption()
        {
            _vm.Options = new List<string> { "Alpha", "Beta" };
            _vm.SelectedIndex = 2;

            var result = _vm.GetResult();

            result.ShouldNotBeNull();
            result!.SelectedOption.ShouldBeNull();
            result.SelectedIndex.ShouldBe(2);
        }

        [Test]
        public void GetResult_WithEmptyOptions_ReturnsNullSelectedOption()
        {
            _vm.Options = Array.Empty<string>();
            _vm.SelectedIndex = 0;

            var result = _vm.GetResult();

            result.ShouldNotBeNull();
            result!.SelectedOption.ShouldBeNull();
        }

        [Test]
        public void GetResult_WithNullUserInput_ReturnsNullText()
        {
            _vm.UserInput = null;
            _vm.Options = new List<string> { "Alpha" };
            _vm.SelectedIndex = 0;

            var result = _vm.GetResult();

            result.ShouldNotBeNull();
            result!.Text.ShouldBeNull();
            result.SelectedOption.ShouldBe("Alpha");
        }

        [Test]
        public void GetResult_DefaultState_ReturnsNullTextAndNullSelectedOptionWithZeroIndex()
        {
            var result = _vm.GetResult();

            result.ShouldNotBeNull();
            result!.Text.ShouldBeNull();
            result.SelectedOption.ShouldBeNull();
            result.SelectedIndex.ShouldBe(0);
        }
    }
}
