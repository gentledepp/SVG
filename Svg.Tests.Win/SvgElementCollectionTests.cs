using NUnit.Framework;
using Shouldly;

namespace Svg.Tests.Win
{
    [TestFixture]
    public class SvgElementCollectionTests
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [Test]
        public void DetachAll_EmptiesCollection_WithoutNullingChildParents()
        {
            // Arrange — Remove() normally sets item._parent = null. DetachAll is the
            // escape hatch for containers that share children with other owners and
            // must leave the items' parent-pointers alone.
            var doc = new SvgDocument();
            var rect = new SvgRectangle { X = 0, Y = 0, Width = 10, Height = 10 };
            var circle = new SvgCircle { Radius = 5 };
            doc.Children.Add(rect);
            doc.Children.Add(circle);
            rect.Parent.ShouldBe(doc);
            circle.Parent.ShouldBe(doc);

            // Act
            doc.Children.DetachAll();

            // Assert
            doc.Children.Count.ShouldBe(0);
            rect.Parent.ShouldBe(doc, "DetachAll must not null _parent (Remove() does that)");
            circle.Parent.ShouldBe(doc);
        }

        [Test]
        public void DetachAll_DoesNotFireChildRemovedEvent()
        {
            // Arrange — Remove() raises ChildRemoved and invokes OnSubTreeChanged.
            // DetachAll skips the notification so listeners don't observe a bogus
            // "removal" when the container is just being torn down.
            var doc = new SvgDocument();
            doc.Children.Add(new SvgRectangle());
            var removedEvents = 0;
            doc.ChildRemoved += (_, _) => removedEvents++;

            // Act
            doc.Children.DetachAll();

            // Assert
            removedEvents.ShouldBe(0);
        }

        [Test]
        public void DetachAll_DoesNotUnregisterChildrenFromIdManager()
        {
            // Arrange — Remove() cascades through OwnerDocument.IdManager to purge
            // the removed subtree's IDs. DetachAll must NOT do that, so shared
            // children remain addressable from their other owner's IdManager.
            var doc = new SvgDocument();
            var rect = new SvgRectangle { ID = "keep-me" };
            doc.Children.Add(rect);
            doc.IdManager.GetElementById("keep-me").ShouldBe(rect);

            // Act
            doc.Children.DetachAll();

            // Assert
            doc.IdManager.GetElementById("keep-me").ShouldBe(rect,
                "DetachAll must leave the IdManager untouched");
        }

        [Test]
        public void DetachAll_OnEmptyCollection_IsNoOp()
        {
            // Arrange
            var doc = new SvgDocument();

            // Act / Assert — must not throw
            Should.NotThrow(() => doc.Children.DetachAll());
            doc.Children.Count.ShouldBe(0);
        }

        [Test]
        public void Clear_StillNullsParent_AndDetachAll_Does_Not()
        {
            // Arrange — pin down the contrast: Clear() goes through Remove() and
            // nulls _parent; DetachAll() does not. Regression guard for either
            // path accidentally taking on the other's behavior.
            var docA = new SvgDocument();
            var docB = new SvgDocument();
            var rectCleared = new SvgRectangle();
            var rectDetached = new SvgRectangle();
            docA.Children.Add(rectCleared);
            docB.Children.Add(rectDetached);

            // Act
            docA.Children.Clear();
            docB.Children.DetachAll();

            // Assert
            rectCleared.Parent.ShouldBeNull("Clear() goes through Remove() which nulls _parent");
            rectDetached.Parent.ShouldBe(docB, "DetachAll() must not null _parent");
        }
    }
}
