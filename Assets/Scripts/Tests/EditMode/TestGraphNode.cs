using BFG.Graphs;
using BFG.Runtime;
using NUnit.Framework;

namespace Tests.EditMode {
public class TestGraphNode {
    [Test]
    public void Test() {
        byte node = 0;
        Assert.IsFalse(GraphNode.IsRight(node));
        Assert.IsFalse(GraphNode.IsUp(node));
        Assert.IsFalse(GraphNode.IsLeft(node));
        Assert.IsFalse(GraphNode.IsDown(node));

        node |= 1 << 0;
        Assert.IsTrue(GraphNode.IsRight(node));
        Assert.IsFalse(GraphNode.IsUp(node));
        Assert.IsFalse(GraphNode.IsLeft(node));
        Assert.IsFalse(GraphNode.IsDown(node));

        node |= 1 << 1;
        Assert.IsTrue(GraphNode.IsRight(node));
        Assert.IsTrue(GraphNode.IsUp(node));
        Assert.IsFalse(GraphNode.IsLeft(node));
        Assert.IsFalse(GraphNode.IsDown(node));

        node |= 1 << 2;
        Assert.IsTrue(GraphNode.IsRight(node));
        Assert.IsTrue(GraphNode.IsUp(node));
        Assert.IsTrue(GraphNode.IsLeft(node));
        Assert.IsFalse(GraphNode.IsDown(node));

        node |= 1 << 3;
        Assert.IsTrue(GraphNode.IsRight(node));
        Assert.IsTrue(GraphNode.IsUp(node));
        Assert.IsTrue(GraphNode.IsLeft(node));
        Assert.IsTrue(GraphNode.IsDown(node));

        byte node1 = 0;
        Assert.IsFalse(GraphNode.IsRight(node1));
        Assert.IsFalse(GraphNode.IsUp(node1));
        Assert.IsFalse(GraphNode.IsLeft(node1));
        Assert.IsFalse(GraphNode.IsDown(node1));
        byte node2 = 1 << 0;
        Assert.IsTrue(GraphNode.IsRight(node2));
        Assert.IsFalse(GraphNode.IsUp(node2));
        Assert.IsFalse(GraphNode.IsLeft(node2));
        Assert.IsFalse(GraphNode.IsDown(node2));
        byte node3 = 1 << 1;
        Assert.IsFalse(GraphNode.IsRight(node3));
        Assert.IsTrue(GraphNode.IsUp(node3));
        Assert.IsFalse(GraphNode.IsLeft(node3));
        Assert.IsFalse(GraphNode.IsDown(node3));
        byte node4 = 1 << 2;
        Assert.IsFalse(GraphNode.IsRight(node4));
        Assert.IsFalse(GraphNode.IsUp(node4));
        Assert.IsTrue(GraphNode.IsLeft(node4));
        Assert.IsFalse(GraphNode.IsDown(node4));
        byte node5 = 1 << 3;
        Assert.IsFalse(GraphNode.IsRight(node5));
        Assert.IsFalse(GraphNode.IsUp(node5));
        Assert.IsFalse(GraphNode.IsLeft(node5));
        Assert.IsTrue(GraphNode.IsDown(node5));
    }
}
}
