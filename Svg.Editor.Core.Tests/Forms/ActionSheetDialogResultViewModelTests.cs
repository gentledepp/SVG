using Avalonia.Labs.Controls;
using NUnit.Framework;
using Shouldly;
using Svg.Editor.Avalon.Forms.Dialog.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Svg.Editor.Core.Tests.Forms
{
    [TestFixture]
    public class ActionSheetDialogResultViewModelTests
    {
        private static readonly string[] StringItems = { "Small", "Medium", "Large" };

        private static readonly List<KeyValuePair<string, object?>> KvpItems = new()
    {
        new("Small", 1),
        new("Medium", 2),
        new("Large", 3),
    };

        [Test]
        public void Constructor_WithStringItems_CreatesOneItemPerInput()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems);

            vm.Items.Count.ShouldBe(StringItems.Length);
        }

        [Test]
        public void Constructor_WithStringItems_TitleAndValueAreTheSameString()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems);

            vm.Items.Select(i => i.Title).ShouldBe(StringItems);
            vm.Items.Select(i => i.Value).ShouldBe(StringItems.Cast<object?>());
        }

        [Test]
        public void Constructor_WithStringItems_NoSelectedIndexProvided_SelectsFirstItem()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems);

            vm.SelectedItem.ShouldNotBeNull();
            vm.SelectedItem!.Title.ShouldBe("Small");
        }

        [Test]
        public void Constructor_WithEmptyStringItems_SelectedItemIsNull()
        {
            var vm = new ActionSheetDialogResultViewModel(Enumerable.Empty<string>());

            vm.Items.ShouldBeEmpty();
            vm.SelectedItem.ShouldBeNull();
        }


        [Test]
        public void Constructor_WithValidSelectedIndex_SelectsThatItem()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems, selectedIndex: 1);

            vm.SelectedItem.ShouldNotBeNull();
            vm.SelectedItem!.Title.ShouldBe("Medium");
        }

        [Test]
        public void Constructor_WithSelectedIndexAtLastPosition_SelectsLastItem()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems, selectedIndex: StringItems.Length - 1);

            vm.SelectedItem!.Title.ShouldBe("Large");
        }

        [Test]
        public void Constructor_WithNegativeSelectedIndex_FallsBackToFirstItem()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems, selectedIndex: -1);

            vm.SelectedItem!.Title.ShouldBe("Small");
        }

        [Test]
        public void Constructor_WithSelectedIndexEqualToCount_FallsBackToFirstItem()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems, selectedIndex: StringItems.Length);

            vm.SelectedItem!.Title.ShouldBe("Small");
        }

        [Test]
        public void Constructor_WithSelectedIndexGreaterThanCount_FallsBackToFirstItem()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems, selectedIndex: 999);

            vm.SelectedItem!.Title.ShouldBe("Small");
        }

        [Test]
        public void Constructor_WithSelectedIndexOnEmptyItems_SelectedItemIsNull()
        {
            var vm = new ActionSheetDialogResultViewModel(Enumerable.Empty<string>(), selectedIndex: 0);

            vm.SelectedItem.ShouldBeNull();
        }

        [Test]
        public void Constructor_WithKeyValuePairs_CreatesItemsWithCorrectTitleAndValue()
        {
            var vm = new ActionSheetDialogResultViewModel(KvpItems);

            vm.Items.Count.ShouldBe(KvpItems.Count);
            vm.Items.Select(i => i.Title).ShouldBe(KvpItems.Select(k => k.Key));
            vm.Items.Select(i => i.Value).ShouldBe(KvpItems.Select(k => k.Value));
        }

        [Test]
        public void Constructor_WithKeyValuePairs_SelectsFirstItemByDefault()
        {
            var vm = new ActionSheetDialogResultViewModel(KvpItems);

            vm.SelectedItem!.Value.ShouldBe(1);
        }

        [Test]
        public void GetResult_ReturnsValueOfSelectedItem()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems, selectedIndex: 2);

            vm.GetResult().ShouldBe("Large");
        }

        [Test]
        public void GetResult_WhenSelectedItemIsNull_ReturnsNull()
        {
            var vm = new ActionSheetDialogResultViewModel(Enumerable.Empty<string>());

            vm.GetResult().ShouldBeNull();
        }

        [Test]
        public void SelectedItem_CanBeChangedAfterConstruction()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems);

            vm.SelectedItem = vm.Items[2];

            vm.SelectedItem.ShouldBe(vm.Items[2]);
            vm.GetResult().ShouldBe("Large");
        }

        [Test]
        public void SelectedItem_Set_RaisesPropertyChanged()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems);
            var raisedProperties = new List<string?>();

            ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

            vm.SelectedItem = vm.Items[1];

            raisedProperties.ShouldContain(nameof(ActionSheetDialogResultViewModel.SelectedItem));
        }

        [Test]
        public void SelectedItem_SetToSameValue_DoesNotRaisePropertyChanged()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems);
            var current = vm.SelectedItem;
            var raiseCount = 0;

            ((INotifyPropertyChanged)vm).PropertyChanged += (_, _) => raiseCount++;

            vm.SelectedItem = current;

            raiseCount.ShouldBe(0);
        }

        [Test]
        public void CanClose_PrimaryResultWithSelection_ReturnsTrue()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems, selectedIndex: 0);

            var result = InvokeCanClose(vm, ContentDialogResult.Primary);

            result.ShouldBeTrue();
        }

        [Test]
        public void CanClose_PrimaryResultWithNoSelection_ReturnsFalse()
        {
            var vm = new ActionSheetDialogResultViewModel(Enumerable.Empty<string>());

            var result = InvokeCanClose(vm, ContentDialogResult.Primary);

            result.ShouldBeFalse();
        }

        [Test]
        public void CanClose_PrimaryResult_AfterClearingSelection_ReturnsFalse()
        {
            var vm = new ActionSheetDialogResultViewModel(StringItems);
            vm.SelectedItem = null;

            var result = InvokeCanClose(vm, ContentDialogResult.Primary);

            result.ShouldBeFalse();
        }

        private static bool InvokeCanClose(ActionSheetDialogResultViewModel vm, ContentDialogResult result)
        {
            var method = typeof(ActionSheetDialogResultViewModel)
                .GetMethod("CanClose", BindingFlags.NonPublic | BindingFlags.Instance);

            method.ShouldNotBeNull("CanClose method should exist via reflection");

            return (bool)method!.Invoke(vm, new object[] { result })!;
        }

        [Test]
        public void ActionSheetItem_ExposesTitleAndValue()
        {
            var item = new ActionSheetItem("MyTitle", 42);

            item.Title.ShouldBe("MyTitle");
            item.Value.ShouldBe(42);
        }

        [Test]
        public void ActionSheetItem_AllowsNullValue()
        {
            var item = new ActionSheetItem("MyTitle", null);

            item.Value.ShouldBeNull();
        }

    }
}
