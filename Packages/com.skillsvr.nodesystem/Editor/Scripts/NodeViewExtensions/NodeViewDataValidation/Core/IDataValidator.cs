using GraphProcessor;
using System.Collections.Generic;

namespace SkillsVRNodes.Editor.NodeViews.Validation
{
	public interface IDataValidator
    {
        IEnumerable<IValidationResult> Validate();
		void SetValidateSourceObject(object dataSource);
	}
}