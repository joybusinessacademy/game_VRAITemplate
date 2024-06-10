namespace SkillsVR.CCK.PackageManager.Data
{
    public static class Flags
    {
        public const string Hide = nameof(Hide);
        public const string ReloadAfterInstall = nameof(ReloadAfterInstall);
        public const string RegistrySource = nameof(RegistrySource);
        public const string Embed = nameof(Embed);
        public const string AutoUpdate = nameof(AutoUpdate);
        public const string Required = nameof(Required);
        public const string Order = nameof(Order);
        public const string ExtImportDir = nameof(ExtImportDir);
        public const string PkgResolver = nameof(PkgResolver);
        public const string PkgAuth = nameof(PkgAuth);
        public const string PreviewResolver = nameof(PreviewResolver);
        public const string PreviewAuth = nameof(PreviewAuth);
        public const string GitBranch = nameof(GitBranch);
    }

    public static class RegistrySourceResolverKeys
    {
        public const string LocalJson = nameof(LocalJson);
        public const string GithubJson = nameof(GithubJson);
    }

    public static class PackageResolverKeys
    {
        public const string Url = nameof(Url);
        public const string Github = nameof(Github);
        public const string AzureNpm = nameof(AzureNpm);
    }
    public static class AuthKeys
    {
        public const string GithubAuthKey = "Github.x-oauth-basic";
    }
}