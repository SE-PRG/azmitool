namespace azmi_main
{
    public class SharedOptions
    {
        public string identity { get; set; }
        public bool verbose { get; set; }
    }

    public static class SharedAzmiOptions
    {
        public readonly static AzmiOption identity = new AzmiOption("identity", "Client or application ID of managed identity used to authenticate. Example: 117dc05c-4d12-4ac2-b5f8-5e239dc8bc54");
        public readonly static AzmiOption verbose = new AzmiOption("verbose", "If enabled, commands will produce more verbose error output.", ArgType.flag);
    }
}
