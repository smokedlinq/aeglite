namespace aeglite.Tests;

public class CircularBufferTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Count_ShouldNeverExceedSize(int size)
    {
        // Arrange
        var buffer = new CircularBuffer<int>(size);

        // Act
        for (int i = 0; i < size * 2; i++)
        {
            buffer.Append(i);
        }

        // Assert
        buffer.Count.Should().Be(size);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Index0_ShouldBeOldestItem(int size)
    {
        // Arrange
        var buffer = new CircularBuffer<int>(size);

        // Act
        for (int i = 0; i < size * 2; i++)
        {
            buffer.Append(i);
        }

        // Assert
        buffer[0].Should().Be(size);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Enumerator_ShouldBeInOrder(int size)
    {
        // Arrange
        var buffer = new CircularBuffer<int>(size);

        // Act
        for (int i = 0; i < size * 2; i++)
        {
            buffer.Append(i);
        }

        // Assert
        var expected = Enumerable.Range(size, size);
        buffer.Should().Equal(expected);
    }
}