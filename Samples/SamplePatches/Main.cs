using MonoPatcherLib;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Utilities;
using Sims3.UI.OnlineDating;
using System;
using System.Reflection;

namespace SamplePatches
{
    // Classes with the [Plugin] attribute will be automatically created as soon as possible by MonoPatcher. You don't need a tuning XML to instantiate.
    [Plugin]
    public class Main
    {
        public Main()
        {
            // Applies all patch attributes found in your mod's DLL. This should always be done in your plugin constructor, before most code is compiled.
            MonoPatcher.PatchAll();
        }

        // Replaces the getter for Sim full names.
        [ReplaceProperty(typeof(SimDescriptionCore), nameof(SimDescriptionCore.FullName))]
        private string SimFullNamePatch
        {
            get
            {
                return "Mono Patcher";
            }
        }

        // Replaces the OnlineDatingProfile body type method to make the weight threshold higher.
        [ReplaceMethod(typeof(OnlineDatingProfile), nameof(OnlineDatingProfile.GetDefaultBodyType))]
        private DatingBodyTypes GetDefaultBodyTypePatch()
        {
            var profile = (OnlineDatingProfile)(this as object);
            // From > 0.0f to 0.5f
            if (profile.mSimDescription.Weight > 0.5f)
            {
                return DatingBodyTypes.MoreToLove;
            }
            if (profile.mSimDescription.Fitness > 0.5f)
            {
                return DatingBodyTypes.Athletic;
            }
            return DatingBodyTypes.Slim;
        }

        // Same as the above patch, but using IL weaving. For more advanced users - harder and limited, but more compatible as multiple IL patches can be stacked together.
        public class GetDefaultBodyTypeILPatch : ILPatch
        {
            protected override MethodInfo TargetMethod
            {
                get
                {
                    return typeof(OnlineDatingProfile).GetMethod(nameof(OnlineDatingProfile.GetDefaultBodyType), BindingFlags.NonPublic | BindingFlags.Instance);
                }
            }

            protected override byte[] TargetFragment
            {
                get
                {
                    return new byte[]
                    {
                        // ldc.r4
                        0x22,
                        // 0f
                        0x00, 0x00, 0x00, 0x00
                    };
                }
            }

            protected override byte[] ReplacementFragment
            {
                get
                {
                    var il = TargetFragment;
                    BitConverter.GetBytes(0.5f).CopyTo(il, 1);
                    return il;
                }
            }
        }
    }
}
