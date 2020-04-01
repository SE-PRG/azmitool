namespace azmi_main
{
    public class SharedAzmiArgumentsClass
    {
        public string identity { get; set; }
        public bool verbose { get; set; }
    }

    public static class SharedAzmiArguments
    {
        public readonly static AzmiArgument identity = new AzmiArgument("identity", "Client or application ID of managed identity used to authenticate. Example: 117dc05c-4d12-4ac2-b5f8-5e239dc8bc54");
        public readonly static AzmiArgument verbose = new AzmiArgument("verbose", "If enabled, commands will produce more verbose error output.", ArgType.flag);
    }
}
