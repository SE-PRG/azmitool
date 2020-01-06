using System;
using Xunit;
using azmi_main;

namespace azmi_tests
{
    public class HelpMessageTests
    {
        [Fact]
        public void TrueIsTrue()
        {
            Assert.True(true);
        }

        //
        // HelpMessage tests
        //

        [Fact]
        public void HelpMessage_ExistsForApplication()
        {
            var helpResponse = HelpMessage.application();
            Assert.Contains(helpResponse, s => s.Contains("Usage"));
            foreach (var subCommand in HelpMessage.supportedSubCommands)
            {
                Assert.Contains(helpResponse, s => s.Contains($"{subCommand} help"));
            }
        }

        [Fact]
        public void HelpMessage_ExistsForEachSubcommand()
        {
            foreach (var subCommand in HelpMessage.supportedSubCommands)
            {
                var helpResponse = HelpMessage.subCommand(subCommand);
                Assert.Contains(helpResponse, s => s.Contains($"{subCommand} help"));
            }
        }

        [Fact]
        public void HelpMessage_InvalidSubcommand()
        {
            var subCommand = "invalidOne";
            var ex = Assert.Throws<ArgumentNullException>(() => HelpMessage.subCommand(subCommand));
            Assert.Contains($"Unknown help for subcommand '{subCommand}'.", ex.Message);
        }
    }
}
