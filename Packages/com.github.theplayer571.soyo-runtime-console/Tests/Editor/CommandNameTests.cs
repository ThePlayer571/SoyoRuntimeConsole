using NUnit.Framework;
using Soyo.SoyoRuntimeConsole;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    public class CommandNameTests
    {
        [Test]
        public void RemoveUnsupportableChar_RemovesNonWordChars()
        {
            Assert.That(CommandName.RemoveUnsupportableChar("hello world"), Is.EqualTo("helloworld"));
            Assert.That(CommandName.RemoveUnsupportableChar("test-123"), Is.EqualTo("test123"));
            Assert.That(CommandName.RemoveUnsupportableChar("a.b.c"), Is.EqualTo("abc"));
            Assert.That(CommandName.RemoveUnsupportableChar("foo!@#bar"), Is.EqualTo("foobar"));
            Assert.That(CommandName.RemoveUnsupportableChar("hello\nworld"), Is.EqualTo("helloworld"));
        }

        [Test]
        public void RemoveUnsupportableChar_KeepsValidChars()
        {
            Assert.That(CommandName.RemoveUnsupportableChar("abc123"), Is.EqualTo("abc123"));
            Assert.That(CommandName.RemoveUnsupportableChar("ABC_DEF"), Is.EqualTo("ABC_DEF"));
            Assert.That(CommandName.RemoveUnsupportableChar("MyCommand_01"), Is.EqualTo("MyCommand_01"));
        }

        [Test]
        public void RemoveUnsupportableChar_AllInvalid_ReturnsEmpty()
        {
            Assert.That(CommandName.RemoveUnsupportableChar("!@#$%"), Is.EqualTo(string.Empty));
            Assert.That(CommandName.RemoveUnsupportableChar("   "), Is.EqualTo(string.Empty));
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
