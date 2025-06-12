using Core.Domain.Authentication;

namespace Core.Domain.UnitTests.Authentication;

[TestClass]
public class SeatKeyUtilitiesTests
{
    [TestMethod]
    public void GenerateKey_ReturnsNonemptyString()
    {
        // Arrange

        // Act
        var result = SeatKeyUtilities.GenerateKey();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual(0, result.Length);
    }

    [DataTestMethod]
    [DataRow(8)]
    [DataRow(16)]
    [DataRow(24)]
    [DataRow(32)]
    [DataRow(64)]
    [DataRow(128)]
    [DataRow(256)]
    public void GenerateKey_WithBitLength_ReturnsStringOfMatchingLength(int bitLength)
    {
        // Arrange
        var base64Length = (bitLength / 6 + 3) & ~3;

        // Act
        var key = SeatKeyUtilities.GenerateKey(bitLength);

        // Assert
        Assert.AreEqual(base64Length, key.Length);
    }

    [TestMethod]
    public void VerifyKey_WhenPassedCorrectKey_Passes()
    {
        // Arrange
        var key = SeatKeyUtilities.GenerateKey();

        // Act
        var result = SeatKeyUtilities.VerifyKey(key, key);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void VerifyKey_WhenPassedSeparateKey_Fails()
    {
        // Arrange
        var key1 = SeatKeyUtilities.GenerateKey();
        var key2 = SeatKeyUtilities.GenerateKey();

        // Act
        var result = SeatKeyUtilities.VerifyKey(key1, key2);

        // Assert
        Assert.IsFalse(result);
    }
}
