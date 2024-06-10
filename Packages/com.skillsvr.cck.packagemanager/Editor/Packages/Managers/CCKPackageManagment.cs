using SkillsVR.CCK.PackageManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkillsVR.CCK.PackageManager.Managers
{
    public class CCKPackageManagment
    {
        public static CCKPackageManagment Main { get; } = new CCKPackageManagment();


        public CCKPackageInfo GetPackageInfo(string packageIdOrName)
        {
            var pkg = GetPackageByName(packageIdOrName);
            if (null == pkg)
            {
                pkg = GetPackageById(packageIdOrName);
            }
            return pkg;
        }

        public CCKPackageInfo GetPackageById(string id)
        {
            return FindPackage(x => id == x.GetFullId());
        }

        public CCKPackageInfo GetPackageByName(string id)
        {
            return FindPackage(x => id == x.name);
        }

        public IEnumerable<CCKPackageInfo> FindPackages(Predicate<CCKPackageInfo> predicate)
        {
            if (null == predicate)
            {
                return null;
            }

            return CCKRegistryManager.Main.Registries
                .Where(registry => null != registry && null == registry.packages)
                .SelectMany(registry => registry.packages
                            .Where(pkg => null != pkg && predicate.Invoke(pkg)));
        }

        public CCKPackageInfo FindPackage(Predicate<CCKPackageInfo> predicate)
        {
            if (null == predicate)
            {
                return null;
            }

            foreach (var registry in CCKRegistryManager.Main.Registries)
            {
                if (null == registry || null == registry.packages)
                {
                    continue;
                }
                var pkg = registry.packages.FirstOrDefault(x => null != x && predicate.Invoke(x));
                if (null != pkg)
                {
                    return pkg;
                }
            }

            return null;
        }
    }
}