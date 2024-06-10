using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Managers;
using SkillsVR.CCK.PackageManager.Registry;
using System.Collections;
using System.Collections.Generic;

namespace SkillsVR.CCK.PackageManager.UI.Events
{
    public class PackageSelectedEvent : AbstractUIEvent<PackageSelectedEvent, CCKPackageInfo>
    {
        public CCKPackageInfo Package => arg1;
    }

    public class PackageCollectionChangedEvent: AbstractUIEvent<PackageCollectionChangedEvent, IEnumerable<CCKPackageInfo>>
    {
        public IEnumerable Packages => arg1;
    }

    public class PackageInstallStateChangedEvent : AbstractUIEvent<PackageInstallStateChangedEvent, string, PackageRegistrationState>
    {
        public string PackageName => arg1;
        public PackageRegistrationState State => arg2;
    }
    public class RegistryLoadedEvent : AbstractUIEvent<RegistryLoadedEvent, CCKRegistry>
    {
        public CCKRegistry CCKRegistry => arg1;
    }

    public class RegistrySelectedEvent : AbstractUIEvent<RegistrySelectedEvent, CCKRegistry>
    {
        public CCKRegistry CCKRegistry => arg1;
    }

    public class SearchTextChangedEvent : AbstractUIEvent<SearchTextChangedEvent, string>
    {
        public string SearchText => arg1;
    }

    public class ShowPackageViewEvent : AbstractUIEvent<ShowPackageViewEvent>
    {
    }

    public class ShowAssetsViewEvent : AbstractUIEvent<ShowAssetsViewEvent>
    {
    }

    public class ReloadViewEvent : AbstractUIEvent<ReloadViewEvent>
    {
    }
}