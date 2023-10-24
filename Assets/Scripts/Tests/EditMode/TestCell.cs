using BFG.Runtime;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public class TestCell {
    [Test]
    public void TestCellCount() {
        {
            var cell = new MCell(false, false, false, false);
            Assert.AreEqual(cell.Count(), 0);
        }

        {
            var cell = new MCell(true, false, false, false);
            Assert.AreEqual(cell.Count(), 1);
        }

        {
            var cell = new MCell(true, true, false, false);
            Assert.AreEqual(cell.Count(), 2);
        }

        {
            var cell = new MCell(false, false, true, true);
            Assert.AreEqual(cell.Count(), 2);
        }

        {
            var cell = new MCell(true, false, true, false);
            Assert.AreEqual(cell.Count(), 2);
        }

        {
            var cell = new MCell(false, true, false, true);
            Assert.AreEqual(cell.Count(), 2);
        }

        {
            var cell = new MCell(false, true, true, true);
            Assert.AreEqual(cell.Count(), 3);
        }

        {
            var cell = new MCell(true, true, true, true);
            Assert.AreEqual(cell.Count(), 4);
        }
    }

    [Test]
    public void TestCellRotation() {
        {
            var cell = new MCell(false, false, false, false);
            Assert.AreEqual(cell.Rotation(), 0);
        }

        {
            var cell = new MCell(true, false, false, false);
            Assert.AreEqual(cell.Rotation(), 0);
        }

        {
            var cell = new MCell(false, false, false, true);
            Assert.AreEqual(cell.Rotation(), 3);
        }

        {
            var cell = new MCell(true, true, false, false);
            Assert.AreEqual(cell.Rotation(), 0);
        }

        {
            var cell = new MCell(false, false, true, true);
            Assert.AreEqual(cell.Rotation(), 2);
        }

        {
            var cell = new MCell(true, false, true, false);
            Assert.AreEqual(cell.Rotation(), 0);
        }

        {
            var cell = new MCell(false, true, false, true);
            Assert.AreEqual(cell.Rotation(), 1);
        }

        {
            var cell = new MCell(true, true, false, true);
            Assert.AreEqual(cell.Rotation(), 0);
        }

        {
            var cell = new MCell(false, true, true, true);
            Assert.AreEqual(cell.Rotation(), 2);
        }

        {
            var cell = new MCell(true, true, true, true);
            Assert.AreEqual(cell.Rotation(), 0);
        }
    }
}
