using NUnit.Framework;
using Soyo.SoyoRuntimeConsole;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    public class CommandNameTests
    {
        [Test]
        public void RemoveUnsupportableChar_RemovesNonWordChars()
        {
            var (result, changed) = CommandName.RemoveUnsupportableChar("hello world");
            Assert.That(result, Is.EqualTo("helloworld"));
            Assert.IsTrue(changed);

            (result, changed) = CommandName.RemoveUnsupportableChar("test-123");
            Assert.That(result, Is.EqualTo("test123"));
            Assert.IsTrue(changed);

            (result, changed) = CommandName.RemoveUnsupportableChar("a.b.c");
            Assert.That(result, Is.EqualTo("abc"));
            Assert.IsTrue(changed);

            (result, changed) = CommandName.RemoveUnsupportableChar("foo!@#bar");
            Assert.That(result, Is.EqualTo("foobar"));
            Assert.IsTrue(changed);

            (result, changed) = CommandName.RemoveUnsupportableChar("hello\nworld");
            Assert.That(result, Is.EqualTo("helloworld"));
            Assert.IsTrue(changed);
        }

        [Test]
        public void RemoveUnsupportableChar_KeepsValidChars()
        {
            var (result, changed) = CommandName.RemoveUnsupportableChar("abc123");
            Assert.That(result, Is.EqualTo("abc123"));
            Assert.IsFalse(changed);

            (result, changed) = CommandName.RemoveUnsupportableChar("ABC_DEF");
            Assert.That(result, Is.EqualTo("ABC_DEF"));
            Assert.IsFalse(changed);

            (result, changed) = CommandName.RemoveUnsupportableChar("MyCommand_01");
            Assert.That(result, Is.EqualTo("MyCommand_01"));
            Assert.IsFalse(changed);
        }

        [Test]
        public void RemoveUnsupportableChar_AllInvalid_ReturnsEmpty()
        {
            var (result, changed) = CommandName.RemoveUnsupportableChar("!@#$%");
            Assert.That(result, Is.EqualTo(string.Empty));
            Assert.IsTrue(changed);

            (result, changed) = CommandName.RemoveUnsupportableChar("   ");
            Assert.That(result, Is.EqualTo(string.Empty));
            Assert.IsTrue(changed);
        }

        [Test]
        public void IsSupportable_ValidNames_ReturnTrue()
        {
            Assert.IsTrue(CommandName.IsSupportable("hello"));
            Assert.IsTrue(CommandName.IsSupportable("HelloWorld"));
            Assert.IsTrue(CommandName.IsSupportable("test_123"));
            Assert.IsTrue(CommandName.IsSupportable("ABC"));
            Assert.IsTrue(CommandName.IsSupportable("_private"));
        }

        [Test]
        public void IsSupportable_InvalidNames_ReturnFalse()
        {
            Assert.IsFalse(CommandName.IsSupportable(""));
            Assert.IsFalse(CommandName.IsSupportable("hello world"));
            Assert.IsFalse(CommandName.IsSupportable("test-123"));
            Assert.IsFalse(CommandName.IsSupportable("foo.bar"));
            Assert.IsFalse(CommandName.IsSupportable("a b c"));
        }

        [Test]
        public void Constructor_ValidName_NameReturnsInput()
        {
            var cmd = new CommandName("test_command");
            Assert.That(cmd.Name, Is.EqualTo("test_command"));

            var cmd2 = new CommandName("MyCommand");
            Assert.That(cmd2.Name, Is.EqualTo("MyCommand"));
        }

        [Test]
        public void Constructor_UnsupportableChars_RemovesThem()
        {
            var cmd = new CommandName("hello world");
            Assert.That(cmd.Name, Is.EqualTo("helloworld"));

            var cmd2 = new CommandName("test-123");
            Assert.That(cmd2.Name, Is.EqualTo("test123"));
        }

        [Test]
        public void Constructor_AllCharsInvalid_DefaultsToNull()
        {
            var cmd = new CommandName("!@#$%");
            Assert.That(cmd.Name, Is.EqualTo("null"));

            var cmd2 = new CommandName("   ");
            Assert.That(cmd2.Name, Is.EqualTo("null"));
        }

        [Test]
        public void Constructor_EmptyString_DefaultsToNull()
        {
            var cmd = new CommandName(string.Empty);
            Assert.That(cmd.Name, Is.EqualTo("null"));
        }
    }
}
