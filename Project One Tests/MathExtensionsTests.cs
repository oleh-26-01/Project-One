using System.Numerics;
using Project_One_Objects.Helpers;

namespace Project_One_Tests;

[TestFixture]
public class MathExtensionsTests
{
    [Test]
    public void LineIntersection_DenominatorEqualsZero_ReturnsNaNVector2()
    {
        // Arrange
        var a = new Vector2(2, 2);
        var b = new Vector2(6, 4);
        var c = new Vector2(1, 3);
        var d = new Vector2(5, 5);
        // Act
        var result = MathExtensions.LineIntersection(a, b, c, d);

        // Assert
        Assert.IsTrue(float.IsNaN(result.X) && float.IsNaN(result.Y));
    }

    [Test]
    public void LineIntersection_DenominatorNotEqualZero_ReturnsIntersection()
    {
        // Arrange
        var a = new Vector2(0, 0);
        var b = new Vector2(2, 2);
        var c = new Vector2(0, 2);
        var d = new Vector2(2, 0);
        var expected = new Vector2(1, 1);

        // Act
        var result = MathExtensions.LineIntersection(a, b, c, d);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void LineIntersection_VerticalLine_ReturnsIntersection()
    {
        // Arrange
        var a = new Vector2(0, 0);
        var b = new Vector2(0, 2);
        var c = new Vector2(1, 1);
        var d = new Vector2(2, 2);
        var expected = new Vector2(0, 0);

        // Act
        var result = MathExtensions.LineIntersection(a, b, c, d);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void LineIntersection_HorizontalLine_ReturnsIntersection()
    {
        // Arrange
        var a = new Vector2(2, 1);
        var b = new Vector2(6, 1);
        var c = new Vector2(4, 0);
        var d = new Vector2(4, 2);
        var expected = new Vector2(4, 1);

        // Act
        var result = MathExtensions.LineIntersection(a, b, c, d);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void LineIntersection_DiagonalLines_ReturnsIntersection()
    {
        // Arrange
        var a = new Vector2(-1, 1);
        var b = new Vector2(1, 1);
        var c = new Vector2(0, -1);
        var d = new Vector2(0, 1);
        var expected = new Vector2(0, 1);

        // Act
        var result = MathExtensions.LineIntersection(a, b, c, d);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    // CInsertionSort sort Vector2 by Y value
    public void InsertionSort_SortedList_ReturnsSameList()
    {
        // Arrange
        Vector2[] list = { new(1, 1), new(2, 2), new(3, 3), new(4, 4), new(5, 5) };

        // Act
        MathExtensions.CInsertionSort(list);

        // Assert
        Assert.AreEqual(1, list[0].Y);
        Assert.AreEqual(2, list[1].Y);
        Assert.AreEqual(3, list[2].Y);
        Assert.AreEqual(4, list[3].Y);
        Assert.AreEqual(5, list[4].Y);
    }

    [Test]
    public void InsertionSort_UnsortedList_ReturnsSortedList()
    {
        // Arrange
        Vector2[] list = { new(5, 5), new(4, 4), new(3, 3), new(2, 2), new(1, 1) };

        // Act
        MathExtensions.CInsertionSort(list);

        // Assert
        Assert.AreEqual(1, list[0].Y);
        Assert.AreEqual(2, list[1].Y);
        Assert.AreEqual(3, list[2].Y);
        Assert.AreEqual(4, list[3].Y);
        Assert.AreEqual(5, list[4].Y);
    }

    [Test]
    public void InsertionSort_SortedListWithDuplicates_ReturnsSortedList()
    {
        // Arrange
        Vector2[] list = { new(1, 1), new(2, 2), new(2, 2), new(3, 3), new(4, 4), new(5, 5) };

        // Act
        MathExtensions.CInsertionSort(list);

        // Assert
        Assert.AreEqual(1, list[0].Y);
        Assert.AreEqual(2, list[1].Y);
        Assert.AreEqual(2, list[2].Y);
        Assert.AreEqual(3, list[3].Y);
        Assert.AreEqual(4, list[4].Y);
        Assert.AreEqual(5, list[5].Y);
    }

    [Test]
    public void InsertionSort_UnsortedListWithDuplicates_ReturnsSortedList()
    {
        // Arrange
        Vector2[] list = { new(5, 5), new(4, 4), new(3, 3), new(2, 2), new(2, 2), new(1, 1) };

        // Act
        MathExtensions.CInsertionSort(list);

        // Assert
        Assert.AreEqual(1, list[0].Y);
        Assert.AreEqual(2, list[1].Y);
        Assert.AreEqual(2, list[2].Y);
        Assert.AreEqual(3, list[3].Y);
        Assert.AreEqual(4, list[4].Y);
        Assert.AreEqual(5, list[5].Y);
    }

    [Test]
    public void InsertionSort_SortedListWithNegativeValues_ReturnsSortedList()
    {
        // Arrange
        Vector2[] list = { new(-5, -5), new(-4, -4), new(-3, -3), new(-2, -2), new(-1, -1) };

        // Act
        MathExtensions.CInsertionSort(list);

        // Assert
        Assert.AreEqual(-5, list[0].Y);
        Assert.AreEqual(-4, list[1].Y);
        Assert.AreEqual(-3, list[2].Y);
        Assert.AreEqual(-2, list[3].Y);
        Assert.AreEqual(-1, list[4].Y);
    }

    [Test]
    public void InsertionSort_UnsortedListWithNegativeValues_ReturnsSortedList()
    {
        // Arrange
        Vector2[] list = { new(-1, -1), new(-2, -2), new(-3, -3), new(-4, -4), new(-5, -5) };

        // Act
        MathExtensions.CInsertionSort(list);

        // Assert
        Assert.AreEqual(-5, list[0].Y);
        Assert.AreEqual(-4, list[1].Y);
        Assert.AreEqual(-3, list[2].Y);
        Assert.AreEqual(-2, list[3].Y);
        Assert.AreEqual(-1, list[4].Y);
    }
}