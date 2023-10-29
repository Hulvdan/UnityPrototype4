using BFG.Runtime;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public class TestMovementGraphTile {
    [Test]
    public void Test_Count() {
        {
            var tile = new MovementGraphTile(false, false, false, false);
            Assert.AreEqual(0, tile.Count());
        }

        {
            var tile = new MovementGraphTile(false, true, false, false);
            Assert.AreEqual(1, tile.Count());
        }

        {
            var tile = new MovementGraphTile(true, true, false, false);
            Assert.AreEqual(2, tile.Count());
        }

        {
            var tile = new MovementGraphTile(false, false, true, true);
            Assert.AreEqual(2, tile.Count());
        }

        {
            var tile = new MovementGraphTile(false, true, false, true);
            Assert.AreEqual(2, tile.Count());
        }

        {
            var tile = new MovementGraphTile(true, false, true, false);
            Assert.AreEqual(2, tile.Count());
        }

        {
            var tile = new MovementGraphTile(true, false, true, true);
            Assert.AreEqual(3, tile.Count());
        }

        {
            var tile = new MovementGraphTile(true, true, true, true);
            Assert.AreEqual(4, tile.Count());
        }
    }

    [Test]
    public void Test_Rotation() {
        {
            var tile = new MovementGraphTile(false, false, false, false);
            Assert.AreEqual(0, tile.Rotation());
        }

        {
            var tile = new MovementGraphTile(false, true, false, false);
            Assert.AreEqual(1, tile.Rotation());
        }

        {
            var tile = new MovementGraphTile(false, false, true, false);
            Assert.AreEqual(2, tile.Rotation());
        }

        {
            var tile = new MovementGraphTile(true, true, false, false);
            Assert.AreEqual(0, tile.Rotation());
        }

        {
            var tile = new MovementGraphTile(false, false, true, true);
            Assert.AreEqual(2, tile.Rotation());
        }

        {
            var tile = new MovementGraphTile(true, false, true, false);
            Assert.AreEqual(0, tile.Rotation());
        }

        {
            var tile = new MovementGraphTile(false, true, false, true);
            Assert.AreEqual(1, tile.Rotation());
        }

        {
            var tile = new MovementGraphTile(true, true, true, false);
            Assert.AreEqual(0, tile.Rotation());
        }

        {
            var tile = new MovementGraphTile(true, false, true, true);
            Assert.AreEqual(2, tile.Rotation());
        }

        {
            var tile = new MovementGraphTile(true, true, true, true);
            Assert.AreEqual(0, tile.Rotation());
        }
    }
}
