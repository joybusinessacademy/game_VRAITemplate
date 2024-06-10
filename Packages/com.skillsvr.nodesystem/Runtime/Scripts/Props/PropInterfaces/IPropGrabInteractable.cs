
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Props.PropInterfaces
{
    public interface IPropGrabInteractable : IBaseProp
    {
        public XRGrabInteractable GetIrabInteractable();
        public XRRaySocketSelectTransformer GetRaySocket();
        public XRGrabOffsetTransformer GetGrabOffset();
        public XRGrabRotationTransformer GetGrabRotation();
        public XRGrabScaleTransformer GetGrabScale();
        public InteractableMover GetMover();
        public SocketableTag GetSocketableTag();
        public SVRColorMaterialPropertyAffordanceReceiver GetMatPropertyReciever();
        public Collider GetCollider();
        public Rigidbody GetRigidBody();
        public  void MuteInScene(DragDropNode referencer);
        public  void UnmuteInScene(DragDropNode referencer);
    }
}
