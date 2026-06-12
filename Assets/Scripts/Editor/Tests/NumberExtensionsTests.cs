using System.Globalization;
using System.Threading;
using BrainTV.Tools.NumberExtensions;
using NUnit.Framework;

/// <summary>
/// Edit-mode tests for the locale-independent number parsing. The historical bug: parsing
/// tried the machine's CurrentCulture first, so the same file gave different values on a
/// French machine than on an English one. The contract is invariant-first with a fr-FR
/// fallback for legacy comma-decimal content - and the result must not depend on the
/// machine's locale.
/// </summary>
public class NumberExtensionsTests
{
    private CultureInfo m_SavedCulture;

    [SetUp]
    public void SetUp()
    {
        m_SavedCulture = Thread.CurrentThread.CurrentCulture;
    }

    [TearDown]
    public void TearDown()
    {
        Thread.CurrentThread.CurrentCulture = m_SavedCulture;
    }

    [Test]
    public void TryParseFloat_ParsesInvariantNotation()
    {
        Assert.IsTrue("1.5".TryParseFloat(out float result));
        Assert.AreEqual(1.5f, result);
    }

    [Test]
    public void TryParseFloat_ParsesLegacyFrenchCommaDecimal()
    {
        Assert.IsTrue("1,5".TryParseFloat(out float result));
        Assert.AreEqual(1.5f, result);
    }

    [Test]
    public void TryParseFloat_DoesNotDependOnTheMachineLocale()
    {
        // The review's case: "1.000" must be 1, never 1000, even on a French machine where
        // '.' could read as a group separator.
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("fr-FR");
        Assert.IsTrue("1.000".TryParseFloat(out float onFrench));
        Assert.AreEqual(1f, onFrench);

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Assert.IsTrue("1.000".TryParseFloat(out float onInvariant));
        Assert.AreEqual(onFrench, onInvariant, "the parsed value must not depend on the machine's locale");
    }

    [Test]
    public void TryParseFloat_RejectsGarbage()
    {
        Assert.IsFalse("abc".TryParseFloat(out float result));
        Assert.AreEqual(0f, result);
    }

    [Test]
    public void TryParseInt_ParsesPlainIntegers_AndRejectsFloats()
    {
        Assert.IsTrue("42".TryParseInt(out int result));
        Assert.AreEqual(42, result);
        Assert.IsFalse("4.2".TryParseInt(out _));
    }
}
