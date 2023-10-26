using BFG.Runtime;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public class TestCell {
    [Test]
    public void TestCellCount() {
        {
            var cell = new MovementGraphCell(false, false, false, false);
            Assert.AreEqual(0, cell.Count());
        }

        {
            var cell = new MovementGraphCell(false, true, false, false);
            Assert.AreEqual(1, cell.Count());
        }

        {
            var cell = new MovementGraphCell(true, true, false, false);
            Assert.AreEqual(2, cell.Count());
        }

        {
            var cell = new MovementGraphCell(false, false, true, true);
            Assert.AreEqual(2, cell.Count());
        }

        {
            var cell = new MovementGraphCell(false, true, false, true);
            Assert.AreEqual(2, cell.Count());
        }

        {
            var cell = new MovementGraphCell(true, false, true, false);
            Assert.AreEqual(2, cell.Count());
        }

        {
            var cell = new MovementGraphCell(true, false, true, true);
            Assert.AreEqual(3, cell.Count());
        }

        {
            var cell = new MovementGraphCell(true, true, true, true);
            Assert.AreEqual(4, cell.Count());
        }
    }

    [Test]
    public void TestCellRotation() {
        {
            var cell = new MovementGraphCell(false, false, false, false);
            Assert.AreEqual(0, cell.Rotation());
        }

        {
            var cell = new MovementGraphCell(false, true, false, false);
            Assert.AreEqual(1, cell.Rotation());
        }

        {
            var cell = new MovementGraphCell(false, false, true, false);
            Assert.AreEqual(2, cell.Rotation());
        }

        {
            var cell = new MovementGraphCell(true, true, false, false);
            Assert.AreEqual(0, cell.Rotation());
        }

        {
            var cell = new MovementGraphCell(false, false, true, true);
            Assert.AreEqual(2, cell.Rotation());
        }

        {
            var cell = new MovementGraphCell(true, false, true, false);
            Assert.AreEqual(0, cell.Rotation());
        }

        {
            var cell = new MovementGraphCell(false, true, false, true);
            Assert.AreEqual(1, cell.Rotation());
        }

        {
            var cell = new MovementGraphCell(true, true, true, false);
            Assert.AreEqual(0, cell.Rotation());
        }

        {
            var cell = new MovementGraphCell(true, false, true, true);
            Assert.AreEqual(2, cell.Rotation());
        }

        {
            var cell = new MovementGraphCell(true, true, true, true);
            Assert.AreEqual(0, cell.Rotation());
        }
    }
}
