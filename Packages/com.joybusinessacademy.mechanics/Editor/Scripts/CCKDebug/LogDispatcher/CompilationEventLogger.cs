using UnityEditor;
using UnityEditor.Compilation;

namespace SkillsVRNodes.Diagnostics
{
    [InitializeOnLoad]
    internal class CompilationEventLogger
    {
        static CompilationEventLogger()
        {
            CompilationPipeline.compilationStarted += CompilationPipeline_compilationStarted;
            CompilationPipeline.compilationFinished += CompilationPipeline_compilationFinished;
        }

        private static void CompilationPipeline_compilationFinished(object obj)
        {
            CCKDebug.Log("Finish Compline");
        }

        private static void CompilationPipeline_compilationStarted(object obj)
        {
            CCKDebug.Log("Start Compline");
        }
    }
}

