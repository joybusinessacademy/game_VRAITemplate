using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GraphProcessor;
using UnityEngine.UIElements;
using SkillsVRNodes.Scripts.Nodes;
using SkillsVRNodes.Editor.NodeViews;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEditor;
using System.Management.Instrumentation;

public class LogicGroupView : GroupView
{

    protected VisualElement topPortContainer;

    EnterGroupNodeView entryNodeView;
    ExitGroupNodeView exitNodeView;
    private UQueryState<UnityEditor.Experimental.GraphView.Node> allNodes;

    public LogicGroup GetLogicGroup => group as LogicGroup;

    readonly string groupStyle = "GraphProcessorStyles/LogicGroupView";

    public LogicGroupView()
    {
        styleSheets.Add(Resources.Load<StyleSheet>(groupStyle));
    }

    public override void Initialize(BaseGraphView graphView, GraphProcessor.Group block)
    {

        LogicGroup groupCast = block as LogicGroup;

        if (graphView == null || block == null)
        {
            return;
        }

        if (groupCast == null)
        {
            base.Initialize(graphView, block);
            return;
        }

        base.Initialize(graphView, groupCast);
        SetUpGroupConnectionNodes(graphView);

        allNodes = owner.nodes;
    }

    private void SetUpGroupConnectionNodes(BaseGraphView graphView)
    {
        if (!GetLogicGroup.enterGroupNodeGUID.IsNullOrWhitespace())
        {
            entryNodeView = graphView?.nodeViews?.First(node => node?.nodeTarget?.GUID == GetLogicGroup?.enterGroupNodeGUID) as EnterGroupNodeView;
        }
        if (entryNodeView == null)
        {
            entryNodeView = graphView?.AddNode(BaseNode.CreateFromType(typeof(EnterGroupNode), this.transform.position)) as EnterGroupNodeView;
            GetLogicGroup.enterGroupNodeGUID = entryNodeView?.nodeTarget.GUID;
        }

        if (!GetLogicGroup.exitGroupNodeGUID.IsNullOrWhitespace())
        {
            exitNodeView = graphView?.nodeViews?.First(node => node?.nodeTarget?.GUID == GetLogicGroup?.exitGroupNodeGUID) as ExitGroupNodeView;
        }
        if (exitNodeView == null)
        {
            exitNodeView = graphView?.AddNode(BaseNode.CreateFromType(typeof(ExitGroupNode), this.transform.position)) as ExitGroupNodeView;
            GetLogicGroup.exitGroupNodeGUID = exitNodeView?.nodeTarget.GUID;
        }

        ProcessDataOnCopy();
        SetupNodeVisuals();
    }

    private void SetupNodeVisuals()
    {
        VisualElement container = contentContainer.Q("centralContainer");
        container.Insert(0, entryNodeView);
        container.Add(exitNodeView);
        entryNodeView.style.position = Position.Relative;
        exitNodeView.style.position = Position.Relative;
        entryNodeView.style.alignSelf = Align.Center;
        exitNodeView.style.alignSelf = Align.Center;

        container.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
    }

    private void ProcessDataOnCopy()
    {

        foreach (var nodePort in GetLogicGroup.entryOnCopy.nodePortsOutput)
        {         
                owner.graph.Connect(nodePort,
                    GetLogicGroup.GetEnterGroupNode(owner.graph).GetOutputPortByName("Complete"));
        }

        foreach (var nodePort in GetLogicGroup.entryOnCopy.nodePortsInput)
        {
           
                owner.graph.Connect(GetLogicGroup.GetEnterGroupNode(owner.graph).GetInputPortByName("executed"),
                   nodePort);

        }


        foreach (var nodePort in GetLogicGroup.exitOnCopy.nodePortsOutput)
        {
            owner.graph.Connect(nodePort,
                GetLogicGroup.GetExitGroupNode(owner.graph).GetOutputPortByName("Complete"));
        }

        foreach (var nodePort in GetLogicGroup.exitOnCopy.nodePortsInput)
        {

            owner.graph.Connect(GetLogicGroup.GetExitGroupNode(owner.graph).GetInputPortByName("executed"),
               nodePort);

        }

        GetLogicGroup.exitOnCopy = new ConnectToOuterNode();
        GetLogicGroup.entryOnCopy = new ConnectToOuterNode();

    }


    protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
    {
        base.OnElementsAdded(elements);

        //return;

        foreach (GraphElement element in elements)
        {
            if (element is not BaseNodeView node)
            {
                continue;
            }



            // For all the edges going into the node
            List<EdgeView> enterEdges = new();
            foreach (PortView inputPorts in node.inputPortViews)
            {
                foreach (EdgeView edgeView in inputPorts.GetEdges())
                {
                    if (edgeView.output.node is EnterGroupNodeView or ExitGroupNodeView)
                    {
                        continue;
                    }
                    if (containedElements.Contains(edgeView.output.node))
                    {
                        continue;
                    }
                    enterEdges.Add(edgeView);
                }
            }



            List<EdgeView> exitEdges = new();
            // For all the edges coming out of the node
            foreach (PortView outputPorts in node.outputPortViews)
            {
                foreach (EdgeView edgeView in outputPorts.GetEdges())
                {
                    if (edgeView.output.node is EnterGroupNodeView or ExitGroupNodeView)
                    {
                        continue;
                    }
                    if (containedElements.Contains(edgeView.input.node))
                    {
                        continue;
                    }
                    exitEdges.Add(edgeView);
                }
            }




            foreach (EdgeView edge in enterEdges)
            {
                if (edge == null || entryNodeView == null)
                {
                    continue;
                }

                PortView entryOutput = entryNodeView.outputPortViews[0];
                PortView entryInput = entryNodeView.inputPortViews[0];

                PortView output = edge.output as PortView;
                PortView input = edge.input as PortView;
                owner.Connect(input, entryOutput);
                owner.Connect(entryInput, output);
                

                edge.serializedEdge.Deserialize();
                owner.Disconnect(edge);
            }

            foreach (EdgeView edge in exitEdges)
            {
                if (edge == null || exitNodeView == null)
                {
                    continue;
                }

                PortView entryOutput = exitNodeView.outputPortViews[0];
                PortView entryInput = exitNodeView.inputPortViews[0];

                PortView output = edge.output as PortView;
                PortView input = edge.input as PortView;
                owner.Connect(input, entryOutput);
                owner.Connect(entryInput, output);

                edge.serializedEdge.Deserialize();
                owner.Disconnect(edge);
            }


            if (!GetLogicGroup.innerNodeGUIDs.Contains(node.nodeTarget.GUID))
            {
                GetLogicGroup.innerNodeGUIDs.Add(node.nodeTarget.GUID);
            }
          
            node.onPortConnected += AddToGroupOnCreate;

        }
       
    }

    private void AddToGroupOnCreate(PortView view)
    {

        foreach (EdgeView edgeView in view.GetEdges())
        {
            if (edgeView.output.node is EnterGroupNodeView or ExitGroupNodeView)
            {
                continue;
            }

            if (edgeView.input.node is EnterGroupNodeView or ExitGroupNodeView)
            {
                continue;
            }

            if (!group.innerNodeGUIDs.Contains(edgeView.serializedEdge.inputPort.owner.GUID))
            {
                group.innerNodeGUIDs.Add(edgeView.serializedEdge.inputPort.owner.GUID);
                AddElement(edgeView.input.node);
            }
            else if (!group.innerNodeGUIDs.Contains(edgeView.serializedEdge.outputPort.owner.GUID))
            {
                group.innerNodeGUIDs.Add(edgeView.serializedEdge.outputPort.owner.GUID);
                AddElement(edgeView.output.node);
            }

        }

    }

    protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
    {
        foreach (GraphElement element in elements)
        {
            if (element is not BaseNodeView node)
            {
                continue;
            }

            node.onPortConnected -= AddToGroupOnCreate;
        }

        base.OnElementsRemoved(elements);
    }


    public override void Dispose()
    {
        foreach (PortView inputPorts in entryNodeView.inputPortViews)
        {
            List<EdgeView> ev = inputPorts.GetEdges();
            for (int i = ev.Count - 1; i >= 0; i--)
            {
                owner.Disconnect(ev[i], true);
            }
        }

        foreach (PortView outputPorts in exitNodeView.outputPortViews)
        {
            List<EdgeView> ev = outputPorts.GetEdges();
            for (int i = ev.Count - 1; i >= 0; i--)
            {
                owner.Disconnect(ev[i], true);
            }
        }

        base.Dispose();
    }
}
