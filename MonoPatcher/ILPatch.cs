using System;
using System.Reflection;

namespace MonoPatcherLib
{
    /// <summary>
    /// This class can be implemented to replace parts of a method body using IL weaving.
    /// </summary>
    /// <remarks>
    /// Having multiple IL replacements on the same method generally will not conflict, unless they both modify the same IL instructions.<br/>
    /// Whole-method replacements (e.g. <seealso cref="ReplaceMethodAttribute"/>) will override any already-applied IL patches, but patches can still be applied after replacement, so long as the bytecode segment to replace still exists.
    /// </remarks>
    public abstract class ILPatch
    {
        /// <summary>
        /// Gets a <see cref="MethodInfo"/> representing the method to be patched.
        /// </summary>
        protected abstract MethodInfo TargetMethod { get; }
        /// <summary>
        /// Gets an array representing a bytecode segment within <see cref="TargetMethod">TargetMethod</see> to be replaced.
        /// </summary>
        /// <remarks>
        /// Make sure the target IL fragment is exactly the same length as its replacement.
        /// This can be done by e.g. adding <c>nop</c>s, or using <c>dup</c> rather than <c>stloc</c> + <c>ldloc</c> to reuse stack values.
        /// </remarks>
        protected abstract byte[] TargetFragment { get; }
        /// <summary>
        /// Gets an array representing a bytecode segment that will replace <see cref="TargetFragment">TargetFragment</see> within <see cref="TargetMethod">TargetMethod</see>.
        /// </summary>
        /// <remarks>
        /// Make sure the replacement IL fragment is exactly the same length as the original.
        /// This can be done by e.g. adding <c>nop</c>s, or using <c>dup</c> rather than <c>stloc</c> + <c>ldloc</c> to reuse stack values.
        /// </remarks>
        protected abstract byte[] ReplacementFragment { get; }

        /// <summary>
        /// Replaces the first instance of <see cref="TargetFragment">TargetFragment</see> within the body of <see cref="TargetMethod">TargetMethod</see> with <see cref="ReplacementFragment">ReplacementFragment</see>.
        /// </summary>
        /// <remarks>
        /// If <c>TargetFragment.Length</c> is not equal to <c>ReplacementFragment.Length</c>, this method will have no effect.
        /// </remarks>
        public void Replace()
        {
            // Cache properties in local variables, to avoid duplication if implementations are computed and not field-backed
            var targetMethod = TargetMethod;
            var targetFragment = TargetFragment;
            var replacementFragment = ReplacementFragment;

            if (targetFragment.Length != replacementFragment.Length) return;

            var methodIl = MonoPatcher.GetIL(targetMethod);

            var replaceIndex = Utility.FindInByteArray(methodIl, targetFragment);

            if (replaceIndex == -1) return;

            Array.Copy(replacementFragment, 0, methodIl, replaceIndex, replacementFragment.Length);
            MonoPatcher.ReplaceIL(targetMethod, methodIl);
        }
    }
}
