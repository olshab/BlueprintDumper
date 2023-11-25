using BlueprintDumper.Components;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using BlueprintDumper.Utils;

namespace BlueprintDumper.BlueprintGeneratedClass
{
    public class UBlueprintGeneratedClass
    {
        public USimpleConstructionScript? SimpleConstructionScript { get; private set; }
        public UInheritableComponentHandler? InheritableComponentHandler { get; private set; }

        public UBlueprintGeneratedClass(ClassExport ClassExport)
        {
            /** SimpleConstructionScript */
            ObjectPropertyData? SCS_Object =
                ClassExport.FindPropertyByName<ObjectPropertyData>("SimpleConstructionScript");

            if (SCS_Object is not null)
                SimpleConstructionScript = new USimpleConstructionScript((NormalExport)SCS_Object.ToExport(ClassExport.Asset));

            /** InheritableComponentHandler */
            ObjectPropertyData? ICH_Object =
                ClassExport.FindPropertyByName<ObjectPropertyData>("InheritableComponentHandler");

            if (ICH_Object is not null)
                InheritableComponentHandler = new UInheritableComponentHandler((NormalExport)ICH_Object.ToExport(ClassExport.Asset));
        }

        public List<UActorComponent> GetAllComponents()
        {
            List<UActorComponent> Components = new List<UActorComponent>();

            if (SimpleConstructionScript is not null)
            {
                foreach (USCS_Node SCS_Node in SimpleConstructionScript.AllNodes)
                    if (SCS_Node.ComponentTemplate is not null)
                        Components.Add(SCS_Node.ComponentTemplate);
            }

            if (InheritableComponentHandler is not null)
            {
                foreach (FComponentOverrideRecord Record in InheritableComponentHandler.Records)
                    if (Record.ComponentTemplate is not null)
                        Components.Add(Record.ComponentTemplate);
            }

            return Components;
        }

        public List<T> GetComponentsOfClass<T>()
            where T : UActorComponent
        {
            List<T> Components = new List<T>();

            if (SimpleConstructionScript is not null)
            {
                foreach (USCS_Node SCS_Node in SimpleConstructionScript.AllNodes)
                    if (SCS_Node.ComponentTemplate is not null && SCS_Node.ComponentTemplate is T Component)
                        Components.Add(Component);
            }

            if (InheritableComponentHandler is not null)
            {
                foreach (FComponentOverrideRecord Record in InheritableComponentHandler.Records)
                    if (Record.ComponentTemplate is not null && Record.ComponentTemplate is T Component)
                        Components.Add(Component);
            }

            return Components;
        }

        public List<UActorComponent> GetComponentsOfClass(string ComponentClass)
        {
            /** Search for all components with the exact Class name */

            List<UActorComponent> Components = new List<UActorComponent>();

            if (SimpleConstructionScript is not null)
            {
                foreach (USCS_Node SCS_Node in SimpleConstructionScript.AllNodes)
                    if (SCS_Node.ComponentTemplate is not null && SCS_Node.ComponentTemplate.ClassName == ComponentClass)
                        Components.Add(SCS_Node.ComponentTemplate);
            }

            if (InheritableComponentHandler is not null)
            {
                foreach (FComponentOverrideRecord Record in InheritableComponentHandler.Records)
                    if (Record.ComponentTemplate is not null && Record.ComponentTemplate.ClassName == ComponentClass)
                        Components.Add(Record.ComponentTemplate);
            }

            return Components;
        }

        public bool HasAnyComponents()
        {
            // Any blueprint which is a child of AActor class has SimpleConstructionScript in it
            if (SimpleConstructionScript is null)
                return false;

            if (SimpleConstructionScript.AllNodes.Count > 1)
                return true;

            // If there is only one SCS_Node, chech if its name is not "DefaultSceneRoot"
            if (SimpleConstructionScript.AllNodes.Count == 1)
                if (SimpleConstructionScript.AllNodes[0].InternalVariableName != "DefaultSceneRoot")
                    return true;

            if (InheritableComponentHandler is not null && InheritableComponentHandler.Records.Count > 0)
                return true;

            return false;
        }
    }
}
