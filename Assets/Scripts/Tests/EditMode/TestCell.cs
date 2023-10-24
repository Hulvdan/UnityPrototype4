using BFG.Runtime;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public class TestCell {
    [Test]
    public void TestCellCount() {
        {
            var cell = new MovementGraphCell(false, false, false, false);
            Assert.AreEqual(cell.Count(), 0);
        }

        {
            var cell = new MovementGraphCell(true, false, false, false);
            Assert.AreEqual(cell.Count(), 1);
        }

        {
            var cell = new MovementGraphCell(true, true, false, false);
            Assert.AreEqual(cell.Count(), 2);
        }

        {
            var cell = new MovementGraphCell(false, false, true, true);
            Assert.AreEqual(cell.Count(), 2);
        }

        {
            var cell = new MovementGraphCell(true, false, true, false);
            Assert.AreEqual(cell.Count(), 2);
        }

        {
            var cell = new MovementGraphCell(false, true, false, true);
            Assert.AreEqual(cell.Count(), 2);
        }

        {
            var cell = new MovementGraphCell(false, true, true, true);
            Assert.AreEqual(cell.Count(), 3);
        }

        {
            var cell = new MovementGraphCell(true, true, true, true);
            Assert.AreEqual(cell.Count(), 4);
        }
    }

    [Test]
    public void TestCellRotation() {
        {
            var cell = new MovementGraphCell(false, false, false, false);
            Assert.AreEqual(cell.Rotation(), 0);
        }

        {
            var cell = new MovementGraphCell(true, false, false, false);
            Assert.AreEqual(cell.Rotation(), 0);
        }

        {
            var cell = new MovementGraphCell(false, false, false, true);
            Assert.AreEqual(cell.Rotation(), 1);
        }

        {
            var cell = new MovementGraphCell(true, true, false, false);
            Assert.AreEqual(cell.Rotation(), 0);
        }

        {
            var cell = new MovementGraphCell(false, false, true, true);
            Assert.AreEqual(cell.Rotation(), 2);
        }

        {
            var cell = new MovementGraphCell(true, false, true, false);
            Assert.AreEqual(cell.Rotation(), 0);
        }

        {
            var cell = new MovementGraphCell(false, true, false, true);
            Assert.AreEqual(cell.Rotation(), 1);
        }

        {
            var cell = new MovementGraphCell(true, true, false, true);
            Assert.AreEqual(cell.Rotation(), 0);
        }

        {
            var cell = new MovementGraphCell(false, true, true, true);
            Assert.AreEqual(cell.Rotation(), 2);
        }

        {
            var cell = new MovementGraphCell(true, true, true, true);
            Assert.AreEqual(cell.Rotation(), 0);
        }
    }
}
